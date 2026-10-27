<template>
  <VcBlade
    :title="item.name ?? ''"
    :subtitle="$t('CATALOG.STORE.FACETS')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcContainer>
      <VcForm class="catalog-form">
        <VcSelect
          v-model="item.type"
          :label="$t('CATALOG.FIELDS.TYPE')"
          :options="types"
          :clearable="false"
          :disabled="item.type === 'PriceRange'"
        />
        <VcSelect
          v-if="!isRange"
          v-model="item.termValuesSortingType"
          :label="$t('CATALOG.STORE.VALUES_SORTING')"
          :options="sortingTypes"
        />
        <VcRow
          v-if="units.length"
          class="tw-gap-4"
        >
          <VcCol>
            <VcField
              :label="$t('CATALOG.PROPERTY.MEASURE')"
              :model-value="measureName"
            />
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="item.unitOfMeasureId"
              :label="$t('CATALOG.FIELDS.MEASURE_UNIT')"
              :options="units"
              option-value="id"
              option-label="name"
            />
          </VcCol>
        </VcRow>
        <VcInput
          v-if="!isRange"
          v-model.number="item.size"
          type="number"
          :label="$t('CATALOG.STORE.SIZE')"
          :hint="$t('CATALOG.STORE.SIZE_HINT')"
        />
        <VcSelect
          v-if="!isRange && known.length"
          v-model="item.values"
          :label="$t('CATALOG.STORE.VALUES')"
          :hint="$t('CATALOG.STORE.VALUES_HINT')"
          :options="known"
          multiple
          searchable
        />
        <VcMultivalue
          v-else
          :model-value="toChips(item.values)"
          :label="isRange ? $t('CATALOG.STORE.RANGE_BOUNDS') : $t('CATALOG.STORE.VALUES')"
          :type="isRange ? 'number' : 'text'"
          :placeholder="$t('CATALOG.PROPERTIES.ADD_CUSTOM')"
          @update:model-value="(list) => (item.values = fromChips(list))"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcCol, VcContainer, VcField, VcForm, VcInput, VcMultivalue, VcRow, VcSelect } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api, clone, fromChips, http, options as toOptions, toChips } from "../api";
import type { MeasureUnit } from "../../api_client/virtocommerce.catalog";

// legacy aggregation-properties-details: type, value sorting, unit, size and values of one store facet
defineBlade({ url: "/catalog-store-facet", name: "CatalogStoreFacetDetails", routable: false });

export interface Facet {
  name?: string;
  type?: string;
  size?: number;
  isSelected?: boolean;
  values?: string[];
  valuesCount?: number;
  termValuesSortingType?: string;
  measureId?: string;
  unitOfMeasureId?: string;
  indexFieldName?: string;
  [key: string]: unknown;
}

const { t } = useI18n({ useScope: "global" });
const { options, closeSelf } = useBlade<{ facet: Facet; storeId: string; onApply: (f: Facet) => void }>();

const item = ref<Facet>(clone(options.value?.facet ?? {}));
// a bare "Priority" now means ascending (legacy)
if (item.value.termValuesSortingType === "Priority") item.value.termValuesSortingType = "PriorityAscending";
item.value.values ??= [];
const loading = ref(false);
const known = ref<{ id: string; title: string }[]>([]);
const units = ref<MeasureUnit[]>([]);
const measureName = ref("");
const isRange = computed(() => item.value.type !== "Attribute");
const types = computed(() => (item.value.type === "PriceRange" ? toOptions(["PriceRange"]) : toOptions(["Attribute", "Range"])));
const sortingTypes = computed(() =>
  ["Score", "PriorityAscending", "PriorityDescending", "NameAscending", "NameDescending", "NumericAscending", "NumericDescending"].map(
    (id) => ({ id, title: t(`CATALOG.STORE.SORTING_TYPES.${id}`) }),
  ),
);
const form = useBladeForm({ data: item });

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "apply",
    title: t("CATALOG.TOOLBAR.APPLY"),
    icon: "lucide-check",
    disabled: !form.isModified.value,
    clickHandler() {
      const facet = { ...item.value, valuesCount: item.value.values?.length ?? 0 };
      // a non-default unit is indexed under name-unitCode (legacy)
      const unit = units.value.find((u) => u.id === facet.unitOfMeasureId);
      if (unit) facet.indexFieldName = unit.isDefault ? facet.name : `${facet.name}-${unit.code}`;
      options.value?.onApply(facet);
      form.setBaseline();
      closeSelf();
    },
  },
]);

onMounted(async () => {
  loading.value = true;
  try {
    const [values, measure] = await Promise.all([
      http<string[]>(
        "GET",
        `/api/catalog/aggregationproperties/${options.value?.storeId}/properties/${encodeURIComponent(item.value.name ?? "")}/values`,
      ).catch(() => []),
      item.value.measureId ? api.measures.getMeasureById(item.value.measureId) : undefined,
    ]);
    known.value = toOptions(values ?? []);
    if (measure) {
      measureName.value = measure.name ?? "";
      units.value = measure.units ?? [];
      item.value.unitOfMeasureId ??= units.value.find((u) => u.isDefault)?.id;
    }
  } finally {
    loading.value = false;
  }
  form.setBaseline();
});
</script>
