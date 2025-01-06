using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class ServiceStep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "steps");

            migrationBuilder.AddColumn<int>(
                name: "service_type_id",
                table: "services",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Идентификатор на тип услуга");

            migrationBuilder.CreateTable(
                name: "service_steps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Индентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    form_parent_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на тип форма"),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false, comment: "Име на стъпката"),
                    service_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на услугата от регистъра"),
                    order_num = table.Column<int>(type: "integer", nullable: false, comment: "Поредност"),
                    step_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на стъпка")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_steps", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_steps_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Стъпка от услуга в регистъра");

            migrationBuilder.CreateIndex(
                name: "ix_service_steps_service_id",
                table: "service_steps",
                column: "service_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "service_steps");

            migrationBuilder.DropColumn(
                name: "service_type_id",
                table: "services");

            migrationBuilder.CreateTable(
                name: "steps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Индентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    service_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на услугата от регистъра"),
                    form_parent_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на тип форма"),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false, comment: "Име на стъпката")
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

            migrationBuilder.CreateIndex(
                name: "ix_steps_service_id",
                table: "steps",
                column: "service_id");
        }
    }
}
