using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class CyrillicLetterRemoverFromIsDeletedInRegisterItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_register_items_administrations_tennant_id",
                table: "register_items");

            migrationBuilder.RenameColumn(
                name: "tennant_id",
                table: "register_items",
                newName: "tenant_id");

            migrationBuilder.RenameColumn(
                name: "is_delеted",
                table: "register_items",
                newName: "is_deleted");

            migrationBuilder.RenameIndex(
                name: "ix_register_items_tennant_id",
                table: "register_items",
                newName: "ix_register_items_tenant_id");

            migrationBuilder.AlterColumn<string>(
                name: "register_number",
                table: "register_items",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                comment: "Номер на вписване ",
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Номер на вписване ");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "register_items",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                comment: "Име",
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Име");

            migrationBuilder.AddForeignKey(
                name: "fk_register_items_administrations_tenant_id",
                table: "register_items",
                column: "tenant_id",
                principalTable: "administrations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_register_items_administrations_tenant_id",
                table: "register_items");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "register_items",
                newName: "tennant_id");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "register_items",
                newName: "is_delеted");

            migrationBuilder.RenameIndex(
                name: "ix_register_items_tenant_id",
                table: "register_items",
                newName: "ix_register_items_tennant_id");

            migrationBuilder.AlterColumn<string>(
                name: "register_number",
                table: "register_items",
                type: "text",
                nullable: false,
                comment: "Номер на вписване ",
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16,
                oldComment: "Номер на вписване ");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "register_items",
                type: "text",
                nullable: false,
                comment: "Име",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldComment: "Име");

            migrationBuilder.AddForeignKey(
                name: "fk_register_items_administrations_tennant_id",
                table: "register_items",
                column: "tennant_id",
                principalTable: "administrations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
