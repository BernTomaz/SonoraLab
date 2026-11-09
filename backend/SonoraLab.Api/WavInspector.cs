using System.Buffers.Binary;

namespace SonoraLab.Audio;

public sealed record WavMetadata(double DurationSeconds, int Channels, int SampleRateHz);

internal sealed record WavPcm(WavMetadata Metadata, long Offset, int Frames);

public static class WavInspector
{
    public const int MaxBytes = 20 * 1024 * 1024;

    public static WavMetadata Inspect(Stream stream) => ReadPcm(stream).Metadata;

    internal static WavPcm ReadPcm(Stream stream)
    {
        Span<byte> header = stackalloc byte[12];
        Read(stream, header);
        long end = 8L + BinaryPrimitives.ReadUInt32LittleEndian(header[4..]);

        if (!header[..4].SequenceEqual("RIFF"u8) || !header[8..].SequenceEqual("WAVE"u8)
            || end != stream.Length)
            throw new InvalidDataException();

        Span<byte> chunk = stackalloc byte[8];
        Span<byte> format = stackalloc byte[16];
        int channels = 0;
        int sampleRate = 0;
        int alignment = 0;
        long dataSize = -1;
        long dataOffset = 0;

        while (stream.Position < end)
        {
            if (end - stream.Position < chunk.Length)
                throw new InvalidDataException();

            Read(stream, chunk);
            uint size = BinaryPrimitives.ReadUInt32LittleEndian(chunk[4..]);
            long next = stream.Position + size + (size & 1);
            if (next > end)
                throw new InvalidDataException();

            if (chunk[..4].SequenceEqual("fmt "u8))
            {
                if (alignment != 0 || size < format.Length)
                    throw new InvalidDataException();

                Read(stream, format);
                channels = BinaryPrimitives.ReadUInt16LittleEndian(format[2..]);
                if (BinaryPrimitives.ReadUInt16LittleEndian(format) != 1
                    || channels is not (1 or 2)
                    || BinaryPrimitives.ReadUInt16LittleEndian(format[14..]) != 16)
                    throw new NotSupportedException();

                sampleRate = BinaryPrimitives.ReadInt32LittleEndian(format[4..]);
                alignment = BinaryPrimitives.ReadUInt16LittleEndian(format[12..]);
                long byteRate = BinaryPrimitives.ReadUInt32LittleEndian(format[8..]);
                if (sampleRate <= 0 || alignment != channels * 2
                    || byteRate != (long)sampleRate * alignment)
                    throw new InvalidDataException();
            }
            else if (chunk[..4].SequenceEqual("data"u8))
            {
                if (alignment == 0 || dataSize >= 0 || size == 0 || size % alignment != 0)
                    throw new InvalidDataException();

                dataSize = size;
                dataOffset = stream.Position;
            }

            stream.Position = next;
        }

        if (dataSize < 0)
            throw new InvalidDataException();

        int frames = checked((int)(dataSize / alignment));
        return new WavPcm(new WavMetadata((double)frames / sampleRate, channels, sampleRate), dataOffset, frames);
    }

    private static void Read(Stream stream, Span<byte> buffer)
    {
        try
        {
            stream.ReadExactly(buffer);
        }
        catch (EndOfStreamException exception)
        {
            throw new InvalidDataException("WAV truncado.", exception);
        }
    }
}
