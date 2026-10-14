<template>
  <VcBlade
    :title="title"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <div class="catalog-list-blade">
      <div class="catalog-browse__crumbs">
        <VcBreadcrumbs
          :items="crumbs"
          separated
        />
      </div>
      <VcDataTable
        v-model:search-value="keyword"
        v-model:selection="selection"
        v-model:sort-field="sortField"
        v-model:sort-order="sortOrder"
        v-model:select-all-active="allSelected"
        class="catalog-browse__table"
        selection-mode="multiple"
        :items="entries"
        :total-count="totalCount"
        :pagination="pagination"
        :searchable="true"
        :search-placeholder="$t('CATALOG.BROWSE.SEARCH')"
        :global-filters="pick || global ? [] : globalFilters"
        :state-key="pick ? 'CATALOG_PICK' : global ? 'CATALOG_SEARCH' : 'CATALOG_BROWSE'"
        :active-item-id="activeItemId"
        :show-all-columns="expanded"
        :row-actions="pick || (!catalogId && !global) ? undefined : rowActions"
        :empty-state="{ icon: 'lucide-folder-open', title: $t('CATALOG.BROWSE.EMPTY') }"
        @row-click="onRowClick"
        @pagination-click="pagination.goToPage"
        @filter="onFilter"
      >
        <VcColumn
          id="imageUrl"
          :title="$t('CATALOG.FIELDS.IMAGE')"
          type="image"
          width="84px"
          always-visible
        />
        <VcColumn
          id="name"
          :title="$t('CATALOG.FIELDS.NAME')"
          :sortable="!!catalogId"
          always-visible
        >
          <template #body="{ data }">
            <span class="catalog-browse__name">
              <VcIcon
                v-if="data.type !== 'product'"
                :icon="data.type === 'catalog' ? 'lucide-library' : 'lucide-folder'"
                size="s"
              />
              {{ data.name }}
            </span>
          </template>
        </VcColumn>
        <VcColumn
          id="code"
          :title="$t('CATALOG.FIELDS.CODE')"
          :sortable="!!catalogId"
        />
        <VcColumn
          v-if="global"
          id="pathText"
          :title="$t('CATALOG.FIELDS.PATH')"
        />
        <VcColumn
          id="type"
          :title="$t('CATALOG.FIELDS.TYPE')"
        >
          <template #body="{ data }">{{ typeLabel(data) }}</template>
        </VcColumn>
        <VcColumn
          id="isActive"
          :title="$t('CATALOG.FIELDS.STATUS')"
        >
          <template #body="{ data }">
            <div class="tw-inline-flex tw-gap-1">
              <VcStatus
                v-for="label in labels(data)"
                :key="label.text"
                :variant="label.variant"
              >
                {{ label.text }}
              </VcStatus>
            </div>
          </template>
        </VcColumn>
      </VcDataTable>
    </div>
  </VcBlade>
</template>

<script setup lang="ts">
/**
 * Catalog list (categories + products of a catalog/category); also the catalog/category picker (options.pick)
 * and the cross-catalog search (options.global).
 *
 * Extension point "catalog-items-toolbar": other modules add toolbar commands to this list
 * (legacy: catalogImportService/catalogExportService registrations, toolbarService on categoriesItemsListController).
 *
 *   useExtensionPoint("catalog-items-toolbar").add({
 *     id: "my-command",
 *     priority: 10,                          // order among the added commands
 *     component: markRaw({ render: () => null }), // required by the framework type, not rendered
 *     meta: {
 *       title: "MYMODULE.COMMAND",           // i18n key or text
 *       icon: "lucide-download",
 *       permissions: "catalog:export",       // optional, string or string[] (any of)
 *       isVisible: (ctx) => !ctx.catalog?.isVirtual, // optional
 *       disabled: (ctx) => !ctx.selection.length,    // optional
 *       onClick: (ctx) => ctx.openBlade({ name: "MyBlade", options: { catalog: ctx.catalog } }),
 *     } satisfies CatalogToolbarCommand,
 *   });
 *
 * ctx (CatalogToolbarContext):
 *   catalog        the browsed catalog (Catalog)
 *   category       the current category { id, name } or undefined at the catalog root
 *   selection      the selected rows (list entries: id, type "category" | "product", name, code, catalogId...)
 *   allSelected    true when "select all" covers every page of the list
 *   totalCount     the number of rows in the list (all pages)
 *   searchCriteria the list's search (catalogId, categoryId, keyword, searchInVariations, responseGroup)
 *   propertyEditor Catalog's property values editor component (props: entity { properties }, types: property
 *                  owner types, header, languages), for commands that edit property values (Bulk actions)
 *   openBlade      opens a blade as a child of this list
 *   reload         reloads the list
 */
