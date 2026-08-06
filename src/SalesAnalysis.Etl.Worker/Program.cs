using SalesAnalysis.Etl.Worker;
using SalesAnalysis.Etl.Worker.Extractors;
using SalesAnalysis.Etl.Worker.Models;
using SalesAnalysis.Etl.Worker.Options;
using SalesAnalysis.Etl.Worker.Staging;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.Configure<CsvSourceOptions>(builder.Configuration.GetSection("CsvSources"));
builder.Services.Configure<ApiSourceOptions>(builder.Configuration.GetSection("ApiSources"));
builder.Services.Configure<StagingOptions>(builder.Configuration.GetSection("Staging"));

builder.Services.AddHttpClient<ApiCustomerExtractor>();
builder.Services.AddHttpClient<ApiProductExtractor>();

builder.Services.AddSingleton<IStagingWriter, JsonStagingWriter>();

builder.Services.AddSingleton<IExtractor<CsvCustomerRecord>, CsvCustomerExtractor>();
builder.Services.AddSingleton<IExtractor<CsvProductRecord>, CsvProductExtractor>();
builder.Services.AddSingleton<IExtractor<CsvOrderRecord>, CsvOrderExtractor>();
builder.Services.AddSingleton<IExtractor<CsvOrderDetailRecord>, CsvOrderDetailExtractor>();
builder.Services.AddSingleton<IExtractor<ApiCustomerRecord>, ApiCustomerExtractor>();
builder.Services.AddSingleton<IExtractor<ApiProductRecord>, ApiProductExtractor>();

var host = builder.Build();
host.Run();
