using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SalesAnalysis.Etl.Worker;
using SalesAnalysis.Etl.Worker.Data;
using SalesAnalysis.Etl.Worker.Extractors;
using SalesAnalysis.Etl.Worker.Models;
using SalesAnalysis.Etl.Worker.Options;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.Configure<CsvSourceOptions>(builder.Configuration.GetSection("CsvSources"));
builder.Services.Configure<ApiSourceOptions>(builder.Configuration.GetSection("ApiSources"));

builder.Services.AddDbContext<WarehouseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("olap_ventas")));

builder.Services.AddScoped<ICustomerDimRepository, CustomerDimRepository>();
builder.Services.AddScoped<IProductDimRepository, ProductDimRepository>();
builder.Services.AddScoped<IDateDimRepository, DateDimRepository>();
builder.Services.AddScoped<IFactTableRepository, FactTableRepository>();

builder.Services.AddHttpClient("ApiClient", (sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<ApiSourceOptions>>().Value;
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
