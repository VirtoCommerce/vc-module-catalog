<template>
  <VcBlade
    :title="$t('CATALOG.WIDGETS.VIDEOS')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <div class="catalog-list-blade">
      <div
        v-if="hasAccess('catalog:update')"
        class="catalog-videos__add"
      >
        <VcInput
          v-model="url"
          class="tw-flex-1"
          :label="$t('CATALOG.VIDEOS.URL')"
          :placeholder="'https://www.youtube.com/watch?v=...'"
          :hint="providers"
        />
        <VcButton
          icon="lucide-plus"
          variant="secondary"
          :disabled="!url"
          @click="add"
        >
          {{ $t("CATALOG.VIDEOS.ADD") }}
        </VcButton>
      </div>
      <VcDataTable
        v-model:selection="selection"
        selection-mode="multiple"
        :items="items"
        :state-key="'CATALOG_VIDEOS'"
        :empty-state="{ icon: 'lucide-video', title: $t('CATALOG.VIDEOS.EMPTY') }"
        @row-click="(e: { data: Video }) => edit(e.data)"
      >
        <VcColumn
          id="thumbnailUrl"
          :title="$t('CATALOG.FIELDS.IMAGE')"
          type="image"
          width="80px"
        />
        <VcColumn
          id="name"
          :title="$t('CATALOG.FIELDS.NAME')"
          body-class="catalog-list__name"
          always-visible
        />
        <VcColumn
          id="languageCode"
          :visible="false"
          :title="$t('CATALOG.FIELDS.LANGUAGE')"
        />
        <VcColumn
          id="sortOrder"
          :title="$t('CATALOG.FIELDS.SORT_ORDER')"
          type="number"
        />
      </VcDataTable>
    </div>
  </VcBlade>
</template>

<script setup lang="ts">
import { useAsync, useBlade, usePermissions, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcButton, VcColumn, VcDataTable, VcInput } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import type { Catalog, CatalogProduct, Video } from "../../api_client/virtocommerce.catalog";

// legacy video-list / video-add: product videos, saved immediately through the videos API
defineBlade({ url: "/catalog-videos", name: "CatalogVideos", routable: false });

const { t } = useI18n({ useScope: "global" });
const { options, openBlade, exposeToChildren } = useBlade<{ item: CatalogProduct; catalog?: Catalog }>();
const { showConfirmation } = usePopup();
const { hasAccess } = usePermissions();

const items = ref<Video[]>([]);
const selection = ref<Video[]>([]);
const url = ref("");
const providers = ref("");

// notify: false: a failure reaches the caller as before, with no extra toast
const { loading, action: busy } = useAsync(async (action?: () => Promise<unknown>) => void (await action?.()), { notify: false });
const ownerId = computed(() => options.value?.item.id);
const load = () =>
  busy(async () => {
    if (!ownerId.value) return;
    const res = await api.videos.searchVideos({ ownerIds: [ownerId.value], ownerType: "Product", sort: "sortOrder", take: 1000 });
    items.value = res.results ?? [];
  });

const add = () =>
  busy(async () => {
    // no sortOrder: the server puts the new video after the last one
    await api.videos.createVideo({
      contentUrl: url.value,
      ownerId: ownerId.value,
      ownerType: "Product",
      languageCode: options.value?.catalog?.defaultLanguage?.languageCode ?? "en-US",
    });
    url.value = "";
    await load();
  });

function edit(video: Video) {
  openBlade({
    name: "CatalogVideoDetails",
    options: {
      video,
      languages: (options.value?.catalog?.languages ?? []).map((l) => l.languageCode!),
    },
  });
}
exposeToChildren({ reload: () => load() });

const toolbar = computed<IBladeToolbar[]>(() => [
  { id: "refresh", title: t("CATALOG.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: load },
  {
    id: "delete",
    title: t("CATALOG.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "catalog:delete",
    disabled: !selection.value.length,
    async clickHandler() {
      if (!(await showConfirmation(t("CATALOG.VIDEOS.DELETE_CONFIRM", { count: selection.value.length })))) return;
      await busy(() => api.videos.delete(selection.value.map((v) => v.id!)));
      selection.value = [];
      await load();
    },
  },
]);

onMounted(async () => {
  // legacy video-list warns when no video provider is configured
  const [, status] = await Promise.all([load(), api.videos.getOptions().catch(() => undefined)]);
  if (status && !status.isYouTubeApiKeyConfigured && !status.isVimeoAccessTokenConfigured)
    providers.value = t("CATALOG.VIDEOS.NO_PROVIDER");
});
</script>

<style lang="scss">
.catalog-videos__add {
  display: flex;
  align-items: flex-end;
  gap: 12px;
  padding: 16px;
}
</style>
