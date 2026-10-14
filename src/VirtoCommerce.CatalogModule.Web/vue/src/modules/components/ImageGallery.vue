<template>
  <div class="catalog-gallery">
    <VcGallery
      :images="shown"
      :label="$t('CATALOG.FIELDS.IMAGES')"
      multiple
      :loading="assets.loading.value"
      @upload="upload"
      @remove="(i) => assets.remove(original(i))"
      @sort="(list) => assets.reorder(list.map((i) => ({ ...original(i), sortOrder: i.sortOrder })))"
      @edit="(i) => edit(original(i))"
    />
    <div class="catalog-gallery__actions">
      <VcButton
        v-if="hasAccess('platform:asset:read')"
        size="sm"
        variant="secondary"
        icon="lucide-folder-search"
        @click="pickAssets"
      >
        {{ $t("CATALOG.IMAGE.FROM_ASSETS") }}
      </VcButton>
      <VcButton
        v-if="hasAccess('catalog:add-external-image')"
        size="sm"
        variant="secondary"
        icon="lucide-link"
        @click="addLink"
      >
        {{ $t("CATALOG.IMAGE.ADD_LINK") }}
      </VcButton>
    </div>
  </div>
</template>

<script setup lang="ts">
import { notification, useAssetsManager, useBlade, usePermissions, usePopup } from "@vc-shell/framework";
import { VcButton, VcGallery } from "@vc-shell/framework/ui";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import { assetUrl, assetsFolder } from "../api";
import type { Image } from "../../api_client/virtocommerce.catalog";
import type { AssetBlob } from "../pages/asset-picker.vue";

// Images of a product or category (legacy images blade + image widget): upload into the legacy catalog folder,
// link existing assets, or add an external image URL.
const props = defineProps<{
  entity: { images?: Image[]; catalogId?: string; code?: string };
  languages: string[];
}>();

type Shown = Image & { _image: Image };

const { t } = useI18n({ useScope: "global" });
const { openBlade } = useBlade();
const { showConfirmation } = usePopup();
const { hasAccess } = usePermissions();

const images = computed({
  get: () => props.entity.images ?? [],
  set: (v) => (props.entity.images = v as Image[]),
});
const assets = useAssetsManager(images, {
  uploadPath: () => assetsFolder(props.entity.catalogId, props.entity.code),
  confirmRemove: () => showConfirmation(t("CATALOG.ALERTS.IMAGE_DELETE")),
});
// display copies with a resolvable url; events hand the original image back to the manager
const shown = computed<Shown[]>(() => images.value.map((i) => ({ ...i, url: assetUrl(i.url), _image: i })));

const original = (i: object) => (i as Shown)._image;
// legacy offered the upload only with platform:asset:create; a failed upload (403...) is reported, not dropped
function upload(files: FileList, startingSortOrder?: number) {
  if (!hasAccess("platform:asset:create")) return notification.error(t("CATALOG.IMAGE.NO_UPLOAD"));
  assets.upload(files, startingSortOrder).catch((e) => notification.error(String((e as Error)?.message ?? e)));
}
// linked images go to the end, in the "images" group (legacy)
const append = (added: Image[]) => {
  const next = Math.max(0, ...images.value.map((i) => i.sortOrder ?? 0)) + 1;
  images.value = [...images.value, ...added.map((i, n) => ({ group: "images", ...i, sortOrder: next + n }))];
};

function edit(image: Image) {
  openBlade({
    name: "CatalogImageDetails",
    options: { image, languages: props.languages, onSave: (i: Image) => assets.updateItem(i) },
  });
}

function pickAssets() {
  openBlade({
    name: "CatalogAssetPicker",
    options: {
      imagesOnly: true,
      onPick: (blobs: AssetBlob[]) =>
        append(
          blobs
            .filter((b) => !images.value.some((i) => i.url === b.url))
            .map((b) => ({ url: b.url, relativeUrl: b.relativeUrl?.replace(/^\//, ""), name: b.name })),
        ),
    },
  });
}

function addLink() {
  openBlade({ name: "CatalogImageLink", options: { languages: props.languages, onApply: (i: Image) => append([i]) } });
}
</script>

<style lang="scss">
.catalog-gallery {
  display: flex;
  flex-direction: column;
  gap: 8px;
  &__actions {
    display: flex;
    gap: 8px;
  }
}
</style>
