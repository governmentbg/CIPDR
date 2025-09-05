using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class PreferredResultDeliveryAddedToProcess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "preferred_result_delivery_method",
                table: "processes",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true,
                comment: "Начин на предоставяне на резултата от ЕАУ от номенклатура Начини на предоставяне на резултата от ЕАУ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "preferred_result_delivery_method",
                table: "processes");
        }
    }
}
