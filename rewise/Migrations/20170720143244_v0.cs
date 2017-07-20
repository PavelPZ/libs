using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace rewise.Migrations
{
    public partial class v0 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Imported = table.Column<DateTime>(nullable: false),
                    Lang = table.Column<byte>(nullable: false),
                    Lessons = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "dm_fts_parsers",
                columns: table => new
                {
                    display_term = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dm_fts_parsers", x => x.display_term);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SoundSources",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoundSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookPhrases",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BookRef = table.Column<int>(nullable: false),
                    Lang = table.Column<byte>(nullable: false),
                    LessonId = table.Column<byte>(nullable: false),
                    Text = table.Column<string>(nullable: true),
                    TextIdxs = table.Column<byte[]>(nullable: true),
                    isPhraseWord = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookPhrases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookPhrases_Books_BookRef",
                        column: x => x.BookRef,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDicts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    LocaleLang = table.Column<byte>(nullable: false),
                    PhraseLang = table.Column<byte>(nullable: false),
                    UserRef = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDicts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDicts_Users_UserRef",
                        column: x => x.UserRef,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SoundFiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Lang = table.Column<byte>(nullable: false),
                    Path = table.Column<string>(nullable: true),
                    SourceRef = table.Column<int>(nullable: false),
                    Text = table.Column<string>(maxLength: 24, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoundFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoundFiles_SoundSources_SourceRef",
                        column: x => x.SourceRef,
                        principalTable: "SoundSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookLocales",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Lang = table.Column<byte>(nullable: false),
                    PhraseRef = table.Column<int>(nullable: false),
                    Text = table.Column<string>(nullable: true),
                    TextIdxs = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookLocales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookLocales_BookPhrases_PhraseRef",
                        column: x => x.PhraseRef,
                        principalTable: "BookPhrases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookPhraseWords",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BookId = table.Column<int>(nullable: true),
                    BookRef = table.Column<int>(nullable: false),
                    Lang = table.Column<byte>(nullable: false),
                    PhraseRef = table.Column<int>(nullable: false),
                    Word = table.Column<string>(maxLength: 24, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookPhraseWords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookPhraseWords_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookPhraseWords_BookPhrases_PhraseRef",
                        column: x => x.PhraseRef,
                        principalTable: "BookPhrases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFacts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Data = table.Column<string>(nullable: true),
                    DictRef = table.Column<int>(nullable: false),
                    LocaleText = table.Column<string>(nullable: true),
                    LocaleTextIdxs = table.Column<byte[]>(nullable: true),
                    PhraseText = table.Column<string>(nullable: true),
                    PhraseTextIdxs = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFacts_UserDicts_DictRef",
                        column: x => x.DictRef,
                        principalTable: "UserDicts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookLocaleWords",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BookId = table.Column<int>(nullable: true),
                    BookRef = table.Column<int>(nullable: false),
                    BookSrcLang = table.Column<byte>(nullable: false),
                    Lang = table.Column<byte>(nullable: false),
                    PhraseRef = table.Column<int>(nullable: false),
                    Word = table.Column<string>(maxLength: 24, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookLocaleWords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookLocaleWords_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookLocaleWords_BookLocales_PhraseRef",
                        column: x => x.PhraseRef,
                        principalTable: "BookLocales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLocaleWords",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DictRef = table.Column<int>(nullable: false),
                    FactRef = table.Column<int>(nullable: false),
                    Lang = table.Column<byte>(nullable: false),
                    Word = table.Column<string>(maxLength: 24, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLocaleWords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLocaleWords_UserDicts_DictRef",
                        column: x => x.DictRef,
                        principalTable: "UserDicts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserLocaleWords_UserFacts_FactRef",
                        column: x => x.FactRef,
                        principalTable: "UserFacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPhraseWords",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DictRef = table.Column<int>(nullable: false),
                    FactRef = table.Column<int>(nullable: false),
                    Lang = table.Column<byte>(nullable: false),
                    Word = table.Column<string>(maxLength: 24, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPhraseWords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPhraseWords_UserDicts_DictRef",
                        column: x => x.DictRef,
                        principalTable: "UserDicts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserPhraseWords_UserFacts_FactRef",
                        column: x => x.FactRef,
                        principalTable: "UserFacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookLocales_PhraseRef",
                table: "BookLocales",
                column: "PhraseRef");

            migrationBuilder.CreateIndex(
                name: "IX_BookLocaleWords_BookId",
                table: "BookLocaleWords",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookLocaleWords_PhraseRef",
                table: "BookLocaleWords",
                column: "PhraseRef");

            migrationBuilder.CreateIndex(
                name: "IX_BookLocaleWords_Word_Lang_BookSrcLang_BookRef",
                table: "BookLocaleWords",
                columns: new[] { "Word", "Lang", "BookSrcLang", "BookRef" });

            migrationBuilder.CreateIndex(
                name: "IX_BookPhrases_BookRef",
                table: "BookPhrases",
                column: "BookRef");

            migrationBuilder.CreateIndex(
                name: "IX_BookPhraseWords_BookId",
                table: "BookPhraseWords",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookPhraseWords_PhraseRef",
                table: "BookPhraseWords",
                column: "PhraseRef");

            migrationBuilder.CreateIndex(
                name: "IX_BookPhraseWords_Word_Lang_BookRef",
                table: "BookPhraseWords",
                columns: new[] { "Word", "Lang", "BookRef" });

            migrationBuilder.CreateIndex(
                name: "IX_UserFacts_DictRef",
                table: "UserFacts",
                column: "DictRef");

            migrationBuilder.CreateIndex(
                name: "IX_UserLocaleWords_DictRef",
                table: "UserLocaleWords",
                column: "DictRef");

            migrationBuilder.CreateIndex(
                name: "IX_UserLocaleWords_FactRef",
                table: "UserLocaleWords",
                column: "FactRef");

            migrationBuilder.CreateIndex(
                name: "IX_UserLocaleWords_Word_DictRef",
                table: "UserLocaleWords",
                columns: new[] { "Word", "DictRef" });

            migrationBuilder.CreateIndex(
                name: "IX_UserPhraseWords_DictRef",
                table: "UserPhraseWords",
                column: "DictRef");

            migrationBuilder.CreateIndex(
                name: "IX_UserPhraseWords_FactRef",
                table: "UserPhraseWords",
                column: "FactRef");

            migrationBuilder.CreateIndex(
                name: "IX_UserPhraseWords_Word_DictRef",
                table: "UserPhraseWords",
                columns: new[] { "Word", "DictRef" });

            migrationBuilder.CreateIndex(
                name: "IX_UserDicts_UserRef",
                table: "UserDicts",
                column: "UserRef");

            migrationBuilder.CreateIndex(
                name: "IX_SoundFiles_SourceRef_Text_Lang",
                table: "SoundFiles",
                columns: new[] { "SourceRef", "Text", "Lang" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookLocaleWords");

            migrationBuilder.DropTable(
                name: "BookPhraseWords");

            migrationBuilder.DropTable(
                name: "dm_fts_parsers");

            migrationBuilder.DropTable(
                name: "UserLocaleWords");

            migrationBuilder.DropTable(
                name: "UserPhraseWords");

            migrationBuilder.DropTable(
                name: "SoundFiles");

            migrationBuilder.DropTable(
                name: "BookLocales");

            migrationBuilder.DropTable(
                name: "UserFacts");

            migrationBuilder.DropTable(
                name: "SoundSources");

            migrationBuilder.DropTable(
                name: "BookPhrases");

            migrationBuilder.DropTable(
                name: "UserDicts");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
