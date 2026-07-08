using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GalleryManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIdempotencyKeyToArtworks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "artworks",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_artworks_IdempotencyKey",
                table: "artworks",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_artworks_IdempotencyKey",
                table: "artworks");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "artworks");
        }
    }
}
