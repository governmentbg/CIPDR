using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.ObjectsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class RegisterRestrictionCodesAddedToFieldType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "register_restriction_codes",
                table: "field_types",
                type: "jsonb",
                nullable: true,
                comment: "Списък на регистрите, за които полето е достъпно. Празен списък означава достъпно за всички");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "register_restriction_codes",
                table: "field_types");
        }
    }
}
