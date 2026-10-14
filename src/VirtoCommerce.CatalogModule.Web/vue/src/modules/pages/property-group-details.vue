<template>
  <VcBlade
    :title="item.name || $t('CATALOG.GROUPS.NEW')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcContainer>
      <VcForm class="catalog-form">
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
              v-model.number="item.displayOrder"
              type="number"
              :label="$t('CATALOG.FIELDS.DISPLAY_ORDER')"
              name="displayOrder"
              rules="required"
              required
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
          field="localizedDescription"
          :header="$t('CATALOG.GROUPS.LOCALIZED_DESCRIPTION')"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcCol, VcContainer, VcForm, VcInput, VcRow } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import LocalizedNames from "../components/LocalizedNames.vue";
import type { PropertyGroup } from "../../api_client/virtocommerce.catalog";

defineBlade({ url: "/catalog-property-group", name: "CatalogPropertyGroupDetails" });

const { t } = useI18n({ useScope: "global" });
const { param, options, callParent, closeSelf, replaceWith } = useBlade<{ catalogId: string }>();
const { showConfirmation } = usePopup();

const item = ref<PropertyGroup>({});
const loading = ref(false);
const languages = ref<string[]>([]);
const form = useBladeForm({ data: item, closeConfirmMessage: computed(() => t("CATALOG.ALERTS.CLOSE_CONFIRM")) });

async function load() {
  loading.value = true;
  try {
    item.value = param.value ? await api.propertyGroups.get(param.value) : { catalogId: options.value?.catalogId, displayOrder: 0 };
    const catalog = await api.catalogs.getCatalog(item.value.catalogId!);
    languages.value = (catalog.languages ?? []).map((l) => l.languageCode!);
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
    permissions: "catalog:metadata-property:edit",
    disabled: !form.canSave.value || !item.value.name || typeof item.value.displayOrder !== "number",
    async clickHandler() {
      loading.value = true;
      try {
        const saved = await api.propertyGroups.update(item.value);
        form.setBaseline();
        callParent("reload");
        if (!param.value) await replaceWith({ name: "CatalogPropertyGroupDetails", param: saved.id });
        else await load();
      } finally {
        loading.value = false;
      }
    },
  },
  { id: "reset", title: t("CATALOG.TOOLBAR.RESET"), icon: "lucide-undo-2", disabled: !form.isModified.value, clickHandler: load },
  {
    id: "delete",
    title: t("CATALOG.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    isVisible: !!param.value,
    permissions: "catalog:metadata-property:edit",
    async clickHandler() {
      if (!(await showConfirmation(t("CATALOG.GROUPS.DELETE_CONFIRM", { count: 1 })))) return;
      await api.propertyGroups.delete([item.value.id!]);
      form.setBaseline();
      callParent("reload");
      closeSelf();
    },
  },
]);

onMounted(load);
</script>
