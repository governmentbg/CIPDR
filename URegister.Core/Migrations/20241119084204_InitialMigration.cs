using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "data_protection_keys",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    friendly_name = table.Column<string>(type: "text", nullable: true),
                    xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_protection_keys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "forms",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Индентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_id = table.Column<int>(type: "integer", nullable: true, comment: "Идентификатор на първата версия на формата"),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false, comment: "Заглавие на формата"),
                    purpose = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, comment: "Предназначение на формата"),
                    field_configuration = table.Column<string>(type: "jsonb", nullable: false, comment: "Конфигурация на полетата"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали записът е активен"),
                    deleted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата на изтриване"),
                    modified_by_user_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "Идентификатор на потребилет променил последно записа"),
                    modified_on = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "Дата на последна промяна")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forms", x => x.id);
                    table.ForeignKey(
                        name: "fk_forms_forms_parent_id",
                        column: x => x.parent_id,
                        principalTable: "forms",
                        principalColumn: "id");
                },
                comment: "Съдържа конфигурацияна на полетата във форма");

            migrationBuilder.CreateTable(
                name: "services",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Индентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false, comment: "Име на услугата")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_services", x => x.id);
                },
                comment: "Услуга в регистъра");

            migrationBuilder.CreateTable(
                name: "workflows",
                columns: table => new
                {
                    workflow_name = table.Column<string>(type: "text", nullable: false),
                    rule_expression_type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workflows", x => x.workflow_name);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<string>(type: "text", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_role_claims_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "AspNetRoles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_user_claims_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "fk_asp_net_user_logins_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    role_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "AspNetRoles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "fk_asp_net_user_tokens_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "steps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Индентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    form_parent_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на тип форма"),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false, comment: "Име на стъпката"),
                    service_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на услугата от регистъра")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_steps", x => x.id);
                    table.ForeignKey(
                        name: "fk_steps_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Стъпка от услуга в регистъра");

            migrationBuilder.CreateTable(
                name: "rules",
                columns: table => new
                {
                    rule_name = table.Column<string>(type: "text", nullable: false),
                    properties = table.Column<string>(type: "text", nullable: true),
                    @operator = table.Column<string>(name: "operator", type: "text", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    rule_expression_type = table.Column<int>(type: "integer", nullable: false),
                    expression = table.Column<string>(type: "text", nullable: true),
                    actions = table.Column<string>(type: "text", nullable: true),
                    success_event = table.Column<string>(type: "text", nullable: true),
                    rule_name_fk = table.Column<string>(type: "text", nullable: true),
                    workflow_name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rules", x => x.rule_name);
                    table.ForeignKey(
                        name: "fk_rules_rules_rule_name_fk",
                        column: x => x.rule_name_fk,
                        principalTable: "rules",
                        principalColumn: "rule_name");
                    table.ForeignKey(
                        name: "fk_rules_workflows_workflow_name",
                        column: x => x.workflow_name,
                        principalTable: "workflows",
                        principalColumn: "workflow_name");
                });

            migrationBuilder.CreateTable(
                name: "scoped_param",
                columns: table => new
                {
                    name = table.Column<string>(type: "text", nullable: false),
                    expression = table.Column<string>(type: "text", nullable: true),
                    rule_name = table.Column<string>(type: "text", nullable: true),
                    workflow_name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scoped_param", x => x.name);
                    table.ForeignKey(
                        name: "fk_scoped_param_rules_rule_name",
                        column: x => x.rule_name,
                        principalTable: "rules",
                        principalColumn: "rule_name");
                    table.ForeignKey(
                        name: "fk_scoped_param_workflows_workflow_name",
                        column: x => x.workflow_name,
                        principalTable: "workflows",
                        principalColumn: "workflow_name");
                });

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_role_claims_role_id",
                table: "AspNetRoleClaims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_claims_user_id",
                table: "AspNetUserClaims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_logins_user_id",
                table: "AspNetUserLogins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_roles_role_id",
                table: "AspNetUserRoles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_forms_parent_id",
                table: "forms",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_rules_rule_name_fk",
                table: "rules",
                column: "rule_name_fk");

            migrationBuilder.CreateIndex(
                name: "ix_rules_workflow_name",
                table: "rules",
                column: "workflow_name");

            migrationBuilder.CreateIndex(
                name: "ix_scoped_param_rule_name",
                table: "scoped_param",
                column: "rule_name");

            migrationBuilder.CreateIndex(
                name: "ix_scoped_param_workflow_name",
                table: "scoped_param",
                column: "workflow_name");

            migrationBuilder.CreateIndex(
                name: "ix_steps_service_id",
                table: "steps",
                column: "service_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "data_protection_keys");

            migrationBuilder.DropTable(
                name: "forms");

            migrationBuilder.DropTable(
                name: "scoped_param");

            migrationBuilder.DropTable(
                name: "steps");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "rules");

            migrationBuilder.DropTable(
                name: "services");

            migrationBuilder.DropTable(
                name: "workflows");
        }
    }
}
