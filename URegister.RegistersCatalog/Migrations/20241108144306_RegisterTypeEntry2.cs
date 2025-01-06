using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class RegisterTypeEntry2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "type_entry",
                table: "registers",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "",
                comment: "Начин на вписване",
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5,
                oldNullable: true,
                oldComment: "Начин на вписване");

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "registers",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "",
                comment: "Вид на регистъра",
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5,
                oldNullable: true,
                oldComment: "Вид на регистъра");

            migrationBuilder.AlterColumn<string>(
                name: "identity_security_level",
                table: "registers",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "",
                comment: "Ниво на осигуреност на средствата за електронна идентификация",
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5,
                oldNullable: true,
                oldComment: "Ниво на осигуреност на средствата за електронна идентификация");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "type_entry",
                table: "registers",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true,
                comment: "Начин на вписване",
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5,
                oldComment: "Начин на вписване");

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "registers",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true,
                comment: "Вид на регистъра",
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5,
                oldComment: "Вид на регистъра");

            migrationBuilder.AlterColumn<string>(
                name: "identity_security_level",
                table: "registers",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true,
                comment: "Ниво на осигуреност на средствата за електронна идентификация",
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5,
                oldComment: "Ниво на осигуреност на средствата за електронна идентификация");
        }
    }
}
