<template>
  <VcBlade
    :title="$t('CATALOG.DICTIONARY.TITLE', { name: property?.name ?? '' })"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:search-value="keyword"
      v-model:selection="selection"
      selection-mode="multiple"
      :items="items"
      :total-count="totalCount"
      :pagination="pagination"
      :searchable="true"
      :state-key="'CATALOG_DICTIONARY'"
      :active-item-id="activeItemId"
      :empty-state="{ icon: 'lucide-book-open', title: $t('CATALOG.DICTIONARY.EMPTY') }"
      @row-click="(e: { data: PropertyDictionaryItem }) => edit(e.data)"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        id="alias"
        :title="$t('CATALOG.DICTIONARY.ALIAS')"
        body-class="catalog-list__name"
        always-visible
      />
      <VcColumn
        id="localizedValues"
        :title="$t('CATALOG.DICTIONARY.VALUES')"
      >
        <template #body="{ data }">
          {{ (data.localizedValues ?? []).map((v: PropertyDictionaryItemLocalizedValue) => `${v.languageCode}: ${v.value}`).join("; ") }}
        </template>
      </VcColumn>
      <VcColumn
        id="colorCode"
        :title="$t('CATALOG.DICTIONARY.COLOR')"
      >
        <template #body="{ data }">
          <span
            v-if="data.colorCode"
            class="catalog-swatch"
            :style="{ background: data.colorCode }"
          />
          {{ data.colorCode }}
        </template>
      </VcColumn>
      <VcColumn
        id="sortOrder"
        :visible="false"
        :title="$t('CATALOG.FIELDS.SORT_ORDER')"
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useDataTablePagination, useFunctions, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import type {
  Property,
  PropertyDictionaryItem,
  PropertyDictionaryItemLocalizedValue,
} from "../../api_client/virtocommerce.catalog";

defineBlade({ url: "/catalog-dictionary", name: "CatalogPropertyDictionary" });

const { t } = useI18n({ useScope: "global" });
const { options, openBlade, closeChildren, exposeToChildren } = useBlade<{
  property: Property;
  languages?: string[];
  defaultLanguage?: string;
}>();
const { showConfirmation } = usePopup();
const { debounce } = useFunctions();

const property = computed(() => options.value?.property);
const items = ref<PropertyDictionaryItem[]>([]);
const totalCount = ref(0);
const loading = ref(false);
const keyword = ref("");
const selection = ref<PropertyDictionaryItem[]>([]);
const activeItemId = ref<string>();
const PAGE_SIZE = 20;
const pagination = useDataTablePagination({ pageSize: PAGE_SIZE, totalCount });

async function load() {
  loading.value = true;
  try {
    const res = await api.dictionary.searchPropertyDictionaryItems({
      propertyIds: [property.value!.id!],
      keyword: keyword.value || undefined,
      skip: pagination.skip,
      take: PAGE_SIZE,
    });
    items.value = res.results ?? [];
    totalCount.value = res.totalCount ?? 0;
  } finally {
    loading.value = false;
  }
}
load();
watch(keyword, () => pagination.setPage(1));
watch([keyword, () => pagination.skip], debounce(load, 300));

function edit(entry?: PropertyDictionaryItem) {
  activeItemId.value = entry?.id;
  openBlade({
    name: "CatalogDictionaryItem",
    options: {
      item: entry,
      property: property.value,
      languages: options.value?.languages,
      defaultLanguage: options.value?.defaultLanguage,
    },
    onClose: () => (activeItemId.value = undefined),
  });
}

const toolbar = computed<IBladeToolbar[]>(() => [
  { id: "refresh", title: t("CATALOG.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: load },
  {
    id: "add",
    title: t("CATALOG.DICTIONARY.ADD"),
    icon: "lucide-plus",
    permissions: "catalog:dictionary-property:edit",
    clickHandler: () => edit(),
  },
  {
    id: "delete",
    title: t("CATALOG.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "catalog:dictionary-property:edit",
    disabled: !selection.value.length,
    async clickHandler() {
      if (!(await showConfirmation(t("CATALOG.DICTIONARY.DELETE_CONFIRM", { count: selection.value.length })))) return;
      await closeChildren();
      await api.dictionary.deletePropertyDictionaryItems(selection.value.map((x) => x.id!));
      selection.value = [];
      await load();
    },
  },
]);

exposeToChildren({ reload: load });
</script>

<style lang="scss">
.catalog-swatch {
  display: inline-block;
  width: 12px;
  height: 12px;
  border-radius: 3px;
  border: 1px solid var(--neutrals-200);
  vertical-align: middle;
  margin-inline-end: 4px;
}
</style>
