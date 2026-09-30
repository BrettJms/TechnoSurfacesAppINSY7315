using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechnoSurfaces.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCatalogueAndQuoting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PropertyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AccountCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    VatNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RateItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    Derivation = table.Column<int>(type: "int", nullable: false),
                    DerivedFromRateItemMultiplier = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    DerivedFromRateItemId = table.Column<int>(type: "int", nullable: true),
                    IsBelowTheLine = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RateItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RateItems_RateItems_DerivedFromRateItemId",
                        column: x => x.DerivedFromRateItemId,
                        principalTable: "RateItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TradingAs = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    PricingStructure = table.Column<int>(type: "int", nullable: false),
                    PriceListDated = table.Column<DateOnly>(type: "date", nullable: false),
                    AdhesivePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DeliveryTerms = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contacts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThicknessMm = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductLines_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RatePrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RateItemId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatePrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RatePrices_RateItems_RateItemId",
                        column: x => x.RateItemId,
                        principalTable: "RateItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RatePrices_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Reference = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    Site = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Project = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidUntil = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quotes_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Quotes_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PriceBands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    ProductLineId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceBands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceBands_ProductLines_ProductLineId",
                        column: x => x.ProductLineId,
                        principalTable: "ProductLines",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PriceBands_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SheetSizes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductLineId = table.Column<int>(type: "int", nullable: false),
                    LengthMm = table.Column<int>(type: "int", nullable: false),
                    WidthMm = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SheetSizes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SheetSizes_ProductLines_ProductLineId",
                        column: x => x.ProductLineId,
                        principalTable: "ProductLines",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InvoiceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuoteId = table.Column<int>(type: "int", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    InvoiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AmountIncVat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RecordedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RecordedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceRecords_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "QuoteVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuoteId = table.Column<int>(type: "int", nullable: false),
                    VersionNo = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MarkupPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    VatRate = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    IsSealed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuoteVersions_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Colours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductLineId = table.Column<int>(type: "int", nullable: false),
                    PriceBandId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SupplierCode = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Range = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Colours_PriceBands_PriceBandId",
                        column: x => x.PriceBandId,
                        principalTable: "PriceBands",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Colours_ProductLines_ProductLineId",
                        column: x => x.ProductLineId,
                        principalTable: "ProductLines",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CostingLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuoteVersionId = table.Column<int>(type: "int", nullable: false),
                    LineType = table.Column<int>(type: "int", nullable: false),
                    MaterialPriceId = table.Column<int>(type: "int", nullable: true),
                    RateItemId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    ResolvedUnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PriceOrigin = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SupplierDiscountPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    IsBelowTheLine = table.Column<bool>(type: "bit", nullable: false),
                    Derivation = table.Column<int>(type: "int", nullable: false),
                    SheetAreaM2 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostingLines", x => x.Id);
                    table.CheckConstraint("CK_CostingLine_MaterialOrRate", "([MaterialPriceId] IS NOT NULL AND [RateItemId] IS NULL) OR ([MaterialPriceId] IS NULL AND [RateItemId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_CostingLines_QuoteVersions_QuoteVersionId",
                        column: x => x.QuoteVersionId,
                        principalTable: "QuoteVersions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "QuotationLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuoteVersionId = table.Column<int>(type: "int", nullable: false),
                    Room = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AmountExVat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationLines_QuoteVersions_QuoteVersionId",
                        column: x => x.QuoteVersionId,
                        principalTable: "QuoteVersions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MaterialPrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColourId = table.Column<int>(type: "int", nullable: true),
                    PriceBandId = table.Column<int>(type: "int", nullable: true),
                    SheetSizeId = table.Column<int>(type: "int", nullable: false),
                    PricePerSqm = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    CapturedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CapturedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialPrices", x => x.Id);
                    table.CheckConstraint("CK_MaterialPrice_ColourOrBand", "([ColourId] IS NOT NULL AND [PriceBandId] IS NULL) OR ([ColourId] IS NULL AND [PriceBandId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_MaterialPrices_Colours_ColourId",
                        column: x => x.ColourId,
                        principalTable: "Colours",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaterialPrices_PriceBands_PriceBandId",
                        column: x => x.PriceBandId,
                        principalTable: "PriceBands",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaterialPrices_SheetSizes_SheetSizeId",
                        column: x => x.SheetSizeId,
                        principalTable: "SheetSizes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_ChangedAtUtc",
                table: "AuditEntries",
                column: "ChangedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_EntityName_EntityKey",
                table: "AuditEntries",
                columns: new[] { "EntityName", "EntityKey" });

            migrationBuilder.CreateIndex(
                name: "IX_Colours_PriceBandId",
                table: "Colours",
                column: "PriceBandId");

            migrationBuilder.CreateIndex(
                name: "IX_Colours_ProductLineId_Name",
                table: "Colours",
                columns: new[] { "ProductLineId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Colours_Status",
                table: "Colours",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_CustomerId",
                table: "Contacts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CostingLines_QuoteVersionId",
                table: "CostingLines",
                column: "QuoteVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Name",
                table: "Customers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRecords_InvoiceNumber",
                table: "InvoiceRecords",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRecords_QuoteId",
                table: "InvoiceRecords",
                column: "QuoteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaterialPrices_ColourId_SheetSizeId_EffectiveFrom",
                table: "MaterialPrices",
                columns: new[] { "ColourId", "SheetSizeId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialPrices_PriceBandId_SheetSizeId_EffectiveFrom",
                table: "MaterialPrices",
                columns: new[] { "PriceBandId", "SheetSizeId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialPrices_SheetSizeId",
                table: "MaterialPrices",
                column: "SheetSizeId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceBands_ProductLineId_Code",
                table: "PriceBands",
                columns: new[] { "ProductLineId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceBands_SupplierId",
                table: "PriceBands",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductLines_SupplierId_Name_ThicknessMm",
                table: "ProductLines",
                columns: new[] { "SupplierId", "Name", "ThicknessMm" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuotationLines_QuoteVersionId",
                table: "QuotationLines",
                column: "QuoteVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_ContactId",
                table: "Quotes",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_CustomerId",
                table: "Quotes",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_Reference",
                table: "Quotes",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_Status_IssueDate",
                table: "Quotes",
                columns: new[] { "Status", "IssueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteVersions_QuoteId_VersionNo",
                table: "QuoteVersions",
                columns: new[] { "QuoteId", "VersionNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RateItems_DerivedFromRateItemId",
                table: "RateItems",
                column: "DerivedFromRateItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RateItems_Name",
                table: "RateItems",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RatePrices_RateItemId_SupplierId_EffectiveFrom",
                table: "RatePrices",
                columns: new[] { "RateItemId", "SupplierId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_RatePrices_SupplierId",
                table: "RatePrices",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SheetSizes_ProductLineId_LengthMm_WidthMm",
                table: "SheetSizes",
                columns: new[] { "ProductLineId", "LengthMm", "WidthMm" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Name",
                table: "Suppliers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditEntries");

            migrationBuilder.DropTable(
                name: "CostingLines");

            migrationBuilder.DropTable(
                name: "InvoiceRecords");

            migrationBuilder.DropTable(
                name: "MaterialPrices");

            migrationBuilder.DropTable(
                name: "QuotationLines");

            migrationBuilder.DropTable(
                name: "RatePrices");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Colours");

            migrationBuilder.DropTable(
                name: "SheetSizes");

            migrationBuilder.DropTable(
                name: "QuoteVersions");

            migrationBuilder.DropTable(
                name: "RateItems");

            migrationBuilder.DropTable(
                name: "PriceBands");

            migrationBuilder.DropTable(
                name: "Quotes");

            migrationBuilder.DropTable(
                name: "ProductLines");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
