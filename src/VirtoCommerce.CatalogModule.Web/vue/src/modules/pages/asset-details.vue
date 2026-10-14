<template>
  <VcBlade
    :title="item.name || $t('CATALOG.WIDGETS.ASSETS')"
    :toolbar-items="toolbar"
    width="40%"
  >
    <VcContainer>
      <VcForm class="catalog-form">
        <VcField
          :label="$t('CATALOG.IMAGE.URL')"
          :model-value="item.url"
          copyable
        />
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="item.group"
              :label="$t('CATALOG.IMAGE.GROUP')"
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
        <VcTextarea
          v-model="item.description"
          :label="$t('CATALOG.IMAGE.DESCRIPTION')"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcCol, VcContainer, VcField, VcForm, VcInput, VcRow, VcSelect, VcTextarea } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { clone, options as toOptions } from "../api";
import type { Asset } from "../../api_client/virtocommerce.catalog";

// legacy item-asset-detail: group, language and description of a product/category file (saved with the owner)
defineBlade({ url: "/catalog-asset", name: "CatalogAssetDetails", routable: false });

const { t } = useI18n({ useScope: "global" });
const { options, closeSelf } = useBlade<{ asset: Asset; languages?: string[]; onApply: (a: Asset) => void }>();

const item = ref<Asset>(clone(options.value?.asset ?? {}));
const languageOptions = computed(() => toOptions(options.value?.languages ?? []));
const form = useBladeForm({ data: item });
form.setBaseline();

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "apply",
    title: t("CATALOG.TOOLBAR.APPLY"),
    icon: "lucide-check",
    disabled: !form.isModified.value,
    clickHandler() {
      options.value?.onApply({ ...item.value });
      form.setBaseline();
      closeSelf();
    },
  },
]);
</script>
