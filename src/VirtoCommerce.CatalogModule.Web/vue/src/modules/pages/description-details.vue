<template>
  <VcBlade
    :title="$t(options?.description ? 'CATALOG.DESCRIPTIONS.EDIT' : 'CATALOG.DESCRIPTIONS.NEW')"
    :toolbar-items="toolbar"
    width="70%"
  >
    <VcContainer>
      <VcForm class="catalog-form">
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcSelect
              v-model="type"
              :label="$t('CATALOG.FIELDS.TYPE')"
              :options="types"
              required
            />
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="item.languageCode"
              :label="$t('CATALOG.FIELDS.LANGUAGE')"
              :options="languageOptions"
            />
          </VcCol>
        </VcRow>
        <VcEditor
          v-model="item.content"
          :label="$t('CATALOG.DESCRIPTIONS.CONTENT')"
          :assets-folder="options?.assetsFolder"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcCol, VcContainer, VcEditor, VcForm, VcRow, VcSelect } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { clone, options as toOptions, settingOptions } from "../api";

defineBlade({ url: "/catalog-description", name: "CatalogDescriptionDetails", routable: false });

type Description = { content?: string; languageCode?: string; reviewType?: string; descriptionType?: string; isInherited?: boolean };

const { t } = useI18n({ useScope: "global" });
const { options, closeSelf } = useBlade<{
  description?: Description;
  kind: "product" | "category";
  languages?: string[];
  assetsFolder?: string;
  onApply: (d: Description) => void;
}>();

const isCategory = computed(() => options.value?.kind === "category");
const item = ref<Description>(clone(options.value?.description ?? { languageCode: options.value?.languages?.[0] }));
const type = computed({
  get: () => (isCategory.value ? item.value.descriptionType : item.value.reviewType),
  set: (v) => (isCategory.value ? (item.value.descriptionType = v) : (item.value.reviewType = v)),
});
const types = ref<{ id: string; title: string }[]>([]);
const languageOptions = computed(() => toOptions(options.value?.languages ?? []));
const form = useBladeForm({ data: item, closeConfirmMessage: computed(() => t("CATALOG.ALERTS.CLOSE_CONFIRM")) });
form.setBaseline();

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "apply",
    title: t("CATALOG.TOOLBAR.APPLY"),
    icon: "lucide-check",
    disabled: !type.value || !(form.isModified.value || !options.value?.description),
    clickHandler() {
      options.value?.onApply({ ...item.value, isInherited: false });
      form.setBaseline();
      closeSelf();
    },
  },
]);

onMounted(async () => {
  types.value = await settingOptions(isCategory.value ? "Catalog.CategoryDescriptionTypes" : "Catalog.EditorialReviewTypes");
  if (!type.value && types.value.length) {
    type.value = types.value[0].id;
    form.setBaseline();
  }
});
</script>
