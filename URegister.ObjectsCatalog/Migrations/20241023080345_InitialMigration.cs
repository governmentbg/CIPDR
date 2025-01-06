using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URegister.ObjectsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "field_type",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Име на поле"),
                    label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Етикет на поле")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_field_type", x => x.id);
                },
                comment: "Типове полета");

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
                name: "field",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Име на поле"),
                    label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Етикет на поле"),
                    field_type_id = table.Column<int>(type: "integer", nullable: false, comment: "Тип на поле"),
                    version = table.Column<int>(type: "integer", nullable: false, comment: "Версия на полето"),
                    is_current = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали полето е последна версия"),
                    field_data = table.Column<string>(type: "jsonb", nullable: false, comment: "Информация за полето. Настройки по подразбиране")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_field", x => x.id);
                    table.ForeignKey(
                        name: "fk_field_field_type_field_type_id",
                        column: x => x.field_type_id,
                        principalTable: "field_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Полета");

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
                name: "ix_field_field_type_id",
                table: "field",
                column: "field_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_field_name_is_current",
                table: "field",
                columns: new[] { "name", "is_current" });

            migrationBuilder.CreateIndex(
                name: "ix_field_name_version",
                table: "field",
                columns: new[] { "name", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_field_type_name",
                table: "field_type",
                column: "name",
                unique: true);

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
                name: "field");

            migrationBuilder.DropTable(
                name: "scoped_param");

            migrationBuilder.DropTable(
                name: "field_type");

            migrationBuilder.DropTable(
                name: "rules");

            migrationBuilder.DropTable(
                name: "workflows");
        }
    }
}
