<template>
  <VcBlade
    :title="item.alias || $t('CATALOG.DICTIONARY.NEW')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="40%"
  >
    <VcContainer>
      <VcForm class="catalog-form">
        <VcInput
          v-model="item.alias"
          :label="$t('CATALOG.DICTIONARY.ALIAS')"
          name="alias"
          rules="required"
          required
        />
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model.number="item.sortOrder"
              type="number"
              :label="$t('CATALOG.FIELDS.SORT_ORDER')"
              name="sortOrder"
              rules="required"
              required
            />
          </VcCol>
          <VcCol>
            <VcColorInput
              v-model="item.colorCode"
              :label="$t('CATALOG.DICTIONARY.COLOR')"
            />
          </VcCol>
        </VcRow>
        <VcInput
          v-for="lang in languages"
          :key="lang"
          :model-value="valueOf(lang)"
          :label="`${$t('CATALOG.FIELDS.VALUE')} (${lang})`"
          @update:model-value="(v) => setValue(lang, String(v ?? ''))"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcCol, VcColorInput, VcContainer, VcForm, VcInput, VcRow } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { clone, api } from "../api";
import type { Property, PropertyDictionaryItem } from "../../api_client/virtocommerce.catalog";

// legacy property-dictionary-details
defineBlade({ url: "/catalog-dictionary-item", name: "CatalogDictionaryItem" });

const { t } = useI18n({ useScope: "global" });
const { options, callParent, closeSelf } = useBlade<{
  item?: PropertyDictionaryItem;
  property: Property;
  languages?: string[];
  defaultLanguage?: string;
}>();

const item = ref<PropertyDictionaryItem>(
  clone({ propertyId: options.value?.property.id, localizedValues: [], sortOrder: 0, ...(options.value?.item ?? {}) }),
);
const loading = ref(false);
// legacy property-dictionary-list: only multilanguage properties have localized values, the alias is the value otherwise
const languages = computed(() =>
  !options.value?.property.multilanguage ? [] : options.value.languages?.length ? options.value.languages : ["en-US"],
);
const form = useBladeForm({ data: item, closeConfirmMessage: computed(() => t("CATALOG.ALERTS.CLOSE_CONFIRM")) });
form.setBaseline();

// a value without a language is the default language's (legacy), so it isn't typed and saved twice
const entry = (lang: string) =>
  item.value.localizedValues?.find((v) => v.languageCode === lang) ??
  (lang === (options.value?.defaultLanguage ?? languages.value[0])
    ? item.value.localizedValues?.find((v) => !v.languageCode)
    : undefined);
const valueOf = (lang: string) => entry(lang)?.value ?? "";
function setValue(lang: string, value: string) {
  const found = entry(lang);
  if (found) Object.assign(found, { languageCode: lang, value });
  else (item.value.localizedValues ??= []).push({ languageCode: lang, value });
}

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "save",
    title: t("CATALOG.TOOLBAR.SAVE"),
    icon: "lucide-save",
    permissions: "catalog:dictionary-property:edit",
    disabled: !form.canSave.value || !item.value.alias || typeof item.value.sortOrder !== "number",
    async clickHandler() {
      loading.value = true;
      try {
        item.value.localizedValues = item.value.localizedValues?.filter((v) => v.value);
        await api.dictionary.saveChanges([item.value]);
        form.setBaseline();
        callParent("reload");
        closeSelf();
      } finally {
        loading.value = false;
      }
    },
  },
]);
</script>
