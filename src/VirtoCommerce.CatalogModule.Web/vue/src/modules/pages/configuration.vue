<template>
  <VcBlade
    :title="$t('CATALOG.CONFIG.TITLE')"
    :subtitle="options?.item?.name"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <div class="catalog-list-blade">
      <div class="catalog-config__head">
        <VcSwitch
          v-model="item.isActive"
          :label="$t('CATALOG.CONFIG.ACTIVE')"
          :disabled="!item.sections?.length"
        />
        <div class="catalog-card__hint">{{ $t("CATALOG.CONFIG.HINT") }}</div>
      </div>
      <VcDataTable
        v-model:selection="selection"
        selection-mode="multiple"
        reorderable-rows
        :items="rows"
        :state-key="'CATALOG_CONFIG_SECTIONS'"
        :active-item-id="activeItemId"
        :empty-state="{ icon: 'lucide-puzzle', title: $t('CATALOG.CONFIG.EMPTY') }"
        @row-reorder="onReorder"
        @row-click="(e: { data: Row }) => edit(e.data._index)"
      >
        <VcColumn
          id="name"
          :title="$t('CATALOG.FIELDS.NAME')"
          body-class="catalog-list__name"
          always-visible
        />
        <VcColumn
          id="type"
          :title="$t('CATALOG.FIELDS.TYPE')"
        />
        <VcColumn
          id="isRequired"
          :title="$t('CATALOG.PROPERTY.REQUIRED')"
        >
          <template #body="{ data }">{{ data.isRequired ? "✓" : "" }}</template>
        </VcColumn>
        <VcColumn
          id="optionsCount"
          :title="$t('CATALOG.CONFIG.OPTIONS')"
        />
      </VcDataTable>
    </div>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcSwitch } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api, applyRow, indexRows, withoutRows } from "../api";
import type { CatalogProduct, ProductConfiguration, ProductConfigurationSection } from "../../api_client/virtocommerce.catalog";

// legacy product-configuration-detail: sections of a configurable product; saved as a whole
defineBlade({ url: "/catalog-configuration", name: "CatalogConfiguration", permissions: ["catalog:configurations:read"], routable: false });

type Row = ProductConfigurationSection & { _index: number; optionsCount: number };

const { t } = useI18n({ useScope: "global" });
const { options, openBlade, callParent } = useBlade<{ item: CatalogProduct }>();
const { showConfirmation } = usePopup();

const item = ref<ProductConfiguration>({ sections: [] });
const loading = ref(false);
const selection = ref<Row[]>([]);
const activeItemId = ref<string>();
const form = useBladeForm({ data: item, closeConfirmMessage: computed(() => t("CATALOG.ALERTS.CLOSE_CONFIRM")) });

const rows = computed<Row[]>(() => indexRows(item.value.sections ?? []).map((s) => ({ ...s, optionsCount: s.options?.length ?? 0 })));

async function load() {
  const productId = options.value?.item.id;
  if (!productId) return;
  loading.value = true;
  try {
    const res = await api.configurations.searchConfigurations({ productId, take: 1 });
    const found = res.results?.[0];
    item.value = found ?? { productId, isActive: false, sections: [] };
    item.value.sections = [...(item.value.sections ?? [])].sort((a, b) => (a.displayOrder ?? 0) - (b.displayOrder ?? 0));
  } finally {
    loading.value = false;
  }
  form.setBaseline();
}

// sections are renumbered in place, so an open section blade still finds its section (applyRow)
function setSections(sections: ProductConfigurationSection[]) {
  sections.forEach((s, i) => (s.displayOrder = i));
  item.value.sections = sections;
  // only a configuration with sections can be active (legacy)
  if (!sections.length) item.value.isActive = false;
}

function onReorder(e: { value: Row[] }) {
  setSections(e.value.map((r) => item.value.sections![r._index]));
}

function edit(index?: number) {
  const section = index === undefined ? undefined : item.value.sections![index];
  activeItemId.value = section ? rows.value[index!].id : undefined;
  openBlade({
    name: "CatalogConfigurationSection",
    options: {
      section,
      otherSections: (item.value.sections ?? []).filter((s) => s !== section && s.id && s.name),
      onApply: (s: ProductConfigurationSection) => setSections(applyRow(item.value.sections ?? [], section, s)),
    },
    onClose: () => (activeItemId.value = undefined),
  });
}

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "save",
    title: t("CATALOG.TOOLBAR.SAVE"),
    icon: "lucide-save",
    permissions: "catalog:configurations:update",
    disabled: !form.canSave.value,
    async clickHandler() {
      loading.value = true;
      try {
        await api.configurations.createOrUpdateConfiguration(item.value);
      } finally {
        loading.value = false;
      }
      await load();
      callParent("reload");
    },
  },
  { id: "reset", title: t("CATALOG.TOOLBAR.RESET"), icon: "lucide-undo-2", disabled: !form.isModified.value, clickHandler: load },
  {
    id: "add",
    title: t("CATALOG.CONFIG.ADD_SECTION"),
    icon: "lucide-plus",
    permissions: "catalog:configurations:update",
    clickHandler: () => edit(),
  },
  {
    id: "delete",
    title: t("CATALOG.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "catalog:configurations:delete",
    disabled: !selection.value.length,
    async clickHandler() {
      if (!(await showConfirmation(t("CATALOG.CONFIG.DELETE_CONFIRM", { count: selection.value.length })))) return;
      const deletedIds = new Set(selection.value.map((r) => r.id));
      const kept = withoutRows(item.value.sections ?? [], selection.value);
      // sections that depended on a deleted one lose the dependency (legacy)
      for (const s of kept) if (s.dependsOnSectionId && deletedIds.has(s.dependsOnSectionId)) s.dependsOnSectionId = undefined;
      setSections(kept);
      selection.value = [];
    },
  },
]);

onMounted(load);
</script>

<style lang="scss">
.catalog-config__head {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 16px 16px 0;
}
</style>
