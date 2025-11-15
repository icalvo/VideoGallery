using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VideoGallery.Library.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Code = table.Column<char>(type: "character(1)", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ForegroundColor = table.Column<string>(type: "text", nullable: false),
                    BackgroundColor = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "NoVideoEvents",
                columns: table => new
                {
                    Date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoVideoEvents", x => x.Date);
                });

            migrationBuilder.CreateTable(
                name: "Queries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false),
                    Spec_Search = table.Column<string>(type: "text", nullable: false),
                    Spec_SortField = table.Column<string>(type: "text", nullable: true),
                    Spec_SortType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Queries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Videos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Filename = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "File name"),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: false, comment: "Video duration"),
                    NumSequences = table.Column<int>(type: "integer", nullable: false, comment: "# of sequences"),
                    Comments = table.Column<string>(type: "text", nullable: true, comment: "Comments")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TagCategoryId = table.Column<char>(type: "character(1)", nullable: false),
                    TagText = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tag_Categories_TagCategoryId",
                        column: x => x.TagCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Watches",
                columns: table => new
                {
                    VideoId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsDateUnknown = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Watches", x => new { x.VideoId, x.StoreDate, x.IsDateUnknown });
                    table.ForeignKey(
                        name: "FK_Watches_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TagVideo",
                columns: table => new
                {
                    TagsId = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagVideo", x => new { x.TagsId, x.VideoId });
                    table.ForeignKey(
                        name: "FK_TagVideo_Tag_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TagVideo_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tag_TagCategoryId",
                table: "Tag",
                column: "TagCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TagVideo_VideoId",
                table: "TagVideo",
                column: "VideoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NoVideoEvents");

            migrationBuilder.DropTable(
                name: "Queries");

            migrationBuilder.DropTable(
                name: "TagVideo");

            migrationBuilder.DropTable(
                name: "Watches");

            migrationBuilder.DropTable(
                name: "Tag");

            migrationBuilder.DropTable(
                name: "Videos");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
