using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VirtoCommerce.CatalogModule.Data.Repositories;

#nullable disable

namespace VirtoCommerce.CatalogModule.Data.Migrations
{
    [DbContext(typeof(CatalogDbContext))]
    partial class CatalogDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.AssetEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<string>("Group")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("ItemId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("LanguageCode")
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)");

                    b.Property<string>("MimeType")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<string>("OuterId")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<long>("Size")
                        .HasColumnType("bigint");

                    b.Property<int>("SortOrder")
                        .HasColumnType("int");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasMaxLength(2083)
                        .HasColumnType("nvarchar(2083)");

                    b.HasKey("Id");

                    b.HasIndex("ItemId");

                    b.ToTable("CatalogAsset", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.AssociationEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("AssociatedCategoryId")
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("AssociatedItemId")
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("AssociationType")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ItemId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("OuterId")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<int>("Priority")
                        .HasColumnType("int");

                    b.Property<int?>("Quantity")
                        .HasColumnType("int");

                    b.Property<string>("Tags")
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.HasKey("Id");

                    b.HasIndex("AssociatedCategoryId");

                    b.HasIndex("AssociatedItemId");

                    b.HasIndex("ItemId");

                    b.ToTable("Association", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.CatalogEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("DefaultLanguage")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("OuterId")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("OwnerId")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<bool>("Virtual")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("Catalog", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.CatalogLanguageEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CatalogId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Language")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("CatalogId");

                    b.ToTable("CatalogLanguage", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.CategoryDescriptionEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CategoryId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Comments")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Locale")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("Priority")
                        .HasColumnType("int");

                    b.Property<int>("ReviewState")
                        .HasColumnType("int");

                    b.Property<string>("Source")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("CategoryDescription", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.CategoryEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CatalogId")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<bool?>("EnableDescription")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ExcludedProperties")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("OuterId")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ParentCategoryId")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<int>("Priority")
                        .HasColumnType("int");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("TaxType")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("CatalogId");

                    b.HasIndex("ParentCategoryId");

                    b.ToTable("Category", (string)null);

                    b.HasCheckConstraint("Parent_category_check", "ParentCategoryId != Id");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.CategoryItemRelationEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CatalogId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CategoryId")
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ItemId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.Property<int>("Priority")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CatalogId");

                    b.HasIndex("CategoryId");

                    b.HasIndex("ItemId");

                    b.ToTable("CategoryItemRelation", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.CategoryRelationEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("SourceCategoryId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("TargetCatalogId")
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("TargetCategoryId")
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("SourceCategoryId");

                    b.HasIndex("TargetCatalogId");

                    b.HasIndex("TargetCategoryId");

                    b.ToTable("CategoryRelation", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.EditorialReviewEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Comments")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ItemId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Locale")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("Priority")
                        .HasColumnType("int");

                    b.Property<int>("ReviewState")
                        .HasColumnType("int");

                    b.Property<string>("Source")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("ItemId");

                    b.ToTable("EditorialReview", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.ImageEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("AltText")
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<string>("CategoryId")
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<string>("Group")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("ItemId")
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("LanguageCode")
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<string>("OuterId")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<int>("SortOrder")
                        .HasColumnType("int");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasMaxLength(2083)
                        .HasColumnType("nvarchar(2083)");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("ItemId");

                    b.ToTable("CatalogImage", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.ItemEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<int>("AvailabilityRule")
                        .HasColumnType("int");

                    b.Property<string>("CatalogId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CategoryId")
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DownloadExpiration")
                        .HasColumnType("datetime2");

                    b.Property<string>("DownloadType")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<bool?>("EnableReview")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Gtin")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<bool?>("HasUserAgreement")
                        .HasColumnType("bit");

                    b.Property<decimal?>("Height")
                        .HasPrecision(18, 4)
                        .HasColumnType("decimal(18,4)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool>("IsBuyable")
                        .HasColumnType("bit");

                    b.Property<decimal?>("Length")
                        .HasPrecision(18, 4)
                        .HasColumnType("decimal(18,4)");

                    b.Property<string>("ManufacturerPartNumber")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<int?>("MaxNumberOfDownload")
                        .HasColumnType("int");

                    b.Property<decimal>("MaxQuantity")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("MeasureUnit")
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<decimal>("MinQuantity")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<string>("OuterId")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("PackageType")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ParentId")
                        .HasColumnType("nvarchar(128)");

                    b.Property<int>("Priority")
                        .HasColumnType("int");

                    b.Property<string>("ProductType")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("ShippingType")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("TaxType")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<bool>("TrackInventory")
                        .HasColumnType("bit");

                    b.Property<string>("Vendor")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<decimal?>("Weight")
                        .HasPrecision(18, 4)
                        .HasColumnType("decimal(18,4)");

                    b.Property<string>("WeightUnit")
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<decimal?>("Width")
                        .HasPrecision(18, 4)
                        .HasColumnType("decimal(18,4)");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("ParentId");

                    b.HasIndex("CatalogId", "ParentId")
                        .HasDatabaseName("IX_CatalogId_ParentId");

                    b.HasIndex("Code", "CatalogId")
                        .IsUnique()
                        .HasDatabaseName("IX_Code_CatalogId");

                    b.HasIndex("CreatedDate", "ParentId")
                        .HasDatabaseName("IX_CreatedDate_ParentId");

                    SqlServerIndexBuilderExtensions.IncludeProperties(b.HasIndex("CreatedDate", "ParentId"), new[] { "ModifiedDate" });

                    b.ToTable("Item", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.PropertyAttributeEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("Priority")
                        .HasColumnType("int");

                    b.Property<string>("PropertyAttributeName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("PropertyAttributeValue")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("PropertyId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("PropertyId");

                    b.ToTable("PropertyAttribute", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.PropertyDictionaryItemEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Alias")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)");

                    b.Property<string>("PropertyId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.Property<int>("SortOrder")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("PropertyId");

                    b.HasIndex("Alias", "PropertyId")
                        .IsUnique()
                        .HasDatabaseName("IX_AliasAndPropertyId");

                    b.ToTable("PropertyDictionaryItem", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.PropertyDictionaryValueEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("DictionaryItemId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Locale")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("Value")
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)");

                    b.HasKey("Id");

                    b.HasIndex("DictionaryItemId");

                    b.ToTable("PropertyDictionaryValue", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.PropertyDisplayNameEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Locale")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("Name")
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)");

                    b.Property<string>("PropertyId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("PropertyId");

                    b.ToTable("PropertyDisplayName", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.PropertyEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<bool>("AllowAlias")
                        .HasColumnType("bit");

                    b.Property<string>("CatalogId")
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CategoryId")
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsEnum")
                        .HasColumnType("bit");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("bit");

                    b.Property<bool>("IsInput")
                        .HasColumnType("bit");

                    b.Property<bool>("IsKey")
                        .HasColumnType("bit");

                    b.Property<bool>("IsLocaleDependant")
                        .HasColumnType("bit");

                    b.Property<bool>("IsMultiValue")
                        .HasColumnType("bit");

                    b.Property<bool>("IsRequired")
                        .HasColumnType("bit");

                    b.Property<bool>("IsSale")
                        .HasColumnType("bit");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("OuterId")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<int>("PropertyValueType")
                        .HasColumnType("int");

                    b.Property<string>("TargetType")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("CatalogId");

                    b.HasIndex("CategoryId");

                    b.ToTable("Property", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.PropertyValidationRuleEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<int?>("CharCountMax")
                        .HasColumnType("int");

                    b.Property<int?>("CharCountMin")
                        .HasColumnType("int");

                    b.Property<bool>("IsUnique")
                        .HasColumnType("bit");

                    b.Property<string>("PropertyId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("RegExp")
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.HasKey("Id");

                    b.HasIndex("PropertyId");

                    b.ToTable("PropertyValidationRule", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.PropertyValueEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<bool>("BooleanValue")
                        .HasColumnType("bit");

                    b.Property<string>("CatalogId")
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CategoryId")
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateTimeValue")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("DecimalValue")
                        .HasPrecision(18, 4)
                        .HasColumnType("decimal(18,4)");

                    b.Property<string>("DictionaryItemId")
                        .HasColumnType("nvarchar(128)");

                    b.Property<int>("IntegerValue")
                        .HasColumnType("int");

                    b.Property<string>("ItemId")
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Locale")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("LongTextValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("OuterId")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ShortTextValue")
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)");

                    b.Property<int>("ValueType")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CatalogId");

                    b.HasIndex("CategoryId");

                    b.HasIndex("DictionaryItemId");

                    b.HasIndex("ItemId");

                    b.ToTable("PropertyValue", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.SeoInfoEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CategoryId")
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ImageAltDescription")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("ItemId")
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Keyword")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("Language")
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)");

                    b.Property<string>("MetaDescription")
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<string>("MetaKeywords")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("StoreId")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Title")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("ItemId");

                    b.ToTable("CatalogSeoInfo", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.VideoEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ContentUrl")
                        .IsRequired()
                        .HasMaxLength(2083)
                        .HasColumnType("nvarchar(2083)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<string>("Duration")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("EmbedUrl")
                        .HasMaxLength(2083)
                        .HasColumnType("nvarchar(2083)");

                    b.Property<string>("LanguageCode")
                        .IsRequired()
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<string>("OwnerId")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("OwnerType")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<int>("SortOrder")
                        .HasColumnType("int");

                    b.Property<string>("ThumbnailUrl")
                        .IsRequired()
                        .HasMaxLength(2083)
                        .HasColumnType("nvarchar(2083)");

                    b.Property<DateTime>("UploadDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("OwnerType", "OwnerId")
                        .HasDatabaseName("IX_OwnerType_OwnerId");

                    b.ToTable("CatalogVideo", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.AssetEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.ItemEntity", "CatalogItem")
                        .WithMany("Assets")
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CatalogItem");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.AssociationEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.CategoryEntity", "AssociatedCategory")
                        .WithMany()
                        .HasForeignKey("AssociatedCategoryId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.ItemEntity", "AssociatedItem")
                        .WithMany("ReferencedAssociations")
                        .HasForeignKey("AssociatedItemId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.ItemEntity", "Item")
                        .WithMany("Associations")
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AssociatedCategory");

                    b.Navigation("AssociatedItem");

                    b.Navigation("Item");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.CatalogLanguageEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.CatalogEntity", "Catalog")
                        .WithMany("CatalogLanguages")
                        .HasForeignKey("CatalogId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Catalog");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.CategoryDescriptionEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.CategoryEntity", "Category")
                        .WithMany("CategoryDescriptions")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.CategoryEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.CatalogEntity", "Catalog")
                        .WithMany()
                        .HasForeignKey("CatalogId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.CategoryEntity", "ParentCategory")
                        .WithMany()
                        .HasForeignKey("ParentCategoryId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Catalog");

                    b.Navigation("ParentCategory");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.CategoryItemRelationEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.CatalogEntity", "Catalog")
                        .WithMany()
                        .HasForeignKey("CatalogId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.CategoryEntity", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.ItemEntity", "CatalogItem")
                        .WithMany("CategoryLinks")
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Catalog");

                    b.Navigation("CatalogItem");

                    b.Navigation("Category");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.CategoryRelationEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.CategoryEntity", "SourceCategory")
                        .WithMany("OutgoingLinks")
                        .HasForeignKey("SourceCategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.CatalogEntity", "TargetCatalog")
                        .WithMany("IncomingLinks")
                        .HasForeignKey("TargetCatalogId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.CategoryEntity", "TargetCategory")
                        .WithMany("IncomingLinks")
                        .HasForeignKey("TargetCategoryId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("SourceCategory");

                    b.Navigation("TargetCatalog");

                    b.Navigation("TargetCategory");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.EditorialReviewEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.ItemEntity", "CatalogItem")
                        .WithMany("EditorialReviews")
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CatalogItem");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.ImageEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.CategoryEntity", "Category")
                        .WithMany("Images")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.ItemEntity", "CatalogItem")
                        .WithMany("Images")
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("CatalogItem");

                    b.Navigation("Category");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.ItemEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.CatalogEntity", "Catalog")
                        .WithMany()
                        .HasForeignKey("CatalogId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.CategoryEntity", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.ItemEntity", "Parent")
                        .WithMany("Childrens")
                        .HasForeignKey("ParentId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Catalog");

                    b.Navigation("Category");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.PropertyAttributeEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.PropertyEntity", "Property")
                        .WithMany("PropertyAttributes")
                        .HasForeignKey("PropertyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Property");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.PropertyDictionaryItemEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.PropertyEntity", "Property")
                        .WithMany("DictionaryItems")
                        .HasForeignKey("PropertyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Property");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.PropertyDictionaryValueEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.PropertyDictionaryItemEntity", "DictionaryItem")
                        .WithMany("DictionaryItemValues")
                        .HasForeignKey("DictionaryItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DictionaryItem");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.PropertyDisplayNameEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.PropertyEntity", "Property")
                        .WithMany("DisplayNames")
                        .HasForeignKey("PropertyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Property");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.PropertyEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.CatalogEntity", "Catalog")
                        .WithMany("Properties")
                        .HasForeignKey("CatalogId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.CategoryEntity", "Category")
                        .WithMany("Properties")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Catalog");

                    b.Navigation("Category");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.PropertyValidationRuleEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.PropertyEntity", "Property")
                        .WithMany("ValidationRules")
                        .HasForeignKey("PropertyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Property");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.PropertyValueEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.CatalogEntity", "Catalog")
                        .WithMany("CatalogPropertyValues")
                        .HasForeignKey("CatalogId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.CategoryEntity", "Category")
                        .WithMany("CategoryPropertyValues")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.PropertyDictionaryItemEntity", "DictionaryItem")
                        .WithMany()
                        .HasForeignKey("DictionaryItemId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.ItemEntity", "CatalogItem")
                        .WithMany("ItemPropertyValues")
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Catalog");

                    b.Navigation("CatalogItem");

                    b.Navigation("Category");

                    b.Navigation("DictionaryItem");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.SeoInfoEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.CategoryEntity", "Category")
                        .WithMany("SeoInfos")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("VirtoCommerce.CatalogModule.Data.Model.ItemEntity", "Item")
                        .WithMany("SeoInfos")
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Category");

                    b.Navigation("Item");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.CatalogEntity", b =>
                {
                    b.Navigation("CatalogLanguages");

                    b.Navigation("CatalogPropertyValues");

                    b.Navigation("IncomingLinks");

                    b.Navigation("Properties");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.CategoryEntity", b =>
                {
                    b.Navigation("CategoryDescriptions");

                    b.Navigation("CategoryPropertyValues");

                    b.Navigation("Images");

                    b.Navigation("IncomingLinks");

                    b.Navigation("OutgoingLinks");

                    b.Navigation("Properties");

                    b.Navigation("SeoInfos");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.ItemEntity", b =>
                {
                    b.Navigation("Assets");

                    b.Navigation("Associations");

                    b.Navigation("CategoryLinks");

                    b.Navigation("Childrens");

                    b.Navigation("EditorialReviews");

                    b.Navigation("Images");

                    b.Navigation("ItemPropertyValues");

                    b.Navigation("ReferencedAssociations");

                    b.Navigation("SeoInfos");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.PropertyDictionaryItemEntity", b =>
                {
                    b.Navigation("DictionaryItemValues");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogModule.Data.Model.PropertyEntity", b =>
                {
                    b.Navigation("DictionaryItems");

                    b.Navigation("DisplayNames");

                    b.Navigation("PropertyAttributes");

                    b.Navigation("ValidationRules");
                });
#pragma warning restore 612, 618
        }
    }
}
