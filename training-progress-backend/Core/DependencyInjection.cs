using Core.Persistence;
using Core.Shared.Events;
using Core.Trainings.Application;
using Core.Trainings.Domain;
using Core.Trainings.Domain.Events;
using Core.Trainings.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<IEventBus, InMemoryEventBus>();

        services.AddScoped<ITrainingRepository, TrainingRepository>();
        services.AddScoped<IEventHandler<TrainingCreatedEvent>, TrainingStatisticsHandler>();
        services.AddScoped<IEventHandler<TrainingUpdatedEvent>, TrainingStatisticsHandler>();

        return services;
    }
}
