using SonoraLab.Audio;
using Xunit;

public sealed class WavInspectorTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void ReadsPcmMetadata(int channels)
    {
        using var wav = new MemoryStream(BuildWave(channels));
        Assert.Equal(new WavMetadata(0.01, channels, 8000), WavInspector.Inspect(wav));
    }

    [Fact]
    public void RejectsTruncatedWave()
    {
        using var wav = new MemoryStream(BuildWave(1)[..^1]);
        Assert.Throws<InvalidDataException>(() => WavInspector.Inspect(wav));
    }

    [Fact]
    public void RejectsUnsupportedEncoding()
    {
        var bytes = BuildWave(1);
        bytes[20] = 3;
        using var wav = new MemoryStream(bytes);
        Assert.Throws<NotSupportedException>(() => WavInspector.Inspect(wav));
    }

    private static byte[] BuildWave(int channels)
    {
        const int rate = 8000;
        int block = channels * 2;
        int dataSize = 80 * block;
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write("RIFF"u8);
        writer.Write(36 + dataSize);
        writer.Write("WAVEfmt "u8);
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)channels);
        writer.Write(rate);
        writer.Write(rate * block);
        writer.Write((short)block);
        writer.Write((short)16);
        writer.Write("data"u8);
        writer.Write(dataSize);
        writer.Write(new byte[dataSize]);
        return stream.ToArray();
    }
}
