using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.IntegrationsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class EDeliveryMessageSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "source_id",
                table: "e_delivery_messages",
                type: "uuid",
                nullable: true,
                comment: "Идентификатор на връзка");

            migrationBuilder.AddColumn<int>(
                name: "source_type",
                table: "e_delivery_messages",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Вид връзка");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "source_id",
                table: "e_delivery_messages");

            migrationBuilder.DropColumn(
                name: "source_type",
                table: "e_delivery_messages");
        }
    }
}
