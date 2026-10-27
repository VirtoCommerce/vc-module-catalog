<template>
  <VcBlade
    :title="item.name || $t('CATALOG.MEASURES.NEW_UNIT')"
    :toolbar-items="toolbar"
    width="50%"
  >
    <VcContainer>
      <VcForm class="catalog-form">
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="item.code"
              :label="$t('CATALOG.FIELDS.CODE')"
              required
              :error="!!item.code && !/^[a-zA-Z0-9]+$/.test(item.code)"
              :error-message="item.code && !/^[a-zA-Z0-9]+$/.test(item.code) ? $t('CATALOG.MEASURES.CODE_RULE') : undefined"
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model="item.name"
              :label="$t('CATALOG.FIELDS.NAME')"
              required
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="item.symbol"
              :label="$t('CATALOG.MEASURES.SYMBOL')"
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model.number="item.conversionFactor"
              type="number"
              :label="$t('CATALOG.MEASURES.FACTOR')"
              required
            />
          </VcCol>
          <VcCol>
            <VcSwitch
              v-model="item.isDefault"
              :label="$t('CATALOG.MEASURES.DEFAULT')"
            />
          </VcCol>
        </VcRow>
        <LocalizedNames
          :entity="item"
          :languages="languages"
        />
        <LocalizedNames
          :entity="item"
          :languages="languages"
          field="localizedSymbol"
          :header="$t('CATALOG.MEASURES.LOCALIZED_SYMBOL')"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcCol, VcContainer, VcForm, VcInput, VcRow, VcSwitch } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { clone, settingValues } from "../api";
import LocalizedNames from "../components/LocalizedNames.vue";
import type { MeasureUnit } from "../../api_client/virtocommerce.catalog";

// legacy measure-unit-details; applied to the parent measure, persisted by its Save
defineBlade({ url: "/measure-unit", name: "CatalogMeasureUnitDetails", routable: false });

const { t } = useI18n({ useScope: "global" });
const { options, closeSelf } = useBlade<{ unit?: MeasureUnit; onApply: (u: MeasureUnit) => void }>();

const item = ref<MeasureUnit>(clone(options.value?.unit ?? { conversionFactor: 1 }));
const languages = ref<string[]>([]);
const form = useBladeForm({ data: item });
form.setBaseline();
const valid = computed(() => !!item.value.name && !!item.value.code && /^[a-zA-Z0-9]+$/.test(item.value.code));

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "apply",
    title: t("CATALOG.TOOLBAR.APPLY"),
    icon: "lucide-check",
    disabled: !valid.value || !(form.isModified.value || !options.value?.unit),
    clickHandler() {
      options.value?.onApply({ ...item.value });
      form.setBaseline();
      closeSelf();
    },
  },
]);

onMounted(async () => {
  languages.value = await settingValues("VirtoCommerce.Core.General.Languages");
});
</script>
