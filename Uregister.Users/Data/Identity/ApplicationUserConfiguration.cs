using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Uregister.Users.Data.Identity
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("identity_users", t =>
            {
                t.HasComment("Потребители");
            })
                .HasKey(user => user.Id);

            // Indexes for "normalized" username and email, to allow efficient lookups
            builder.HasIndex(u => u.NormalizedUserName).HasDatabaseName("user_name_index").IsUnique();
            builder.HasIndex(u => u.NormalizedEmail).HasDatabaseName("email_index");

            // Each User can have many UserClaims
            builder.HasMany(e => e.Claims)
                .WithOne(e => e.User)
                .HasForeignKey(uc => uc.UserId)
                .IsRequired();

            // Each User can have many UserLogins
            builder.HasMany(e => e.Logins)
                .WithOne(e => e.User)
                .HasForeignKey(ul => ul.UserId)
                .IsRequired();

            // Each User can have many UserTokens
            builder.HasMany(e => e.Tokens)
                .WithOne(e => e.User)
                .HasForeignKey(ut => ut.UserId)
                .IsRequired();

            // Each User can have many entries in the UserRole join table
            builder.HasMany(e => e.UserRoles)
                .WithOne(e => e.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            builder.Property(p => p.Id)
                .HasColumnName("id")
                .HasComment("Идентификатор на запис");
            builder.Property(p => p.AccessFailedCount)
                .HasColumnName("access_failed_count")
                .HasComment("Брой грешни опити за вход");
            builder.Property(p => p.ConcurrencyStamp)
                .HasColumnName("concurrency_stamp")
                .IsConcurrencyToken()
                .HasComment("Защита от конкурентни заявки");
            builder.Property(p => p.Email)
                .HasColumnName("email")
                .HasMaxLength(256)
                .HasComment("Електронна поща");
            builder.Property(p => p.EmailConfirmed)
                .HasColumnName("email_confirmed")
                .HasComment("Дали електронната поща е потвърдена");
            builder.Property(p => p.LockoutEnabled)
                .HasColumnName("lockout_enabled")
                .HasComment("Дали е позволено заключване");
            builder.Property(p => p.LockoutEnd)
                .HasColumnName("lockout_end")
                .HasComment("Кога приключва заключването");
            builder.Property(p => p.NormalizedEmail)
                .HasColumnName("normalized_email")
                .HasMaxLength(256)
                .HasComment("Нормализирана електронна поща");
            builder.Property(p => p.NormalizedUserName)
                .HasColumnName("normalized_user_name")
                .HasMaxLength(256)
                .HasComment("Нормализирано потребителско име");
            builder.Property(p => p.PasswordHash)
                .HasColumnName("password_hash")
                .HasComment("Хеш на парола");
            builder.Property(p => p.PhoneNumber)
                .HasColumnName("phone_number")
                .HasComment("Телефонен номер");
            builder.Property(p => p.PhoneNumberConfirmed)
                .HasColumnName("phone_number_confirmed")
                .HasComment("Дали телефонния номер е потвърден");
            builder.Property(p => p.SecurityStamp)
                .HasColumnName("security_stamp")
                .HasComment("Допълнителен елемент за сигурност");
            builder.Property(p => p.TwoFactorEnabled)
                .HasColumnName("two_factor_enabled")
                .HasComment("Разрешен ли е втори фактор");
            builder.Property(p => p.UserName)
                .HasColumnName("user_name")
                .HasMaxLength(256)
                .HasComment("Потребителско име");
            builder.Property(p => p.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(100)
                .HasComment("Собствено име");
            builder.Property(p => p.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(100)
                .HasComment("Фамилно име");
            builder.Property(p => p.AdministrationId)
                .HasColumnName("administration_id")
                .HasComment("Идентификатор на администрация");
            builder.Property(p => p.Administration)
                .HasMaxLength(500)
                .HasColumnName("administration")
                .HasComment("Администрация");
        }
    }
}
