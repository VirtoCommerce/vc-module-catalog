<template>
  <VcBlade
    :title="$t('CATALOG.WIDGETS.SEO')"
    :toolbar-items="toolbar"
    width="50%"
  >
    <VcDataTable
      v-model:selection="selection"
      selection-mode="multiple"
      :items="rows"
      :state-key="'CATALOG_SEO'"
      :active-item-id="activeItemId"
      :empty-state="{ icon: 'lucide-globe', title: $t('CATALOG.SEO.EMPTY') }"
      @row-click="(e: { data: Row }) => edit(e.data)"
    >
      <VcColumn
        id="semanticUrl"
        :title="$t('CATALOG.SEO.SLUG')"
        body-class="catalog-list__name"
        always-visible
      />
      <VcColumn
        id="storeName"
        :title="$t('CATALOG.SEO.STORE')"
      />
      <VcColumn
        id="languageCode"
        :title="$t('CATALOG.FIELDS.LANGUAGE')"
      />
      <VcColumn
        id="isActive"
        :title="$t('CATALOG.FIELDS.STATUS')"
      >
        <template #body="{ data }">
          <div class="tw-inline-flex">
            <VcStatus :variant="data.isActive ? 'success' : 'warning'">
              {{ $t(data.isActive ? "CATALOG.SEO.ACTIVE" : "CATALOG.SEO.INACTIVE") }}
            </VcStatus>
          </div>
        </template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcStatus } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { applyRow, getStores, indexRows, withoutRows, type Store } from "../api";
import type { SeoInfo } from "../../api_client/virtocommerce.catalog";

// SEO records of a catalog, category or product (legacy SEO module widget). Local edits: the owner's Save persists them.
defineBlade({ url: "/catalog-seo", name: "CatalogSeo", routable: false });

type Row = SeoInfo & { _index: number; storeName?: string };

const { t } = useI18n({ useScope: "global" });
const { showConfirmation } = usePopup();
const { options, openBlade } = useBlade<{
  item: { seoInfos?: SeoInfo[]; name?: string; id?: string; languages?: { languageCode?: string }[] };
  objectType: string;
  languages?: string[];
}>();

const stores = ref<Store[]>([]);
getStores().then((s) => (stores.value = s));
const list = computed(() => options.value?.item.seoInfos ?? []);
const rows = computed<Row[]>(() =>
  indexRows(list.value).map((s) => ({ ...s, storeName: stores.value.find((x) => x.id === s.storeId)?.name ?? s.storeId })),
);
const selection = ref<Row[]>([]);
const activeItemId = ref<string>();
const languages = computed(
  () => options.value?.languages ?? (options.value?.item.languages ?? []).map((l) => l.languageCode!).filter(Boolean),
);

function edit(row?: Row) {
  activeItemId.value = row?.id;
  const seo = row ? list.value[row._index] : undefined;
  openBlade({
    name: "CatalogSeoDetails",
    options: {
      seo,
      objectType: options.value?.objectType,
      objectId: options.value?.item.id,
      name: options.value?.item.name,
      existing: list.value.filter((_, i) => i !== row?._index).map((s) => s.semanticUrl),
      languages: languages.value,
      onApply: (s: SeoInfo) => {
        if (options.value) options.value.item.seoInfos = applyRow(list.value, seo, s);
      },
    },
    onClose: () => (activeItemId.value = undefined),
  });
}

const toolbar = computed<IBladeToolbar[]>(() => [
  { id: "add", title: t("CATALOG.SEO.ADD"), icon: "lucide-plus", clickHandler: () => edit() },
  {
    id: "delete",
    title: t("CATALOG.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    disabled: !selection.value.length,
    async clickHandler() {
      if (!options.value || !(await showConfirmation(t("CATALOG.SEO.DELETE_CONFIRM", { count: selection.value.length })))) return;
      options.value.item.seoInfos = withoutRows(list.value, selection.value);
      selection.value = [];
    },
  },
]);
</script>
