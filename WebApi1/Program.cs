using System.Reflection;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ConfigureLogging(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

public static class LoggerExtension
{
    public static void ConfigureLogging(this ILoggingBuilder loggingBuilder, IConfiguration configuration)
    {
        loggingBuilder.ClearProviders();
        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .WriteTo.Debug()
            .WriteTo.Console()
            .WriteTo.Elasticsearch(ConfigureElasticsearch(configuration))
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
        loggingBuilder.AddSerilog(logger);
    }

    private static ElasticsearchSinkOptions ConfigureElasticsearch(IConfiguration configuration)
    {
        var uri = configuration["ElasticConfiguration:Uri"];
        return new ElasticsearchSinkOptions(new Uri(uri!))
        {
            AutoRegisterTemplate = true,
            IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name!.ToLower()}",
            NumberOfReplicas = 1,
            NumberOfShards = 2
        };
    }
}
