using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class OpenData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "open_data_category_id",
                table: "registers",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Категория opendata");

            migrationBuilder.AddColumn<string>(
                name: "open_data_tags",
                table: "registers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "Тагове opendata");

            migrationBuilder.AddColumn<string>(
                name: "open_data_data_set_id",
                table: "register_administrations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "OpenData DataSetId");

            migrationBuilder.AddColumn<string>(
                name: "open_data_api_key",
                table: "administration",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "api-key за opendata");

            migrationBuilder.AddColumn<int>(
                name: "open_data_org_id",
                table: "administration",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Идентификатор на организация в  opendata");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "open_data_category_id",
                table: "registers");

            migrationBuilder.DropColumn(
                name: "open_data_tags",
                table: "registers");

            migrationBuilder.DropColumn(
                name: "open_data_data_set_id",
                table: "register_administrations");

            migrationBuilder.DropColumn(
                name: "open_data_api_key",
                table: "administration");

            migrationBuilder.DropColumn(
                name: "open_data_org_id",
                table: "administration");
        }
    }
}
