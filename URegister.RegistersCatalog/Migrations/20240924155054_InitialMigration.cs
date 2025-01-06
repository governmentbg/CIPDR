using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "master_person_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    master_person_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Идентификатор на основно лице"),
                    pid = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "Идентификатор на лице"),
                    pid_type = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false, comment: "Тип на идентификатора"),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false, comment: "Име"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали е активен"),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата на създаване"),
                    deleted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата на изтриване")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_master_person_records", x => x.id);
                },
                comment: "Глобална партида на лице");

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
                name: "registers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Код на регистър"),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "Име на регистър"),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "Описание"),
                    legal_basis = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, comment: "Правно основание"),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата на създаване"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали е активен"),
                    deleted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата на изтриване"),
                    master_person_records_index_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_registers", x => x.id);
                    table.ForeignKey(
                        name: "fk_registers_master_person_records_master_person_records_index",
                        column: x => x.master_person_records_index_id,
                        principalTable: "master_person_records",
                        principalColumn: "id");
                },
                comment: "Регистри");

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
                name: "administrations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    register_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на регистър"),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "ЕБК Код"),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "Име"),
                    legal_basis = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, comment: "Правно основание"),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата на създаване"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали е активен"),
                    deleted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата на изтриване")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_administrations", x => x.id);
                    table.ForeignKey(
                        name: "fk_administrations_registers_register_id",
                        column: x => x.register_id,
                        principalTable: "registers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Администрации");

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
                name: "ix_administrations_code",
                table: "administrations",
                column: "code");

            migrationBuilder.CreateIndex(
                name: "ix_administrations_register_id",
                table: "administrations",
                column: "register_id");

            migrationBuilder.CreateIndex(
                name: "ix_master_person_records_pid_pid_type",
                table: "master_person_records",
                columns: new[] { "pid", "pid_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_registers_code",
                table: "registers",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_registers_master_person_records_index_id",
                table: "registers",
                column: "master_person_records_index_id");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "administrations");

            migrationBuilder.DropTable(
                name: "scoped_param");

            migrationBuilder.DropTable(
                name: "registers");

            migrationBuilder.DropTable(
                name: "rules");

            migrationBuilder.DropTable(
                name: "master_person_records");

            migrationBuilder.DropTable(
                name: "workflows");
        }
    }
}
