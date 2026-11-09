using SonoraLab.Audio;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.WebHost.ConfigureKestrel(options =>
    options.Limits.MaxRequestBodySize = WavInspector.MaxBytes + 65536);

var app = builder.Build();
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();
app.Run();
