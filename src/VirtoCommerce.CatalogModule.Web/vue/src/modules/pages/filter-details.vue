<template>
  <VcBlade
    :title="item.name || $t('CATALOG.FILTERS.NEW')"
    :toolbar-items="toolbar"
    width="40%"
  >
    <VcContainer>
      <VcForm class="catalog-form">
        <VcInput
          v-model="item.name"
          :label="$t('CATALOG.FIELDS.NAME')"
          required
        />
        <VcInput
          v-model="item.keyword"
          :label="$t('CATALOG.FILTERS.KEYWORD')"
          :hint="$t('CATALOG.FILTERS.KEYWORD_HINT')"
        />
        <VcSwitch
          v-model="item.searchInVariations"
          :label="$t('CATALOG.BROWSE.FILTERS.VARIATIONS')"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { generateId, useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcContainer, VcForm, VcInput, VcSwitch } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { applyRow, clone, savedFilters, storeFilters, type SavedFilter } from "../api";

// legacy filter-detail: name, keyword (search syntax) and "display variations"
defineBlade({ url: "/catalog-filter", name: "CatalogFilterDetails", routable: false });

const { t } = useI18n({ useScope: "global" });
const { options, callParent, closeSelf } = useBlade<{ filter?: SavedFilter }>();

const item = ref<SavedFilter>(clone(options.value?.filter) ?? { id: "", name: "", keyword: "" });
const form = useBladeForm({ data: item });
form.setBaseline();

function save(list: SavedFilter[]) {
  storeFilters(list);
  form.setBaseline();
  callParent("reload");
  closeSelf();
}

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "save",
    title: t("CATALOG.TOOLBAR.SAVE"),
    icon: "lucide-save",
    disabled: !item.value.name || !(form.isModified.value || !item.value.id),
    clickHandler() {
      const existing = item.value.id ? item.value : undefined;
      save(applyRow(savedFilters(), existing, existing ?? { ...item.value, id: generateId() }));
    },
  },
  {
    id: "delete",
    title: t("CATALOG.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    isVisible: !!item.value.id,
    clickHandler: () => save(savedFilters().filter((f) => f.id !== item.value.id)),
  },
]);
</script>