import {
  defineExtensionPoint,
  getBladeConfig,
  ModulesReadyKey,
  useBlade,
  useDataTablePagination,
  useDataTableSort,
  useFunctions,
  useLatestRequest,
  usePermissions,
  usePopup,
  type IBladeToolbar,
} from "@vc-shell/framework";
import { VcBlade, VcBreadcrumbs, VcColumn, VcDataTable, VcIcon, VcStatus } from "@vc-shell/framework/ui";
import { computed, inject, markRaw, ref, watch, type Component } from "vue";
import { useI18n } from "vue-i18n";
import { api, assetUrl, clipboard, savedFilters, type SavedFilter } from "../api";
import PropertyValuesEditor from "../components/PropertyValues.vue";
import type { Catalog, CatalogListEntrySearchCriteria, ListEntryBase } from "../../api_client/virtocommerce.catalog";

defineBlade({ url: "/catalog-browse", name: "CatalogBrowse" });

export interface CatalogToolbarContext {
  catalog?: Catalog;
  category?: { id: string; name?: string };
  selection: ListEntryBase[];
  allSelected: boolean;
  totalCount: number;
  searchCriteria: CatalogListEntrySearchCriteria;
  propertyEditor: Component;
  openBlade: ReturnType<typeof useBlade>["openBlade"];
  reload: () => Promise<void>;
}
export interface CatalogToolbarCommand {
  title: string;
  icon?: string;
  permissions?: string | string[];
  isVisible?: (ctx: CatalogToolbarContext) => boolean;
  disabled?: (ctx: CatalogToolbarContext) => boolean;
  onClick: (ctx: CatalogToolbarContext) => unknown;
}
const extensions = defineExtensionPoint<CatalogToolbarCommand>("catalog-items-toolbar", {
  description: "Toolbar commands of the catalog items list",
});

type Entry = ListEntryBase & { productType?: string };
interface Folder {
  id: string;
  name?: string;
  /** a real category linked into this virtual catalog, or one inside it (legacy isBrowsingLinkedCategory) */
  linked?: boolean;
}
export interface PickOptions {
  title?: string;
  /** "entries": pick selected rows; "folder": pick the current catalog/category */
  mode: "entries" | "folder";
  /** limit the selectable rows (e.g. ["product"]) */
  types?: string[];
  /** leave products of this type out of the list (legacy excludeProductType) */
  excludeProductType?: string;
  onPick: (
    entries: Entry[],
    folder: { catalogId?: string; categoryId?: string; catalog?: Catalog },
    /** set when "select all" covers every page: the search behind the selection (legacy bulkcreatelinks) */
    all?: CatalogListEntrySearchCriteria,
  ) => unknown;
}

const { t } = useI18n({ useScope: "global" });
const { param, options, openBlade, closeSelf, closeChildren, exposeToChildren, expanded } = useBlade<{
  catalog?: Catalog;
  categoryId?: string;
  categoryName?: string;
  keyword?: string;
  pick?: PickOptions;
  /** search across all catalogs (legacy catalogs-list keyword search) */
  global?: boolean;
}>();
const { showConfirmation } = usePopup();
const { debounce } = useFunctions();
const { hasAccess } = usePermissions();
// legacy categoriesItemsListController canDelete / canCutPaste
const canDelete = () => hasAccess(["catalog:delete", "catalog:products:delete", "catalog:categories:delete"]);
const canCutPaste = () => hasAccess(["catalog:update", "catalog:products:update", "catalog:categories:update"]);

const pick = computed(() => options.value?.pick);
const global = computed(() => !!options.value?.global);
const catalog = ref<Catalog | undefined>(options.value?.catalog);
const catalogId = ref<string | undefined>(param.value ?? catalog.value?.id);
const path = ref<Folder[]>(
  options.value?.categoryId ? [{ id: options.value.categoryId, name: options.value.categoryName }] : [],
);
const categoryId = computed(() => path.value[path.value.length - 1]?.id);

