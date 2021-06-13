using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ReservationAPI.Migrations
{
    public partial class Initial_Migration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ItemTypes",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "A normal car", "Car" },
                    { 2, "A normal bike", "Bike" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "FirstName", "LastName", "UserName" },
                values: new object[,]
                {
                    { 1, null, null, "user1" },
                    { 2, null, null, "user1" }
                });

            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "Id", "Created", "DailyFee", "Description", "ItemTypeId", "Location", "Modified", "OwnerId", "Picture", "Title", "Withdrawn" },
                values: new object[] { 1, new DateTime(2021, 6, 8, 13, 49, 16, 998, DateTimeKind.Local).AddTicks(5730), 0f, "Nice 2016 model", 1, "Pierrefonds", new DateTime(2021, 6, 8, 13, 49, 16, 998, DateTimeKind.Local).AddTicks(4429), 1, null, "My Ford Focus", false });

            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "Id", "Created", "DailyFee", "Description", "ItemTypeId", "Location", "Modified", "OwnerId", "Picture", "Title", "Withdrawn" },
                values: new object[] { 2, new DateTime(2021, 6, 8, 13, 49, 16, 998, DateTimeKind.Local).AddTicks(6994), 0f, "Old 2011 model", 1, "Ile-Bizzard", new DateTime(2021, 6, 8, 13, 49, 16, 998, DateTimeKind.Local).AddTicks(6965), 2, null, "My Nissan ", false });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
