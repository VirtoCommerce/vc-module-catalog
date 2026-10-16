<template>
  <VcBlade
    :title="$t('CATALOG.WIDGETS.ASSETS')"
    :toolbar-items="toolbar"
    :loading="manager.loading.value"
    width="50%"
  >
    <div class="catalog-list-blade">
      <VcFileUpload
        class="catalog-assets__upload"
        variant="file-upload"
        accept="*"
        multiple
        @upload="(files: FileList) => manager.upload(files)"
      />
      <VcDataTable
        v-model:selection="selection"
        selection-mode="multiple"
        :items="rows"
        :state-key="'CATALOG_ASSETS'"
        :empty-state="{ icon: 'lucide-paperclip', title: $t('CATALOG.ASSETS.EMPTY') }"
        @row-click="(e: { data: object }) => edit((e.data as { _index: number })._index)"
      >
        <VcColumn
          id="name"
          :title="$t('CATALOG.FIELDS.NAME')"
          body-class="catalog-list__name"
          always-visible
        >
          <template #body="{ data }">
            <a
              :href="assetUrl(data.url)"
              target="_blank"
              rel="noopener"
            >{{ data.name }}</a>
          </template>
        </VcColumn>
        <VcColumn
          id="group"
          :title="$t('CATALOG.IMAGE.GROUP')"
        />
        <VcColumn
          id="languageCode"
          :title="$t('CATALOG.FIELDS.LANGUAGE')"
          :visible="false"
        />
        <VcColumn
          id="mimeType"
          :title="$t('CATALOG.ASSETS.TYPE')"
        />
        <VcColumn
          id="readableSize"
          :title="$t('CATALOG.ASSETS.SIZE')"
        />
        <VcColumn
          id="sortOrder"
          :visible="false"
          :title="$t('CATALOG.FIELDS.SORT_ORDER')"
        />
      </VcDataTable>
    </div>
  </VcBlade>
</template>

<script setup lang="ts">
import { readableSize, useAssetsManager, useBlade, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcFileUpload } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { assetUrl } from "../api";
import type { AssetBlob } from "./asset-picker.vue";
import type { Asset } from "../../api_client/virtocommerce.catalog";

// legacy item-asset-list: files of a product or category. Local edits: the owner's Save persists them.
defineBlade({ url: "/catalog-assets", name: "CatalogAssets", routable: false });

const { t } = useI18n({ useScope: "global" });
const { options, openBlade } = useBlade<{ item: { assets?: Asset[]; code?: string }; languages?: string[] }>();
const { showConfirmation } = usePopup();

const assets = computed({
  get: () => options.value?.item.assets ?? [],
  set: (v) => options.value && (options.value.item.assets = v as Asset[]),
});
const manager = useAssetsManager(assets, {
  uploadPath: () => `catalog/${options.value?.item.code ?? "new"}`,
  confirmRemove: () => showConfirmation(t("CATALOG.ASSETS.DELETE_CONFIRM", { count: selection.value.length })),
});
const rows = computed(() =>
  assets.value.map((a, i) => ({
    ...a,
    _index: i,
    id: a.id ?? a.url ?? String(i),
    readableSize: a.readableSize ?? (a.size ? readableSize(a.size, 1) : ""),
  })),
);
const selection = ref<Asset[]>([]);

function edit(index: number) {
  openBlade({
    name: "CatalogAssetDetails",
    options: {
      asset: assets.value[index],
      languages: options.value?.languages,
      // by url (the manager's key, not editable there): rows can be deleted or added while the details are open
      onApply: (a: Asset) => manager.updateItem(a),
    },
  });
}

// legacy "link": existing files from the platform assets
function pick() {
  openBlade({
    name: "CatalogAssetPicker",
    options: {
      onPick: (blobs: AssetBlob[]) => {
        const next = Math.max(0, ...assets.value.map((a) => a.sortOrder ?? 0)) + 1;
        assets.value = [
          ...assets.value,
          ...blobs
            .filter((b) => !assets.value.some((a) => a.url === b.url))
            .map((b, n) => ({
              url: b.url,
              relativeUrl: b.relativeUrl?.replace(/^\//, ""),
              name: b.name,
              mimeType: b.contentType,
              size: b.size,
              sortOrder: next + n,
            })),
        ];
      },
    },
  });
}

const toolbar = computed<IBladeToolbar[]>(() => [
  { id: "pick", title: t("CATALOG.IMAGE.FROM_ASSETS"), icon: "lucide-folder-search", clickHandler: pick },
  {
    id: "delete",
    title: t("CATALOG.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    disabled: !selection.value.length,
    async clickHandler() {
      const urls = new Set(selection.value.map((a) => a.url));
      await manager.removeMany(assets.value.filter((a) => urls.has(a.url)));
      selection.value = [];
    },
  },
]);
</script>

<style lang="scss">
.catalog-assets__upload {
  margin: 16px;
}
</style>
