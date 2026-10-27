<template>
  <VcBlade
    :title="$t('CATALOG.IMAGE.ADD_LINK')"
    :toolbar-items="toolbar"
    width="40%"
  >
    <VcContainer>
      <VcForm class="catalog-form">
        <VcInput
          v-model="item.url"
          :label="$t('CATALOG.IMAGE.URL')"
          placeholder="https://"
          required
        />
        <VcImage
          v-if="item.url"
          :src="item.url"
          size="xl"
          bordered
        />
        <VcInput
          v-model="item.name"
          :label="$t('CATALOG.FIELDS.NAME')"
          :placeholder="fileName"
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
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcCol, VcContainer, VcForm, VcImage, VcInput, VcRow, VcSelect } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { options as toOptions, settingOptions } from "../api";
import type { Image } from "../../api_client/virtocommerce.catalog";

// legacy images-add-external-link: an image hosted elsewhere, added to the owner's gallery (saved with it)
defineBlade({ url: "/catalog-image-link", name: "CatalogImageLink", permissions: ["catalog:add-external-image"], routable: false });

const { t } = useI18n({ useScope: "global" });
const { options, closeSelf } = useBlade<{ languages?: string[]; onApply: (i: Image) => void }>();

const item = ref<Image>({ group: "images" });
const groups = ref<{ id: string; title: string }[]>([]);
const languageOptions = computed(() => toOptions(options.value?.languages ?? []));
const fileName = computed(() => {
  const name = (item.value.url ?? "").split("?")[0].split("/").pop() ?? "";
  // a bare % (".../100%cotton.jpg", or while typing "%20") is not an escape
  try {
    return decodeURIComponent(name);
  } catch {
    return name;
  }
});
const valid = computed(() => /^https?:\/\/\S+$/i.test(item.value.url ?? ""));

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "apply",
    title: t("CATALOG.TOOLBAR.APPLY"),
    icon: "lucide-check",
    disabled: !valid.value,
    clickHandler() {
      options.value?.onApply({ ...item.value, name: item.value.name || fileName.value, relativeUrl: item.value.url });
      closeSelf();
    },
  },
]);

onMounted(async () => {
  groups.value = await settingOptions("Catalog.ImageCategories");
});
</script>
