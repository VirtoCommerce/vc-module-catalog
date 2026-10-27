<template>
  <VcBlade
    :title="$t('CATALOG.STORE.SEARCH')"
    :subtitle="options?.store?.name"
    :toolbar-items="toolbar"
    :loading="loading"
    width="60%"
  >
    <VcContainer>
      <VcForm class="catalog-form">
        <VcCard
          :header="$t('CATALOG.STORE.FACETS')"
          is-collapsable
        >
          <div class="catalog-card">
            <div class="catalog-card__hint">{{ $t("CATALOG.STORE.FACETS_HINT") }}</div>
            <VcDataTable
              v-model:selection="selectedFacets"
              selection-mode="multiple"
              reorderable-rows
              :items="facets"
              data-key="name"
              :state-key="'CATALOG_STORE_FACETS'"
              :empty-state="{ icon: 'lucide-filter', title: $t('CATALOG.STORE.NO_FACETS') }"
              @row-reorder="(e: { value: Facet[] }) => (facets = e.value)"
              @row-click="(e: { data: Facet }) => editFacet(e.data)"
            >
              <VcColumn
                id="name"
                :title="$t('CATALOG.FIELDS.NAME')"
                body-class="catalog-list__name"
                always-visible
              />
              <VcColumn
                id="type"
                :title="$t('CATALOG.FIELDS.TYPE')"
              />
              <VcColumn
                id="size"
                :title="$t('CATALOG.STORE.SIZE')"
              />
            </VcDataTable>
          </div>
        </VcCard>

        <VcCard
          :header="$t('CATALOG.STORE.SORTING')"
          is-collapsable
        >
          <template #actions>
            <div @click.stop>
              <VcButton
                size="sm"
                variant="secondary"
                icon="lucide-plus"
                @click="editSorting()"
              >
                {{ $t("CATALOG.STORE.ADD_SORTING") }}
              </VcButton>
            </div>
          </template>
          <div class="catalog-card">
            <div class="catalog-card__hint">{{ $t("CATALOG.STORE.SORTING_HINT") }}</div>
            <VcDataTable
              v-model:selection="visibleSortings"
              selection-mode="multiple"
              reorderable-rows
              :items="sortings"
              data-key="code"
              :state-key="'CATALOG_STORE_SORTINGS'"
              :row-actions="sortingActions"
              @row-reorder="(e: { value: ProductSorting[] }) => (sortings = e.value)"
              @row-click="(e: { data: ProductSorting }) => editSorting(e.data)"
            >
              <VcColumn
                id="name"
                :title="$t('CATALOG.FIELDS.NAME')"
                body-class="catalog-list__name"
                always-visible
              />
              <VcColumn
                id="sortExpression"
                :title="$t('CATALOG.STORE.EXPRESSION')"
              />
            </VcDataTable>
          </div>
        </VcCard>

        <VcCard
          :header="$t('CATALOG.STORE.BARCODE')"
          is-collapsable
        >
          <div class="catalog-card">
            <VcSwitch
              v-model="barcode.scannerEnabled"
              :label="$t('CATALOG.STORE.SCANNER_ENABLED')"
            />
            <VcSelect
              v-model="barcode.fields"
              :label="$t('CATALOG.STORE.BARCODE_FIELDS')"
              :options="barcodeFields"
              multiple
              searchable
            />
          </div>
        </VcCard>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcButton, VcCard, VcColumn, VcContainer, VcDataTable, VcForm, VcSelect, VcSwitch } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api, http, options as toOptions, type Store } from "../api";
import type { BarcodeSearchSettings, ProductSorting } from "../../api_client/virtocommerce.catalog";
import type { Facet } from "./store-facet.vue";

// Search configuration of a store (catalog widget on the Store blade; legacy search-configuration menu):
// facets (aggregation properties), product sorting, barcode scanner.
defineBlade({ url: "/catalog-store-search", name: "CatalogStoreSearch", permissions: ["catalog:BrowseFilters:Read"], routable: false });

const { t } = useI18n({ useScope: "global" });
const { options, openBlade } = useBlade<{ store: Store }>();
const { showConfirmation } = usePopup();
const sortableFields = ref<string[]>([]);
const storeId = computed(() => options.value?.store.id ?? "");

const facets = ref<Facet[]>([]);
const selectedFacets = ref<Facet[]>([]);
const sortings = ref<ProductSorting[]>([]);
const visibleSortings = ref<ProductSorting[]>([]);
const barcode = ref<BarcodeSearchSettings>({});
const barcodeFields = ref<{ id: string; title: string }[]>([]);
const loading = ref(false);

