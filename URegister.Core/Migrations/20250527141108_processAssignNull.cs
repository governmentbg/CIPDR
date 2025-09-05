using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class processAssignNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "assigned_to_user",
                table: "processes",
                type: "uuid",
                nullable: true,
                comment: "Потребител, на който е присвоена услугата",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "Потребител, на който е присвоена услугата");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "assigned_to_user",
                table: "processes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                comment: "Потребител, на който е присвоена услугата",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "Потребител, на който е присвоена услугата");
        }
    }
}
