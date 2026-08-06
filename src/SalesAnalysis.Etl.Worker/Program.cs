using SalesAnalysis.Etl.Worker;
using SalesAnalysis.Etl.Worker.Extractors;
using SalesAnalysis.Etl.Worker.Models;
using SalesAnalysis.Etl.Worker.Options;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.Configure<CsvSourceOptions>(builder.Configuration.GetSection("CsvSources"));
builder.Services.Configure<ApiSourceOptions>(builder.Configuration.GetSection("ApiSources"));

builder.Services.AddHttpClient<ApiCustomerExtractor>();
builder.Services.AddHttpClient<ApiProductExtractor>();

builder.Services.AddScoped<IExtractor<CsvCustomerRecord>, CsvCustomerExtractor>();
builder.Services.AddScoped<IExtractor<CsvProductRecord>, CsvProductExtractor>();
builder.Services.AddScoped<IExtractor<CsvOrderRecord>, CsvOrderExtractor>();
builder.Services.AddScoped<IExtractor<CsvOrderDetailRecord>, CsvOrderDetailExtractor>();
builder.Services.AddScoped<IExtractor<ApiCustomerRecord>, ApiCustomerExtractor>();
builder.Services.AddScoped<IExtractor<ApiProductRecord>, ApiProductExtractor>();

var host = builder.Build();
host.Run();
