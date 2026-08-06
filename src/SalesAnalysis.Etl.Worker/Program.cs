using SalesAnalysis.Etl.Worker;
using SalesAnalysis.Etl.Worker.Extractors;
using SalesAnalysis.Etl.Worker.Models;
using SalesAnalysis.Etl.Worker.Options;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.Configure<CsvSourceOptions>(builder.Configuration.GetSection("CsvSources"));
builder.Services.Configure<ApiSourceOptions>(builder.Configuration.GetSection("ApiSources"));

builder.Services.AddHttpClient("ApiClient", (sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ApiSourceOptions>>().Value;
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

builder.Services.AddSingleton<IExtractor<CsvCustomerRecord>, CsvCustomerExtractor>();
builder.Services.AddSingleton<IExtractor<CsvProductRecord>, CsvProductExtractor>();
builder.Services.AddSingleton<IExtractor<CsvOrderRecord>, CsvOrderExtractor>();
builder.Services.AddSingleton<IExtractor<CsvOrderDetailRecord>, CsvOrderDetailExtractor>();
builder.Services.AddSingleton<IExtractor<ApiCustomerRecord>, ApiCustomerExtractor>();
builder.Services.AddSingleton<IExtractor<ApiProductRecord>, ApiProductExtractor>();

var host = builder.Build();
host.Run();
