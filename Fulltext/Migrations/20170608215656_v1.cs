using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Fulltext.Migrations
{
    public partial class v1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Dicts",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dicts_Name",
                table: "Dicts",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Dicts_Name",
                table: "Dicts");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Dicts",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
