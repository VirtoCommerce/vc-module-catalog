<template>
  <VcBlade
    :title="item.name || $t('CATALOG.WIDGETS.VIDEOS')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcContainer>
      <VcForm class="catalog-form">
        <VcVideo
          v-if="item.embedUrl"
          :source="item.embedUrl"
          :label="$t('CATALOG.VIDEOS.PREVIEW')"
        />
        <VcImage
          v-else-if="item.thumbnailUrl"
          :src="item.thumbnailUrl"
          size="xl"
          bordered
        />
        <VcInput
          v-model="item.name"
          :label="$t('CATALOG.FIELDS.NAME')"
          required
        />
        <VcTextarea
          v-model="item.description"
          :label="$t('CATALOG.IMAGE.DESCRIPTION')"
          required
        />
        <VcInput
          v-model="item.contentUrl"
          :label="$t('CATALOG.VIDEOS.URL')"
          required
        />
        <VcInput
          v-model="item.embedUrl"
          :label="$t('CATALOG.VIDEOS.EMBED_URL')"
        />
        <VcInput
          v-model="item.thumbnailUrl"
          :label="$t('CATALOG.VIDEOS.THUMBNAIL_URL')"
          required
        />
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcDatePicker
              v-model="item.uploadDate"
              :label="$t('CATALOG.VIDEOS.UPLOAD_DATE')"
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model="item.duration"
              :label="$t('CATALOG.VIDEOS.DURATION')"
              placeholder="PT1M30S"
              maxlength="20"
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model.number="item.sortOrder"
              type="number"
              :label="$t('CATALOG.FIELDS.SORT_ORDER')"
            />
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="item.languageCode"
              :label="$t('CATALOG.FIELDS.LANGUAGE')"
              :options="languageOptions"
            />
          </VcCol>
        </VcRow>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import {
  VcBlade,
  VcCol,
  VcContainer,
  VcDatePicker,
  VcForm,
  VcImage,
  VcInput,
  VcRow,
  VcSelect,
  VcTextarea,
  VcVideo,
} from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api, clone, options as toOptions } from "../api";
import type { Video } from "../../api_client/virtocommerce.catalog";

// legacy video-detail: saved immediately through the videos API
defineBlade({ url: "/catalog-video", name: "CatalogVideoDetails", routable: false });

const { t } = useI18n({ useScope: "global" });
const { options, callParent, closeSelf } = useBlade<{ video: Video; languages?: string[] }>();
const { showConfirmation } = usePopup();

const item = ref<Video>(clone(options.value?.video ?? {}));
const loading = ref(false);
const languageOptions = computed(() => toOptions(options.value?.languages ?? []));
const form = useBladeForm({ data: item, closeConfirmMessage: computed(() => t("CATALOG.ALERTS.CLOSE_CONFIRM")) });
form.setBaseline();

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "save",
    title: t("CATALOG.TOOLBAR.SAVE"),
    icon: "lucide-save",
    permissions: "catalog:update",
    // name, description, thumbnail and content URLs are required columns; sort order is 0 or more (legacy)
    disabled:
      !form.canSave.value ||
      !item.value.name ||
      !item.value.description ||
      !item.value.thumbnailUrl ||
      !item.value.contentUrl ||
      !(Number.isInteger(item.value.sortOrder) && item.value.sortOrder! >= 0),
    async clickHandler() {
      loading.value = true;
      try {
        const [saved] = await api.videos.update([item.value]);
        if (saved) item.value = saved;
        form.setBaseline();
        callParent("reload");
      } finally {
        loading.value = false;
      }
    },
  },
  {
    id: "reset",
    title: t("CATALOG.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    disabled: !form.isModified.value,
    // back to the last saved values (the baseline), not the ones the blade opened with
    clickHandler: () => form.revert(),
  },
  {
    id: "delete",
    title: t("CATALOG.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "catalog:delete",
    async clickHandler() {
      if (!(await showConfirmation(t("CATALOG.VIDEOS.DELETE_CONFIRM", { count: 1 })))) return;
      await api.videos.delete([item.value.id!]);
      form.setBaseline();
      callParent("reload");
      closeSelf();
    },
  },
]);
</script>
