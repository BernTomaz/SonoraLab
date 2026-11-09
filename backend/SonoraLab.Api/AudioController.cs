using Microsoft.AspNetCore.Mvc;
using SonoraLab.Audio;

[ApiController]
[Route("api/audio")]
public sealed class AudioController : ControllerBase
{
    [HttpPost("inspect")]
    public IActionResult Inspect([FromForm] IFormFile? file)
    {
        if (file is null || file.Length == 0)
            return Problem(statusCode: 400, title: "Envie um arquivo WAV.");

        if (file.Length > WavInspector.MaxBytes)
            return Problem(statusCode: 413, title: "O limite por arquivo é de 20 MiB.");

        using var stream = file.OpenReadStream();
        try
        {
            var metadata = WavInspector.Inspect(stream);
            if (metadata.DurationSeconds > 120)
                return Problem(statusCode: 413, title: "O limite de duração é de 120 segundos.");

            return Ok(metadata);
        }
        catch (NotSupportedException)
        {
            return Problem(statusCode: 415, title: "Use WAV PCM de 16 bits, mono ou estéreo.");
        }
        catch (InvalidDataException)
        {
            return Problem(statusCode: 400, title: "O conteúdo do arquivo WAV é inválido.");
        }
    }
}
