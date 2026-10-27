import { DiagnosticsClient } from "@vc-shell/framework";
import { ref } from "vue";
import {
  AutomaticLinkQueryClient,
  CatalogModuleAssociationsClient,
  CatalogModuleBarcodeSearchClient,
  CatalogModuleBrandSettingClient,
  CatalogModuleCatalogsClient,
  CatalogModuleCategoriesClient,
  CatalogModuleConfigurationsClient,
  CatalogModuleListEntryClient,
  CatalogModuleMeasuresClient,
  CatalogModuleProductsClient,
  CatalogModuleProductSortingClient,
  CatalogModulePropertiesClient,
  CatalogModulePropertyDictionaryItemsClient,
  CatalogModulePropertyGroupsClient,
  CatalogModuleVideosClient,
  type Catalog,
  type ListEntryBase,
  type Property,
} from "../api_client/virtocommerce.catalog";

// The generated clients are plain fetch wrappers (same-origin cookie auth), so one instance each is enough.
export const api = {
  catalogs: new CatalogModuleCatalogsClient(),
  categories: new CatalogModuleCategoriesClient(),
  products: new CatalogModuleProductsClient(),
  listEntries: new CatalogModuleListEntryClient(),
  properties: new CatalogModulePropertiesClient(),
  dictionary: new CatalogModulePropertyDictionaryItemsClient(),
  propertyGroups: new CatalogModulePropertyGroupsClient(),
  associations: new CatalogModuleAssociationsClient(),
  measures: new CatalogModuleMeasuresClient(),
  videos: new CatalogModuleVideosClient(),
  configurations: new CatalogModuleConfigurationsClient(),
  autoLinks: new AutomaticLinkQueryClient(),
  brandSettings: new CatalogModuleBrandSettingClient(),
  sortings: new CatalogModuleProductSortingClient(),
  barcode: new CatalogModuleBarcodeSearchClient(),
};

/** Small JSON fetch for endpoints of other modules (stores, members, inventory, bulk actions, assets...). */
export async function http<T = unknown>(method: string, url: string, body?: unknown): Promise<T> {
  const res = await fetch(url, {
    method,
    headers: body !== undefined ? { "Content-Type": "application/json" } : {},
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });
  if (!res.ok) throw new Error(`${method} ${url}: ${res.status} ${await res.text()}`);
  const text = await res.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

let installed: Promise<string[]> | undefined;
/** Ids of the installed platform modules (bulk actions, export, search are optional). */
export function installedModules(): Promise<string[]> {
  installed ??= new DiagnosticsClient()
    .getSystemInfo()
    .then((x) => (x.installedModules ?? []).map((m) => m.id!))
    .catch(() => []);
  return installed;
}

const settingCache: Record<string, Promise<string[]>> = {};
/** Allowed values of a global platform setting (languages, tax types, measure units...); settings/values needs no setting permission. */
export function settingValues(name: string): Promise<string[]> {
  settingCache[name] ??= http<unknown[] | null>("GET", `/api/platform/settings/values/${encodeURIComponent(name)}`)
    .then((values) => (values ?? []).map(String))
    .catch(() => []);
  return settingCache[name];
}

export interface Store {
  id: string;
  name?: string;
  catalog?: string;
  languages?: string[];
  defaultLanguage?: string;
}
let stores: Promise<Store[]> | undefined;
export function getStores(): Promise<Store[]> {
  stores ??= http<{ results?: Store[] }>("POST", "/api/stores/search", { take: 1000 })
    .then((x) => x.results ?? [])
    .catch(() => []);
  return stores;
}

/** Folder the legacy UI uploads catalog images into: catalog/ + catalogImagesFolderPathHelper (images-add.js). */
export function assetsFolder(catalogId?: string, code?: string) {
  return `catalog/${(catalogId ?? "").substring(0, 5)}/${code || "new"}`;
}

export const options = (values: string[]) => values.map((v) => ({ id: v, title: v }));
/** A setting's allowed values as select options. */
export const settingOptions = (name: string) => settingValues(name).then(options);

/** The catalog's language codes, by priority (the first is the main one). */
export const catalogLanguages = (catalog?: Catalog) =>
  [...(catalog?.languages ?? [])].sort((a, b) => (a.priority ?? 0) - (b.priority ?? 0)).map((l) => l.languageCode!);

/** A URL slug of a name (legacy getslug). */
export const slug = (s?: unknown) =>
  String(s ?? "")
    .toLowerCase()
    .trim()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-|-$/g, "");

