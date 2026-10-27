<template>
  <VcBlade
    :title="item.name || $t('CATALOG.FIELDS.IMAGES')"
    :toolbar-items="toolbar"
    width="40%"
  >
    <VcContainer>
      <VcForm class="catalog-form">
        <VcImage
          :src="assetUrl(item.url)"
          size="xl"
          bordered
        />
        <VcInput
          v-model="item.name"
          :label="$t('CATALOG.FIELDS.NAME')"
          required
        />
        <VcInput
          v-model="item.altText"
          :label="$t('CATALOG.IMAGE.ALT_TEXT')"
        />
        <VcTextarea
          v-model="item.description"
          :label="$t('CATALOG.IMAGE.DESCRIPTION')"
        />
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcSelect
              v-model="item.group"
              :label="$t('CATALOG.IMAGE.GROUP')"
              :options="groups"
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
        <VcField
          :label="$t('CATALOG.IMAGE.URL')"
          :model-value="item.url"
          copyable
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import {
  VcBlade,
  VcCol,
  VcContainer,
  VcField,
  VcForm,
  VcImage,
  VcInput,
  VcRow,
  VcSelect,
  VcTextarea,
} from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { assetUrl, clone, options as toOptions, settingOptions } from "../api";
import type { Image } from "../../api_client/virtocommerce.catalog";

// legacy image-detail: local edit of one image, applied to the gallery of the owner blade
defineBlade({ url: "/catalog-image", name: "CatalogImageDetails", routable: false });

const { t } = useI18n({ useScope: "global" });
const { options, closeSelf } = useBlade<{ image: Image; languages?: string[]; onSave: (i: Image) => void }>();

const item = ref<Image>(clone(options.value?.image ?? {}));
const groups = ref<{ id: string; title: string }[]>([]);
const languageOptions = computed(() => toOptions(options.value?.languages ?? []));
const form = useBladeForm({ data: item });
form.setBaseline();

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "apply",
    title: t("CATALOG.TOOLBAR.APPLY"),
    icon: "lucide-check",
    disabled: !form.isModified.value || !item.value.name,
    clickHandler() {
      options.value?.onSave({ ...item.value });
      form.setBaseline();
      closeSelf();
    },
  },
]);

onMounted(async () => {
  groups.value = await settingOptions("Catalog.ImageCategories");
});
</script>
