<template>
  <VcBlade
    :title="$t('CATALOG.MENU.MEASURES')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="40%"
  >
    <VcDataTable
      v-model:search-value="keyword"
      v-model:selection="selection"
      selection-mode="multiple"
      :items="rows"
      :searchable="true"
      :state-key="'CATALOG_MEASURES'"
      :active-item-id="activeItemId"
      :show-all-columns="expanded"
      :empty-state="{
        icon: 'lucide-ruler',
        title: $t('CATALOG.MEASURES.EMPTY'),
        actionLabel: $t('CATALOG.MEASURES.ADD_DEFAULTS'),
        actionHandler: openDefaults,
      }"
      @row-click="(e: { data: Measure }) => edit(e.data.id)"
    >
      <VcColumn
        id="name"
        :title="$t('CATALOG.FIELDS.NAME')"
        body-class="catalog-list__name"
        always-visible
      />
      <VcColumn
        id="code"
        :title="$t('CATALOG.FIELDS.CODE')"
      />
      <VcColumn
        id="units"
        :title="$t('CATALOG.MEASURES.UNITS')"
      >
        <template #body="{ data }">{{ (data.units ?? []).map((u: MeasureUnit) => u.symbol || u.code).join(", ") }}</template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import type { Measure, MeasureUnit } from "../../api_client/virtocommerce.catalog";

// Units of measure workspace (legacy measures-list)
defineBlade({
  url: "/measures",
  name: "CatalogMeasures",
  isWorkspace: true,
  permissions: ["measures:access"],
  menuItem: { title: "CATALOG.MENU.MEASURES", icon: "lucide-ruler", priority: 40 },
});

const { t } = useI18n({ useScope: "global" });
const { openBlade, exposeToChildren, expanded } = useBlade();
const { showConfirmation } = usePopup();

const items = ref<Measure[]>([]);
const loading = ref(false);
const keyword = ref("");
const selection = ref<Measure[]>([]);
const activeItemId = ref<string>();
const rows = computed(() =>
  items.value.filter((m) => !keyword.value || [m.name, m.code].some((s) => s?.toLowerCase().includes(keyword.value.toLowerCase()))),
);

async function load() {
  loading.value = true;
  try {
    items.value = (await api.measures.searchMeasures({ take: 1000, sort: "name" })).results ?? [];
  } finally {
    loading.value = false;
  }
}
load();

function edit(id?: string) {
  activeItemId.value = id;
  openBlade({ name: "CatalogMeasureDetails", param: id, onClose: () => (activeItemId.value = undefined) });
}
function openDefaults() {
  openBlade({ name: "CatalogMeasureDefaults" });
}

const toolbar = computed<IBladeToolbar[]>(() => [
  { id: "refresh", title: t("CATALOG.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: load },
  { id: "add", title: t("CATALOG.MEASURES.ADD"), icon: "lucide-plus", permissions: "measures:create", clickHandler: () => edit() },
  {
    id: "defaults",
    title: t("CATALOG.MEASURES.ADD_DEFAULTS"),
    icon: "lucide-list-plus",
    permissions: "measures:create",
    clickHandler: openDefaults,
  },
  {
    id: "delete",
    title: t("CATALOG.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "measures:delete",
    disabled: !selection.value.length,
    async clickHandler() {
      if (!(await showConfirmation(t("CATALOG.MEASURES.DELETE_CONFIRM", { count: selection.value.length })))) return;
      await api.measures.deleteMeasures(selection.value.map((m) => m.id!));
      selection.value = [];
      await load();
    },
  },
]);

exposeToChildren({ reload: load });
</script>
