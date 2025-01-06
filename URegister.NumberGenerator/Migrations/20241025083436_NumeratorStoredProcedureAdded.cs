using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.NumberGenerator.Migrations
{
    /// <inheritdoc />
    public partial class NumeratorStoredProcedureAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION ""public"".""get_sequence""(p_prefix int4)
  RETURNS ""public"".""numerators"" AS $BODY$
declare
	num numerators;
begin
	insert into numerators (""sequence"", ""prefix"")
	values (1, p_prefix)
	on conflict (""prefix"") do 
		update set ""sequence"" = numerators.""sequence"" + 1
	returning * into num;

	return num;
END $BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS ""public"".""get_sequence""(p_prefix int4);");
        }
    }
}
