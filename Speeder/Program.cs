using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Prometheus;
using Speeder.Infra;
using Speeder.Infra.Impl;
using Speeder.Services;

const string serviceName = "speeder";
const int InternalSpeederVersion = 110;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddOpenTelemetry(o =>
{
    o.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName)).AddConsoleExporter();
    o.AddOtlpExporter(o => o.Endpoint = new Uri("http://localhost:4317"));
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName))    
    .WithLogging();

builder.Services.AddControllers();
builder.Services.AddMetricServer(o => {});

builder.Services.AddSingleton<ServerPool>();
builder.Services.AddSingleton<Iperf3Adapter>();

builder.Services.AddSingleton<OoklaAdapter>();

builder.Services.AddHostedService<SpeedTestService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var cfg = app.Configuration.GetRequiredSection("Iperf:Servers").Get<List<string>>();

if (cfg == null || cfg.Count < 1) throw new ApplicationException("at least one target server must be defined");
var pool = app.Services.GetRequiredService<ServerPool>();

foreach(var srv in cfg)
{
    var split = srv.Split('#');
    if (split.Length < 2) continue;
    pool.AddServer(split[0], split[1]);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMetricServer();
app.MapControllers().WithOpenApi();

// "fake" metrics
var version = Metrics.CreateGauge("speeder_version", "speeder service version");
version.Set(InternalSpeederVersion);
var up = Metrics.CreateGauge("speeder_up", "is speeder up");
up.Set(1);

app.Run();