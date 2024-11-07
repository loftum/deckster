using System.Dynamic;
using System.Reflection;
using System.Text.Json.Serialization;
using Deckster.Client.Logging;
using Deckster.Core;
using Deckster.Core.Extensions;
using Deckster.Core.Protocol;
using Deckster.Core.Serialization;
using Deckster.Games.CodeGeneration.Meta;
using Deckster.Server.Authentication;
using Deckster.Server.Configuration;
using Deckster.Server.Data;
using Deckster.Server.Games;
using Deckster.Server.Games.CrazyEights;
using Deckster.Server.Middleware;
using Deckster.Server.Swagger;
using Marten;
using Marten.Events.Projections;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Weasel.Core;

namespace Deckster.Server;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services, DecksterConfig config)
    {
        var logger = Log.Factory.CreateLogger<Program>();
        services.AddSingleton(config);
        services.AddLogging(b => b.AddConsole());
        services.AddWebSockets(o =>
        {
            o.KeepAliveInterval = TimeSpan.FromSeconds(10);
        });
        
        services.AddControllers();

        logger.LogInformation("Using {type} repo", config.Repo.Type);
        switch (config.Repo.Type)
        {
            case RepoType.InMemory:
                services.AddSingleton<IRepo, InMemoryRepo>();
                break;
            case RepoType.Marten:
                
                services.AddMarten(o =>
                {
                    o.Projections.Add<CrazyEightsProjection>(ProjectionLifecycle.Inline);
                    o.Connection(config.Repo.Marten.ConnectionString);
                    o.UseSystemTextJsonForSerialization(DecksterJson.Options, EnumStorage.AsString, Casing.CamelCase);
                    o.AutoCreateSchemaObjects = AutoCreate.All;
                });
                services.AddSingleton<IRepo, MartenRepo>();
                break;
        }

        services.AddDataProtection(o =>
        {
        });

        services.AddDeckster();

        var mvc = services.AddMvc().AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
        mvc.AddRazorRuntimeCompilation();
        
        services.AddAuthentication(o =>
            {
                o.DefaultScheme = AuthenticationSchemes.Cookie;
            })
            .AddCookie(AuthenticationSchemes.Cookie, o =>
            {
                o.LoginPath = "/login";
                o.LogoutPath = "/logout";
                o.Cookie.Name = "deckster";
                o.Cookie.HttpOnly = true;
                o.Cookie.SameSite = SameSiteMode.Lax;
                o.Cookie.IsEssential = true;
                o.Cookie.MaxAge = TimeSpan.FromDays(180);
                o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                o.SlidingExpiration = true;
                o.ExpireTimeSpan = TimeSpan.FromDays(180);
            });
        services.AddRouting(o =>
        {
            o.LowercaseUrls = true;
            o.LowercaseQueryStrings = true;
        });

        services.AddEndpointsApiExplorer();
        
        services.AddTransient<ISchemaGenerator, DecksterSchemaGeneratorForDealingWithSwaggerImbecility>();
        services.AddSwaggerGen(o =>
        {
            o.DescribeAllParametersInCamelCase();
            o.UseAllOfForInheritance();
            o.SchemaGeneratorOptions.SupportNonNullableReferenceTypes = true;
            o.SchemaGeneratorOptions.NonNullableReferenceTypesAsRequired = true;
            o.SchemaGeneratorOptions.DiscriminatorNameSelector = t => t.GetProperty("Type")?.Name.ToCamelCase();
            o.SchemaGeneratorOptions.DiscriminatorValueSelector = t => t.GetGameNamespacedName();
            o.SchemaGeneratorOptions.SchemaIdSelector = t => t.GetGameNamespacedName();
                //t => t.InheritsFrom<DecksterMessage>() ? t.GetGameNamespacedName() : t.Name;
        });
    }
    
    public static void Configure(IApplicationBuilder app)
    {
        app.UseStaticFiles();
        app.UseSwagger();
        app.UseSwaggerUI(o =>
        {
            o.DocumentTitle = "Deckster";
            o.RoutePrefix = "swagger";
        });
        
        app.MapExtensionToAcceptHeader();
        app.UseAuthentication();
        app.LoadUser();
        app.UseWebSockets();
        app.UseRouting();
        
        app.UseEndpoints(e =>
        {
            e.MapControllers();
            e.MapSwagger();
        });

    }
}
