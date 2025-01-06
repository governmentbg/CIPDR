using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URegister.ObjectsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class ServiceTypeAndStepsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_field_field_type_field_type_id",
                table: "field");

            migrationBuilder.DropPrimaryKey(
                name: "pk_field_type",
                table: "field_type");

            migrationBuilder.DropPrimaryKey(
                name: "pk_field",
                table: "field");

            migrationBuilder.RenameTable(
                name: "field_type",
                newName: "field_types");

            migrationBuilder.RenameTable(
                name: "field",
                newName: "fields");

            migrationBuilder.RenameIndex(
                name: "ix_field_type_name",
                table: "field_types",
                newName: "ix_field_types_name");

            migrationBuilder.RenameIndex(
                name: "ix_field_name_version",
                table: "fields",
                newName: "ix_fields_name_version");

            migrationBuilder.RenameIndex(
                name: "ix_field_name_is_current",
                table: "fields",
                newName: "ix_fields_name_is_current");

            migrationBuilder.RenameIndex(
                name: "ix_field_field_type_id",
                table: "fields",
                newName: "ix_fields_field_type_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_field_types",
                table: "field_types",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_fields",
                table: "fields",
                column: "id");

            migrationBuilder.CreateTable(
                name: "service_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на тип на услуга")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Име на тип на услуга")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_types", x => x.id);
                },
                comment: "Тип на услуга");

            migrationBuilder.CreateTable(
                name: "steps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на стъпка")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Име на стъпка"),
                    type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Тип на обработчик на стъпка"),
                    method = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Метод на обработчик на стъпка"),
                    configuration = table.Column<string>(type: "jsonb", nullable: true, comment: "Конфигурация на стъпка")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_steps", x => x.id);
                },
                comment: "Стъпка");

            migrationBuilder.CreateTable(
                name: "service_type_steps",
                columns: table => new
                {
                    service_type_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на вид услуга"),
                    step_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на стъпка")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_type_steps", x => new { x.service_type_id, x.step_id });
                    table.ForeignKey(
                        name: "fk_service_type_steps_service_types_service_type_id",
                        column: x => x.service_type_id,
                        principalTable: "service_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_service_type_steps_steps_step_id",
                        column: x => x.step_id,
                        principalTable: "steps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Стъпки към вид услуга");

            migrationBuilder.CreateIndex(
                name: "ix_service_type_steps_step_id",
                table: "service_type_steps",
                column: "step_id");

            migrationBuilder.AddForeignKey(
                name: "fk_fields_field_types_field_type_id",
                table: "fields",
                column: "field_type_id",
                principalTable: "field_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_fields_field_types_field_type_id",
                table: "fields");

            migrationBuilder.DropTable(
                name: "service_type_steps");

            migrationBuilder.DropTable(
                name: "service_types");

            migrationBuilder.DropTable(
                name: "steps");

            migrationBuilder.DropPrimaryKey(
                name: "pk_fields",
                table: "fields");

            migrationBuilder.DropPrimaryKey(
                name: "pk_field_types",
                table: "field_types");

            migrationBuilder.RenameTable(
                name: "fields",
                newName: "field");

            migrationBuilder.RenameTable(
                name: "field_types",
                newName: "field_type");

            migrationBuilder.RenameIndex(
                name: "ix_fields_name_version",
                table: "field",
                newName: "ix_field_name_version");

            migrationBuilder.RenameIndex(
                name: "ix_fields_name_is_current",
                table: "field",
                newName: "ix_field_name_is_current");

            migrationBuilder.RenameIndex(
                name: "ix_fields_field_type_id",
                table: "field",
                newName: "ix_field_field_type_id");

            migrationBuilder.RenameIndex(
                name: "ix_field_types_name",
                table: "field_type",
                newName: "ix_field_type_name");

            migrationBuilder.AddPrimaryKey(
                name: "pk_field",
                table: "field",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_field_type",
                table: "field_type",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_field_field_type_field_type_id",
                table: "field",
                column: "field_type_id",
                principalTable: "field_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
