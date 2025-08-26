using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class RegisterPersonRecordRoleId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_register_person_records",
                table: "register_person_records");

            migrationBuilder.AddColumn<int>(
                name: "role_id",
                table: "register_person_records",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Роля партида/заявител");

            migrationBuilder.AddPrimaryKey(
                name: "pk_register_person_records",
                table: "register_person_records",
                columns: new[] { "register_id", "master_person_record_id", "role_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_register_person_records",
                table: "register_person_records");

            migrationBuilder.DropColumn(
                name: "role_id",
                table: "register_person_records");

            migrationBuilder.AddPrimaryKey(
                name: "pk_register_person_records",
                table: "register_person_records",
                columns: new[] { "register_id", "master_person_record_id" });
        }
    }
}
