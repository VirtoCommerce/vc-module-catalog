<template>
  <VcBlade
    :title="item.name || $t('CATALOG.MEASURES.NEW')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="60%"
  >
    <VcContainer>
      <VcForm class="catalog-form">
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="item.name"
              :label="$t('CATALOG.FIELDS.NAME')"
              name="name"
              rules="required"
              required
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model="item.code"
              :label="$t('CATALOG.FIELDS.CODE')"
              name="code"
              rules="required"
              required
            />
          </VcCol>
        </VcRow>
        <VcTextarea
          v-model="item.description"
          :label="$t('CATALOG.MEASURES.DESCRIPTION')"
        />
        <VcCard :header="$t('CATALOG.MEASURES.UNITS')">
          <template #actions>
            <div @click.stop>
              <VcButton
                size="sm"
                variant="secondary"
                icon="lucide-plus"
                @click="editUnit()"
              >
                {{ $t("CATALOG.MEASURES.ADD_UNIT") }}
              </VcButton>
            </div>
          </template>
          <VcDataTable
            :items="units"
            :state-key="'CATALOG_MEASURE_UNITS'"
            :row-actions="unitActions"
            :empty-state="{ icon: 'lucide-ruler', title: $t('CATALOG.MEASURES.NO_UNITS') }"
            @row-click="(e: { data: UnitRow }) => editUnit(e.data)"
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
              id="symbol"
              :title="$t('CATALOG.MEASURES.SYMBOL')"
            />
            <VcColumn
              id="conversionFactor"
              :title="$t('CATALOG.MEASURES.FACTOR')"
            />
            <VcColumn
              id="isDefault"
              :title="$t('CATALOG.MEASURES.DEFAULT')"
            >
              <template #body="{ data }">
                <div
                  v-if="data.isDefault"
                  class="tw-inline-flex"
                >
                  <VcStatus variant="success">{{ $t("CATALOG.MEASURES.DEFAULT") }}</VcStatus>
                </div>
              </template>
            </VcColumn>
          </VcDataTable>
        </VcCard>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import {
  VcBlade,
  VcButton,
  VcCard,
  VcCol,
  VcColumn,
  VcContainer,
  VcDataTable,
  VcForm,
  VcInput,
  VcRow,
  VcStatus,
  VcTextarea,
} from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api, applyRow, indexRows } from "../api";
import type { Measure, MeasureUnit } from "../../api_client/virtocommerce.catalog";

// legacy measure-details + measure-units-list (units are saved with the measure)
defineBlade({ url: "/measure", name: "CatalogMeasureDetails", permissions: ["measures:read"] });

type UnitRow = MeasureUnit & { _index: number };

const { t } = useI18n({ useScope: "global" });
const { param, openBlade, callParent, closeSelf, replaceWith } = useBlade();
const { showConfirmation, showError } = usePopup();

const item = ref<Measure>({ units: [] });
const loading = ref(false);
const form = useBladeForm({ data: item, closeConfirmMessage: computed(() => t("CATALOG.ALERTS.CLOSE_CONFIRM")) });
const units = computed<UnitRow[]>(() => indexRows(item.value.units ?? []));

async function load() {
  loading.value = true;
  try {
    item.value = param.value ? await api.measures.getMeasureById(param.value) : { units: [] };
  } finally {
    loading.value = false;
  }
  form.setBaseline();
}

// The list changes in place (default flags) or by filtering, so an open unit blade still finds its unit (applyRow)
function setUnits(next: MeasureUnit[]) {
  // the first unit is the default one when none is marked (legacy)
  if (next.length && !next.some((u) => u.isDefault)) next[0].isDefault = true;
  item.value.units = next;
}

function editUnit(row?: UnitRow) {
  const unit = row ? item.value.units![row._index] : undefined;
  openBlade({
    name: "CatalogMeasureUnitDetails",
    options: {
      unit,
      onApply: (u: MeasureUnit) => {
        const next = applyRow(item.value.units ?? [], unit, u);
        if (u.isDefault) for (const x of next) if (x !== u) x.isDefault = false;
        setUnits(next);
      },
    },
  });
}

const unitActions = (row: UnitRow) => [
  {
    icon: "lucide-star",
    title: t("CATALOG.MEASURES.MAKE_DEFAULT"),
    type: "info" as const,
    clickHandler: () => (item.value.units ?? []).forEach((u, i) => (u.isDefault = i === row._index)),
  },
  {
    icon: "lucide-trash-2",
    title: t("CATALOG.TOOLBAR.DELETE"),
    type: "danger" as const,
    async clickHandler() {
      if (!(await showConfirmation(t("CATALOG.MEASURES.UNIT_DELETE_CONFIRM", { name: row.name ?? row.code })))) return;
      setUnits((item.value.units ?? []).filter((_, i) => i !== row._index));
    },
  },
];

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "save",
    title: t("CATALOG.TOOLBAR.SAVE"),
    icon: "lucide-save",
    permissions: param.value ? "measures:update" : "measures:create",
    disabled: !form.canSave.value || !item.value.name || !item.value.code,
    async clickHandler() {
      loading.value = true;
      try {
        if (item.value.id) await api.measures.updateMeasure(item.value);
        else {
          const [created] = await api.measures.createMeasure([item.value]);
          form.setBaseline();
          callParent("reload");
          await replaceWith({ name: "CatalogMeasureDetails", param: created.id });
          return;
        }
        form.setBaseline();
        callParent("reload");
        await load();
      } catch (e) {
        showError(String((e as Error).message ?? e));
      } finally {
        loading.value = false;
      }
    },
  },
  { id: "reset", title: t("CATALOG.TOOLBAR.RESET"), icon: "lucide-undo-2", disabled: !form.isModified.value, clickHandler: load },
  {
    id: "delete",
    title: t("CATALOG.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    isVisible: !!param.value,
    permissions: "measures:delete",
    async clickHandler() {
      if (!(await showConfirmation(t("CATALOG.MEASURES.DELETE_CONFIRM", { count: 1 })))) return;
      await api.measures.deleteMeasures([item.value.id!]);
      form.setBaseline();
      callParent("reload");
      closeSelf();
    },
  },
]);

onMounted(load);
</script>
