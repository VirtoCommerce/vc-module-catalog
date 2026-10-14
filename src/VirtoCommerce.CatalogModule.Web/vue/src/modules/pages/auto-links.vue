<template>
  <VcBlade
    :title="$t('CATALOG.WIDGETS.AUTO_LINKS')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcContainer>
      <VcForm class="catalog-form">
        <VcSelect
          v-model="query.sourceCatalogId"
          :label="$t('CATALOG.AUTO_LINKS.SOURCE_CATALOG')"
          :options="catalogs"
          option-value="id"
          option-label="name"
          required
        />
        <VcInput
          v-model="query.sourceCatalogQuery"
          :label="$t('CATALOG.AUTO_LINKS.QUERY')"
          :hint="$t('CATALOG.AUTO_LINKS.QUERY_HINT')"
        />
        <div class="catalog-card__hint">{{ $t("CATALOG.AUTO_LINKS.HELP") }}</div>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useAsync, useBlade, useBladeForm, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcContainer, VcForm, VcInput, VcSelect } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import type { AutomaticLinkQuery, Catalog, Category } from "../../api_client/virtocommerce.catalog";

// legacy automatic-links: products matching a query in a real catalog are linked into this virtual category
defineBlade({ url: "/catalog-auto-links", name: "CatalogAutoLinks" });

const { t } = useI18n({ useScope: "global" });
const { options, openBlade, callParent } = useBlade<{ item: Category }>();
const { showConfirmation } = usePopup();

const query = ref<AutomaticLinkQuery>({});
const catalogs = ref<Catalog[]>([]);
const form = useBladeForm({ data: query, closeConfirmMessage: computed(() => t("CATALOG.ALERTS.CLOSE_CONFIRM")) });
const categoryId = computed(() => options.value!.item.id!);

// notify: false: a failure reaches the caller as before, with no extra toast
const { loading, action: busy } = useAsync(async (action?: () => Promise<unknown>) => void (await action?.()), { notify: false });

const load = () =>
  busy(async () => {
    const res = await api.autoLinks.search({ targetCategoryId: categoryId.value, take: 1 });
    query.value = res.results?.[0] ?? { targetCategoryId: categoryId.value };
    form.setBaseline();
  });

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "save",
    title: t("CATALOG.TOOLBAR.SAVE"),
    icon: "lucide-save",
    // the server's PUT saves new and existing queries alike and needs catalog:update (legacy automaticLinkQueryApi.update)
    permissions: "catalog:update",
    disabled: !form.canSave.value || !query.value.sourceCatalogId,
    clickHandler: () =>
      busy(async () => {
        query.value = await api.autoLinks.update(query.value);
        form.setBaseline();
      }),
  },
  { id: "reset", title: t("CATALOG.TOOLBAR.RESET"), icon: "lucide-undo-2", disabled: !form.isModified.value, clickHandler: load },
  {
    id: "preview",
    title: t("CATALOG.AUTO_LINKS.PREVIEW"),
    icon: "lucide-eye",
    disabled: !query.value.sourceCatalogId,
    clickHandler: () =>
      openBlade({
        name: "CatalogBrowse",
        param: query.value.sourceCatalogId,
        options: { catalog: catalogs.value.find((c) => c.id === query.value.sourceCatalogId), keyword: query.value.sourceCatalogQuery },
      }),
  },
  {
    id: "create",
    title: t("CATALOG.AUTO_LINKS.CREATE"),
    icon: "lucide-link",
    disabled: !query.value.id || form.isModified.value,
    async clickHandler() {
      if (!(await showConfirmation(t("CATALOG.AUTO_LINKS.CREATE_CONFIRM")))) return;
      await busy(() => api.categories.updateAutomaticLinks(categoryId.value));
      callParent("reload");
    },
  },
  {
    id: "delete",
    title: t("CATALOG.AUTO_LINKS.DELETE"),
    icon: "lucide-unlink",
    disabled: !query.value.id,
    async clickHandler() {
      if (!(await showConfirmation(t("CATALOG.AUTO_LINKS.DELETE_CONFIRM")))) return;
      await busy(() => api.categories.deleteAutomaticLinks(categoryId.value));
      callParent("reload");
    },
  },
]);

// the source catalogs only fill a select: they load alongside the query
onMounted(async () => {
  await Promise.all([
    api.catalogs.searchCatalogs({ take: 1000 }).then((r) => (catalogs.value = (r.results ?? []).filter((c) => !c.isVirtual))),
    load(),
  ]);
});
</script>
