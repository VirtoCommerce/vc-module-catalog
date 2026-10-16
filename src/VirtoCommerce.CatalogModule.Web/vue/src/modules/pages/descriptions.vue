<template>
  <VcBlade
    :title="$t('CATALOG.WIDGETS.DESCRIPTIONS')"
    :toolbar-items="toolbar"
    width="50%"
  >
    <VcDataTable
      v-model:selection="selection"
      selection-mode="multiple"
      :items="rows"
      :state-key="'CATALOG_DESCRIPTIONS'"
      :active-item-id="activeItemId"
      :empty-state="{ icon: 'lucide-file-text', title: $t('CATALOG.DESCRIPTIONS.EMPTY') }"
      @row-click="(e: { data: Row }) => edit(e.data)"
    >
      <VcColumn
        id="type"
        :title="$t('CATALOG.FIELDS.TYPE')"
        body-class="catalog-list__name"
        always-visible
      />
      <VcColumn
        id="languageCode"
        :title="$t('CATALOG.FIELDS.LANGUAGE')"
      />
      <VcColumn
        id="preview"
        :title="$t('CATALOG.DESCRIPTIONS.CONTENT')"
        :line-clamp="2"
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { applyRow, indexRows, withoutRows } from "../api";

// Product editorial reviews / category descriptions. Local edits: the product or category Save persists them (legacy).
defineBlade({ url: "/catalog-descriptions", name: "CatalogDescriptions", routable: false });

type Description = { id?: string; content?: string; languageCode?: string; reviewType?: string; descriptionType?: string };
type Row = Description & { _index: number; type?: string; preview: string };

const { t } = useI18n({ useScope: "global" });
const { showConfirmation } = usePopup();
const { options, openBlade } = useBlade<{
  item: { reviews?: Description[]; descriptions?: Description[]; code?: string; catalogId?: string };
  kind: "product" | "category";
  languages?: string[];
}>();

const field = computed(() => (options.value?.kind === "category" ? "descriptions" : "reviews"));
const typeField = computed(() => (options.value?.kind === "category" ? "descriptionType" : "reviewType"));
const list = computed<Description[]>(() => options.value?.item[field.value] ?? []);
const rows = computed<Row[]>(() =>
  indexRows(list.value).map((d) => ({
    ...d,
    type: d[typeField.value],
    preview: (d.content ?? "").replace(/<[^>]+>/g, " ").slice(0, 200),
  })),
);
const selection = ref<Row[]>([]);
const activeItemId = ref<string>();

function edit(row?: Row) {
  activeItemId.value = row?.id;
  const description = row ? list.value[row._index] : undefined;
  openBlade({
    name: "CatalogDescriptionDetails",
    options: {
      description,
      kind: options.value?.kind,
      languages: options.value?.languages,
      assetsFolder: `catalog/${options.value?.item.code ?? ""}`,
      onApply: (d: Description) => {
        if (options.value) options.value.item[field.value] = applyRow(list.value, description, d);
      },
    },
    onClose: () => (activeItemId.value = undefined),
  });
}

const toolbar = computed<IBladeToolbar[]>(() => [
  { id: "add", title: t("CATALOG.DESCRIPTIONS.ADD"), icon: "lucide-plus", clickHandler: () => edit() },
  {
    id: "delete",
    title: t("CATALOG.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    disabled: !selection.value.length,
    async clickHandler() {
      if (!options.value || !(await showConfirmation(t("CATALOG.DESCRIPTIONS.DELETE_CONFIRM", { count: selection.value.length }))))
        return;
      options.value.item[field.value] = withoutRows(list.value, selection.value);
      selection.value = [];
    },
  },
]);
</script>
