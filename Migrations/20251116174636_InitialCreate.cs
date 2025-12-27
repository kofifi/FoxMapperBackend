using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoxMapperBackend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Couriers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Couriers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Depots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AddressLine = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Lat = table.Column<double>(type: "REAL", nullable: false),
                    Lng = table.Column<double>(type: "REAL", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Depots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CourierId = table.Column<int>(type: "INTEGER", nullable: false),
                    DepotId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTimeUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTimeUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastUpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryRuns_Couriers_CourierId",
                        column: x => x.CourierId,
                        principalTable: "Couriers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeliveryRuns_Depots_DepotId",
                        column: x => x.DepotId,
                        principalTable: "Depots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeliveryRunId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExternalCode = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    RecipientName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AddressLine = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Lat = table.Column<double>(type: "REAL", nullable: false),
                    Lng = table.Column<double>(type: "REAL", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DeliveredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Packages_DeliveryRuns_DeliveryRunId",
                        column: x => x.DeliveryRunId,
                        principalTable: "DeliveryRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RouteStops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeliveryRunId = table.Column<int>(type: "INTEGER", nullable: false),
                    OrderIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    PackageId = table.Column<int>(type: "INTEGER", nullable: true),
                    Lat = table.Column<double>(type: "REAL", nullable: false),
                    Lng = table.Column<double>(type: "REAL", nullable: false),
                    PlannedArrivalUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteStops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteStops_DeliveryRuns_DeliveryRunId",
                        column: x => x.DeliveryRunId,
                        principalTable: "DeliveryRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RouteStops_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Couriers_Code",
                table: "Couriers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Couriers_LastUpdatedUtc",
                table: "Couriers",
                column: "LastUpdatedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRuns_CourierId",
                table: "DeliveryRuns",
                column: "CourierId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRuns_DepotId",
                table: "DeliveryRuns",
                column: "DepotId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRuns_LastUpdatedUtc",
                table: "DeliveryRuns",
                column: "LastUpdatedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRuns_Status",
                table: "DeliveryRuns",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Depots_LastUpdatedUtc",
                table: "Depots",
                column: "LastUpdatedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_DeliveryRunId",
                table: "Packages",
                column: "DeliveryRunId");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_ExternalCode",
                table: "Packages",
                column: "ExternalCode");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_LastUpdatedUtc",
                table: "Packages",
                column: "LastUpdatedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_Status",
                table: "Packages",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RouteStops_DeliveryRunId_OrderIndex",
                table: "RouteStops",
                columns: new[] { "DeliveryRunId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RouteStops_LastUpdatedUtc",
                table: "RouteStops",
                column: "LastUpdatedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_RouteStops_PackageId",
                table: "RouteStops",
                column: "PackageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RouteStops");

            migrationBuilder.DropTable(
                name: "Packages");

            migrationBuilder.DropTable(
                name: "DeliveryRuns");

            migrationBuilder.DropTable(
                name: "Couriers");

            migrationBuilder.DropTable(
                name: "Depots");
        }
    }
}
