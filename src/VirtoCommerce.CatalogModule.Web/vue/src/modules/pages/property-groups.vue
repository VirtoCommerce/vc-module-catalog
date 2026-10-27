<template>
  <VcBlade
    :title="$t('CATALOG.WIDGETS.PROPERTY_GROUPS')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="40%"
  >
    <VcDataTable
      v-model:selection="selection"
      selection-mode="multiple"
      :items="items"
      :state-key="'CATALOG_PROPERTY_GROUPS'"
      :active-item-id="activeItemId"
      :empty-state="{ icon: 'lucide-group', title: $t('CATALOG.GROUPS.EMPTY') }"
      @row-click="(e: { data: PropertyGroup }) => edit(e.data.id)"
    >
      <VcColumn
        id="name"
        :title="$t('CATALOG.FIELDS.NAME')"
        body-class="catalog-list__name"
        always-visible
      />
      <VcColumn
        id="displayOrder"
        :title="$t('CATALOG.FIELDS.DISPLAY_ORDER')"
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import type { PropertyGroup } from "../../api_client/virtocommerce.catalog";

defineBlade({ url: "/catalog-property-groups", name: "CatalogPropertyGroups" });

const { t } = useI18n({ useScope: "global" });
const { options, openBlade, callParent, exposeToChildren } = useBlade<{ catalogId: string }>();
const { showConfirmation } = usePopup();

const items = ref<PropertyGroup[]>([]);
const selection = ref<PropertyGroup[]>([]);
const loading = ref(false);
const activeItemId = ref<string>();

async function load() {
  loading.value = true;
  try {
    const res = await api.propertyGroups.search({ catalogId: options.value!.catalogId, take: 1000, sort: "displayOrder" });
    items.value = res.results ?? [];
  } finally {
    loading.value = false;
  }
}
load();

function edit(id?: string) {
  activeItemId.value = id;
  openBlade({
    name: "CatalogPropertyGroupDetails",
    param: id,
    options: { catalogId: options.value?.catalogId },
    onClose: () => (activeItemId.value = undefined),
  });
}

const toolbar = computed<IBladeToolbar[]>(() => [
  { id: "refresh", title: t("CATALOG.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: load },
  {
    id: "add",
    title: t("CATALOG.GROUPS.ADD"),
    icon: "lucide-plus",
    permissions: "catalog:metadata-property:edit",
    clickHandler: () => edit(),
  },
  {
    id: "delete",
    title: t("CATALOG.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "catalog:metadata-property:edit",
    disabled: !selection.value.length,
    async clickHandler() {
      if (!(await showConfirmation(t("CATALOG.GROUPS.DELETE_CONFIRM", { count: selection.value.length })))) return;
      await api.propertyGroups.delete(selection.value.map((g) => g.id!));
      selection.value = [];
      await reload();
    },
  },
]);

async function reload() {
  await load();
  callParent("reload");
}
exposeToChildren({ reload });
</script>
