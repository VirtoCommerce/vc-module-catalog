<template>
  <VcBlade
    :title="$t(bom ? 'CATALOG.WIDGETS.BOM' : 'CATALOG.WIDGETS.ASSOCIATIONS')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <div class="catalog-list-blade">
      <div
        v-if="!bom"
        class="catalog-assoc__group"
      >
        <VcSelect
          v-model="group"
          :label="$t('CATALOG.ASSOCIATIONS.GROUP')"
          :options="groups"
          :clearable="false"
        />
      </div>
      <VcDataTable
        v-model:selection="selection"
        selection-mode="multiple"
        :items="rows"
        :state-key="'CATALOG_ASSOCIATIONS'"
        :active-item-id="activeItemId"
        :empty-state="{ icon: 'lucide-link-2', title: $t('CATALOG.ASSOCIATIONS.EMPTY') }"
        @row-click="(e: { data: ProductAssociation }) => open(e.data)"
      >
        <VcColumn
          id="associatedObjectImg"
          :title="$t('CATALOG.FIELDS.IMAGE')"
          type="image"
          width="84px"
        />
        <VcColumn
          id="associatedObjectName"
          :title="$t('CATALOG.FIELDS.NAME')"
          body-class="catalog-list__name"
          always-visible
        />
        <VcColumn
          v-if="!bom"
          id="type"
          :visible="false"
          :title="$t('CATALOG.ASSOCIATIONS.GROUP')"
        />
        <VcColumn
          id="associatedObjectType"
          :visible="false"
          :title="$t('CATALOG.FIELDS.TYPE')"
        />
        <VcColumn
          id="tags"
          :title="$t('CATALOG.ASSOCIATIONS.TAGS')"
          :visible="false"
        >
          <template #body="{ data }">{{ (data.tags ?? []).join(", ") }}</template>
        </VcColumn>
        <VcColumn
          id="quantity"
          :title="$t('CATALOG.ASSOCIATIONS.QUANTITY')"
          type="number"
        />
        <VcColumn
          id="priority"
          :visible="false"
          :title="$t('CATALOG.FIELDS.PRIORITY')"
          type="number"
        />
      </VcDataTable>
    </div>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcSelect } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api, assetUrl, searchAllEntries, settingOptions } from "../api";
import type { PickOptions } from "./browse.vue";
import type { CatalogProduct, ProductAssociation } from "../../api_client/virtocommerce.catalog";

// Associations (legacy item-associations-list + association-wizard) and, with options.bom, the bill of materials
// (legacy bill-of-materials-list). Saved immediately through the associations API; the product blade then reloads them.
defineBlade({ url: "/catalog-associations", name: "CatalogAssociations", routable: false });

const { t } = useI18n({ useScope: "global" });
const { options, openBlade, callParent, exposeToChildren } = useBlade<{ item: CatalogProduct; bom?: boolean }>();
const activeItemId = ref<string>();
const { showConfirmation } = usePopup();

const bom = computed(() => !!options.value?.bom);
const items = ref<ProductAssociation[]>([]);
const loading = ref(false);
const selection = ref<ProductAssociation[]>([]);
const groups = ref<{ id: string; title: string }[]>([]);
const group = ref<string>();
const rows = computed(() =>
  items.value
    .filter((a) => (bom.value ? a.type === "BillOfMaterials" : a.type !== "BillOfMaterials"))
    .map((a) => ({ ...a, associatedObjectImg: assetUrl(a.associatedObjectImg) }))
    .sort((a, b) => (a.priority ?? 0) - (b.priority ?? 0)),
);

async function load() {
  const productId = options.value?.item.id;
  if (!productId) return;
  loading.value = true;
  try {
    items.value = await api.associations.getProductAssociations(productId);
  } finally {
    loading.value = false;
  }
}

async function changed() {
  await load();
  callParent("reload");
}

function open(association: ProductAssociation) {
  activeItemId.value = association.id;
  openBlade({
    name: "CatalogAssociationDetails",
    options: { association: items.value.find((a) => a.id === association.id) ?? association },
    onClose: () => (activeItemId.value = undefined),
  });
}
exposeToChildren({ reload: () => changed() });


function add() {
  openBlade({
    name: "CatalogBrowse",
    options: {
      pick: {
        title: t(bom.value ? "CATALOG.ASSOCIATIONS.PICK_BOM" : "CATALOG.ASSOCIATIONS.PICK"),
        mode: "entries",
        types: bom.value ? ["product"] : ["product", "category"],
        excludeProductType: bom.value ? "BillOfMaterials" : undefined,
        onPick: async (picked, _folder, all) => {
          // "select all" covers every page of the list, not just the rows shown
          const entries = all
            ? await searchAllEntries(all) // the pick's excludeProductType is part of the list's search
            : picked;
          const existing = new Set(rows.value.map((a) => a.associatedObjectId));
          const type = bom.value ? "BillOfMaterials" : group.value;
          await api.associations.updateAssociations(
            entries
              // a bill of materials takes products, but not another bill of materials (legacy excludeProductType)
              .filter((e) => (bom.value ? e.type === "product" && e.productType !== "BillOfMaterials" : e.type !== "catalog"))
              .filter((e) => !existing.has(e.id))
              .map((e, i) => ({
                itemId: options.value?.item.id,
                associatedObjectId: e.id,
                associatedObjectType: e.type,
                type,
                quantity: 1,
                priority: rows.value.length + i + 1,
              })),
          );
          await changed();
        },
      } satisfies PickOptions,
    },
  });
}

const toolbar = computed<IBladeToolbar[]>(() => [
  { id: "refresh", title: t("CATALOG.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: load },
  {
    id: "add",
    title: t("CATALOG.ASSOCIATIONS.ADD"),
    icon: "lucide-plus",
    // what the associations API checks (update: catalog:update, delete: catalog:delete)
    permissions: "catalog:update",
    disabled: !bom.value && !group.value,
    clickHandler: add,
  },
  {
    id: "delete",
    title: t("CATALOG.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "catalog:delete",
    disabled: !selection.value.length,
    async clickHandler() {
      if (!(await showConfirmation(t("CATALOG.ASSOCIATIONS.DELETE_CONFIRM", { count: selection.value.length })))) return;
      await api.associations.delete(selection.value.map((a) => a.id!));
      selection.value = [];
      await changed();
    },
  },
]);

// the groups only fill the group select: they load alongside the associations
onMounted(async () => {
  const groupsLoaded = settingOptions("Catalog.AssociationGroups").then((x) => {
    groups.value = x;
    group.value = x[0]?.id;
  });
  await Promise.all([groupsLoaded, load()]);
});
</script>

<style lang="scss">
.catalog-assoc__group {
  padding: 16px 16px 0;
  max-width: 360px;
}
</style>
