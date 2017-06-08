using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Fulltext.Migrations
{
    public partial class v2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SrcLang",
                table: "Phrases");

            migrationBuilder.AddColumn<byte>(
                name: "SrcLang",
                table: "Dicts",
                nullable: false,
                defaultValue: (byte)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SrcLang",
                table: "Dicts");

            migrationBuilder.AddColumn<byte>(
                name: "SrcLang",
                table: "Phrases",
                nullable: false,
                defaultValue: (byte)0);
        }
    }
}
