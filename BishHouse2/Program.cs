using BishHouse2;
using BishHouse2.Repository;
using BishHouse2.Repository.Data;
using BishHouse2.Repository.Factory;
using BishHouse2.Services;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

using IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, services, configuration) =>
    {
        if (!context.HostingEnvironment.IsProduction())
        {
            configuration
                .ReadFrom.Configuration(context.Configuration);
        } 
        else
        {
            configuration
                .MinimumLevel.Information()
                .WriteTo.Console();
        }


    })
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        var env = hostingContext.HostingEnvironment;        

        if (!env.IsProduction())
        {
             config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false, reloadOnChange: true);
        }
        else
        {
            config.AddEnvironmentVariables();
        }
         
    })
    .ConfigureServices((context, services) =>
    {
        services.AddDbContextFactory<DiscordDBContext>(opt =>
                opt.UseSqlServer(context.Configuration.GetConnectionString("DiscordDB")));

        services.ConfigureDiscordSocketClient();
        services.AddSingleton(x =>
        {
            var discord = x.GetRequiredService<DiscordSocketClient>();

            // TODO : MRB Add custom InteractionServiceConfig if needed
            return new InteractionService(discord);
        });

        services.AddHostedService<InteractionHandlingService>();
        services.AddHostedService<DiscordStartupService>();
        services.AddHostedService<VoiceChannelMonitorService>();
        services.AddHostedService<UserMonitorService>();
        services.AddHostedService<ComponentInteractionHandlingService>();
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddMemoryCache();
        services.AddTransient<IRepositoryFactory, RepositoryFactory>();
        services.AddHttpClient<IDadJokeService, DadJokeService>();

    })
    .Build();

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Error(ex, "An error occurred while running the host");
}
finally
{
       Log.CloseAndFlush();
}