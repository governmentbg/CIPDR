using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class RegixReportsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "regix_reports",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Индентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на потребител"),
                    event_date = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "Дата на събитието"),
                    regix_guid = table.Column<Guid>(type: "uuid", nullable: true, comment: "Guid от Regix"),
                    request_data = table.Column<string>(type: "text", nullable: false, comment: "Съдържание на заявка"),
                    response_data = table.Column<string>(type: "jsonb", nullable: false, comment: "Съдържание на отговор"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Описание"),
                    regix_request_type = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, comment: "Номенклатурна стойност на тип заявка")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_regix_reports", x => x.id);
                },
                comment: "Комуникация с Regix");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "regix_reports");
        }
    }
}
