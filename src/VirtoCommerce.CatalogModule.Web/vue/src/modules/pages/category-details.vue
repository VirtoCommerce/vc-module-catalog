<template>
  <VcBlade
    :title="item.name || $t('CATALOG.CATEGORY.NEW')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="70%"
  >
    <VcContainer>
      <VcForm class="catalog-form">
        <ImageGallery
          :entity="item"
          :languages="languages"
        />
        <VcBanner
          v-if="item.id && item.parentIsActive === false"
          variant="warning"
          icon="lucide-eye-off"
        >
          {{ $t("CATALOG.CATEGORY.PARENT_HIDDEN") }}
        </VcBanner>
        <VcRow class="tw-gap-4">
          <VcCol :size="2">
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
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcSwitch
              v-model="item.isActive"
              :label="$t('CATALOG.FIELDS.VISIBLE')"
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model.number="item.priority"
              type="number"
              :label="$t('CATALOG.FIELDS.PRIORITY')"
            />
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="item.taxType"
              :label="$t('CATALOG.FIELDS.TAX_TYPE')"
              :options="taxTypes"
            />
          </VcCol>
        </VcRow>

        <LocalizedNames
          :entity="item"
          :languages="languages"
        />

        <PropertyValues
          v-if="item.id"
          :entity="item"
          :types="['Category']"
          :header="$t('CATALOG.CATEGORY.PROPERTIES')"
          :languages="languages"
          :default-language="catalog?.defaultLanguage?.languageCode"
          :edit-handler="editProperty"
          :manage-handler="manageProperties"
        />

        <VcSelect
          v-if="item.id"
          v-model="excluded"
          :label="$t('CATALOG.CATEGORY.EXCLUDED')"
          :tooltip="$t('CATALOG.CATEGORY.EXCLUDED_HINT')"
          :options="options((item.properties ?? []).map((p) => p.name!))"
          multiple
          searchable
        />

        <VcCard
          :header="$t('CATALOG.PRODUCT.SYSTEM')"
          is-collapsable
          is-collapsed
        >
          <div class="catalog-card">
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcField
                  :label="$t('CATALOG.FIELDS.ID')"
                  :model-value="item.id"
                  copyable
                />
              </VcCol>
              <VcCol>
                <VcInput
                  v-model="item.outerId"
                  :label="$t('CATALOG.FIELDS.OUTER_ID')"
                />
              </VcCol>
            </VcRow>
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcField
                  :label="$t('CATALOG.FIELDS.CATALOG')"
                  :model-value="catalog?.name"
                />
              </VcCol>
              <VcCol>
                <VcField
                  :label="$t('CATALOG.FIELDS.PATH')"
                  :model-value="item.path"
                />
              </VcCol>
            </VcRow>
          </div>
        </VcCard>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import {
  defineBladeContext,
  useBlade,
  useBladeForm,
  useBladeWidgets,
  usePopup,
  type IBladeToolbar,
} from "@vc-shell/framework";
import {
  VcBanner,
  VcBlade,
  VcCard,
  VcCol,
  VcContainer,
  VcField,
  VcForm,
  VcInput,
  VcRow,
  VcSelect,
  VcSwitch,
} from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api, catalogLanguages, http, installedModules, keepValues, options, settingOptions } from "../api";
import PropertyValues from "../components/PropertyValues.vue";
import ImageGallery from "../components/ImageGallery.vue";
import LocalizedNames from "../components/LocalizedNames.vue";
import type { Catalog, Category, Property } from "../../api_client/virtocommerce.catalog";

defineBlade({ url: "/catalog-category", name: "CatalogCategoryDetails" });

const { t } = useI18n({ useScope: "global" });
const { param, options: bladeOptions, openBlade, callParent, closeSelf, replaceWith, exposeToChildren } = useBlade<{
  catalogId?: string;
  parentId?: string;
}>();
const { showConfirmation } = usePopup();

const item = ref<Category>({});
const catalog = ref<Catalog>();
const loading = ref(false);
const taxTypes = ref<{ id: string; title: string }[]>([]);
const modules = ref<string[]>([]);


const languages = computed(() => catalogLanguages(catalog.value));

// legacy property-list (category): properties excluded from inheritance by the category's children
const excluded = computed<string[]>({
  get: () => (item.value.excludedProperties ?? []).map((p) => p.name!),
  // keep the existing entries: inherited ones stay flagged, so the server doesn't store them as this category's own
  set: (names) =>
    (item.value.excludedProperties = names.map((name) => item.value.excludedProperties?.find((p) => p.name === name) ?? { name })),
});

const form = useBladeForm({ data: item, closeConfirmMessage: computed(() => t("CATALOG.ALERTS.CLOSE_CONFIRM")) });


async function load() {
  loading.value = true;
  try {
    if (param.value) item.value = await api.categories.getCategory(param.value);
    else {
      item.value = await api.categories.getNewCategory(bladeOptions.value!.catalogId!, bladeOptions.value?.parentId);
      item.value.isActive ??= true;
    }
    if (item.value.catalogId && catalog.value?.id !== item.value.catalogId)
      catalog.value = await api.catalogs.getCatalog(item.value.catalogId);
  } finally {
    loading.value = false;
  }
  form.setBaseline();
}

