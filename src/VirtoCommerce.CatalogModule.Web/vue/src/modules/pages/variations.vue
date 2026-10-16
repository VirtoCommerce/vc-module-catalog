<template>
  <VcBlade
    :title="$t('CATALOG.WIDGETS.VARIATIONS')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:search-value="keyword"
      v-model:selection="selection"
      selection-mode="multiple"
      :items="rows"
      :searchable="true"
      :state-key="'CATALOG_VARIATIONS'"
      :active-item-id="activeItemId"
      :empty-state="{ icon: 'lucide-git-branch', title: $t('CATALOG.VARIATIONS.EMPTY') }"
      @row-click="(e: { data: Variation }) => open(e.data.id)"
    >
      <VcColumn
        id="imgSrc"
        :title="$t('CATALOG.FIELDS.IMAGE')"
        type="image"
        width="84px"
      />
      <VcColumn
        id="name"
        :title="$t('CATALOG.FIELDS.NAME')"
        body-class="catalog-list__name"
        always-visible
      />
      <VcColumn
        id="code"
        :title="$t('CATALOG.FIELDS.SKU')"
      />
      <VcColumn
        id="manufacturerPartNumber"
        :visible="false"
        :title="$t('CATALOG.FIELDS.MPN')"
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api, assetUrl } from "../api";
import type { CatalogProduct, Variation } from "../../api_client/virtocommerce.catalog";

// legacy item-variation-list (+ new-variation wizard via getNewVariation).
// Not routable: the product arrives only through options.
defineBlade({ url: "/catalog-variations", name: "CatalogVariations", routable: false });

const { t } = useI18n({ useScope: "global" });
const { options, openBlade, closeChildren, callParent, exposeToChildren } = useBlade<{ item: CatalogProduct }>();
const { showConfirmation } = usePopup();

// the product blade's item (a getter): it reads the product with the full response group, variation images included
const variations = computed<Variation[]>(() => options.value?.item.variations ?? []);
const loading = ref(false);
const keyword = ref("");
const selection = ref<Variation[]>([]);
const activeItemId = ref<string>();
const rows = computed(() =>
  variations.value
    .filter((v) => !keyword.value || [v.name, v.code].some((s) => s?.toLowerCase().includes(keyword.value.toLowerCase())))
    .map((v) => ({ ...v, imgSrc: assetUrl(v.imgSrc) })),
);

function open(id?: string, product?: CatalogProduct) {
  activeItemId.value = id;
  openBlade({
    name: "CatalogProductDetails",
    param: id,
    options: { product },
    onClose: () => (activeItemId.value = undefined),
  });
}

// the product blade re-reads the product (variations included) and refreshes its own parent
async function reload() {
  loading.value = true;
  try {
    await callParent("reload");
  } finally {
    loading.value = false;
  }
}

const toolbar = computed<IBladeToolbar[]>(() => [
  { id: "refresh", title: t("CATALOG.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: reload },
  {
    id: "add",
    title: t("CATALOG.VARIATIONS.ADD"),
    icon: "lucide-plus",
    permissions: ["catalog:create", "catalog:products:create"],
    async clickHandler() {
      const parent = options.value?.item;
      if (!parent?.id) return;
      const variation = await api.products.getNewVariation(parent.id);
      // the server copies the parent; keep only variation-level properties (legacy new-variation wizard)
      variation.properties = (variation.properties ?? []).filter((p) => p.type === "Variation");
      variation.productType ??= parent.productType;
      open(undefined, variation);
    },
  },
  {
    id: "delete",
    title: t("CATALOG.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: ["catalog:delete", "catalog:products:delete"],
    disabled: !selection.value.length,
    async clickHandler() {
      if (!(await showConfirmation(t("CATALOG.VARIATIONS.DELETE_CONFIRM", { count: selection.value.length })))) return;
      await closeChildren();
      await api.products.deleteProduct(selection.value.map((v) => v.id!));
      selection.value = [];
      await reload();
    },
  },
]);

exposeToChildren({ reload });
</script>
