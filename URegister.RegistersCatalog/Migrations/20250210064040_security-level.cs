using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class securitylevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