async function load() {
  if (!storeId.value) return;
  loading.value = true;
  try {
    const [f, s, b, fields, sortable] = await Promise.all([
      http<Facet[]>("GET", `/api/catalog/aggregationproperties/${storeId.value}/properties`),
      api.sortings.getSortings(storeId.value),
      api.barcode.getSettings(storeId.value),
      api.barcode.getAvailableFields(storeId.value),
      api.sortings.getSortableFields(storeId.value).catch(() => []),
    ]);
    sortableFields.value = (sortable ?? []).map((x) => x.name!).filter(Boolean);
    // selected facets first, in their saved order (legacy: two lists)
    facets.value = [...(f ?? []).filter((x) => x.isSelected), ...(f ?? []).filter((x) => !x.isSelected)];
    selectedFacets.value = facets.value.filter((x) => x.isSelected);
    sortings.value = s ?? [];
    visibleSortings.value = sortings.value.filter((x) => x.isVisible);
    barcode.value = b ?? {};
    barcodeFields.value = toOptions((fields ?? []).map((x) => x.name!).filter(Boolean));
  } finally {
    loading.value = false;
  }
}

// facet details (legacy aggregation-properties-details), applied here and saved with the blade
function editFacet(facet: Facet) {
  openBlade({
    name: "CatalogStoreFacetDetails",
    options: {
      facet,
      storeId: storeId.value,
      onApply: (f: Facet) => {
        facets.value = facets.value.map((x) => (x.name === f.name ? f : x));
        selectedFacets.value = selectedFacets.value.map((x) => (x.name === f.name ? f : x));
      },
    },
  });
}

// sorting details and custom sortings (legacy product-sorting-list/detail)
function editSorting(sorting?: ProductSorting) {
  openBlade({
    name: "CatalogStoreSortingDetails",
    options: {
      // visibility as ticked in the table now, not as loaded
      sorting: sorting && { ...sorting, isVisible: visibleSortings.value.some((x) => x.code === sorting.code) },
      fields: sortableFields.value,
      existingCodes: sortings.value.map((x) => x.code!),
      languages: options.value?.store.languages ?? [],
      onApply: (s: ProductSorting) => {
        if (sorting) {
          sortings.value = sortings.value.map((x) => (x.code === sorting.code ? s : x));
          visibleSortings.value = visibleSortings.value.filter((x) => x.code !== sorting.code);
        } else sortings.value = [...sortings.value, s];
        if (s.isVisible) visibleSortings.value = [...visibleSortings.value, s];
      },
    },
  });
}
const sortingActions = (s: ProductSorting) =>
  s.isCustom
    ? [
        {
          icon: "lucide-trash-2",
          title: t("CATALOG.TOOLBAR.DELETE"),
          type: "danger" as const,
          async clickHandler() {
            if (!(await showConfirmation(t("CATALOG.STORE.DELETE_SORTING", { name: s.name })))) return;
            sortings.value = sortings.value.filter((x) => x.code !== s.code);
            visibleSortings.value = visibleSortings.value.filter((x) => x.code !== s.code);
          },
        },
      ]
    : [];

async function save() {
  loading.value = true;
  try {
    const on = new Set(selectedFacets.value.map((x) => x.name));
    const visible = new Set(visibleSortings.value.map((x) => x.code));
    await Promise.all([
      http("PUT", `/api/catalog/aggregationproperties/${storeId.value}/properties`, facets.value.map((x) => ({ ...x, isSelected: on.has(x.name) }))),
      api.sortings.saveSortings(
        storeId.value,
        sortings.value.map((x, i) => ({ ...x, order: i, isVisible: visible.has(x.code) })),
      ),
      api.barcode.saveSettings(storeId.value, barcode.value),
    ]);
  } finally {
    loading.value = false;
  }
  await load();
}

const toolbar = computed<IBladeToolbar[]>(() => [
  // all three PUTs need catalog:BrowseFilters:Update
  { id: "save", title: t("CATALOG.TOOLBAR.SAVE"), icon: "lucide-save", permissions: "catalog:BrowseFilters:Update", clickHandler: save },
  { id: "reset", title: t("CATALOG.TOOLBAR.RESET"), icon: "lucide-undo-2", clickHandler: load },
]);

onMounted(load);
</script>
