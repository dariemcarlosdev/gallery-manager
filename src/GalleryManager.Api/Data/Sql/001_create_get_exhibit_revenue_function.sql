-- This function is now applied automatically via the
-- AddExhibitRevenueFunction EF Core migration (Migrations/20260708005519_AddExhibitRevenueFunction.cs).
-- This file remains the readable source of truth; keep both in sync if the
-- function changes, and update the migration's Up() body accordingly.
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
