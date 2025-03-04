﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Uregister.Users.Data;

#nullable disable

namespace Uregister.Users.Migrations
{
    [DbContext(typeof(UserDbContext))]
    partial class UserDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .HasComment("Идентификатор на запис");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("FriendlyName")
                        .HasColumnType("text")
                        .HasColumnName("friendly_name")
                        .HasComment("Име на ключа");

                    b.Property<string>("Xml")
                        .HasColumnType("text")
                        .HasColumnName("xml")
                        .HasComment("Ключ в XML формат");

                    b.HasKey("Id")
                        .HasName("pk_data_protection_keys");

                    b.ToTable("data_protection_keys", null, t =>
                        {
                            t.HasComment("Ключове за защита на данни");
                        });
                });

            modelBuilder.Entity("Uregister.Users.Data.Identity.ApplicationRole", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasComment("Идентификатор на запис");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text")
                        .HasColumnName("concurrency_stamp")
                        .HasComment("Защита от конкурентни заявки");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("label");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("name")
                        .HasComment("Име на роля");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("normalized_name")
                        .HasComment("Нормализирано име на роля");

                    b.HasKey("Id")
                        .HasName("pk_identity_roles");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("role_name_index");

                    b.ToTable("identity_roles", null, t =>
                        {
                            t.HasComment("Роли");
                        });
                });

            modelBuilder.Entity("Uregister.Users.Data.Identity.ApplicationRoleClaim", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .HasComment("Идентификатор на запис");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text")
                        .HasColumnName("claim_type")
                        .HasComment("Тип на клейм");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text")
                        .HasColumnName("claim_value")
                        .HasComment("Стойност на клейм");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid")
                        .HasColumnName("role_id")
                        .HasComment("Идентификатор на роля");

                    b.HasKey("Id")
                        .HasName("pk_identity_role_claims");

                    b.HasIndex("RoleId")
                        .HasDatabaseName("ix_identity_role_claims_role_id");

                    b.ToTable("identity_role_claims", null, t =>
                        {
                            t.HasComment("Клейма към роля");
                        });
                });

            modelBuilder.Entity("Uregister.Users.Data.Identity.ApplicationUser", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasComment("Идентификатор на запис");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer")
                        .HasColumnName("access_failed_count")
                        .HasComment("Брой грешни опити за вход");

                    b.Property<string>("Administration")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("administration")
                        .HasComment("Администрация");

                    b.Property<Guid>("AdministrationId")
                        .HasColumnType("uuid")
                        .HasColumnName("administration_id")
                        .HasComment("Идентификатор на администрация");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text")
                        .HasColumnName("concurrency_stamp")
                        .HasComment("Защита от конкурентни заявки");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("email")
                        .HasComment("Електронна поща");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean")
                        .HasColumnName("email_confirmed")
                        .HasComment("Дали електронната поща е потвърдена");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("first_name")
                        .HasComment("Собствено име");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("last_name")
                        .HasComment("Фамилно име");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean")
                        .HasColumnName("lockout_enabled")
                        .HasComment("Дали е позволено заключване");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("lockout_end")
                        .HasComment("Кога приключва заключването");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("normalized_email")
                        .HasComment("Нормализирана електронна поща");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("normalized_user_name")
                        .HasComment("Нормализирано потребителско име");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text")
                        .HasColumnName("password_hash")
                        .HasComment("Хеш на парола");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text")
                        .HasColumnName("phone_number")
                        .HasComment("Телефонен номер");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean")
                        .HasColumnName("phone_number_confirmed")
                        .HasComment("Дали телефонния номер е потвърден");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text")
                        .HasColumnName("security_stamp")
                        .HasComment("Допълнителен елемент за сигурност");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean")
                        .HasColumnName("two_factor_enabled")
                        .HasComment("Разрешен ли е втори фактор");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("user_name")
                        .HasComment("Потребителско име");

                    b.HasKey("Id")
                        .HasName("pk_identity_users");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("email_index");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("user_name_index");

                    b.ToTable("identity_users", null, t =>
                        {
                            t.HasComment("Потребители");
                        });
                });

            modelBuilder.Entity("Uregister.Users.Data.Identity.ApplicationUserClaim", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .HasComment("Идентификатор на запис");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text")
                        .HasColumnName("claim_type")
                        .HasComment("Тип на клейм");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text")
                        .HasColumnName("claim_value")
                        .HasComment("Стойност на клейм");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id")
                        .HasComment("Идентификатор на потребител");

                    b.HasKey("Id")
                        .HasName("pk_identity_user_claims");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_identity_user_claims_user_id");

                    b.ToTable("identity_user_claims", null, t =>
                        {
                            t.HasComment("Клейма на потребител");
                        });
                });

            modelBuilder.Entity("Uregister.Users.Data.Identity.ApplicationUserLogin", b =>
                {
                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("provider_key")
                        .HasComment("Ключ на доставчик");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("login_provider")
                        .HasComment("Доставчик на удостоверителни услуги");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text")
                        .HasColumnName("provider_display_name")
                        .HasComment("Етикет на доставчик на удостоверителни услуги");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id")
                        .HasComment("Идентификатор на потребител");

                    b.HasKey("ProviderKey", "LoginProvider")
                        .HasName("pk_identity_user_logins");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_identity_user_logins_user_id");

                    b.ToTable("identity_user_logins", null, t =>
                        {
                            t.HasComment("Външни доставчици на удостоверителни услуги");
                        });
                });

            modelBuilder.Entity("Uregister.Users.Data.Identity.ApplicationUserRole", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id")
                        .HasComment("Идентификатор на потребител");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid")
                        .HasColumnName("role_id")
                        .HasComment("Идентификатор на роля");

                    b.Property<string>("RegisterCode")
                        .HasColumnType("text")
                        .HasColumnName("register_code")
                        .HasComment("Код на регистър");

                    b.HasKey("UserId", "RoleId", "RegisterCode")
                        .HasName("pk_identity_user_roles");

                    b.HasIndex("RoleId")
                        .HasDatabaseName("ix_identity_user_roles_role_id");

                    b.ToTable("identity_user_roles", null, t =>
                        {
                            t.HasComment("Роли на потребител");
                        });
                });

            modelBuilder.Entity("Uregister.Users.Data.Identity.ApplicationUserToken", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id")
                        .HasComment("Идентификатор на потребител");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("login_provider")
                        .HasComment("Доставчик на удостоверителни услуги");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("name")
                        .HasComment("Име на токън");

                    b.Property<string>("Value")
                        .HasColumnType("text")
                        .HasColumnName("value")
                        .HasComment("Стойност на токън");

                    b.HasKey("UserId", "LoginProvider", "Name")
                        .HasName("pk_identity_user_tokens");

                    b.ToTable("identity_user_tokens", null, t =>
                        {
                            t.HasComment("Потребителски токъни");
                        });
                });

            modelBuilder.Entity("Uregister.Users.Data.Identity.ApplicationRoleClaim", b =>
                {
                    b.HasOne("Uregister.Users.Data.Identity.ApplicationRole", "Role")
                        .WithMany("RoleClaims")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_identity_role_claims_identity_roles_role_id");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("Uregister.Users.Data.Identity.ApplicationUserClaim", b =>
                {
                    b.HasOne("Uregister.Users.Data.Identity.ApplicationUser", "User")
                        .WithMany("Claims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_identity_user_claims_identity_users_user_id");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Uregister.Users.Data.Identity.ApplicationUserLogin", b =>
                {
                    b.HasOne("Uregister.Users.Data.Identity.ApplicationUser", "User")
                        .WithMany("Logins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_identity_user_logins_identity_users_user_id");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Uregister.Users.Data.Identity.ApplicationUserRole", b =>
                {
                    b.HasOne("Uregister.Users.Data.Identity.ApplicationRole", "Role")
                        .WithMany("UserRoles")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_identity_user_roles_identity_roles_role_id");

                    b.HasOne("Uregister.Users.Data.Identity.ApplicationUser", "User")
                        .WithMany("UserRoles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_identity_user_roles_identity_users_user_id");

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Uregister.Users.Data.Identity.ApplicationUserToken", b =>
                {
                    b.HasOne("Uregister.Users.Data.Identity.ApplicationUser", "User")
                        .WithMany("Tokens")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_identity_user_tokens_identity_users_user_id");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Uregister.Users.Data.Identity.ApplicationRole", b =>
                {
                    b.Navigation("RoleClaims");

                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("Uregister.Users.Data.Identity.ApplicationUser", b =>
                {
                    b.Navigation("Claims");

                    b.Navigation("Logins");

                    b.Navigation("Tokens");

                    b.Navigation("UserRoles");
                });
#pragma warning restore 612, 618
        }
    }
}
