﻿using System.Runtime.InteropServices;
using Microsoft.OpenApi.Models;
using Realworlddotnet.Api.Features.Articles;
using Realworlddotnet.Api.Features.Profiles;
using Realworlddotnet.Api.Features.Tags;
using Realworlddotnet.Api.Features.Users;
using Realworlddotnet.Core.Repositories;

namespace Thor
{
    class Thor
    {
        [DllImport("thor_mvp.dll")]
        public static extern void start_rapl([MarshalAs(UnmanagedType.LPUTF8Str)] string lpString);

        [DllImport("thor_mvp.dll")]
        public static extern void stop_rapl([MarshalAs(UnmanagedType.LPUTF8Str)] string lpString);

    }
}

class Program
{
    private static int Main(string[] args)
    {
        // IF YOU GET AN ERROR HERE, MAKE SURE YOU'RE RUNNING AS ADMINISTRATOR (and maybe driver is running too)
        Thor.Thor.start_rapl("Program.Start");
        var builder = WebApplication.CreateBuilder(args);

        // add logging
        builder.Host.UseSerilog((hostBuilderContext, services, loggerConfiguration) =>
        {
            loggerConfiguration.ConfigureBaseLogging("realworldDotnet");
            loggerConfiguration.AddApplicationInsightsLogging(services, hostBuilderContext.Configuration);
        });

        // setup database connection (used for in memory SQLite).
        // SQLite in memory requires an open connection during the application lifetime
#pragma warning disable S125
        // to use a file based SQLite use: "Filename=../realworld.db";
#pragma warning restore S125
        const string connectionString = "Filename=:memory:";
        var connection = new SqliteConnection(connectionString);
        connection.Open();


        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SupportNonNullableReferenceTypes();
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "realworlddotnet", Version = "v1" });
        });

        builder.Services.AddScoped<IConduitRepository, ConduitRepository>();
        builder.Services.AddScoped<IUserHandler, UserHandler>();
        builder.Services.AddScoped<IArticlesHandler, ArticlesHandler>();
        builder.Services.AddScoped<ITagsHandler, TagsHandler>();
        builder.Services.AddScoped<IProfilesHandler, ProfilesHandler>();
        builder.Services.AddSingleton<CertificateProvider>();

        builder.Services.AddSingleton<ITokenGenerator>(container =>
        {
            var logger = container.GetRequiredService<ILogger<CertificateProvider>>();
            var certificateProvider = new CertificateProvider(logger);
            var cert = certificateProvider.LoadFromFile("identityserver_testing.pfx", "password");

            return new TokenGenerator(cert);
        });

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        builder.Services.AddAuthorization();
        builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<ILogger<CertificateProvider>>((o, logger) =>
            {
                var certificateProvider = new CertificateProvider(logger);
                var cert = certificateProvider.LoadFromFile("identityserver_testing.pfx", "password");

                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    IssuerSigningKey = new RsaSecurityKey(cert.GetRSAPublicKey())
                };
                o.Events = new JwtBearerEvents { OnMessageReceived = CustomOnMessageReceivedHandler.OnMessageReceived };
            });

        // for SQLite in memory a connection is provided rather than a connection string
        builder.Services.AddDbContext<ConduitContext>(options => { options.UseSqlite(connection); });

        builder.Services.AddProblemDetails((Hellang.Middleware.ProblemDetails.ProblemDetailsOptions options) => { });
        builder.Services.ConfigureOptions<ProblemDetailsLogging>();
        builder.Services.AddCarter();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        Log.Information("Start configuring http request pipeline");

        // when using in memory SQLite ensure the tables are created
        using (var scope = app.Services.CreateScope())
        {
            using var context = scope.ServiceProvider.GetService<ConduitContext>();
            context?.Database.EnsureCreated();
        }

        app.UseSerilogRequestLogging(options =>
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                diagnosticContext.Set("UserId", httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "")
        );



        app.UseProblemDetails();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "realworlddotnet v1"));
        app.MapCarter();


        try
        {
            Log.Information("Starting web host");
            app.Run();
            Thor.Thor.stop_rapl("Program.Start");
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            Thor.Thor.stop_rapl("Program.Start");
            return 1;
        }
        finally
        {
            connection.Close();
            Log.CloseAndFlush();
            Thread.Sleep(2000);
        }
    }
}
