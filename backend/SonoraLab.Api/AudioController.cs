using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using SonoraLab.Audio;

[ApiController]
[Route("api/audio")]
public sealed class AudioController : ControllerBase
{
    [HttpPost("inspect")]
    public IActionResult Inspect([FromForm] IFormFile? file, CancellationToken cancellation) =>
        WithWave(file, (stream, wav) => new
        {
            wav.Metadata.DurationSeconds,
            wav.Metadata.Channels,
            wav.Metadata.SampleRateHz,
            Waveform = AudioAnalysis.Waveform(stream, wav, cancellation)
        });

    [HttpPost("analyze")]
    public IActionResult Analyze([FromForm] IFormFile? file, [FromForm] string? startSeconds,
        [FromForm] string? endSeconds, CancellationToken cancellation)
    {
        if (!double.TryParse(startSeconds, NumberStyles.Float, CultureInfo.InvariantCulture, out double start) ||
            !double.TryParse(endSeconds, NumberStyles.Float, CultureInfo.InvariantCulture, out double end))
            return Problem(statusCode: 400, title: "Informe início e fim do trecho.");

        return WithWave(file, (stream, wav) =>
            AudioAnalysis.Analyze(stream, wav, start, end, cancellation));
    }

    private IActionResult WithWave(IFormFile? file, Func<Stream, WavPcm, object> process)
    {
        if (file is null || file.Length == 0)
            return Problem(statusCode: 400, title: "Envie um arquivo WAV.");

        if (file.Length > WavInspector.MaxBytes)
            return Problem(statusCode: 413, title: "O limite por arquivo é de 20 MiB.");

        using var stream = file.OpenReadStream();
        try
        {
            var wav = WavInspector.ReadPcm(stream);
            if (wav.Metadata.DurationSeconds > 120)
                return Problem(statusCode: 413, title: "O limite de duração é de 120 segundos.");

            return Ok(process(stream, wav));
        }
        catch (NotSupportedException)
        {
            return Problem(statusCode: 415, title: "Use WAV PCM de 16 bits, mono ou estéreo.");
        }
        catch (Exception exception) when (exception is InvalidDataException or EndOfStreamException)
        {
            return Problem(statusCode: 400, title: "O conteúdo do arquivo WAV é inválido.");
        }
        catch (ArgumentOutOfRangeException)
        {
            return Problem(statusCode: 400,
                title: "Selecione um trecho dentro do áudio, com 4 a 524.288 amostras por canal e até 5 segundos.");
        }
    }
}