const entries = ref<Entry[]>([]);
const totalCount = ref(0);
const loading = ref(false);
const selection = ref<Entry[]>([]);
const activeItemId = ref<string>();
const keyword = ref(options.value?.keyword ?? "");
const { sortField, sortOrder, sortExpression } = useDataTableSort();
const filter = ref<{ keyword?: string; searchInVariations?: boolean }>({});
const allSelected = ref(false);
const filters = ref<SavedFilter[]>(savedFilters());
// the Export module's UI is loaded (legacy: virtoCommerce.exportModule): its ExportSettings blade is registered
const modulesReady = inject(ModulesReadyKey, ref(true));
const exportAvailable = computed(() => modulesReady.value && !!getBladeConfig("ExportSettings"));

const PAGE_SIZE = 20;
const pagination = useDataTablePagination({ pageSize: PAGE_SIZE, totalCount });

const isVirtual = computed(() => !!catalog.value?.isVirtual);
// its entries belong to a real catalog: no add, delete, cut, paste or link here (legacy toolbarCommands.splice)
const browsingLinked = computed(() => !!path.value[path.value.length - 1]?.linked);
const title = computed(
  () =>
    pick.value?.title ??
    (global.value
      ? t("CATALOG.BROWSE.SEARCH_ALL")
      : path.value[path.value.length - 1]?.name || catalog.value?.name || t("CATALOG.CATALOGS.TITLE")),
);

const crumbs = computed(() => [
  ...(pick.value ? [{ id: "", title: t("CATALOG.CATALOGS.TITLE"), clickHandler: () => goTo(-1) }] : []),
  ...(catalog.value ? [{ id: catalog.value.id!, title: catalog.value.name, clickHandler: () => goTo(0) }] : []),
  ...path.value.map((f, i) => ({ id: f.id, title: f.name, clickHandler: () => goTo(i + 1) })),
]);

const globalFilters = computed(() => [
  {
    id: "preset",
    label: t("CATALOG.BROWSE.FILTER"),
    filter: {
      options: [
        { value: "variations", label: t("CATALOG.BROWSE.FILTERS.VARIATIONS") },
        { value: "is:hidden", label: t("CATALOG.BROWSE.FILTERS.HIDDEN") },
        { value: "is:priced", label: t("CATALOG.BROWSE.FILTERS.PRICED") },
        { value: "is:unpriced", label: t("CATALOG.BROWSE.FILTERS.UNPRICED") },
        ...filters.value.map((f) => ({ value: `saved:${f.id}`, label: f.name })),
      ],
    },
  },
]);

const activePreset = ref<string>();
function presetFilter(preset?: string) {
  const saved = filters.value.find((f) => `saved:${f.id}` === preset);
  if (saved) return { keyword: saved.keyword, searchInVariations: saved.searchInVariations };
  if (preset === "variations") return { searchInVariations: true };
  return preset ? { keyword: preset } : {};
}
function onFilter(event: { filters: Record<string, unknown> }) {
  const preset = event.filters.preset as string | undefined;
  activePreset.value = preset;
  filter.value = presetFilter(preset);
  pagination.setPage(1);
  load();
}

// drill-down resets the page and reloads; a page watcher may load too, so only the newest response is kept
const search = useLatestRequest();
async function load() {
  loading.value = true;
  try {
    if (!catalogId.value && !global.value) {
      // picker root: the catalogs themselves
      const catalogRes = await search.latest(api.catalogs.searchCatalogs({ keyword: keyword.value || undefined, take: 1000, sort: "name" }));
      if (!catalogRes) return;
      entries.value = (catalogRes.results ?? []).map((c) => ({ ...c, type: "catalog", isActive: true }) as Entry);
      totalCount.value = entries.value.length;
      return;
    }
    if (global.value && !keyword.value) {
      search.invalidate();
      entries.value = [];
      totalCount.value = 0;
      return;
    }
    const res = await search.latest(api.listEntries.listItemsSearch({ ...criteria(), skip: pagination.skip, take: PAGE_SIZE }));
    if (!res) return;
    entries.value = (res.results ?? []).map((e) => ({
      ...e,
      imageUrl: assetUrl(e.imageUrl),
      pathText: (e.path ?? []).join(" / "),
    })) as Entry[];
    totalCount.value = res.totalCount ?? 0;
  } finally {
    // a newer request still running keeps the spinner
    if (!search.pending.value) loading.value = false;
  }
}

