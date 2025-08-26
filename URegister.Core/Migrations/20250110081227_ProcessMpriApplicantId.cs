using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class ProcessMpriApplicantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "mpri_id",
                table: "processes",
                type: "uuid",
                nullable: false,
                comment: "Идентификатор на партида в MasterPersonIndex",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "Идентификатор на MasterPersonIndex");

            migrationBuilder.AddColumn<Guid>(
                name: "mpri_applicant_id",
                table: "processes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                comment: "Идентификатор на заявител в MasterPersonIndex");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mpri_applicant_id",
                table: "processes");

            migrationBuilder.AlterColumn<Guid>(
                name: "mpri_id",
                table: "processes",
                type: "uuid",
                nullable: false,
                comment: "Идентификатор на MasterPersonIndex",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "Идентификатор на партида в MasterPersonIndex");
        }
    }
}
