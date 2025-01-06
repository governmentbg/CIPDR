using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.ObjectsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class NameVersionIndexInFieldRestored : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_field_name_version",
                table: "field",
                columns: new[] { "name", "version" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_field_name_version",
                table: "field");
        }
    }
}
