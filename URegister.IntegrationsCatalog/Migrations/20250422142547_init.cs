using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.IntegrationsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "e_delivery_file_metadata",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    file_source_type_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на роля на файла"),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Име на файла"),
                    process_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Идентификатор на заявена услуга, по-която е качен файла"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на администрация"),
                    register_id = table.Column<int>(type: "integer", nullable: true, comment: "Идентификатор на регистър")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_e_delivery_file_metadata", x => x.id);
                },
                comment: "Инфорация за качен от потребител файл");

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
                name: "e_delivery_file_metadata");

            migrationBuilder.DropTable(
                name: "scoped_param");

            migrationBuilder.DropTable(
                name: "rules");

            migrationBuilder.DropTable(
                name: "workflows");
        }
    }
}