async function save() {
  loading.value = true;
  try {
    const isNew = !item.value.id;
    // the legacy endpoint: unlike the batch one, it gives a new category its SEO slug
    const saved = await http<Category>("POST", "/api/catalog/categories", item.value);
    form.setBaseline();
    callParent("reload");
    if (isNew) {
      await replaceWith({ name: "CatalogCategoryDetails", param: saved.id });
      return;
    }
    item.value = await api.categories.getCategory(saved.id!);
    form.setBaseline();
  } finally {
    loading.value = false;
  }
}

function editProperty(property: Property) {
  openBlade({ name: "CatalogPropertyDetails", param: property.id, options: { languages: languages.value } });
}
function manageProperties() {
  openBlade({ name: "CatalogPropertyList", options: { catalogId: item.value.catalogId, categoryId: item.value.id } });
}

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "save",
    title: t("CATALOG.TOOLBAR.SAVE"),
    icon: "lucide-save",
    permissions: item.value.id
      ? ["catalog:update", "catalog:categories:update"]
      : ["catalog:create", "catalog:categories:create"],
    disabled: !form.canSave.value || !item.value.name || !item.value.code,
    clickHandler: save,
  },
  {
    id: "reset",
    title: t("CATALOG.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    disabled: !form.isModified.value,
    clickHandler: load,
  },
  {
    id: "reindex",
    title: t("CATALOG.CATEGORY.REINDEX"),
    icon: "lucide-refresh-ccw-dot",
    isVisible: !!item.value.id && modules.value.includes("VirtoCommerce.Search"),
    permissions: "search:index:rebuild",
    async clickHandler() {
      if (!(await showConfirmation(t("CATALOG.CATEGORY.REINDEX_CONFIRM")))) return;
      await api.categories.indexCategoryProducts(item.value.id!);
    },
  },
  {
    id: "delete",
    title: t("CATALOG.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    isVisible: !!item.value.id,
    permissions: ["catalog:delete", "catalog:categories:delete"],
    async clickHandler() {
      if (!(await showConfirmation(t("CATALOG.ALERTS.DELETE_CATEGORY", { name: item.value.name })))) return;
      await api.categories.deleteCategory([item.value.id!]);
      form.setBaseline();
      callParent("reload");
      closeSelf();
    },
  },
]);

const saved = computed(() => !!item.value.id);
// item is a getter: save and reset replace the category, and open children must follow (legacy x.refresh(blade.currentEntity))
const open = (name: string, extra: Record<string, unknown> = {}) =>
  openBlade({
    name,
    options: {
      get item() {
        return item.value;
      },
      kind: "category",
      catalog: catalog.value,
      languages: languages.value,
      ...extra,
    },
  });
const { refreshAll } = useBladeWidgets([
  {
    id: "CatalogCategoryProperties",
    icon: "lucide-list-tree",
    title: "CATALOG.WIDGETS.PROPERTIES",
    badge: computed(() => item.value.properties?.length ?? 0),
    isVisible: saved,
    onClick: manageProperties,
  },
  {
    id: "CatalogDescriptions",
    icon: "lucide-file-text",
    title: "CATALOG.WIDGETS.DESCRIPTIONS",
    badge: computed(() => item.value.descriptions?.length ?? 0),
    onClick: () => open("CatalogDescriptions"),
  },
  {
    id: "CatalogSeo",
    icon: "lucide-globe",
    title: "CATALOG.WIDGETS.SEO",
    badge: computed(() => item.value.seoInfos?.length ?? 0),
    onClick: () => open("CatalogSeo", { objectType: "Category" }),
  },
  {
    id: "CatalogLinks",
    icon: "lucide-folder-tree",
    title: "CATALOG.WIDGETS.LINKS",
    badge: computed(() => item.value.links?.length ?? 0),
    isVisible: saved,
    onClick: () => open("CatalogLinks"),
  },
  {
    id: "CatalogAssets",
    icon: "lucide-paperclip",
    title: "CATALOG.WIDGETS.ASSETS",
    badge: computed(() => item.value.assets?.length ?? 0),
    onClick: () => open("CatalogAssets"),
  },
  {
    id: "CatalogAutoLinks",
    icon: "lucide-wand-sparkles",
    title: "CATALOG.WIDGETS.AUTO_LINKS",
    isVisible: computed(() => saved.value && !!catalog.value?.isVirtual),
    onClick: () => open("CatalogAutoLinks"),
  },
]);

// the category, for widgets other modules add to this blade
defineBladeContext({ item });

onMounted(async () => {
  const modulesLoaded = installedModules().then((m) => {
    modules.value = m;
    refreshAll();
  });
  // the tax types only fill a select: they load alongside the category
  await Promise.all([settingOptions("VirtoCommerce.Core.General.TaxTypes").then((x) => (taxTypes.value = x)), load(), modulesLoaded]);
});

exposeToChildren({
  async reload() {
    if (!item.value.id) return;
    const wasModified = form.isModified.value;
    const fresh = await api.categories.getCategory(item.value.id);
    item.value.properties = wasModified ? keepValues(fresh.properties, item.value.properties) : fresh.properties;
    item.value.links = fresh.links;
    if (!wasModified) form.setBaseline();
    callParent("reload");
  },
});
</script>
