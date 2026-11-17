using Games.Application;
using Games.Application.ChallengeOverview;
using Games.Application.HiddenPointAuthoring;
using Games.Application.ImageViewing;
using Games.Application.LocationChallengeAuthoring;
using Games.Application.LocationChallenges;
using Games.Application.QuizAuthoring;
using Games.Application.QuizLifecycle;
using Games.Application.Quizzes;
using Games.Application.SocialChallengeAuthoring;
using Games.Application.SocialChallengeLifecycle;
using Games.Application.SocialChallenges;
using Games.Application.UploadedImages;
using Games.Infrastructure.Persistence;
using Games.Infrastructure.Persistence.LocationChallenges;
using Games.Infrastructure.Persistence.Quizzes;
using Games.Infrastructure.Persistence.SocialChallenges;
using Games.Infrastructure.Persistence.UploadedImages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;

namespace Games.Infrastructure;

public static class GamesModuleExtensions
{
    public static IServiceCollection AddGamesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<GamesDbContext>(configuration, "games");
        services.AddHostedService<GamesModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<GamesMapperProfile>());

        services.AddScoped<ISocialChallengeRepository, SocialChallengeRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<GamesDbContext>());
        services.AddScoped<ISocialChallengeReadRepository, SocialChallengeReadRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IQuizReadRepository, QuizReadRepository>();
        services.AddScoped<ILocationChallengeRepository, LocationChallengeRepository>();
        services.AddScoped<ILocationChallengeReadRepository, LocationChallengeReadRepository>();
        services.AddScoped<IUploadedImageRepository, UploadedImageRepository>();
        services.AddScoped<IUploadedImageReadRepository, UploadedImageReadRepository>();

        services.AddScoped<SocialChallengeAuthoringService>();
        services.AddScoped<SocialChallengeAuthoringQueries>();
        services.AddScoped<SocialChallengeLifecycleService>();
        services.AddScoped<QuizAuthoringService>();
        services.AddScoped<QuizAuthoringQueries>();
        services.AddScoped<QuizLifecycleService>();
        services.AddScoped<ChallengeOverviewQueries>();
        services.AddScoped<LocationChallengeAuthoringService>();
        services.AddScoped<LocationChallengeAuthoringQueries>();
        services.AddScoped<HiddenPointAuthoringService>();
        services.AddScoped<ImageViewingQueries>();

        return services;
    }
}