/** Entries cut in a catalog list, pasted (moved) into another category (legacy $sessionStorage.catalogClipboardContent). */
export const clipboard = ref<ListEntryBase[]>([]);

/** Deep copy that also works on reactive proxies and blade options (structuredClone rejects proxies). */
export const clone = <T>(x: T): T => (x === undefined ? x : JSON.parse(JSON.stringify(x)));

/** Fresh property definitions (a child blade changed them) that keep the values being edited in the form. */
export const keepValues = (fresh?: Property[], current?: Property[]) =>
  fresh?.map((p) => ({ ...p, values: current?.find((c) => c.id === p.id)?.values ?? p.values }));

/**
 * Display URL of a catalog image/asset. The API returns absolute URLs when the asset provider has a public URL;
 * a relative one ("assets/...") resolved against the platform root in the legacy admin, but would resolve against
 * /apps/admin-shell/ here, so it is made root-relative. Only for display: saved data keeps the API value.
 */
export function assetUrl(url?: string | null) {
  if (!url || /^([a-z][a-z0-9+.-]*:|\/)/i.test(url)) return url ?? undefined;
  return "/" + url;
}

/** Saved catalog search filters (legacy filter-detail; per browser, like the legacy $localStorage). */
export interface SavedFilter {
  id: string;
  name: string;
  keyword?: string;
  searchInVariations?: boolean;
}
const FILTERS_KEY = "catalog.searchFilters";
export function savedFilters(): SavedFilter[] {
  try {
    return JSON.parse(localStorage.getItem(FILTERS_KEY) ?? "[]") as SavedFilter[];
  } catch {
    return [];
  }
}
export function storeFilters(filters: SavedFilter[]) {
  try {
    localStorage.setItem(FILTERS_KEY, JSON.stringify(filters));
  } catch {
    // per-browser convenience only
  }
}

/** string[] <-> VcMultivalue items (free-text chips are labelled by `title`) */
export const toChips = (values?: string[] | null) => options(values ?? []) as { id?: string }[];
export const fromChips = (items: object[]) =>
  items.map((x) => String((x as { title?: unknown; id?: unknown }).title ?? (x as { id?: unknown }).id ?? "")).filter(Boolean);

/**
 * A detail blade's Apply: replaces the row it was opened for (`target`, found by reference, else by id) or appends a
 * new one. Rows can be deleted or reordered while the details are open, so their index at open time can't be trusted;
 * a row deleted meanwhile stays deleted.
 */
export function applyRow<T extends { id?: string }>(list: T[], target: T | undefined, value: T): T[] {
  return target ? list.map((x) => (x === target || (!!target.id && x.id === target.id) ? value : x)) : [...list, value];
}

/** Rows of a local list for a table: `_index` points back into the list, unsaved records get a stand-in id. */
export const indexRows = <T extends { id?: string }>(list: T[]) =>
  list.map((x, i) => ({ ...x, _index: i, id: x.id ?? `new-${i}` }));

/** The list without the selected rows (matched by `_index`). */
export function withoutRows<T>(list: T[], rows: { _index: number }[]): T[] {
  const drop = new Set(rows.map((r) => r._index));
  return list.filter((_, i) => !drop.has(i));
}

/** Every entry of a list search, page by page: a picker's "select all" covers all pages, not just the shown one. */
export async function searchAllEntries(criteria: Parameters<typeof api.listEntries.listItemsSearch>[0]) {
  const all: (ListEntryBase & { productType?: string })[] = [];
  for (;;) {
    const res = await api.listEntries.listItemsSearch({ ...criteria, skip: all.length, take: 1000 });
    all.push(...(res.results ?? []));
    if (!res.results?.length || all.length >= (res.totalCount ?? 0)) return all;
  }
}
