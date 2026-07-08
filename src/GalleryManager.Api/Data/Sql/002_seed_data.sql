-- Seed data for local/dev Neon database.
-- Idempotent: safe to re-run (clears existing rows first).
TRUNCATE TABLE artworks RESTART IDENTITY CASCADE;
TRUNCATE TABLE exhibits RESTART IDENTITY CASCADE;

INSERT INTO exhibits ("Id", "Name", "StartDate", "EndDate") VALUES
    (1, 'Modern Impressions', '2026-01-10', '2026-03-01'),
    (2, 'Coastal Light', '2026-03-15', '2026-05-01'),
    (3, 'Urban Abstractions', '2026-05-20', '2026-07-10');
SELECT setval(pg_get_serial_sequence('exhibits', 'Id'), (SELECT MAX("Id") FROM exhibits));

INSERT INTO artworks ("Title", "Artist", "Medium", "Price", "Status", "ExhibitId", "CreatedAtUtc") VALUES
    ('Sunset Over the Bay', 'Elena Marsh', 'Oil on canvas', 2400.00, 'Sold', 1, now()),
    ('Quiet Harbor', 'Elena Marsh', 'Oil on canvas', 1800.00, 'Available', 1, now()),
    ('Fragments I', 'Tomas Reyes', 'Mixed media', 3200.00, 'Sold', 2, now()),
    ('Fragments II', 'Tomas Reyes', 'Mixed media', 3200.00, 'OnLoan', 2, now()),
    ('City Grid', 'Priya Nandan', 'Acrylic on panel', 1500.00, 'Available', 3, now()),
    ('Night Lines', 'Priya Nandan', 'Acrylic on panel', 2100.00, 'Sold', 3, now()),
    ('Untitled Study', 'Marco Chen', 'Charcoal on paper', 600.00, 'Available', NULL, now());