const PropertyValues = markRaw(PropertyValuesEditor);
const toolbarContext = (): CatalogToolbarContext => ({
  catalog: catalog.value,
  category: path.value[path.value.length - 1],
  selection: selection.value,
  allSelected: allSelected.value,
  totalCount: totalCount.value,
  searchCriteria: criteria(),
  propertyEditor: PropertyValues,
  openBlade,
  reload: load,
});

/** the search behind the list (also used for "select all" bulk operations) */
function criteria(): CatalogListEntrySearchCriteria {
  const words = [filter.value.keyword, keyword.value].filter(Boolean).join(" ");
  return {
    catalogId: catalogId.value,
    categoryId: categoryId.value,
    keyword: words || undefined,
    searchInVariations: filter.value.searchInVariations ?? false,
    excludeProductType: pick.value?.excludeProductType,
    sort: sortExpression.value ?? (words ? "relevance" : undefined),
    responseGroup: "withCategories, withProducts",
  };
}

// the list needs only the catalog id: the catalog loads alongside it
async function init() {
  await Promise.all([
    catalogId.value && !catalog.value ? api.catalogs.getCatalog(catalogId.value).then((c) => (catalog.value = c)) : undefined,
    load(),
  ]);
}
init();
watch(keyword, () => pagination.setPage(1));
watch([keyword, sortExpression, () => pagination.skip], debounce(load, 300));

async function navigate() {
  selection.value = [];
  keyword.value = "";
  pagination.setPage(1);
  if (!pick.value) await closeChildren();
  await load();
}

/** -1 = catalogs root (picker), 0 = catalog root, n = n-th category of the path */
function goTo(level: number) {
  if (level < 0) {
    catalog.value = undefined;
    catalogId.value = undefined;
    path.value = [];
  } else path.value = path.value.slice(0, level);
  navigate();
}

function onRowClick(event: { data: Entry }) {
  const e = event.data;
  if (e.type === "catalog") {
    catalog.value = e as Catalog;
    catalogId.value = e.id;
    path.value = [];
    navigate();
  } else if (e.type === "category" && global.value) {
    // a search result opens its own catalog list at that category (legacy opens a new browse level)
    openBlade({ name: "CatalogBrowse", param: e.catalogId, options: { categoryId: e.id, categoryName: e.name } });
  } else if (e.type === "category") {
    const linked = browsingLinked.value || (isVirtual.value && !!linkHere(e));
    path.value = [...path.value, { id: e.id!, name: e.name, linked }];
    navigate();
  } else if (!pick.value) {
    openProduct(e.id);
  }
}

function openProduct(id?: string, extra: Record<string, unknown> = {}) {
  activeItemId.value = id;
  openBlade({
    name: "CatalogProductDetails",
    param: id,
    options: {
      catalogId: catalogId.value,
      categoryId: categoryId.value,
      // in a virtual catalog the product's priority is the priority of its link here (legacy _priority)
      ...(isVirtual.value ? { linkCatalogId: catalogId.value, linkCategoryId: categoryId.value } : {}),
      ...extra,
    },
    onClose: () => (activeItemId.value = undefined),
  });
}

function openCategory(id?: string) {
  activeItemId.value = id;
  openBlade({
    name: "CatalogCategoryDetails",
    param: id,
    options: { catalogId: catalogId.value, parentId: categoryId.value, isVirtual: isVirtual.value },
    onClose: () => (activeItemId.value = undefined),
  });
}

function manage() {
  if (categoryId.value) openCategory(categoryId.value);
  else openBlade({ name: "CatalogDetails", param: catalogId.value });
}

function typeLabel(e: Entry) {
  if (e.type === "product") return e.productType ?? "";
  return t(e.type === "catalog" ? "CATALOG.TYPES.CATALOG" : "CATALOG.TYPES.CATEGORY");
}

/** legacy labels: hidden category, manual/automatic link in a virtual catalog */
function labels(e: Entry) {
  const result: { text: string; variant: "info" | "warning" | "success" }[] = [];
  if (e.isActive === false) result.push({ text: t("CATALOG.BROWSE.HIDDEN"), variant: "warning" });
  if (isVirtual.value) {
    const link = linkHere(e);
    if (link) result.push({ text: t(link.isAutomatic ? "CATALOG.BROWSE.AUTO_LINK" : "CATALOG.BROWSE.LINK"), variant: "info" });
  }
  return result;
}
const linkHere = (e: Entry) =>
  e.links?.find((l) => l.catalogId === catalogId.value && (!categoryId.value || l.categoryId === categoryId.value));

