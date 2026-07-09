using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GalleryManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddExhibitArtworksForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_artworks_ExhibitId",
                table: "artworks",
                column: "ExhibitId");

            migrationBuilder.AddForeignKey(
                name: "FK_artworks_exhibits_ExhibitId",
                table: "artworks",
                column: "ExhibitId",
                principalTable: "exhibits",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_artworks_exhibits_ExhibitId",
                table: "artworks");

            migrationBuilder.DropIndex(
                name: "IX_artworks_ExhibitId",
                table: "artworks");
        }
    }
}
