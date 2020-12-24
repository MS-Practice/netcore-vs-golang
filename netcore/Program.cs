using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;
using NLog.Web;
using Serilog;
using Serilog.Events;
using ILogger = Microsoft.Extensions.Logging.ILogger;
//using Dapper;

class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args)
            //.UseNLog()
            .Build()
            .Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args)
    {
        var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .Build();

        var host = new WebHostBuilder()
            .UseKestrel()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseConfiguration(config)
            .UseUrls("http://*:5002")
            .UseStartup<Startup>()
            //.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
            //        .ReadFrom.Configuration(hostingContext.Configuration)
            //        .Enrich.FromLogContext()
            //        .WriteTo.File("logs/.log", outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"))
            //.ConfigureLogging(logging => {
            //    logging.ClearProviders();
            //    logging.SetMinimumLevel(LogLevel.Information);
            //})
            ;

        return host;
    }
}

class Response
{
    public string Id { get; set; }
    public string Name { get; set; }
    public long Time { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public DateTimeOffset Birthday { get; set; }
}

class Startup
{
    private static readonly HttpMessageHandler _httpHandler = new HttpClientHandler
    {
        MaxConnectionsPerServer = 4000
    };

    private static readonly HttpClient _http = new HttpClient(_httpHandler)
    {
        BaseAddress = new Uri($"http://{Environment.GetEnvironmentVariable("HOST")}:5002")
    };

    private static void HandleTest(IApplicationBuilder app)
    {
        app.Run(async ctx =>
        {
            using (var rsp = await _http.GetAsync("/data"))
            {
                var str = await rsp.Content.ReadAsStringAsync();

                // deserialize
                var obj = JsonSerializer.Deserialize<Response>(str);

                // serialize
                var json = JsonSerializer.Serialize<Response>(obj);

                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsync(json);
            }
        });
    }

    public void Configure(IApplicationBuilder app)
    {
        //app.UseSerilogRequestLogging();
        //var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
        app.Map("/test", HandleTest);
        app.Map("/user", UserHandlTest);

        app.Run(async ctx =>
        {
            //var logger = ctx.RequestServices.GetService<Serilog.ILogger>();
            //logger.Information($"Hello, {ctx.Request.Path}");
            await ctx.Response.WriteAsync($"Hello, {ctx.Request.Path}");
        });
    }

    private void UserHandlTest(IApplicationBuilder app)
    {
        app.Run(async ctx =>
        {
            using (var connection = new MySqlConnector.MySqlConnection("server=192.168.3.125;database=go_testdb;uid=root;pwd=123456;charset='utf8';SslMode=None"))
            {
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = "select id,name,age,birthday from users where id = 1";
                command.Connection = connection;
                command.CommandTimeout = 1;
                var reader = await command.ExecuteReaderAsync();
                //_ = await connection.QueryFirstAsync<User>("select id,name,age,birthday from users");
            }
        });
    }
}
