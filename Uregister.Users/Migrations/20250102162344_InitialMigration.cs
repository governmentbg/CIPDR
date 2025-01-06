using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Uregister.Users.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "data_protection_keys",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на запис")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    friendly_name = table.Column<string>(type: "text", nullable: true, comment: "Име на ключа"),
                    xml = table.Column<string>(type: "text", nullable: true, comment: "Ключ в XML формат")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_protection_keys", x => x.id);
                },
                comment: "Ключове за защита на данни");

            migrationBuilder.CreateTable(
                name: "identity_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на запис"),
                    label = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "Име на роля"),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "Нормализирано име на роля"),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true, comment: "Защита от конкурентни заявки")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_roles", x => x.id);
                },
                comment: "Роли");

            migrationBuilder.CreateTable(
                name: "identity_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на запис"),
                    administration_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на администрация"),
                    administration = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "Администрация"),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Собствено име"),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Фамилно име"),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "Потребителско име"),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "Нормализирано потребителско име"),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "Електронна поща"),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "Нормализирана електронна поща"),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали електронната поща е потвърдена"),
                    password_hash = table.Column<string>(type: "text", nullable: true, comment: "Хеш на парола"),
                    security_stamp = table.Column<string>(type: "text", nullable: true, comment: "Допълнителен елемент за сигурност"),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true, comment: "Защита от конкурентни заявки"),
                    phone_number = table.Column<string>(type: "text", nullable: true, comment: "Телефонен номер"),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали телефонния номер е потвърден"),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false, comment: "Разрешен ли е втори фактор"),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Кога приключва заключването"),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали е позволено заключване"),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false, comment: "Брой грешни опити за вход")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_users", x => x.id);
                },
                comment: "Потребители");

            migrationBuilder.CreateTable(
                name: "identity_role_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на запис")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на роля"),
                    claim_type = table.Column<string>(type: "text", nullable: true, comment: "Тип на клейм"),
                    claim_value = table.Column<string>(type: "text", nullable: true, comment: "Стойност на клейм")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_identity_role_claims_identity_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "identity_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Клейма към роля");

            migrationBuilder.CreateTable(
                name: "identity_user_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на запис")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на потребител"),
                    claim_type = table.Column<string>(type: "text", nullable: true, comment: "Тип на клейм"),
                    claim_value = table.Column<string>(type: "text", nullable: true, comment: "Стойност на клейм")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_identity_user_claims_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Клейма на потребител");

            migrationBuilder.CreateTable(
                name: "identity_user_logins",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Доставчик на удостоверителни услуги"),
                    provider_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Ключ на доставчик"),
                    provider_display_name = table.Column<string>(type: "text", nullable: true, comment: "Етикет на доставчик на удостоверителни услуги"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на потребител")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_logins", x => new { x.provider_key, x.login_provider });
                    table.ForeignKey(
                        name: "fk_identity_user_logins_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Външни доставчици на удостоверителни услуги");

            migrationBuilder.CreateTable(
                name: "identity_user_roles",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на потребител"),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на роля"),
                    register_code = table.Column<string>(type: "text", nullable: false, comment: "Код на регистър")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_identity_user_roles_identity_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "identity_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_identity_user_roles_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Роли на потребител");

            migrationBuilder.CreateTable(
                name: "identity_user_tokens",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на потребител"),
                    login_provider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Доставчик на удостоверителни услуги"),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Име на токън"),
                    value = table.Column<string>(type: "text", nullable: true, comment: "Стойност на токън")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "fk_identity_user_tokens_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Потребителски токъни");

            migrationBuilder.CreateIndex(
                name: "ix_identity_role_claims_role_id",
                table: "identity_role_claims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "role_name_index",
                table: "identity_roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_identity_user_claims_user_id",
                table: "identity_user_claims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_user_logins_user_id",
                table: "identity_user_logins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_user_roles_role_id",
                table: "identity_user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "email_index",
                table: "identity_users",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "user_name_index",
                table: "identity_users",
                column: "normalized_user_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_protection_keys");

            migrationBuilder.DropTable(
                name: "identity_role_claims");

            migrationBuilder.DropTable(
                name: "identity_user_claims");

            migrationBuilder.DropTable(
                name: "identity_user_logins");

            migrationBuilder.DropTable(
                name: "identity_user_roles");

            migrationBuilder.DropTable(
                name: "identity_user_tokens");

            migrationBuilder.DropTable(
                name: "identity_roles");

            migrationBuilder.DropTable(
                name: "identity_users");
        }
    }
}
