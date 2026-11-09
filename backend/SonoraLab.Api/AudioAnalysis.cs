using System.Buffers.Binary;
using System.Numerics;

namespace SonoraLab.Audio;

public sealed record WaveformBlock(double Min, double Max);
public sealed record SpectrumPoint(double FrequencyHz, double Magnitude);
public sealed record SpectrumResult(double? DominantFrequencyHz, int SampleCount,
    int FftSize, double BinSpacingHz, double NyquistHz, SpectrumPoint[] Points);

public static class AudioAnalysis
{
    private const int PlotPoints = 1024;
    private const int MaxFrames = 524288;

    internal static WaveformBlock[] Waveform(Stream stream, WavPcm wav, CancellationToken cancellation)
    {
        var pcm = ReadFrames(stream, wav, 0, wav.Frames);
        var blocks = new WaveformBlock[Math.Min(wav.Frames, PlotPoints)];
        for (int i = 0; i < blocks.Length; i++)
        {
            cancellation.ThrowIfCancellationRequested();
            int start = (int)((long)i * wav.Frames / blocks.Length);
            int end = (int)((long)(i + 1) * wav.Frames / blocks.Length);
            double min = 1, max = -1;
            for (int frame = start; frame < end; frame++)
            {
                double sample = Sample(pcm, frame, wav.Metadata.Channels);
                min = Math.Min(min, sample);
                max = Math.Max(max, sample);
            }
            blocks[i] = new WaveformBlock(min, max);
        }
        return blocks;
    }

    public static SpectrumResult Analyze(Stream stream, double start, double end) =>
        Analyze(stream, WavInspector.ReadPcm(stream), start, end, CancellationToken.None);

    internal static SpectrumResult Analyze(Stream stream, WavPcm wav, double start, double end,
        CancellationToken cancellation)
    {
        int rate = wav.Metadata.SampleRateHz;
        if (!double.IsFinite(start) || !double.IsFinite(end) ||
            start < 0 || end <= start || end > wav.Metadata.DurationSeconds)
            throw new ArgumentOutOfRangeException(nameof(start));

        int first = (int)Math.Round(start * rate);
        int count = Math.Min(wav.Frames, (int)Math.Round(end * rate)) - first;
        if (count < 4 || count > MaxFrames || count > 5L * rate)
            throw new ArgumentOutOfRangeException(nameof(end));

        int size = (int)BitOperations.RoundUpToPowerOf2((uint)count);
        var pcm = ReadFrames(stream, wav, first, count);
        var bins = new Complex[size];
        double mean = 0;
        for (int i = 0; i < count; i++)
            mean += Sample(pcm, i, wav.Metadata.Channels);
        mean /= count;

        double windowSum = 0;
        for (int i = 0; i < count; i++)
        {
            double window = 0.5 - 0.5 * Math.Cos(2 * Math.PI * i / (count - 1));
            bins[i] = (Sample(pcm, i, wav.Metadata.Channels) - mean) * window;
            windowSum += window;
        }
        Transform(bins, cancellation);

        double spacing = (double)rate / size;
        int positiveBins = size / 2;
        var points = new SpectrumPoint[Math.Min(positiveBins, PlotPoints)];
        double peak = 0;
        double? dominant = null;
        for (int i = 0; i < points.Length; i++)
        {
            int from = 1 + i * positiveBins / points.Length;
            int to = 1 + (i + 1) * positiveBins / points.Length;
            int bestBin = from;
            double best = 0;
            for (int bin = from; bin < to; bin++)
            {
                double magnitude = bins[bin].Magnitude * (bin == positiveBins ? 1 : 2) / windowSum;
                if (magnitude <= best) continue;
                best = magnitude;
                bestBin = bin;
            }
            points[i] = new SpectrumPoint(bestBin * spacing, best);
            if (best <= peak) continue;
            peak = best;
            dominant = bestBin * spacing;
        }

        return new SpectrumResult(peak > 1e-12 ? dominant : null, count, size, spacing, rate / 2.0, points);
    }

    private static byte[] ReadFrames(Stream stream, WavPcm wav, int first, int count)
    {
        int bytesPerFrame = wav.Metadata.Channels * 2;
        var buffer = new byte[count * bytesPerFrame];
        stream.Position = wav.Offset + (long)first * bytesPerFrame;
        stream.ReadExactly(buffer);
        return buffer;
    }

    private static double Sample(byte[] pcm, int frame, int channels)
    {
        int offset = frame * channels * 2;
        int left = BinaryPrimitives.ReadInt16LittleEndian(pcm.AsSpan(offset));
        return channels == 1
            ? left / 32768.0
            : (left + BinaryPrimitives.ReadInt16LittleEndian(pcm.AsSpan(offset + 2))) / 65536.0;
    }

    private static void Transform(Complex[] values, CancellationToken cancellation)
    {
        // Reordena por bits invertidos para a FFT iterativa de base 2.
        for (int i = 1, reversed = 0; i < values.Length; i++)
        {
            int bit = values.Length >> 1;
            for (; (reversed & bit) != 0; bit >>= 1)
                reversed ^= bit;
            reversed ^= bit;
            if (i < reversed)
                (values[i], values[reversed]) = (values[reversed], values[i]);
        }

        for (int length = 2; length <= values.Length; length *= 2)
        {
            cancellation.ThrowIfCancellationRequested();
            int half = length / 2;
            var step = Complex.FromPolarCoordinates(1, -2 * Math.PI / length);
            for (int block = 0; block < values.Length; block += length)
            {
                Complex phase = Complex.One;
                for (int i = block; i < block + half; i++)
                {
                    Complex even = values[i];
                    Complex odd = values[i + half] * phase;
                    values[i] = even + odd;
                    values[i + half] = even - odd;
                    phase *= step;
                }
            }
        }
    }
}
