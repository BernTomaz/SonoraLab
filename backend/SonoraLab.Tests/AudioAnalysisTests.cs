using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SonoraLab.Audio;
using Xunit;

public sealed class AudioAnalysisTests
{
    [Theory]
    [InlineData(0.0, 1.0)]
    [InlineData(0.137, 0.287)]
    public void FindsTheToneInsideTheSelectedInterval(double start, double end)
    {
        using var wav = Wave(i => (short)(8192 * Math.Sin(2 * Math.PI * 440 * i / 8000)));
        var result = AudioAnalysis.Analyze(wav, start, end);

        Assert.NotNull(result.DominantFrequencyHz);
        Assert.InRange(result.DominantFrequencyHz.Value, 440 - result.BinSpacingHz, 440 + result.BinSpacingHz);
        Assert.Equal((int)Math.Round((end - start) * 8000), result.SampleCount);
        Assert.Equal(8000.0 / result.FftSize, result.BinSpacingHz);
    }

    [Fact]
    public void SilenceInTheSelectedIntervalHasNoPeak()
    {
        using var wav = Wave(i => i < 4000 ? (short)1000 : (short)0);
        var result = AudioAnalysis.Analyze(wav, 0.5, 1);
        Assert.Null(result.DominantFrequencyHz);
        Assert.All(result.Points, point => Assert.Equal(0, point.Magnitude));
    }

    [Fact]
    public void RejectsIntervalsOutsideTheAudio()
    {
        using var wav = Wave(_ => 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => AudioAnalysis.Analyze(wav, 0, 1.01));
    }

    [Fact]
    public void InspectReturnsBoundedWaveformThatPreservesExtrema()
    {
        using var wav = Wave(i => i % 2 == 0 ? short.MinValue : short.MaxValue);
        var file = new FormFile(wav, 0, wav.Length, "file", "peaks.wav");
        var response = Assert.IsType<OkObjectResult>(new AudioController().Inspect(file, CancellationToken.None));
        var json = System.Text.Json.JsonSerializer.SerializeToElement(response.Value);
        var blocks = json.GetProperty("Waveform").EnumerateArray().ToArray();
        Assert.Equal(1024, blocks.Length);
        Assert.All(blocks, block =>
        {
            Assert.Equal(-1, block.GetProperty("Min").GetDouble());
            Assert.Equal(32767 / 32768.0, block.GetProperty("Max").GetDouble());
        });
    }

    private static MemoryStream Wave(Func<int, short> sample)
    {
        const int rate = 8000;
        const int bytes = rate * 2;
        var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        writer.Write("RIFF"u8);
        writer.Write(36 + bytes);
        writer.Write("WAVEfmt "u8);
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(rate);
        writer.Write(rate * 2);
        writer.Write((short)2);
        writer.Write((short)16);
        writer.Write("data"u8);
        writer.Write(bytes);
        for (int i = 0; i < rate; i++)
            writer.Write(sample(i));
        stream.Position = 0;
        return stream;
    }
}
