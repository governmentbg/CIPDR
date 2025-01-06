using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class RegisterPersonRecordAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "register_person_records",
                columns: table => new
                {
                    register_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на регистър"),
                    master_person_record_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на партида")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_register_person_records", x => new { x.register_id, x.master_person_record_id });
                    table.ForeignKey(
                        name: "fk_register_person_records_master_person_records_master_person",
                        column: x => x.master_person_record_id,
                        principalTable: "master_person_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_register_person_records_registers_register_id",
                        column: x => x.register_id,
                        principalTable: "registers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Записи на лица в регистър");

            migrationBuilder.CreateIndex(
                name: "ix_register_person_records_master_person_record_id",
                table: "register_person_records",
                column: "master_person_record_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "register_person_records");
        }
    }
}