async function remove(items: Entry[]) {
  // in a virtual catalog linked entries are unlinked, not deleted (legacy deleteList)
  const links = items.filter((e) => isVirtual.value && (e.type === "product" || linkHere(e)));
  const ids = items.filter((e) => !links.includes(e)).map((e) => e.id!);
  const message = t("CATALOG.BROWSE.DELETE_CONFIRM", { count: ids.length, links: links.length });
  if (!(await showConfirmation(message))) return;
  loading.value = true;
  try {
    await closeChildren();
    if (links.length)
      await api.listEntries.deleteLinks(
        links.map((e) => ({ listEntryId: e.id, listEntryType: e.type, catalogId: catalogId.value, categoryId: categoryId.value })),
      );
    if (ids.length) await api.listEntries.delete({ objectIds: ids });
  } finally {
    selection.value = [];
    await load();
  }
}

async function paste() {
  loading.value = true;
  try {
    await api.listEntries.move({ catalog: catalogId.value, category: categoryId.value, listEntries: clipboard.value });
    clipboard.value = [];
  } finally {
    await load();
  }
}

function linkItems() {
  openBlade({
    name: "CatalogBrowse",
    options: {
      pick: {
        title: t("CATALOG.BROWSE.LINK_TITLE"),
        mode: "entries",
        // only what the user may link (legacy canLinkListItem)
        types: [
          ...(hasAccess("catalog:categories:link") ? ["category"] : []),
          ...(hasAccess("catalog:products:link") ? ["product"] : []),
        ],
        onPick: async (picked: Entry[], _folder, all?: CatalogListEntrySearchCriteria) => {
          if (all) await api.listEntries.bulkCreateLinks({ searchCriteria: all, catalogId: catalogId.value, categoryId: categoryId.value });
          else
            await api.listEntries.createLinks(
              picked
                .filter((e) => e.type !== "catalog")
                .map((e) => ({ listEntryId: e.id, listEntryType: e.type, catalogId: catalogId.value, categoryId: categoryId.value })),
            );
          await load();
        },
      } satisfies PickOptions,
    },
  });
}

const rowActions = (e: Entry) => [
  ...(e.type === "category"
    ? [{ icon: "lucide-pencil", title: t("CATALOG.TOOLBAR.MANAGE"), type: "info" as const, clickHandler: () => openCategory(e.id) }]
    : []),
  { icon: "lucide-copy", title: t("CATALOG.BROWSE.COPY_ID"), type: "info" as const, clickHandler: () => navigator.clipboard.writeText(e.id!) },
  ...(e.type === "product" && e.code
    ? [{ icon: "lucide-barcode", title: t("CATALOG.BROWSE.COPY_SKU"), type: "info" as const, clickHandler: () => navigator.clipboard.writeText(e.code!) }]
    : []),
  ...(isVirtual.value || browsingLinked.value || !canCutPaste()
    ? []
    : [{ icon: "lucide-scissors", title: t("CATALOG.BROWSE.CUT"), type: "info" as const, clickHandler: () => (clipboard.value = [e]) }]),
  ...(browsingLinked.value || !canDelete()
    ? []
    : [{ icon: "lucide-trash-2", title: t("CATALOG.TOOLBAR.DELETE"), type: "danger" as const, clickHandler: () => remove([e]) }]),
];

