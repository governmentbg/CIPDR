using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Uregister.Users.Data;
using Uregister.Users.Data.Identity;
using Uregister.Users.Services;

namespace Uregister.Users.Extensions
{
    /// <summary>
    /// Набор от методи за регистриране на услуги в IoC контейнера
    /// </summary>
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// Регистрира услугите на приложението в IoC контейнера
        /// </summary>
        /// <param name="services">Регистрирани услуги</param>
        /// <returns></returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUserManagerService, UserMangerService>();

            return services;
        }

        /// <summary>
        /// Регистрира контекстите на приложението в IoC контейнера
        /// </summary>
        /// <param name="services">Регистрирани услуги</param>
        /// <param name="Configuration">Настройки на приложението</param>
        public static void AddApplicationDbContext(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddDbContext<UserDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("UsersConnection"))
                .UseSnakeCaseNamingConvention());

            services.AddScoped<IUserRepository, UserRepository>();
        }

        /// <summary>
        /// Регистрира Identity provider в IoC контейнера
        /// </summary>
        /// <param name="services">Регистрирани услуги</param>
        public static void AddApplicationIdentity(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.SignIn.RequireConfirmedAccount = true;

            })
            .AddUserStore<ApplicationUserStore>()
            .AddRoleStore<ApplicationRoleStore>()
            .AddDefaultTokenProviders();
        }
    }
}
