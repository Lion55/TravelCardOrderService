using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace TravelCardOrderService.Data.Migrations
{
    public partial class OrderAttributeAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Orders",
                newName: "UserName");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Orders",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Orders",
                newName: "Name");
        }
    }
}
