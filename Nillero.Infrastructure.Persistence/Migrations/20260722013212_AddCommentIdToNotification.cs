using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nillero.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentIdToNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommentId",
                table: "Notifications",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommentId",
                table: "Notifications");
        }
    }
}
