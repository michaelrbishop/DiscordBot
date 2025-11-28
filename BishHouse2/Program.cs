using BishHouse2;
using BishHouse2.Repository;
using BishHouse2.Repository.Data;
using BishHouse2.Repository.Factory;
using BishHouse2.Services;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        var env = hostingContext.HostingEnvironment;

        if (!env.IsProduction())
        {
             config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false, reloadOnChange: true);
        }
        else
        {
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
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

await host.RunAsync();


// TODO : MRB
// Create UserDomain

// Create repository to write user data to a db
// Create hosted IMemoryCache for user data


// On startup grab all users of guild and check against db
// If they don't exist, add them to the db and then the in memory cache

// Create handle for message event to check if we know the user
// If we don't know the user, send them an efhemeral form to add their info

