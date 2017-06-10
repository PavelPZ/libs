using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Fulltext.Migrations
{
    public partial class v1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dicts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Imported = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    SrcLang = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dicts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "dm_fts_parser",
                columns: table => new
                {
                    display_term = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dm_fts_parser", x => x.display_term);
                });

            migrationBuilder.CreateTable(
                name: "Phrases",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Base = table.Column<string>(maxLength: 128, nullable: false),
                    DestLang = table.Column<byte>(nullable: false),
                    DictId = table.Column<int>(nullable: false),
                    SrcLang = table.Column<byte>(nullable: false),
                    SrcRef = table.Column<int>(nullable: true),
                    Text = table.Column<string>(nullable: false),
                    TextIdxs = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phrases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Phrases_Dicts_DictId",
                        column: x => x.DictId,
                        principalTable: "Dicts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Phrases_Phrases_SrcRef",
                        column: x => x.SrcRef,
                        principalTable: "Phrases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhraseWords",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DestLang = table.Column<byte>(nullable: false),
                    PhraseId = table.Column<int>(nullable: false),
                    PhraseRef = table.Column<int>(nullable: false),
                    SrcLang = table.Column<byte>(nullable: false),
                    Word = table.Column<string>(maxLength: 24, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhraseWords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhraseWords_Phrases_PhraseId",
                        column: x => x.PhraseId,
                        principalTable: "Phrases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dicts_Name",
                table: "Dicts",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Phrases_DictId",
                table: "Phrases",
                column: "DictId");

            migrationBuilder.CreateIndex(
                name: "IX_Phrases_SrcRef",
                table: "Phrases",
                column: "SrcRef");

            migrationBuilder.CreateIndex(
                name: "IX_Phrases_Base_SrcLang_DestLang",
                table: "Phrases",
                columns: new[] { "Base", "SrcLang", "DestLang" });

            migrationBuilder.CreateIndex(
                name: "IX_PhraseWords_PhraseId",
                table: "PhraseWords",
                column: "PhraseId");

            migrationBuilder.CreateIndex(
                name: "IX_PhraseWords_Word_SrcLang_DestLang",
                table: "PhraseWords",
                columns: new[] { "Word", "SrcLang", "DestLang" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dm_fts_parser");

            migrationBuilder.DropTable(
                name: "PhraseWords");

            migrationBuilder.DropTable(
                name: "Phrases");

            migrationBuilder.DropTable(
                name: "Dicts");
        }
    }
}