const toolbar = computed<IBladeToolbar[]>(() => {
  if (pick.value) {
    const p = pick.value;
    const picked = selection.value.filter((e) => !p.types || p.types.includes(e.type!));
    const all = p.mode === "entries" && allSelected.value && !!catalogId.value;
    return [
      {
        id: "pick",
        title: t(p.mode === "folder" ? "CATALOG.BROWSE.PICK_FOLDER" : "CATALOG.BROWSE.PICK", { count: all ? totalCount.value : picked.length }),
        icon: "lucide-check",
        disabled: p.mode === "folder" ? !catalogId.value : !picked.length && !all,
        async clickHandler() {
          loading.value = true;
          try {
            await p.onPick(
              picked,
              { catalogId: catalogId.value, categoryId: categoryId.value, catalog: catalog.value },
              all ? criteria() : undefined,
            );
          } finally {
            loading.value = false;
          }
          await closeSelf();
        },
      },
      { id: "refresh", title: t("CATALOG.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: load },
    ];
  }
  const products = selection.value.filter((e) => e.type === "product");
  if (global.value) return [{ id: "refresh", title: t("CATALOG.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: load }];
  const ctx = toolbarContext();
  return [
    { id: "refresh", title: t("CATALOG.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: load },
    {
      id: "addCategory",
      title: t("CATALOG.BROWSE.ADD_CATEGORY"),
      icon: "lucide-folder-plus",
      isVisible: !browsingLinked.value,
      permissions: ["catalog:create", "catalog:categories:create"],
      clickHandler: () => openCategory(),
    },
    {
      id: "addProduct",
      title: t("CATALOG.BROWSE.ADD_PRODUCT"),
      icon: "lucide-plus",
      isVisible: !isVirtual.value,
      permissions: ["catalog:create", "catalog:products:create"],
      clickHandler: () => openProduct(undefined, { productType: "Physical" }),
    },
    {
      id: "link",
      title: t("CATALOG.BROWSE.LINK_ITEMS"),
      icon: "lucide-link",
      isVisible: isVirtual.value && !browsingLinked.value,
      permissions: ["catalog:categories:link", "catalog:products:link"],
      clickHandler: linkItems,
    },
    { id: "manage", title: t("CATALOG.TOOLBAR.MANAGE"), icon: "lucide-pencil", clickHandler: manage },
    {
      id: "delete",
      title: t("CATALOG.TOOLBAR.DELETE"),
      icon: "lucide-trash-2",
      isVisible: !browsingLinked.value,
      permissions: ["catalog:delete", "catalog:products:delete", "catalog:categories:delete"],
      disabled: !selection.value.length,
      clickHandler: () => remove(selection.value),
    },
    {
      id: "cut",
      title: t("CATALOG.BROWSE.CUT"),
      icon: "lucide-scissors",
      isVisible: !isVirtual.value,
      permissions: ["catalog:update", "catalog:products:update", "catalog:categories:update"],
      disabled: !selection.value.length,
      clickHandler: () => (clipboard.value = [...selection.value]),
    },
    {
      id: "paste",
      title: t("CATALOG.BROWSE.PASTE", { count: clipboard.value.length }),
      icon: "lucide-clipboard-paste",
      isVisible: !isVirtual.value,
      permissions: ["catalog:update", "catalog:products:update", "catalog:categories:update"],
      disabled: !clipboard.value.length,
      clickHandler: paste,
    },
    {
      // the Export module's generic export, pre-filled like the legacy catalogExportService registration
      id: "genericExport",
      title: t("CATALOG.BROWSE.GENERIC_EXPORT"),
      icon: "lucide-file-output",
      permissions: "export:access",
      isVisible: exportAvailable.value,
      clickHandler: () =>
        openBlade({
          name: "ExportSettings",
          options: {
            exportDataRequest: {
              exportTypeName: "VirtoCommerce.CatalogModule.Core.Model.Export.ExportableProduct",
              dataQuery: {
                exportTypeName: "ProductExportDataQuery",
                categoryIds: selection.value.filter((e) => e.type === "category").map((e) => e.id),
                objectIds: products.map((e) => e.id),
                catalogIds: [catalogId.value],
                searchInChildren: true,
                isAllSelected: true,
              },
            },
            totalItemsCount: products.length,
          },
        }),
    },
    {
      id: "filters",
      title: t("CATALOG.BROWSE.SAVED_FILTERS"),
      icon: "lucide-filter",
      clickHandler: () => openBlade({ name: "CatalogFilters" }),
    },
    // commands other modules add (extension point "catalog-items-toolbar")
    ...extensions.components.value
      .filter((x) => x.meta && (!x.meta.isVisible || x.meta.isVisible(ctx)))
      .map((x) => ({
        id: x.id,
        title: t(x.meta!.title),
        icon: x.meta!.icon,
        permissions: x.meta!.permissions,
        disabled: !!x.meta!.disabled?.(ctx),
        // a fresh context at click time
        clickHandler: () => x.meta!.onClick(toolbarContext()),
      })),
  ];
});

exposeToChildren({
  reload: load,
  /** saved filters changed (CatalogFilters) */
  reloadFilters: () => {
    filters.value = savedFilters();
    const active = activePreset.value;
    if (active?.startsWith("saved:"))
      onFilter({ filters: { preset: filters.value.some((f) => `saved:${f.id}` === active) ? active : undefined } });
  },
});
</script>
