<template>
  <!-- Catalog fields of the Export module's filter (legacy metafields "<ExportableProduct|ExportableCatalogFull>ExportFilter") -->
  <VcSelect
    v-if="kind === 'catalogs'"
    :model-value="filter.catalogIds ?? []"
    :label="$t('CATALOG.EXPORT.CATALOGS')"
    :options="catalogs"
    option-value="id"
    option-label="name"
    multiple
    searchable
    @update:model-value="(v: string[]) => (filter.catalogIds = v)"
  />
  <VcSelect
    v-else-if="kind === 'categories'"
    :model-value="filter.categoryIds ?? []"
    :label="$t('CATALOG.EXPORT.CATEGORIES')"
    :options="searchCategories"
    option-value="id"
    option-label="title"
    multiple
    searchable
    @update:model-value="(v: string[]) => (filter.categoryIds = v)"
  />
  <VcSwitch
    v-else
    :model-value="!!filter[kind]"
    :label="$t(kind === 'searchInVariations' ? 'CATALOG.EXPORT.IN_VARIATIONS' : 'CATALOG.EXPORT.IN_CHILDREN')"
    @update:model-value="(v?: boolean) => (filter[kind] = !!v)"
  />
</template>

<script setup lang="ts">
import { VcSelect, VcSwitch } from "@vc-shell/framework/ui";
import { onMounted, ref } from "vue";
import { api } from "../api";
import type { Catalog } from "../../api_client/virtocommerce.catalog";

const props = defineProps<{
  kind: "catalogs" | "categories" | "searchInVariations" | "searchInChildren";
  /** the reactive export filter being edited (Export module filter-details) */
  filter: Record<string, unknown> & { catalogIds?: string[]; categoryIds?: string[] };
}>();

const catalogs = ref<Catalog[]>([]);
onMounted(async () => {
  if (props.kind === "catalogs") catalogs.value = (await api.catalogs.searchCatalogs({ take: 1000, sort: "name" })).results ?? [];
});

// categories of the selected catalogs (all catalogs when none), labelled with their path
async function searchCategories(keyword?: string, skip = 0, ids?: string[]) {
  const res = await api.listEntries.listItemsSearch({
    objectType: "Category",
    objectIds: ids?.length ? ids : undefined,
    catalogIds: props.filter.catalogIds?.length ? props.filter.catalogIds : undefined,
    keyword: keyword || undefined,
    searchInChildren: true,
    skip,
    take: 20,
  });
  return {
    results: (res.results ?? []).map((e) => ({ id: e.id!, title: [...(e.path ?? []), e.name].join(" / ") })),
    totalCount: res.totalCount ?? 0,
  };
}
</script>
