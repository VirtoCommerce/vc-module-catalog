using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogModule.Data.PostgreSql.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Catalog",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Virtual = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DefaultLanguage = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OwnerId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OuterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogVideo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "character varying(2083)", maxLength: 2083, nullable: false),
                    ContentUrl = table.Column<string>(type: "character varying(2083)", maxLength: 2083, nullable: false),
                    EmbedUrl = table.Column<string>(type: "character varying(2083)", maxLength: 2083, nullable: true),
                    Duration = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LanguageCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    OwnerId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    OwnerType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogVideo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogLanguage",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Language = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CatalogId = table.Column<string>(type: "character varying(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogLanguage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogLanguage_Catalog_CatalogId",
                        column: x => x.CatalogId,
                        principalTable: "Catalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TaxType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OuterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EnableDescription = table.Column<bool>(type: "boolean", nullable: true),
                    CatalogId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ParentCategoryId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ExcludedProperties = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                    table.CheckConstraint("CK_Category_Parent_category_check", "\"ParentCategoryId\" != \"Id\"");
                    table.ForeignKey(
                        name: "FK_Category_Catalog_CatalogId",
                        column: x => x.CatalogId,
                        principalTable: "Catalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Category_Category_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategoryDescription",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    ReviewState = table.Column<int>(type: "integer", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    Locale = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CategoryId = table.Column<string>(type: "character varying(128)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryDescription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryDescription_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CategoryRelation",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceCategoryId = table.Column<string>(type: "character varying(128)", nullable: false),
                    TargetCatalogId = table.Column<string>(type: "character varying(128)", nullable: true),
                    TargetCategoryId = table.Column<string>(type: "character varying(128)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryRelation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryRelation_Catalog_TargetCatalogId",
                        column: x => x.TargetCatalogId,
                        principalTable: "Catalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CategoryRelation_Category_SourceCategoryId",
                        column: x => x.SourceCategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryRelation_Category_TargetCategoryId",
                        column: x => x.TargetCategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsBuyable = table.Column<bool>(type: "boolean", nullable: false),
                    AvailabilityRule = table.Column<int>(type: "integer", nullable: false),
                    MinQuantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxQuantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TrackInventory = table.Column<bool>(type: "boolean", nullable: false),
                    PackageType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ManufacturerPartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Gtin = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ProductType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    WeightUnit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Weight = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    MeasureUnit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Height = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Length = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Width = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    EnableReview = table.Column<bool>(type: "boolean", nullable: true),
                    MaxNumberOfDownload = table.Column<int>(type: "integer", nullable: true),
                    DownloadExpiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DownloadType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    HasUserAgreement = table.Column<bool>(type: "boolean", nullable: true),
                    ShippingType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TaxType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Vendor = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    OuterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CatalogId = table.Column<string>(type: "character varying(128)", nullable: false),
                    CategoryId = table.Column<string>(type: "character varying(128)", nullable: true),
                    ParentId = table.Column<string>(type: "character varying(128)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Item_Catalog_CatalogId",
                        column: x => x.CatalogId,
                        principalTable: "Catalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Item_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Item_Item_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Property",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TargetType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    IsKey = table.Column<bool>(type: "boolean", nullable: false),
                    IsSale = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnum = table.Column<bool>(type: "boolean", nullable: false),
                    IsInput = table.Column<bool>(type: "boolean", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsMultiValue = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false),
                    IsLocaleDependant = table.Column<bool>(type: "boolean", nullable: false),
                    AllowAlias = table.Column<bool>(type: "boolean", nullable: false),
                    PropertyValueType = table.Column<int>(type: "integer", nullable: false),
                    OuterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CatalogId = table.Column<string>(type: "character varying(128)", nullable: true),
                    CategoryId = table.Column<string>(type: "character varying(128)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Property", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Property_Catalog_CatalogId",
                        column: x => x.CatalogId,
                        principalTable: "Catalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Property_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Association",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AssociationType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    Tags = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    OuterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ItemId = table.Column<string>(type: "character varying(128)", nullable: false),
                    AssociatedItemId = table.Column<string>(type: "character varying(128)", nullable: true),
                    AssociatedCategoryId = table.Column<string>(type: "character varying(128)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Association", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Association_Category_AssociatedCategoryId",
                        column: x => x.AssociatedCategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Association_Item_AssociatedItemId",
                        column: x => x.AssociatedItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Association_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogAsset",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Url = table.Column<string>(type: "character varying(2083)", maxLength: 2083, nullable: false),
                    Name = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    MimeType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    OuterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Group = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ItemId = table.Column<string>(type: "character varying(128)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogAsset", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogAsset_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogImage",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Url = table.Column<string>(type: "character varying(2083)", maxLength: 2083, nullable: false),
                    Name = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    LanguageCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    Group = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    OuterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    AltText = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ItemId = table.Column<string>(type: "character varying(128)", nullable: true),
                    CategoryId = table.Column<string>(type: "character varying(128)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogImage_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogImage_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogSeoInfo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Keyword = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StoreId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Language = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    MetaDescription = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    MetaKeywords = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ImageAltDescription = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ItemId = table.Column<string>(type: "character varying(128)", nullable: true),
                    CategoryId = table.Column<string>(type: "character varying(128)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogSeoInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogSeoInfo_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogSeoInfo_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CategoryItemRelation",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<string>(type: "character varying(128)", nullable: false),
                    CategoryId = table.Column<string>(type: "character varying(128)", nullable: true),
                    CatalogId = table.Column<string>(type: "character varying(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryItemRelation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryItemRelation_Catalog_CatalogId",
                        column: x => x.CatalogId,
                        principalTable: "Catalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CategoryItemRelation_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CategoryItemRelation_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EditorialReview",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    ReviewState = table.Column<int>(type: "integer", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    Locale = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ItemId = table.Column<string>(type: "character varying(128)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditorialReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EditorialReview_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyAttribute",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PropertyAttributeName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PropertyAttributeValue = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    PropertyId = table.Column<string>(type: "character varying(128)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyAttribute", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyAttribute_Property_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Property",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyDictionaryItem",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Alias = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    PropertyId = table.Column<string>(type: "character varying(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyDictionaryItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyDictionaryItem_Property_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Property",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyDisplayName",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Locale = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    PropertyId = table.Column<string>(type: "character varying(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyDisplayName", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyDisplayName_Property_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Property",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyValidationRule",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsUnique = table.Column<bool>(type: "boolean", nullable: false),
                    CharCountMin = table.Column<int>(type: "integer", nullable: true),
                    CharCountMax = table.Column<int>(type: "integer", nullable: true),
                    RegExp = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    PropertyId = table.Column<string>(type: "character varying(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyValidationRule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyValidationRule_Property_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Property",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyDictionaryValue",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Locale = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DictionaryItemId = table.Column<string>(type: "character varying(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyDictionaryValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyDictionaryValue_PropertyDictionaryItem_DictionaryIt~",
                        column: x => x.DictionaryItemId,
                        principalTable: "PropertyDictionaryItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyValue",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ValueType = table.Column<int>(type: "integer", nullable: false),
                    ShortTextValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    LongTextValue = table.Column<string>(type: "text", nullable: true),
                    DecimalValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    IntegerValue = table.Column<int>(type: "integer", nullable: false),
                    BooleanValue = table.Column<bool>(type: "boolean", nullable: false),
                    DateTimeValue = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Locale = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OuterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ItemId = table.Column<string>(type: "character varying(128)", nullable: true),
                    CatalogId = table.Column<string>(type: "character varying(128)", nullable: true),
                    CategoryId = table.Column<string>(type: "character varying(128)", nullable: true),
                    DictionaryItemId = table.Column<string>(type: "character varying(128)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyValue_Catalog_CatalogId",
                        column: x => x.CatalogId,
                        principalTable: "Catalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyValue_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyValue_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyValue_PropertyDictionaryItem_DictionaryItemId",
                        column: x => x.DictionaryItemId,
                        principalTable: "PropertyDictionaryItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Association_AssociatedCategoryId",
                table: "Association",
                column: "AssociatedCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Association_AssociatedItemId",
                table: "Association",
                column: "AssociatedItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Association_ItemId",
                table: "Association",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogAsset_ItemId",
                table: "CatalogAsset",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogImage_CategoryId",
                table: "CatalogImage",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogImage_ItemId",
                table: "CatalogImage",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogLanguage_CatalogId",
                table: "CatalogLanguage",
                column: "CatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogSeoInfo_CategoryId",
                table: "CatalogSeoInfo",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogSeoInfo_ItemId",
                table: "CatalogSeoInfo",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnerType_OwnerId",
                table: "CatalogVideo",
                columns: new[] { "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_Category_CatalogId",
                table: "Category",
                column: "CatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_Category_ParentCategoryId",
                table: "Category",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryDescription_CategoryId",
                table: "CategoryDescription",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryItemRelation_CatalogId",
                table: "CategoryItemRelation",
                column: "CatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryItemRelation_CategoryId",
                table: "CategoryItemRelation",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryItemRelation_ItemId",
                table: "CategoryItemRelation",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryRelation_SourceCategoryId",
                table: "CategoryRelation",
                column: "SourceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryRelation_TargetCatalogId",
                table: "CategoryRelation",
                column: "TargetCatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryRelation_TargetCategoryId",
                table: "CategoryRelation",
                column: "TargetCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_EditorialReview_ItemId",
                table: "EditorialReview",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogId_ParentId",
                table: "Item",
                columns: new[] { "CatalogId", "ParentId" });

            migrationBuilder.CreateIndex(
                name: "IX_Code_CatalogId",
                table: "Item",
                columns: new[] { "Code", "CatalogId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreatedDate_ParentId",
                table: "Item",
                columns: new[] { "CreatedDate", "ParentId" });

            migrationBuilder.CreateIndex(
                name: "IX_Item_CategoryId",
                table: "Item",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_ParentId",
                table: "Item",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Property_CatalogId",
                table: "Property",
                column: "CatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_Property_CategoryId",
                table: "Property",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyAttribute_PropertyId",
                table: "PropertyAttribute",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_AliasAndPropertyId",
                table: "PropertyDictionaryItem",
                columns: new[] { "Alias", "PropertyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDictionaryItem_PropertyId",
                table: "PropertyDictionaryItem",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDictionaryValue_DictionaryItemId",
                table: "PropertyDictionaryValue",
                column: "DictionaryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDisplayName_PropertyId",
                table: "PropertyDisplayName",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyValidationRule_PropertyId",
                table: "PropertyValidationRule",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyValue_CatalogId",
                table: "PropertyValue",
                column: "CatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyValue_CategoryId",
                table: "PropertyValue",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyValue_DictionaryItemId",
                table: "PropertyValue",
                column: "DictionaryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyValue_ItemId",
                table: "PropertyValue",
                column: "ItemId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Association");

            migrationBuilder.DropTable(
                name: "CatalogAsset");

            migrationBuilder.DropTable(
                name: "CatalogImage");

            migrationBuilder.DropTable(
                name: "CatalogLanguage");

            migrationBuilder.DropTable(
                name: "CatalogSeoInfo");

            migrationBuilder.DropTable(
                name: "CatalogVideo");

            migrationBuilder.DropTable(
                name: "CategoryDescription");

            migrationBuilder.DropTable(
                name: "CategoryItemRelation");

            migrationBuilder.DropTable(
                name: "CategoryRelation");

            migrationBuilder.DropTable(
                name: "EditorialReview");

            migrationBuilder.DropTable(
                name: "PropertyAttribute");

            migrationBuilder.DropTable(
                name: "PropertyDictionaryValue");

            migrationBuilder.DropTable(
                name: "PropertyDisplayName");

            migrationBuilder.DropTable(
                name: "PropertyValidationRule");

            migrationBuilder.DropTable(
                name: "PropertyValue");

            migrationBuilder.DropTable(
                name: "Item");

            migrationBuilder.DropTable(
                name: "PropertyDictionaryItem");

            migrationBuilder.DropTable(
                name: "Property");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "Catalog");
        }
    }
}
