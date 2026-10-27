<template>
  <VcBlade
    :title="$t('CATALOG.STORE.BRANDS')"
    :subtitle="options?.store?.name"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcContainer>
      <VcForm class="catalog-form">
        <VcSwitch
          v-model="item.brandsEnabled"
          :label="$t('CATALOG.STORE.BRANDS_ENABLED')"
        />
        <VcSelect
          v-model="item.brandCatalogId"
          :label="$t('CATALOG.STORE.BRAND_CATALOG')"
          :options="catalogs"
          option-value="id"
          option-label="name"
        />
        <VcSelect
          v-model="item.brandPropertyName"
          :label="$t('CATALOG.STORE.BRAND_PROPERTY')"
          :options="propertyNames"
          searchable
        />
        <div>
          <VcButton
            variant="secondary"
            icon="lucide-globe"
            :disabled="!item.brandCatalogId"
            @click="openSeo"
          >
            {{ $t("CATALOG.STORE.BRAND_SEO", { count: brandCatalog?.seoInfos?.length ?? 0 }) }}
          </VcButton>
        </div>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcButton, VcContainer, VcForm, VcSelect, VcSwitch } from "@vc-shell/framework/ui";
import { computed, onMounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { api, http, options as toOptions, type Store } from "../api";
import type { BrandStoreSetting, Catalog } from "../../api_client/virtocommerce.catalog";

// legacy brand-setting-details (catalog widget on the Store blade)
defineBlade({ url: "/catalog-store-brands", name: "CatalogStoreBrands", permissions: ["catalog:read"], routable: false });

const { t } = useI18n({ useScope: "global" });
const { options, openBlade } = useBlade<{ store: Store }>();


const item = ref<BrandStoreSetting>({});
const catalogs = ref<Catalog[]>([]);
const propertyNames = ref<{ id: string; title: string }[]>([]);
const loading = ref(false);
const form = useBladeForm({ data: item, closeConfirmMessage: computed(() => t("CATALOG.ALERTS.CLOSE_CONFIRM")) });

// SEO of the selected brand catalog (legacy brandsSettingsCatalogDetail SEO widget): edited locally, saved with the settings
const brandCatalog = ref<Catalog>();
let brandSeoSnapshot = "";
const seoModified = computed(() => !!brandCatalog.value && JSON.stringify(brandCatalog.value.seoInfos ?? []) !== brandSeoSnapshot);
async function loadBrandCatalog(id?: string) {
  brandCatalog.value = id ? await api.catalogs.getCatalog(id) : undefined;
  brandSeoSnapshot = JSON.stringify(brandCatalog.value?.seoInfos ?? []);
}
watch(() => item.value.brandCatalogId, (id) => loadBrandCatalog(id));
function openSeo() {
  if (brandCatalog.value) openBlade({ name: "CatalogSeo", options: { item: brandCatalog.value, objectType: "Catalog" } });
}
const storeId = computed(() => options.value?.store.id ?? "");

async function load() {
  loading.value = true;
  try {
    item.value = (await api.brandSettings.getByStoreId(storeId.value)) ?? { storeId: storeId.value };
    item.value.storeId ??= storeId.value;
  } finally {
    loading.value = false;
  }
  form.setBaseline();
}

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "save",
    title: t("CATALOG.TOOLBAR.SAVE"),
    icon: "lucide-save",
    permissions: "catalog:update",
    disabled: !form.canSave.value && !seoModified.value,
    async clickHandler() {
      loading.value = true;
      try {
        if (seoModified.value) await api.catalogs.updateCatalog(brandCatalog.value);
        await api.brandSettings.update({ ...item.value, store: undefined });
        await loadBrandCatalog(item.value.brandCatalogId);
      } finally {
        loading.value = false;
      }
      await load();
    },
  },
  {
    id: "reset",
    title: t("CATALOG.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    disabled: !form.isModified.value && !seoModified.value,
    async clickHandler() {
      await load();
      await loadBrandCatalog(item.value.brandCatalogId);
    },
  },
]);

// the catalogs and property names only fill selects: they load alongside the settings
onMounted(async () => {
  if (!storeId.value) return;
  await Promise.all([
    api.catalogs.searchCatalogs({ take: 1000, sort: "name" }).then((c) => (catalogs.value = c.results ?? [])),
    http<{ name?: string }[]>("GET", `/api/catalog/aggregationproperties/${storeId.value}/properties`)
      .catch(() => [])
      .then((props) => (propertyNames.value = toOptions((props ?? []).map((p) => p.name!).filter(Boolean)))),
    load(),
  ]);
});
</script>
