using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GalleryManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddExhibitRevenueFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Source of truth: Data/Sql/001_create_get_exhibit_revenue_function.sql
            // Keep both files in sync if the function changes.
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION get_exhibit_revenue(exhibit_id_param INT)
                RETURNS TABLE(artwork_title TEXT, sale_price NUMERIC) AS $$
                BEGIN
                    RETURN QUERY
                    SELECT a.""Title"", a.""Price""
                    FROM artworks a
                    WHERE a.""ExhibitId"" = exhibit_id_param
                      AND a.""Status"" = 'Sold'
                    ORDER BY a.""Title"";
                END;
                $$ LANGUAGE plpgsql;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS get_exhibit_revenue(INT);");
        }
    }
}
