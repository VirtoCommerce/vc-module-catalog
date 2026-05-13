-- =============================================================================
-- Migrate per-store FilteredBrowsing configuration from the legacy
-- "FilteredBrowsing" dynamic property into the store-scoped setting
-- "Catalog.BrowseFilters.FilteredBrowsing".
--
-- Target  : PostgreSQL 13+ (uses gen_random_uuid())
-- Module  : VirtoCommerce.CatalogModule
-- Ticket  : VCST-4173
--
-- Safe to re-run: every INSERT is gated by NOT EXISTS, so a second execution
-- is a no-op on rows that have already been migrated.
--
-- Authoritative copy (run automatically via EF Core migration):
--   src/VirtoCommerce.CatalogModule.Data.PostgreSql/Migrations/
--       20260504120200_MigrateFilteredBrowsingToSettings.cs
-- =============================================================================

DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'StoreDynamicPropertyObjectValue' AND table_schema = current_schema())
       AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'PlatformSetting' AND table_schema = current_schema())
       AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'PlatformSettingValue' AND table_schema = current_schema()) THEN

        -- 1) Create per-store PlatformSetting rows for any store that still has a legacy DP value
        --    and does not already have the new setting.
        INSERT INTO "PlatformSetting" ("Id", "Name", "ObjectType", "ObjectId", "CreatedDate", "ModifiedDate", "CreatedBy", "ModifiedBy")
        SELECT gen_random_uuid()::text, 'Catalog.BrowseFilters.FilteredBrowsing', 'Store', sdpov."ObjectId", (now() AT TIME ZONE 'utc'), NULL, 'FilteredBrowsingMigration', NULL
        FROM "StoreDynamicPropertyObjectValue" sdpov
        WHERE sdpov."PropertyName" = 'FilteredBrowsing'
          AND sdpov."ObjectType" = 'VirtoCommerce.StoreModule.Core.Model.Store'
          AND sdpov."LongTextValue" IS NOT NULL
          AND char_length(sdpov."LongTextValue") > 0
          AND NOT EXISTS (
              SELECT 1 FROM "PlatformSetting" ps
              WHERE ps."Name" = 'Catalog.BrowseFilters.FilteredBrowsing'
                AND ps."ObjectType" = 'Store'
                AND ps."ObjectId" = sdpov."ObjectId");

        -- 2) Create the matching PlatformSettingValue row for each newly created PlatformSetting.
        INSERT INTO "PlatformSettingValue" ("Id", "ValueType", "ShortTextValue", "LongTextValue", "DecimalValue", "IntegerValue", "BooleanValue", "DateTimeValue", "SettingId", "CreatedDate", "ModifiedDate", "CreatedBy", "ModifiedBy")
        SELECT gen_random_uuid()::text, 'Json', NULL, sdpov."LongTextValue", 0, 0, false, NULL, ps."Id", (now() AT TIME ZONE 'utc'), NULL, 'FilteredBrowsingMigration', NULL
        FROM "PlatformSetting" ps
        INNER JOIN "StoreDynamicPropertyObjectValue" sdpov
                ON sdpov."ObjectId" = ps."ObjectId"
               AND sdpov."PropertyName" = 'FilteredBrowsing'
               AND sdpov."ObjectType" = 'VirtoCommerce.StoreModule.Core.Model.Store'
        WHERE ps."Name" = 'Catalog.BrowseFilters.FilteredBrowsing'
          AND ps."ObjectType" = 'Store'
          AND NOT EXISTS (SELECT 1 FROM "PlatformSettingValue" psv WHERE psv."SettingId" = ps."Id");

        -- 3) Mark the migration as completed at the global scope (ObjectType / ObjectId NULL).
        --    The runtime startup task reads this flag and skips its scan when it is set.
        INSERT INTO "PlatformSetting" ("Id", "Name", "ObjectType", "ObjectId", "CreatedDate", "ModifiedDate", "CreatedBy", "ModifiedBy")
        SELECT gen_random_uuid()::text, 'Catalog.BrowseFilters.FilteredBrowsingMigrated', NULL, NULL, (now() AT TIME ZONE 'utc'), NULL, 'FilteredBrowsingMigration', NULL
        WHERE NOT EXISTS (
            SELECT 1 FROM "PlatformSetting"
            WHERE "Name" = 'Catalog.BrowseFilters.FilteredBrowsingMigrated'
              AND "ObjectType" IS NULL
              AND "ObjectId" IS NULL);

        INSERT INTO "PlatformSettingValue" ("Id", "ValueType", "ShortTextValue", "LongTextValue", "DecimalValue", "IntegerValue", "BooleanValue", "DateTimeValue", "SettingId", "CreatedDate", "ModifiedDate", "CreatedBy", "ModifiedBy")
        SELECT gen_random_uuid()::text, 'Boolean', NULL, NULL, 0, 0, true, NULL, ps."Id", (now() AT TIME ZONE 'utc'), NULL, 'FilteredBrowsingMigration', NULL
        FROM "PlatformSetting" ps
        WHERE ps."Name" = 'Catalog.BrowseFilters.FilteredBrowsingMigrated'
          AND ps."ObjectType" IS NULL
          AND ps."ObjectId" IS NULL
          AND NOT EXISTS (SELECT 1 FROM "PlatformSettingValue" psv WHERE psv."SettingId" = ps."Id");

    END IF;
END $$;

-- Verification: the count below should equal the number of stores that had a
-- non-empty legacy FilteredBrowsing dynamic-property value.
-- SELECT COUNT(*) AS "MigratedStores"
-- FROM "PlatformSetting" ps
-- INNER JOIN "PlatformSettingValue" psv ON psv."SettingId" = ps."Id"
-- WHERE ps."Name" = 'Catalog.BrowseFilters.FilteredBrowsing' AND ps."ObjectType" = 'Store';
