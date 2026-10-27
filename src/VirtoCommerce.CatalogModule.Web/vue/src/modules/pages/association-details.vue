<template>
  <VcBlade
    :title="item.associatedObjectName || $t('CATALOG.WIDGETS.ASSOCIATIONS')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="40%"
  >
    <VcContainer>
      <VcForm class="catalog-form">
        <VcSelect
          v-model="item.type"
          :label="$t('CATALOG.ASSOCIATIONS.GROUP')"
          :options="groups"
          :clearable="false"
          :disabled="item.type === 'BillOfMaterials'"
        />
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model.number="item.quantity"
              type="number"
              :label="$t('CATALOG.ASSOCIATIONS.QUANTITY')"
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model.number="item.priority"
              type="number"
              :label="$t('CATALOG.FIELDS.PRIORITY')"
            />
          </VcCol>
        </VcRow>
        <VcMultivalue
          :model-value="toChips(item.tags)"
          :label="$t('CATALOG.ASSOCIATIONS.TAGS')"
          :placeholder="$t('CATALOG.ASSOCIATIONS.ADD_TAG')"
          @update:model-value="(list) => (item.tags = fromChips(list))"
        />
        <VcField
          :label="$t('CATALOG.FIELDS.TYPE')"
          :model-value="item.associatedObjectType"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcCol, VcContainer, VcField, VcForm, VcInput, VcMultivalue, VcRow, VcSelect } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api, clone, fromChips, settingOptions, toChips } from "../api";
import type { ProductAssociation } from "../../api_client/virtocommerce.catalog";

// legacy item-association-detail: group, quantity, tags (+ open the associated item); saved immediately
defineBlade({ url: "/catalog-association", name: "CatalogAssociationDetails", routable: false });

const { t } = useI18n({ useScope: "global" });
const { options, openBlade, callParent } = useBlade<{ association: ProductAssociation }>();

const item = ref<ProductAssociation>(clone(options.value?.association ?? {}));
const groups = ref<{ id: string; title: string }[]>([]);
const loading = ref(false);
const form = useBladeForm({ data: item, closeConfirmMessage: computed(() => t("CATALOG.ALERTS.CLOSE_CONFIRM")) });
form.setBaseline();

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "save",
    title: t("CATALOG.TOOLBAR.SAVE"),
    icon: "lucide-save",
    // what the associations API checks
    permissions: "catalog:update",
    disabled: !form.canSave.value || (item.value.quantity ?? 1) < 1,
    async clickHandler() {
      loading.value = true;
      try {
        await api.associations.updateAssociations([item.value]);
        form.setBaseline();
        await callParent("reload");
      } finally {
        loading.value = false;
      }
    },
  },
  {
    id: "open",
    title: t("CATALOG.ASSOCIATIONS.OPEN_ITEM"),
    icon: "lucide-external-link",
    clickHandler: () =>
      openBlade({
        name: item.value.associatedObjectType === "category" ? "CatalogCategoryDetails" : "CatalogProductDetails",
        param: item.value.associatedObjectId,
      }),
  },
]);

onMounted(async () => {
  groups.value = await settingOptions("Catalog.AssociationGroups");
});
</script>
