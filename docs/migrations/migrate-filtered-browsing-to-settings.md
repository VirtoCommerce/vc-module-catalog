# Migrate FilteredBrowsing from Dynamic Property to Store Settings

## Why

Before this change, the per-store faceted-browsing configuration (the list of attribute, range and price-range filters returned by
`IBrowseFilterService`) was stored as a JSON-typed dynamic property called `FilteredBrowsing` on the `Store` entity.

That had two drawbacks:

1. **Admins can delete it.** Because it appeared in the platform's  *Dynamic Properties* admin UI, an administrator could remove the
   property registration or its value with one click and silently wipe a store's facet configuration.
2. **Inconsistency.** Every other piece of catalog configuration scoped to a store (Brands, search options, etc.) lives in `Store.Settings`.
   `FilteredBrowsing` was the odd one out.

The configuration now lives in the new store-scoped setting `Catalog.BrowseFilters.FilteredBrowsing` (value type `Json`). The
behavior of `IBrowseFilterService` and the AngularJS *Filtering properties* widget is unchanged.

## What the migration does

For every store whose legacy `FilteredBrowsing` dynamic property has a non-empty value, the migration:

1. Inserts a row into `PlatformSetting` with 
   * `Name = 'Catalog.BrowseFilters.FilteredBrowsing'`
   * `ObjectType = 'Store'`
   * `ObjectId = <store id>`
2. Inserts a row into `PlatformSettingValue` with
   * `ValueType = 'Json'`
   * `LongTextValue = <legacy JSON value>`
   * `SettingId = <id of the row from step 1>`
3. Inserts the global flag setting
   `Catalog.BrowseFilters.FilteredBrowsingMigrated = true`
   (`ObjectType` / `ObjectId` `NULL`). The runtime startup task in the
   catalog module reads this flag and skips its scan once it is set.

The original `StoreDynamicPropertyObjectValue` rows are left in place
so an older build of the module can still read them if you need to
roll back. A future cleanup migration can drop them once all
deployments are past this version.

The whole body is wrapped in an existence check for
`StoreDynamicPropertyObjectValue`, `PlatformSetting`, and
`PlatformSettingValue`, so it is a no-op on databases where one of
them is missing (e.g. a fresh install with no store data yet).

## How partners run it

There are two equivalent ways. 

### Option 1 — Install/upgrade the module (recommended)

When you install or upgrade `VirtoCommerce.CatalogModule` to a version that includes this change, the platform applies migration once automatically as part of module startup.

A runtime startup task (`FilteredBrowsingMigrationStartupTask`) runs after `Database.Migrate()`.

You don't have to do anything else.

### Option 2 — Run the SQL script manually

If you prefer to run the migration yourself before upgrading, or if your deployment splits the platform DB across multiple connection
strings, run the script for your database engine against the database that owns `PlatformSetting`:


| Provider | Script |
|----------|--------|
| SQL Server | [`scripts/migrate-filtered-browsing.sqlserver.sql`](./scripts/migrate-filtered-browsing.sqlserver.sql) |
| MySQL 8.0+ | [`scripts/migrate-filtered-browsing.mysql.sql`](./scripts/migrate-filtered-browsing.mysql.sql) |
| PostgreSQL 13+ | [`scripts/migrate-filtered-browsing.postgresql.sql`](./scripts/migrate-filtered-browsing.postgresql.sql) |

Each script is the same logic shipped in the EF migration above, but in a form you can paste into SSMS, MySQL Workbench, `psql`, DBeaver, etc.
They are safe to re-run.

## Verification

After running the migration, the count of new settings should match the count of legacy dynamic-property values.

## Rollback

The migration only inserts rows. To roll back manually, delete the new
settings and the global flag:

```sql
DELETE FROM PlatformSettingValue
WHERE SettingId IN (
    SELECT Id FROM PlatformSetting
    WHERE Name IN ('Catalog.BrowseFilters.FilteredBrowsing',
                   'Catalog.BrowseFilters.FilteredBrowsingMigrated'));

DELETE FROM PlatformSetting
WHERE Name IN ('Catalog.BrowseFilters.FilteredBrowsing',
               'Catalog.BrowseFilters.FilteredBrowsingMigrated');
```

The legacy dynamic-property rows are untouched, so rolling the module
back to version restores the previous behavior with no data loss.
