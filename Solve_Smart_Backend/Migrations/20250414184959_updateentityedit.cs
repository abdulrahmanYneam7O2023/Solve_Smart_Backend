using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Solve_Smart_Backend.Migrations
{
    /// <inheritdoc />
    public partial class updateentityedit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminRequest_AspNetUsers_UserId",
                table: "AdminRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdminRequest",
                table: "AdminRequest");

            migrationBuilder.RenameTable(
                name: "AdminRequest",
                newName: "adminRequests");

            migrationBuilder.RenameIndex(
                name: "IX_AdminRequest_UserId",
                table: "adminRequests",
                newName: "IX_adminRequests_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_adminRequests",
                table: "adminRequests",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_adminRequests_AspNetUsers_UserId",
                table: "adminRequests",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_adminRequests_AspNetUsers_UserId",
                table: "adminRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_adminRequests",
                table: "adminRequests");

            migrationBuilder.RenameTable(
                name: "adminRequests",
                newName: "AdminRequest");

            migrationBuilder.RenameIndex(
                name: "IX_adminRequests_UserId",
                table: "AdminRequest",
                newName: "IX_AdminRequest_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdminRequest",
                table: "AdminRequest",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminRequest_AspNetUsers_UserId",
                table: "AdminRequest",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
