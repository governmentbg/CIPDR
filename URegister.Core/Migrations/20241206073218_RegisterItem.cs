using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class RegisterItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "processes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    incoming_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "Входящ номер"),
                    register_number = table.Column<string>(type: "text", nullable: true, comment: "Номер на вписване "),
                    incoming_date = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "Дата на входиране"),
                    service_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на услуга"),
                    status_id = table.Column<int>(type: "integer", nullable: false),
                    tennant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на администрация")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_processes", x => x.id);
                    table.ForeignKey(
                        name: "fk_processes_administrations_tennant_id",
                        column: x => x.tennant_id,
                        principalTable: "administrations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_processes_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Процеси");

            migrationBuilder.CreateTable(
                name: "process_steps",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    process_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на процес"),
                    step_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на стъпка"),
                    form_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на форма"),
                    step_data = table.Column<string>(type: "jsonb", nullable: false, comment: "Информация за стъпка")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_process_steps", x => x.id);
                    table.ForeignKey(
                        name: "fk_process_steps_forms_form_id",
                        column: x => x.form_id,
                        principalTable: "forms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_process_steps_processes_process_id",
                        column: x => x.process_id,
                        principalTable: "processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_process_steps_service_steps_step_id",
                        column: x => x.step_id,
                        principalTable: "service_steps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Стъпки към процес");

            migrationBuilder.CreateTable(
                name: "register_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    mpri_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на MasterPersonIndex"),
                    process_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на процес"),
                    tennant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на администрация"),
                    register_number = table.Column<string>(type: "text", nullable: false, comment: "Номер на вписване "),
                    field_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на поле"),
                    parent_field_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на главно поле"),
                    is_complex = table.Column<bool>(type: "boolean", nullable: false, comment: "Комплексно поле"),
                    name = table.Column<string>(type: "text", nullable: false, comment: "Име"),
                    value = table.Column<string>(type: "text", nullable: true, comment: "Стойност"),
                    cl_value = table.Column<string>(type: "text", nullable: true, comment: "Стойност на номенклатура"),
                    is_public = table.Column<bool>(type: "boolean", nullable: false, comment: "Публично поле"),
                    is_delеted = table.Column<bool>(type: "boolean", nullable: false, comment: "Заличено поле")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_register_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_register_items_administrations_tennant_id",
                        column: x => x.tennant_id,
                        principalTable: "administrations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_register_items_processes_process_id",
                        column: x => x.process_id,
                        principalTable: "processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Вписвания");

            migrationBuilder.CreateIndex(
                name: "ix_process_steps_form_id",
                table: "process_steps",
                column: "form_id");

            migrationBuilder.CreateIndex(
                name: "ix_process_steps_process_id",
                table: "process_steps",
                column: "process_id");

            migrationBuilder.CreateIndex(
                name: "ix_process_steps_step_id",
                table: "process_steps",
                column: "step_id");

            migrationBuilder.CreateIndex(
                name: "ix_processes_service_id",
                table: "processes",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_processes_tennant_id",
                table: "processes",
                column: "tennant_id");

            migrationBuilder.CreateIndex(
                name: "ix_register_items_process_id",
                table: "register_items",
                column: "process_id");

            migrationBuilder.CreateIndex(
                name: "ix_register_items_tennant_id",
                table: "register_items",
                column: "tennant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "process_steps");

            migrationBuilder.DropTable(
                name: "register_items");

            migrationBuilder.DropTable(
                name: "processes");
        }
    }
}
