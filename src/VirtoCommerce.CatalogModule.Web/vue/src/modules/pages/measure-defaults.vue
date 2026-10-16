<template>
  <VcBlade
    :title="$t('CATALOG.MEASURES.DEFAULTS')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="40%"
  >
    <VcDataTable
      v-model:selection="selection"
      selection-mode="multiple"
      :items="items"
      :state-key="'CATALOG_MEASURE_DEFAULTS'"
      :empty-state="{ icon: 'lucide-ruler', title: $t('CATALOG.MEASURES.NO_DEFAULTS') }"
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
import { useBlade, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import type { Measure, MeasureUnit } from "../../api_client/virtocommerce.catalog";

// legacy create-measures: pick predefined measures that do not exist yet and create them
defineBlade({ url: "/measure-defaults", name: "CatalogMeasureDefaults" });

const { t } = useI18n({ useScope: "global" });
const { callParent, closeSelf } = useBlade();

const items = ref<Measure[]>([]);
const selection = ref<Measure[]>([]);
const loading = ref(false);

async function load() {
  loading.value = true;
  try {
    const [defaults, existing] = await Promise.all([api.measures.getDefaultMeasures(), api.measures.searchMeasures({ take: 1000 })]);
    const codes = new Set((existing.results ?? []).map((m) => m.code));
    items.value = defaults.filter((m) => !codes.has(m.code)).map((m, i) => ({ ...m, id: m.id ?? m.code ?? String(i) }));
  } finally {
    loading.value = false;
  }
}
load();

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "create",
    title: t("CATALOG.MEASURES.CREATE_SELECTED", { count: selection.value.length }),
    icon: "lucide-check",
    permissions: "measures:create",
    disabled: !selection.value.length,
    async clickHandler() {
      loading.value = true;
      try {
        await api.measures.createMeasure(selection.value.map((m) => ({ ...m, id: undefined })));
      } finally {
        loading.value = false;
      }
      callParent("reload");
      closeSelf();
    },
  },
]);
</script>
