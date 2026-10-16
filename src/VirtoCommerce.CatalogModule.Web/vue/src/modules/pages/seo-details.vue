<template>
  <VcBlade
    :title="item.semanticUrl || $t('CATALOG.SEO.NEW')"
    :toolbar-items="toolbar"
    width="50%"
  >
    <VcContainer>
      <VcForm class="catalog-form">
        <VcInput
          v-model="item.semanticUrl"
          :label="$t('CATALOG.SEO.SLUG')"
          name="semanticUrl"
          rules="required"
          required
          :hint="$t('CATALOG.SEO.SLUG_HINT')"
          :error="invalidSlug"
          :error-message="invalidSlug ? $t('CATALOG.SEO.SLUG_INVALID') : undefined"
        />
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcSelect
              v-model="item.storeId"
              :label="$t('CATALOG.SEO.STORE')"
              :options="stores"
              option-value="id"
              option-label="name"
            />
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="item.languageCode"
              :label="$t('CATALOG.FIELDS.LANGUAGE')"
              :options="languageOptions"
            />
          </VcCol>
          <VcCol>
            <VcSwitch
              v-model="item.isActive"
              :label="$t('CATALOG.SEO.ACTIVE')"
            />
          </VcCol>
        </VcRow>
        <VcInput
          v-model="item.pageTitle"
          :label="$t('CATALOG.SEO.PAGE_TITLE')"
        />
        <VcTextarea
          v-model="item.metaDescription"
          :label="$t('CATALOG.SEO.META_DESCRIPTION')"
        />
        <VcInput
          v-model="item.metaKeywords"
          :label="$t('CATALOG.SEO.META_KEYWORDS')"
        />
        <VcInput
          v-model="item.imageAltDescription"
          :label="$t('CATALOG.SEO.IMAGE_ALT')"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcCol, VcContainer, VcForm, VcInput, VcRow, VcSelect, VcSwitch, VcTextarea } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { clone, getStores, options as toOptions, slug, type Store } from "../api";
import type { SeoInfo } from "../../api_client/virtocommerce.catalog";

defineBlade({ url: "/catalog-seo-details", name: "CatalogSeoDetails", routable: false });

const { t } = useI18n({ useScope: "global" });
const { options, closeSelf } = useBlade<{
  seo?: SeoInfo;
  objectType: string;
  objectId?: string;
  name?: string;
  /** slugs already used by the owner's other records */
  existing?: string[];
  languages?: string[];
  onApply: (s: SeoInfo) => void;
}>();

// a new record gets a slug from the name, like the legacy wizard's getslug
function freeSlug(name?: string) {
  const base = slug(name);
  const taken = new Set(options.value?.existing ?? []);
  let candidate = base;
  let i = 2;
  while (candidate && taken.has(candidate)) {
    candidate = `${base}-${i}`;
    i++;
  }
  return candidate;
}
const item = ref<SeoInfo>(
  clone(
    options.value?.seo ?? {
      semanticUrl: freeSlug(options.value?.name),
      isActive: true,
      languageCode: options.value?.languages?.[0],
      objectType: options.value?.objectType,
      objectId: options.value?.objectId,
    },
  ),
);
const stores = ref<Store[]>([]);
getStores().then((s) => (stores.value = s));
const languageOptions = computed(() => toOptions(options.value?.languages ?? []));
const form = useBladeForm({ data: item, closeConfirmMessage: computed(() => t("CATALOG.ALERTS.CLOSE_CONFIRM")) });
form.setBaseline();
// legacy seoUtils.isValidSemanticUrl (the server doesn't check): these characters break storefront routing
const invalidSlug = computed(() => /[$+;=%{}[\]|\\/@ ~#!^*&?:'<>,]/.test(item.value.semanticUrl ?? ""));

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "apply",
    title: t("CATALOG.TOOLBAR.APPLY"),
    icon: "lucide-check",
    disabled: !item.value.semanticUrl || invalidSlug.value || !(form.isModified.value || !options.value?.seo),
    clickHandler() {
      options.value?.onApply({ ...item.value });
      form.setBaseline();
      closeSelf();
    },
  },
]);
</script>
