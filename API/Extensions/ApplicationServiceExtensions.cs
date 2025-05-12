using System;
using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
    IConfiguration config)
    {
        services.AddControllers();
        services.AddDbContext<DataContext>(opt =>
        {
            opt.UseSqlite(config.GetConnectionString("DefaultConnection"));
        });

        services.AddCors();
        services.AddScoped<ITokenService, TokenService>();
        // repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILikesRepository, LikesRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        // PHOTO MANAGEMENT TASK
        services.AddScoped<IPhotoRepository, PhotoRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        // photos
        services.AddScoped<IPhotoService, PhotoService>();
        // user activity logging
        services.AddScoped<LogUserActivity>();
        // automapper general setup for app
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        // cloudinary connecting via settings class
        services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
        //signalR
        services.AddSignalR();
        services.AddSingleton<PresenceTracker>();

        return services;
    }
}
