<template>
  <VcBlade
    :title="$t('CATALOG.ASSETS.PICK_TITLE')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <div class="catalog-list-blade">
      <div class="catalog-browse__crumbs">
        <VcBreadcrumbs
          :items="crumbs"
          separated
        />
      </div>
      <VcDataTable
        v-model:search-value="keyword"
        v-model:selection="selection"
        class="catalog-browse__table"
        selection-mode="multiple"
        :items="items"
        :searchable="true"
        :state-key="'CATALOG_ASSET_PICKER'"
        :is-row-selectable="(b: AssetBlob) => b.type === 'blob'"
        :empty-state="{ icon: 'lucide-folder-open', title: $t('CATALOG.ASSETS.EMPTY') }"
        @row-click="(e: { data: Row }) => e.data.type === 'folder' && open(e.data._blob)"
      >
        <VcColumn
          id="url"
          :title="$t('CATALOG.FIELDS.IMAGE')"
          type="image"
          width="84px"
          always-visible
        />
        <VcColumn
          id="name"
          :title="$t('CATALOG.FIELDS.NAME')"
          always-visible
        >
          <template #body="{ data }">
            <span class="catalog-browse__name">
              <VcIcon
                v-if="data.type === 'folder'"
                icon="lucide-folder"
                size="s"
              />
              {{ data.name }}
            </span>
          </template>
        </VcColumn>
        <VcColumn
          id="contentType"
          :title="$t('CATALOG.ASSETS.TYPE')"
        />
      </VcDataTable>
    </div>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useFunctions, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcBreadcrumbs, VcColumn, VcDataTable, VcIcon } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { assetUrl, http } from "../api";

// legacy assetsModule.assetSelectController: pick existing files from the platform assets
defineBlade({ url: "/catalog-asset-picker", name: "CatalogAssetPicker", routable: false });

export interface AssetBlob {
  type?: "folder" | "blob";
  name?: string;
  url?: string;
  relativeUrl?: string;
  contentType?: string;
  size?: number;
}

const { t } = useI18n({ useScope: "global" });
const { options, closeSelf } = useBlade<{ imagesOnly?: boolean; onPick: (blobs: AssetBlob[]) => void }>();
const { debounce } = useFunctions();

type Row = AssetBlob & { id?: string; _blob: AssetBlob };

// opened folders from the root (legacy starts there); each is listed by its own url, as the listing returned it
const folders = ref<AssetBlob[]>([]);
const all = ref<AssetBlob[]>([]);
const keyword = ref("");
const selection = ref<Row[]>([]);
const loading = ref(false);
const items = computed<Row[]>(() =>
  all.value
    .filter((b) => b.type === "folder" || !options.value?.imagesOnly || b.contentType?.startsWith("image/"))
    .map((b) => ({ ...b, id: b.url, url: b.type === "blob" ? assetUrl(b.url) : undefined, _blob: b })),
);
const crumbs = computed(() => [
  { id: "/", title: "assets", clickHandler: () => goTo(0) },
  ...folders.value.map((f, i) => ({ id: f.url ?? String(i), title: f.name, clickHandler: () => goTo(i + 1) })),
]);

async function load() {
  loading.value = true;
  try {
    const query = new URLSearchParams();
    const folderUrl = folders.value[folders.value.length - 1]?.url;
    if (folderUrl) query.set("folderUrl", folderUrl);
    if (keyword.value) query.set("keyword", keyword.value);
    const res = await http<{ results?: AssetBlob[] }>("GET", `/api/assets?${query}`);
    all.value = res.results ?? [];
  } finally {
    loading.value = false;
  }
}
load();
watch(keyword, debounce(load, 300));

function open(b: AssetBlob) {
  folders.value = [...folders.value, b];
  selection.value = [];
  load();
}
function goTo(level: number) {
  folders.value = folders.value.slice(0, level);
  selection.value = [];
  load();
}

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "pick",
    title: t("CATALOG.BROWSE.PICK", { count: selection.value.length }),
    icon: "lucide-check",
    disabled: !selection.value.length,
    clickHandler() {
      // the selection itself: picks stay when a search no longer lists them
      options.value?.onPick(selection.value.map((r) => r._blob));
      closeSelf();
    },
  },
]);
</script>
