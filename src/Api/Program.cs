using System.Reflection;
using Api.Extensions;
using Api;
using Application;
using Infrastructure;
using Scalar.AspNetCore;
using Serilog;
using Web.Api.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services
        .AddPresentation(builder.Configuration)
        .AddApplication()
        .AddInfrastructure(builder.Configuration);

    builder.Host.UseSerilog((context, services, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services));
    builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    app.UseRequestContextLogging();
    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();

    app.MapEndpoints();
    app.MapScalarApiReference();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unhandled exception occurred during startup");
}
finally
{
    Log.CloseAndFlush();
}

namespace Api // required for testing
{
    public partial class Program;
}