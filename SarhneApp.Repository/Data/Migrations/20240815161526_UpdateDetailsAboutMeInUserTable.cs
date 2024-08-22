using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SarhneApp.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDetailsAboutMeInUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DetaisAboutMe",
                table: "AspNetUsers",
                newName: "DetailsAboutMe");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DetailsAboutMe",
                table: "AspNetUsers",
                newName: "DetaisAboutMe");
        }
    }
}
