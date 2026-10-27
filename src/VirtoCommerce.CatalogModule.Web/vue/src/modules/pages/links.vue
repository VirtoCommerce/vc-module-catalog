<template>
  <VcBlade
    :title="$t('CATALOG.WIDGETS.LINKS')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:selection="selection"
      selection-mode="multiple"
      edit-mode="cell"
      :items="rows"
      :state-key="'CATALOG_LINKS'"
      :empty-state="{ icon: 'lucide-folder-tree', title: $t('CATALOG.LINKS.EMPTY') }"
      @cell-edit-complete="onPriority"
    >
      <VcColumn
        id="catalogName"
        :title="$t('CATALOG.FIELDS.CATALOG')"
        body-class="catalog-list__name"
        always-visible
      />
      <VcColumn
        id="name"
        :title="$t('CATALOG.FIELDS.CATEGORY')"
      />
      <VcColumn
        id="isAutomatic"
        :title="$t('CATALOG.FIELDS.TYPE')"
      >
        <template #body="{ data }">
          {{ $t(data.isAutomatic ? "CATALOG.BROWSE.AUTO_LINK" : "CATALOG.BROWSE.LINK") }}
        </template>
      </VcColumn>
      <VcColumn
        id="priority"
        :title="$t('CATALOG.FIELDS.PRIORITY')"
        type="number"
        editable
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useAsync, useBlade, usePermissions, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import type { PickOptions } from "./browse.vue";
import type { Catalog, CategoryLink } from "../../api_client/virtocommerce.catalog";

// legacy links-list: virtual catalog/category links of a product or category, saved immediately
defineBlade({ url: "/catalog-links", name: "CatalogLinks" });

const { t } = useI18n({ useScope: "global" });
const { options, openBlade, callParent } = useBlade<{
  item: { id?: string; links?: CategoryLink[] };
  kind: "product" | "category";
}>();
const { showConfirmation } = usePopup();
const { hasAccess } = usePermissions();
// permission segment of the entity: catalog:categories:* / catalog:products:*
const type = computed(() => (options.value?.kind === "category" ? "categories" : "products"));

const catalogs = ref<Catalog[]>([]);
const selection = ref<CategoryLink[]>([]);
const rows = computed(() =>
  (options.value?.item.links ?? []).map((l) => ({
    ...l,
    id: `${l.catalogId}/${l.categoryId ?? ""}`,
    catalogName: catalogs.value.find((c) => c.id === l.catalogId)?.name ?? l.catalogId,
    name: l.categoryId ? l.name : "",
  })),
);

// the position in the virtual catalog (legacy _priority): edited here, saved with the product/category
function onPriority(e: { data: { catalogId?: string; categoryId?: string }; newValue: unknown }) {
  const priority = Number(e.newValue);
  if (Number.isNaN(priority)) return;
  options.value!.item.links = (options.value!.item.links ?? []).map((l) =>
    l.catalogId === e.data.catalogId && (l.categoryId ?? "") === (e.data.categoryId ?? "") ? { ...l, priority } : l,
  );
}

const entry = (l: { catalogId?: string; categoryId?: string }): CategoryLink => ({
  listEntryId: options.value!.item.id,
  listEntryType: options.value!.kind,
  catalogId: l.catalogId,
  categoryId: l.categoryId,
});

// the owner re-reads its links; notify: false: a failure reaches the caller as before, with no extra toast
const { loading, action: run } = useAsync(
  async (action?: () => Promise<unknown>) => {
    await action?.();
    await callParent("reload");
  },
  { notify: false },
);

function add() {
  openBlade({
    name: "CatalogBrowse",
    options: {
      pick: {
        title: t("CATALOG.LINKS.PICK"),
        mode: "folder",
        onPick: async (_, folder) => {
          if (!folder.catalog?.isVirtual) throw new Error(t("CATALOG.LINKS.VIRTUAL_ONLY"));
          if (folder.categoryId === options.value?.item.id) return;
          await run(() => api.listEntries.createLinks([entry(folder)]));
        },
      } satisfies PickOptions,
    },
  });
}

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "add",
    title: t("CATALOG.LINKS.ADD"),
    icon: "lucide-plus",
    // legacy links-list: catalog:update plus the link permission of the entity's type (the server checks the latter)
    permissions: "catalog:update",
    disabled: !hasAccess(`catalog:${type.value}:link`),
    clickHandler: add,
  },
  {
    id: "delete",
    title: t("CATALOG.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    // legacy needed catalog:update; the server's unlink checks the type's delete permission
    permissions: "catalog:update",
    isVisible: hasAccess([`catalog:${type.value}:delete`, "catalog:delete"]),
    disabled: !selection.value.length,
    async clickHandler() {
      if (!(await showConfirmation(t("CATALOG.LINKS.DELETE_CONFIRM", { count: selection.value.length })))) return;
      await run(() => api.listEntries.deleteLinks(selection.value.map(entry)));
      selection.value = [];
    },
  },
]);

onMounted(async () => {
  catalogs.value = (await api.catalogs.searchCatalogs({ take: 1000 })).results ?? [];
});
</script>
