using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Post_Revisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PostRevisions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<long>(type: "bigint", nullable: false),
                    BeforeTitle = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    AfterTitle = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    BeforeBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EditorId = table.Column<long>(type: "bigint", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByMemberId = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedByMemberId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostRevisions_Members_EditorId",
                        column: x => x.EditorId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PostRevisions_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostRevisions_EditorId",
                table: "PostRevisions",
                column: "EditorId");

            migrationBuilder.CreateIndex(
                name: "IX_PostRevisions_PostId",
                table: "PostRevisions",
                column: "PostId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostRevisions");
        }
    }
}
