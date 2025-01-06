using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class RegisterAdministration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_administrations_administration_administration_id",
                table: "administrations");

            migrationBuilder.DropForeignKey(
                name: "fk_administrations_registers_register_id",
                table: "administrations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_administrations",
                table: "administrations");

            migrationBuilder.RenameTable(
                name: "administrations",
                newName: "register_administrations");

            migrationBuilder.RenameIndex(
                name: "ix_administrations_register_id_administration_id",
                table: "register_administrations",
                newName: "ix_register_administrations_register_id_administration_id");

            migrationBuilder.RenameIndex(
                name: "ix_administrations_administration_id",
                table: "register_administrations",
                newName: "ix_register_administrations_administration_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_register_administrations",
                table: "register_administrations",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_register_administrations_administration_administration_id",
                table: "register_administrations",
                column: "administration_id",
                principalTable: "administration",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_register_administrations_registers_register_id",
                table: "register_administrations",
                column: "register_id",
                principalTable: "registers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_register_administrations_administration_administration_id",
                table: "register_administrations");

            migrationBuilder.DropForeignKey(
                name: "fk_register_administrations_registers_register_id",
                table: "register_administrations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_register_administrations",
                table: "register_administrations");

            migrationBuilder.RenameTable(
                name: "register_administrations",
                newName: "administrations");

            migrationBuilder.RenameIndex(
                name: "ix_register_administrations_register_id_administration_id",
                table: "administrations",
                newName: "ix_administrations_register_id_administration_id");

            migrationBuilder.RenameIndex(
                name: "ix_register_administrations_administration_id",
                table: "administrations",
                newName: "ix_administrations_administration_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_administrations",
                table: "administrations",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_administrations_administration_administration_id",
                table: "administrations",
                column: "administration_id",
                principalTable: "administration",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_administrations_registers_register_id",
                table: "administrations",
                column: "register_id",
                principalTable: "registers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
