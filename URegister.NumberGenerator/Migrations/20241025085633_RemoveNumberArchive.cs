using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URegister.NumberGenerator.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNumberArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "number_archives");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "number_archives",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    initial_document_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на инициращият документ"),
                    number = table.Column<long>(type: "bigint", nullable: false, comment: "Номер"),
                    register = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Код на регистър")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_number_archives", x => x.id);
                },
                comment: "Архив на номератора");

            migrationBuilder.CreateIndex(
                name: "ix_number_archives_number",
                table: "number_archives",
                column: "number",
                unique: true);
        }
    }
}
