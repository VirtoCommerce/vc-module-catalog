-- =============================================================================
-- Migrate per-store FilteredBrowsing configuration from the legacy
-- "FilteredBrowsing" dynamic property into the store-scoped setting
-- "Catalog.BrowseFilters.FilteredBrowsing".
--
-- Target  : MySQL 8.0+
-- Module  : VirtoCommerce.CatalogModule
-- Ticket  : VCST-4173
--
-- Safe to re-run: every INSERT is gated by NOT EXISTS, so a second execution
-- is a no-op on rows that have already been migrated.
--
-- Authoritative copy (run automatically via EF Core migration):
--   src/VirtoCommerce.CatalogModule.Data.MySql/Migrations/
--       20260504120100_MigrateFilteredBrowsingToSettings.cs
-- =============================================================================

DROP PROCEDURE IF EXISTS `vc_catalog_migrate_filtered_browsing`;

CREATE PROCEDURE `vc_catalog_migrate_filtered_browsing`()
BEGIN
    IF (SELECT COUNT(*) FROM information_schema.tables
        WHERE table_schema = DATABASE()
          AND table_name IN ('StoreDynamicPropertyObjectValue', 'PlatformSetting', 'PlatformSettingValue')) = 3 THEN

        -- 1) Create per-store PlatformSetting rows for any store that still has a legacy DP value
        --    and does not already have the new setting.
        INSERT INTO `PlatformSetting` (`Id`, `Name`, `ObjectType`, `ObjectId`, `CreatedDate`, `ModifiedDate`, `CreatedBy`, `ModifiedBy`)
        SELECT UUID(), 'Catalog.BrowseFilters.FilteredBrowsing', 'Store', sdpov.`ObjectId`, UTC_TIMESTAMP(6), NULL, 'FilteredBrowsingMigration', NULL
        FROM `StoreDynamicPropertyObjectValue` sdpov
        WHERE sdpov.`PropertyName` = 'FilteredBrowsing'
          AND sdpov.`ObjectType` = 'VirtoCommerce.StoreModule.Core.Model.Store'
          AND sdpov.`LongTextValue` IS NOT NULL
          AND CHAR_LENGTH(sdpov.`LongTextValue`) > 0
          AND NOT EXISTS (
              SELECT 1 FROM `PlatformSetting` ps
              WHERE ps.`Name` = 'Catalog.BrowseFilters.FilteredBrowsing'
                AND ps.`ObjectType` = 'Store'
                AND ps.`ObjectId` = sdpov.`ObjectId`);

        -- 2) Create the matching PlatformSettingValue row for each newly created PlatformSetting.
        INSERT INTO `PlatformSettingValue` (`Id`, `ValueType`, `ShortTextValue`, `LongTextValue`, `DecimalValue`, `IntegerValue`, `BooleanValue`, `DateTimeValue`, `SettingId`, `CreatedDate`, `ModifiedDate`, `CreatedBy`, `ModifiedBy`)
        SELECT UUID(), 'Json', NULL, sdpov.`LongTextValue`, 0, 0, 0, NULL, ps.`Id`, UTC_TIMESTAMP(6), NULL, 'FilteredBrowsingMigration', NULL
        FROM `PlatformSetting` ps
        INNER JOIN `StoreDynamicPropertyObjectValue` sdpov
                ON sdpov.`ObjectId` = ps.`ObjectId`
               AND sdpov.`PropertyName` = 'FilteredBrowsing'
               AND sdpov.`ObjectType` = 'VirtoCommerce.StoreModule.Core.Model.Store'
        WHERE ps.`Name` = 'Catalog.BrowseFilters.FilteredBrowsing'
          AND ps.`ObjectType` = 'Store'
          AND NOT EXISTS (SELECT 1 FROM `PlatformSettingValue` psv WHERE psv.`SettingId` = ps.`Id`);

        -- 3) Mark the migration as completed at the global scope (ObjectType / ObjectId NULL).
        --    The runtime startup task reads this flag and skips its scan when it is set.
        INSERT INTO `PlatformSetting` (`Id`, `Name`, `ObjectType`, `ObjectId`, `CreatedDate`, `ModifiedDate`, `CreatedBy`, `ModifiedBy`)
        SELECT UUID(), 'Catalog.BrowseFilters.FilteredBrowsingMigrated', NULL, NULL, UTC_TIMESTAMP(6), NULL, 'FilteredBrowsingMigration', NULL
        FROM dual
        WHERE NOT EXISTS (
            SELECT 1 FROM `PlatformSetting`
            WHERE `Name` = 'Catalog.BrowseFilters.FilteredBrowsingMigrated'
              AND `ObjectType` IS NULL
              AND `ObjectId` IS NULL);

        INSERT INTO `PlatformSettingValue` (`Id`, `ValueType`, `ShortTextValue`, `LongTextValue`, `DecimalValue`, `IntegerValue`, `BooleanValue`, `DateTimeValue`, `SettingId`, `CreatedDate`, `ModifiedDate`, `CreatedBy`, `ModifiedBy`)
        SELECT UUID(), 'Boolean', NULL, NULL, 0, 0, 1, NULL, ps.`Id`, UTC_TIMESTAMP(6), NULL, 'FilteredBrowsingMigration', NULL
        FROM `PlatformSetting` ps
        WHERE ps.`Name` = 'Catalog.BrowseFilters.FilteredBrowsingMigrated'
          AND ps.`ObjectType` IS NULL
          AND ps.`ObjectId` IS NULL
          AND NOT EXISTS (SELECT 1 FROM `PlatformSettingValue` psv WHERE psv.`SettingId` = ps.`Id`);

    END IF;
END;

CALL `vc_catalog_migrate_filtered_browsing`();

DROP PROCEDURE IF EXISTS `vc_catalog_migrate_filtered_browsing`;

-- Verification: the count below should equal the number of stores that had a
-- non-empty legacy FilteredBrowsing dynamic-property value.
-- SELECT COUNT(*) AS MigratedStores
-- FROM `PlatformSetting` ps
-- INNER JOIN `PlatformSettingValue` psv ON psv.`SettingId` = ps.`Id`
-- WHERE ps.`Name` = 'Catalog.BrowseFilters.FilteredBrowsing' AND ps.`ObjectType` = 'Store';
