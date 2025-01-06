using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.NumberGenerator.Migrations
{
    /// <inheritdoc />
    public partial class ExternalNumeratorArchiveAndNumeratorArchiveAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "external_number_archives",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    register = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Име на регистър"),
                    number = table.Column<long>(type: "bigint", nullable: false, comment: "Номер"),
                    initial_document_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Идентификатор на инициращият документ")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_external_number_archives", x => x.id);
                },
                comment: "Архив на номератора за външни системи");

            migrationBuilder.CreateTable(
                name: "number_archives",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    register = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Код на регистър"),
                    number = table.Column<long>(type: "bigint", nullable: false, comment: "Номер"),
                    initial_document_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на инициращият документ")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_number_archives", x => x.id);
                },
                comment: "Архив на номератора");

            migrationBuilder.CreateIndex(
                name: "ix_external_number_archives_number",
                table: "external_number_archives",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_number_archives_number",
                table: "number_archives",
                column: "number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "external_number_archives");

            migrationBuilder.DropTable(
                name: "number_archives");
        }
    }
}
