using Prometheus;
using Speeder.Infra;
using Speeder.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMetricServer(o => {});
builder.Services.AddSingleton<ServerPool>();
builder.Services.AddHostedService<IperfService>();

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
app.Run();