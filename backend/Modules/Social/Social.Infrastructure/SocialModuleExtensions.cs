using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;
using Social.Application;
using Social.Application.BlogAuthoring;
using Social.Application.BlogCommenting;
using Social.Application.BlogReading;
using Social.Application.Blogs;
using Social.Application.ClubAuthoring;
using Social.Application.ClubReading;
using Social.Application.Clubs;
using Social.Application.Connections;
using Social.Application.FollowerManagement;
using Social.Application.Following;
using Social.Domain.Clubs;
using Social.Infrastructure.Persistence;
using Social.Infrastructure.Persistence.Blogs;
using Social.Infrastructure.Persistence.Clubs;
using Social.Infrastructure.Persistence.Connections;

namespace Social.Infrastructure;

public static class SocialModuleExtensions
{
    public static IServiceCollection AddSocialModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<SocialDbContext>(configuration, "social");
        services.AddHostedService<SocialModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<SocialMapperProfile>());

        services.AddScoped<IBlogRepository, BlogRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<SocialDbContext>());
        services.AddScoped<IBlogReadRepository, BlogReadRepository>();
        services.AddScoped<IConnectionRepository, ConnectionRepository>();
        services.AddScoped<IConnectionReadRepository, ConnectionReadRepository>();
        services.AddScoped<IClubRepository, ClubRepository>();
        services.AddScoped<IClubReadRepository, ClubReadRepository>();
        services.AddScoped<ClubNameUniqueness>();

        services.AddScoped<BlogAuthoringService>();
        services.AddScoped<BlogAuthoringQueries>();
        services.AddScoped<BlogReadingQueries>();
        services.AddScoped<BlogCommentingService>();
        services.AddScoped<FollowingService>();
        services.AddScoped<FollowingQueries>();
        services.AddScoped<FollowerManagementService>();
        services.AddScoped<FollowerManagementQueries>();
        services.AddScoped<ClubAuthoringService>();
        services.AddScoped<ClubReadingQueries>();

        return services;
    }
}
