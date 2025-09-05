using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.IntegrationsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class EDeliveryIncomingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "incoming_date",
                table: "e_delivery_messages",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Входяща дата на заявена услуга");

            migrationBuilder.AddColumn<string>(
                name: "incoming_number",
                table: "e_delivery_messages",
                type: "text",
                nullable: true,
                comment: "Входящ номер на заявена услуга");

            migrationBuilder.AddColumn<Guid>(
                name: "outbox_id",
                table: "e_delivery_messages",
                type: "uuid",
                nullable: true,
                comment: "Идентификатор на изходящо съобщение");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "incoming_date",
                table: "e_delivery_messages");

            migrationBuilder.DropColumn(
                name: "incoming_number",
                table: "e_delivery_messages");

            migrationBuilder.DropColumn(
                name: "outbox_id",
                table: "e_delivery_messages");
        }
    }
}
