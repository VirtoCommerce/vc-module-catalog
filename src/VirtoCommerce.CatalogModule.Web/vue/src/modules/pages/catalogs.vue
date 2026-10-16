<template>
  <VcBlade
    :title="$t('CATALOG.CATALOGS.TITLE')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="30%"
  >
    <!-- phones: a row is the name with its type badge and language; the modified date is desktop-only -->
    <VcDataTable
      v-model:search-value="searchValue"
      v-model:selection="selection"
      selection-mode="multiple"
      :items="items"
      :total-count="totalCount"
      :searchable="true"
      :state-key="'CATALOG_CATALOGS'"
      :active-item-id="activeItemId"
      :show-all-columns="expanded"
      :empty-state="{ icon: 'lucide-folder', title: $t('CATALOG.CATALOGS.EMPTY') }"
      @row-click="(e: { data: Catalog }) => browse(e.data)"
    >
      <VcColumn
        id="name"
        :title="$t('CATALOG.FIELDS.NAME')"
        body-class="catalog-list__name"
        always-visible
        mobile-role="title"
      />
      <VcColumn
        id="isVirtual"
        :title="$t('CATALOG.FIELDS.TYPE')"
        mobile-role="status"
      >
        <template #body="{ data }">
          <div class="tw-inline-flex">
            <VcStatus :variant="data.isVirtual ? 'info' : 'success'">
              {{ $t(data.isVirtual ? "CATALOG.CATALOGS.VIRTUAL" : "CATALOG.CATALOGS.REAL") }}
            </VcStatus>
          </div>
        </template>
      </VcColumn>
      <VcColumn
        id="defaultLanguage"
        :title="$t('CATALOG.FIELDS.DEFAULT_LANGUAGE')"
        mobile-role="field"
      >
        <template #body="{ data }">{{ data.defaultLanguage?.languageCode }}</template>
      </VcColumn>
      <VcColumn
        id="modifiedDate"
        :title="$t('CATALOG.FIELDS.MODIFIED')"
        type="date"
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useFunctions, usePopup, useTableSearch, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcStatus } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import type { Catalog } from "../../api_client/virtocommerce.catalog";

defineBlade({
  url: "/catalog",
  name: "CatalogList",
  isWorkspace: true,
  permissions: ["catalog:access"],
  menuItem: { title: "CATALOG.MENU.CATALOG", icon: "lucide-folder", priority: 10 },
});

const { t } = useI18n({ useScope: "global" });
const { openBlade, closeChildren, exposeToChildren, expanded } = useBlade();
const { showConfirmation } = usePopup();
const { debounce } = useFunctions();
const { searchValue } = useTableSearch({ stateKey: "CATALOG_CATALOGS" });

const items = ref<Catalog[]>([]);
const totalCount = ref(0);
const loading = ref(false);
const selection = ref<Catalog[]>([]);
const activeItemId = ref<string>();

async function load() {
  loading.value = true;
  try {
    const res = await api.catalogs.searchCatalogs({ keyword: searchValue.value || undefined, take: 1000, sort: "name" });
    items.value = res.results ?? [];
    totalCount.value = res.totalCount ?? 0;
  } finally {
    loading.value = false;
  }
}
load();
watch(searchValue, debounce(load, 300));

function browse(catalog: Catalog) {
  activeItemId.value = catalog.id;
  openBlade({
    name: "CatalogBrowse",
    param: catalog.id,
    options: { catalog },
    onClose: () => (activeItemId.value = undefined),
  });
}

function edit(catalog?: Catalog, isVirtual = false) {
  openBlade({ name: "CatalogDetails", param: catalog?.id, options: { isVirtual } });
}

const one = computed(() => selection.value.length === 1);
const toolbar = computed<IBladeToolbar[]>(() => [
  { id: "refresh", title: t("CATALOG.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: load },
  {
    id: "add",
    title: t("CATALOG.CATALOGS.ADD"),
    icon: "lucide-plus",
    permissions: "catalog:create",
    clickHandler: () => edit(undefined, false),
  },
  {
    id: "addVirtual",
    title: t("CATALOG.CATALOGS.ADD_VIRTUAL"),
    icon: "lucide-folder-plus",
    permissions: "catalog:create",
    clickHandler: () => edit(undefined, true),
  },
  {
    // legacy catalogs-list keyword search: categories and products of every catalog
    id: "searchAll",
    title: t("CATALOG.BROWSE.SEARCH_ALL"),
    icon: "lucide-search",
    clickHandler: () => openBlade({ name: "CatalogBrowse", options: { global: true, keyword: searchValue.value } }),
  },
  {
    id: "manage",
    title: t("CATALOG.TOOLBAR.MANAGE"),
    icon: "lucide-pencil",
    disabled: !one.value,
    clickHandler: () => edit(selection.value[0]),
  },
  {
    id: "delete",
    title: t("CATALOG.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "catalog:delete",
    disabled: !selection.value.length,
    async clickHandler() {
      if (!(await showConfirmation(t("CATALOG.CATALOGS.DELETE_CONFIRM", { count: selection.value.length })))) return;
      loading.value = true;
      try {
        await closeChildren();
        for (const c of selection.value) await api.catalogs.deleteCatalog(c.id!);
      } finally {
        selection.value = [];
        await load();
      }
    },
  },
]);

exposeToChildren({ reload: load });
</script>
