<template>
  <VcBlade
    :title="$t('CATALOG.BROWSE.SAVED_FILTERS')"
    :toolbar-items="toolbar"
    width="40%"
  >
    <VcDataTable
      v-model:selection="selection"
      selection-mode="multiple"
      :items="items"
      :state-key="'CATALOG_FILTERS'"
      :active-item-id="activeItemId"
      :empty-state="{ icon: 'lucide-filter', title: $t('CATALOG.FILTERS.EMPTY') }"
      @row-click="(e: { data: SavedFilter }) => edit(e.data)"
    >
      <VcColumn
        id="name"
        :title="$t('CATALOG.FIELDS.NAME')"
        body-class="catalog-list__name"
        always-visible
      />
      <VcColumn
        id="keyword"
        :title="$t('CATALOG.FILTERS.KEYWORD')"
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { savedFilters, storeFilters, type SavedFilter } from "../api";

// legacy itemSearch filter dropdown + filter-detail: the user's saved catalog search filters
defineBlade({ url: "/catalog-filters", name: "CatalogFilters" });

const { t } = useI18n({ useScope: "global" });
const { openBlade, callParent, exposeToChildren } = useBlade();

const items = ref<SavedFilter[]>(savedFilters());
const selection = ref<SavedFilter[]>([]);
const activeItemId = ref<string>();

function reload() {
  items.value = savedFilters();
  callParent("reloadFilters");
}

function edit(filter?: SavedFilter) {
  activeItemId.value = filter?.id;
  openBlade({ name: "CatalogFilterDetails", options: { filter }, onClose: () => (activeItemId.value = undefined) });
}

const toolbar = computed<IBladeToolbar[]>(() => [
  { id: "add", title: t("CATALOG.FILTERS.ADD"), icon: "lucide-plus", clickHandler: () => edit() },
  {
    id: "delete",
    title: t("CATALOG.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    disabled: !selection.value.length,
    clickHandler() {
      const ids = new Set(selection.value.map((f) => f.id));
      storeFilters(savedFilters().filter((f) => !ids.has(f.id)));
      selection.value = [];
      reload();
    },
  },
]);

exposeToChildren({ reload });
</script>
