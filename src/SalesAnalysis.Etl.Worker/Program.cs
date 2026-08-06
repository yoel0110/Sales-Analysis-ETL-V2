using SalesAnalysis.Etl.Worker;
using SalesAnalysis.Etl.Worker.Options;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.Configure<CsvSourceOptions>(builder.Configuration.GetSection("CsvSources"));

var host = builder.Build();
host.Run();
