<template>
  <VcBlade
    :title="$t('CATALOG.PROPERTY.LIST_TITLE', { name: owner?.name ?? '' })"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:search-value="keyword"
      :items="items"
      :searchable="true"
      :state-key="'CATALOG_PROPERTIES'"
      :active-item-id="activeItemId"
      :empty-state="{ icon: 'lucide-list-tree', title: $t('CATALOG.PROPERTIES.EMPTY') }"
      @row-click="(e: { data: Property }) => edit(e.data.id)"
    >
      <VcColumn
        id="name"
        :title="$t('CATALOG.FIELDS.NAME')"
        body-class="catalog-list__name"
        always-visible
      />
      <VcColumn
        id="type"
        :title="$t('CATALOG.PROPERTY.APPLIES_TO')"
      />
      <VcColumn
        id="valueType"
        :title="$t('CATALOG.PROPERTIES.VALUE_TYPE')"
      />
      <VcColumn
        id="flags"
        :title="$t('CATALOG.PROPERTY.FLAGS')"
      >
        <template #body="{ data }">{{ flags(data) }}</template>
      </VcColumn>
      <VcColumn
        id="isInherited"
        :title="$t('CATALOG.PROPERTY.OWNER')"
      >
        <template #body="{ data }">
          {{ data.isInherited ? $t("CATALOG.PROPERTY.INHERITED") : $t("CATALOG.PROPERTY.OWN") }}
        </template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import type { Property } from "../../api_client/virtocommerce.catalog";

// property definitions of a catalog or a category (legacy property-list "manage" mode + property-type-list)
defineBlade({ url: "/catalog-properties", name: "CatalogPropertyList" });

const { t } = useI18n({ useScope: "global" });
const { options, openBlade, callParent, exposeToChildren } = useBlade<{ catalogId?: string; categoryId?: string }>();

const owner = ref<{ name?: string; properties?: Property[] }>();
const loading = ref(false);
const keyword = ref("");
const activeItemId = ref<string>();
const items = computed(() =>
  (owner.value?.properties ?? [])
    .filter((p) => !keyword.value || p.name?.toLowerCase().includes(keyword.value.toLowerCase()))
    .sort((a, b) => (a.name ?? "").localeCompare(b.name ?? "")),
);

const flags = (p: Property) =>
  (["required", "dictionary", "multivalue", "multilanguage", "hidden"] as const)
    .filter((f) => p[f])
    .map((f) => t(`CATALOG.PROPERTY.${f.toUpperCase()}`))
    .join(", ");

async function load() {
  loading.value = true;
  try {
    const o = options.value;
    owner.value = o?.categoryId ? await api.categories.getCategory(o.categoryId) : await api.catalogs.getCatalog(o!.catalogId!);
  } finally {
    loading.value = false;
  }
}
load();

function edit(id?: string) {
  activeItemId.value = id;
  openBlade({
    name: "CatalogPropertyDetails",
    param: id,
    options: { catalogId: options.value?.catalogId, categoryId: options.value?.categoryId },
    onClose: () => (activeItemId.value = undefined),
  });
}

const toolbar = computed<IBladeToolbar[]>(() => [
  { id: "refresh", title: t("CATALOG.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: load },
  {
    id: "add",
    title: t("CATALOG.PROPERTY.ADD"),
    icon: "lucide-plus",
    permissions: "catalog:metadata-property:edit",
    clickHandler: () => edit(),
  },
]);

exposeToChildren({
  async reload() {
    await load();
    callParent("reload");
  },
});
</script>
