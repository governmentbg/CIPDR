using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class mpri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "mpri_id",
                table: "processes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                comment: "Идентификатор на MasterPersonIndex");

            migrationBuilder.AddColumn<Guid>(
                name: "registered_step_id",
                table: "processes",
                type: "uuid",
                nullable: true,
                comment: "Идентификатор на стъпка вписване");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mpri_id",
                table: "processes");

            migrationBuilder.DropColumn(
                name: "registered_step_id",
                table: "processes");
        }
    }
}
