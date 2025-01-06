using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class mpri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_registers_master_person_records_master_person_records_index",
                table: "registers");

            migrationBuilder.DropIndex(
                name: "ix_registers_master_person_records_index_id",
                table: "registers");

            migrationBuilder.DropColumn(
                name: "master_person_records_index_id",
                table: "registers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "master_person_records_index_id",
                table: "registers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_registers_master_person_records_index_id",
                table: "registers",
                column: "master_person_records_index_id");

            migrationBuilder.AddForeignKey(
                name: "fk_registers_master_person_records_master_person_records_index",
                table: "registers",
                column: "master_person_records_index_id",
                principalTable: "master_person_records",
                principalColumn: "id");
        }
    }
}
