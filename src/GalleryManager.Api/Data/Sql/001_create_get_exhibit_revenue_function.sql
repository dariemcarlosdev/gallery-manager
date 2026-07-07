-- Run this manually against the DB after EF Core migrations, or wire it into
-- a migration via migrationBuilder.Sql(File.ReadAllText(...)).
--
-- Returns each sold artwork's title + sale price for a given exhibit,
-- used by GET /api/exhibits/{id}/revenue

CREATE OR REPLACE FUNCTION get_exhibit_revenue(exhibit_id_param INT)
RETURNS TABLE(artwork_title TEXT, sale_price NUMERIC) AS $$
BEGIN
    RETURN QUERY
    SELECT a."Title", a."Price"
    FROM artworks a
    WHERE a."ExhibitId" = exhibit_id_param
      AND a."Status" = 'Sold'
    ORDER BY a."Title";
END;
$$ LANGUAGE plpgsql;
