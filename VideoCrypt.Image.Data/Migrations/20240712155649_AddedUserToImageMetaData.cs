using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoCrypt.Image.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserToImageMetaData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "user_id",
                table: "image_metadata",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "i_x_image_metadata_user_id",
                table: "image_metadata",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_image_metadata__asp_net_users_user_id",
                table: "image_metadata",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_image_metadata__asp_net_users_user_id",
                table: "image_metadata");

            migrationBuilder.DropIndex(
                name: "i_x_image_metadata_user_id",
                table: "image_metadata");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "image_metadata");
        }
    }
}
