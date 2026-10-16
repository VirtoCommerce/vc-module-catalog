<template>
  <VcBlade
    :title="item.name || $t(isVirtual ? 'CATALOG.CATALOGS.NEW_VIRTUAL' : 'CATALOG.CATALOGS.NEW')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcContainer>
      <VcForm class="catalog-form">
        <VcInput
          v-model="item.name"
          :label="$t('CATALOG.FIELDS.NAME')"
          required
          rules="required"
          name="name"
        />
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcSelect
              v-model="defaultLanguage"
              :label="$t('CATALOG.FIELDS.DEFAULT_LANGUAGE')"
              :options="languageOptions"
              :clearable="false"
              required
            />
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="otherLanguages"
              :label="$t('CATALOG.FIELDS.LANGUAGES')"
              :options="languageOptions.filter((l) => l.id !== defaultLanguage)"
              multiple
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcField
              :label="$t('CATALOG.FIELDS.TYPE')"
              :model-value="$t(isVirtual ? 'CATALOG.CATALOGS.VIRTUAL' : 'CATALOG.CATALOGS.REAL')"
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model="item.outerId"
              :label="$t('CATALOG.FIELDS.OUTER_ID')"
            />
          </VcCol>
        </VcRow>
        <VcField
          v-if="item.id"
          :label="$t('CATALOG.FIELDS.ID')"
          :model-value="item.id"
          copyable
        />
        <PropertyValues
          v-if="item.id"
          :entity="item"
          :types="['Catalog']"
          :header="$t('CATALOG.CATALOGS.PROPERTIES')"
          :languages="(item.languages ?? []).map((l) => l.languageCode!)"
          :default-language="defaultLanguage"
          :edit-handler="(p) => openBlade({ name: 'CatalogPropertyDetails', param: p.id })"
          :manage-handler="() => openBlade({ name: 'CatalogPropertyList', options: { catalogId: item.id } })"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { defineBladeContext, useBlade, useBladeForm, useBladeWidgets, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcCol, VcContainer, VcField, VcForm, VcInput, VcRow, VcSelect } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api, keepValues, settingOptions } from "../api";
import PropertyValues from "../components/PropertyValues.vue";
import type { Catalog, CatalogLanguage } from "../../api_client/virtocommerce.catalog";

defineBlade({ url: "/catalog-details", name: "CatalogDetails" });

const { t } = useI18n({ useScope: "global" });
const { param, options: bladeOptions, openBlade, callParent, closeSelf, exposeToChildren } = useBlade<{ isVirtual?: boolean }>();
const { showConfirmation } = usePopup();

const item = ref<Catalog>({});
const loading = ref(false);
const languageOptions = ref<{ id: string; title: string }[]>([]);
const isVirtual = computed(() => item.value.isVirtual ?? !!bladeOptions.value?.isVirtual);

const form = useBladeForm({ data: item, closeConfirmMessage: computed(() => t("CATALOG.ALERTS.CLOSE_CONFIRM")) });

// languages are edited as "default + others" and stored as CatalogLanguage[] (legacy catalog-languages blade)
const defaultLanguage = computed({
  get: () => item.value.languages?.find((l) => l.isDefault)?.languageCode,
  // the old default stays as one of the other languages
  set: (code) => setLanguages(code, (item.value.languages ?? []).map((l) => l.languageCode!)),
});
const otherLanguages = computed<string[]>({
  get: () => (item.value.languages ?? []).filter((l) => !l.isDefault).map((l) => l.languageCode!),
  set: (codes) => setLanguages(defaultLanguage.value, codes),
});
function setLanguages(def: string | undefined, others: string[]) {
  const old = item.value.languages ?? [];
  const keep = (code: string, isDefault: boolean): CatalogLanguage => ({
    ...(old.find((l) => l.languageCode === code) ?? { languageCode: code, catalogId: item.value.id }),
    isDefault,
  });
  item.value.languages = [...(def ? [keep(def, true)] : []), ...others.filter((c) => c !== def).map((c) => keep(c, false))];
}

async function load() {
  loading.value = true;
  try {
    if (param.value) item.value = await api.catalogs.getCatalog(param.value);
    else {
      item.value = await (bladeOptions.value?.isVirtual ? api.catalogs.getNewVirtualCatalog() : api.catalogs.getNewCatalog());
      if (!item.value.languages?.length) setLanguages("en-US", []);
    }
  } finally {
    loading.value = false;
  }
  form.setBaseline();
}

async function save() {
  loading.value = true;
  try {
    if (item.value.id) await api.catalogs.updateCatalog(item.value);
    else item.value = await api.catalogs.createCatalog(item.value);
    item.value = await api.catalogs.getCatalog(item.value.id!);
    form.setBaseline();
    callParent("reload");
  } finally {
    loading.value = false;
  }
}

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "save",
    title: t("CATALOG.TOOLBAR.SAVE"),
    icon: "lucide-save",
    permissions: item.value.id ? "catalog:update" : "catalog:create",
    disabled: !form.canSave.value || !item.value.name || !defaultLanguage.value,
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
    id: "delete",
    title: t("CATALOG.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "catalog:delete",
    isVisible: !!item.value.id,
    async clickHandler() {
      if (!(await showConfirmation(t("CATALOG.CATALOGS.DELETE_CONFIRM", { count: 1 })))) return;
      await api.catalogs.deleteCatalog(item.value.id!);
      form.setBaseline();
      callParent("reload");
      closeSelf();
    },
  },
]);

const saved = computed(() => !!item.value.id);
useBladeWidgets([
  {
    id: "CatalogProperties",
    icon: "lucide-list-tree",
    title: "CATALOG.WIDGETS.PROPERTIES",
    badge: computed(() => item.value.properties?.length ?? 0),
    isVisible: saved,
    onClick: () => openBlade({ name: "CatalogPropertyList", options: { catalogId: item.value.id } }),
  },
  {
    id: "CatalogPropertyGroups",
    icon: "lucide-group",
    title: "CATALOG.WIDGETS.PROPERTY_GROUPS",
    badge: computed(() => item.value.propertyGroups?.length ?? 0),
    isVisible: saved,
    onClick: () => openBlade({ name: "CatalogPropertyGroups", options: { catalogId: item.value.id } }),
  },
  {
    id: "CatalogSeo",
    icon: "lucide-globe",
    title: "CATALOG.WIDGETS.SEO",
    badge: computed(() => item.value.seoInfos?.length ?? 0),
    // item is a getter: save and reset replace the catalog, and the open SEO blade must follow
    onClick: () =>
      openBlade({
        name: "CatalogSeo",
        options: {
          get item() {
            return item.value;
          },
          objectType: "Catalog",
        },
      }),
  },
]);

// the catalog, for widgets other modules add to this blade
defineBladeContext({ item });

// the language list only fills the selects: it loads alongside the catalog
onMounted(async () => {
  await Promise.all([settingOptions("VirtoCommerce.Core.General.Languages").then((x) => (languageOptions.value = x)), load()]);
});

exposeToChildren({
  async reload() {
    if (!item.value.id) return;
    const wasModified = form.isModified.value;
    const fresh = await api.catalogs.getCatalog(item.value.id);
    // keep unsaved edits (property values too), refresh what child blades change on the server
    item.value.properties = wasModified ? keepValues(fresh.properties, item.value.properties) : fresh.properties;
    item.value.propertyGroups = fresh.propertyGroups;
    if (!wasModified) form.setBaseline();
  },
});
</script>
