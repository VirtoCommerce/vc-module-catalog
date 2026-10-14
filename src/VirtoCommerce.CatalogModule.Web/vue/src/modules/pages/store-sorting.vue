<template>
  <VcBlade
    :title="item.name || $t('CATALOG.STORE.NEW_SORTING')"
    :subtitle="$t('CATALOG.STORE.SORTING')"
    :toolbar-items="toolbar"
    width="50%"
  >
    <VcContainer>
      <VcForm class="catalog-form">
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="item.name"
              :label="$t('CATALOG.FIELDS.NAME')"
              required
              :disabled="!canEditDisplay"
              @update:model-value="proposeCode"
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model="item.code"
              :label="$t('CATALOG.FIELDS.CODE')"
              required
              :disabled="!isNew"
              :error="duplicate"
              :error-message="duplicate ? $t('CATALOG.STORE.CODE_TAKEN') : undefined"
              @update:model-value="codeTouched = true"
            />
          </VcCol>
          <VcCol>
            <VcSwitch
              v-model="item.isVisible"
              :label="$t('CATALOG.STORE.VISIBLE')"
              :disabled="!canEditDisplay"
            />
          </VcCol>
        </VcRow>
        <VcCard
          v-if="options?.languages?.length"
          :header="$t('CATALOG.FIELDS.LOCALIZED_NAME')"
          is-collapsable
        >
          <div class="catalog-card">
            <VcInput
              v-for="lang in options.languages"
              :key="lang"
              :model-value="item.localizedNames?.[lang]"
              :label="lang"
              :disabled="!canEditDisplay"
              @update:model-value="(v) => (item.localizedNames = { ...(item.localizedNames ?? {}), [lang]: String(v ?? '') })"
            />
          </div>
        </VcCard>
        <VcCard :header="$t('CATALOG.STORE.CLAUSES')">
          <template #actions>
            <div @click.stop>
              <VcButton
                v-if="canEditExpression"
                size="sm"
                variant="secondary"
                icon="lucide-plus"
                @click="(item.clauses ??= []).push({ field: '', isDescending: false })"
              >
                {{ $t("CATALOG.STORE.ADD_CLAUSE") }}
              </VcButton>
            </div>
          </template>
          <div class="catalog-card">
            <VcRow
              v-for="(clause, i) in item.clauses ?? []"
              :key="i"
              class="tw-gap-4 tw-items-end"
            >
              <VcCol :size="2">
                <VcSelect
                  v-model="clause.field"
                  :label="$t('CATALOG.STORE.FIELD')"
                  :options="fields"
                  searchable
                  :disabled="!canEditExpression"
                />
              </VcCol>
              <VcCol>
                <VcSwitch
                  v-model="clause.isDescending"
                  :label="$t('CATALOG.STORE.DESCENDING')"
                  :disabled="!canEditExpression"
                />
              </VcCol>
              <VcButton
                v-if="canEditExpression"
                icon="lucide-trash-2"
                text
                :aria-label="$t('CATALOG.TOOLBAR.DELETE')"
                @click="item.clauses!.splice(i, 1)"
              />
            </VcRow>
            <VcField
              :label="$t('CATALOG.STORE.EXPRESSION')"
              :model-value="expression || '—'"
            />
          </div>
        </VcCard>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcButton, VcCard, VcCol, VcContainer, VcField, VcForm, VcInput, VcRow, VcSelect, VcSwitch } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { clone, options as toOptions, slug } from "../api";
import type { ProductSorting } from "../../api_client/virtocommerce.catalog";

// legacy product-sorting-detail: name, code, localized names, visibility and sort clauses (custom sortings)
defineBlade({ url: "/catalog-store-sorting", name: "CatalogStoreSortingDetails", routable: false });

const { t } = useI18n({ useScope: "global" });
const { options, closeSelf } = useBlade<{
  sorting?: ProductSorting;
  fields: string[];
  existingCodes: string[];
  languages?: string[];
  onApply: (s: ProductSorting) => void;
}>();

const isNew = !options.value?.sorting;
const item = ref<ProductSorting>(
  clone(
    options.value?.sorting ?? {
      code: "",
      name: "",
      localizedNames: {},
      isVisible: true,
      isDefault: false,
      isCustom: true,
      isExpressionEditable: true,
      allowOverride: true,
      clauses: [],
      sortExpression: "",
    },
  ),
);
item.value.clauses ??= [];

const canEditDisplay = computed(() => item.value.allowOverride !== false);
const canEditExpression = computed(() => item.value.isExpressionEditable !== false);
const fields = computed(() => toOptions(options.value?.fields ?? []));
const expression = computed(() =>
  (item.value.clauses ?? [])
    .filter((c) => c.field)
    .map((c) => `${c.field}:${c.isDescending ? "desc" : "asc"}`)
    .join(";"),
);
const duplicate = computed(() => {
  const code = item.value.code?.toLowerCase();
  return !!code && (options.value?.existingCodes ?? []).some((c) => c.toLowerCase() === code && c.toLowerCase() !== (options.value?.sorting?.code ?? "").toLowerCase());
});
const valid = computed(
  () => !!item.value.code && !!item.value.name && !duplicate.value && (!item.value.isCustom || (item.value.clauses ?? []).some((c) => c.field)),
);

// propose a code from the name for new sortings until the code is edited (legacy)
const codeTouched = ref(!isNew);
function proposeCode(name: unknown) {
  if (!codeTouched.value) item.value.code = slug(name);
}

const form = useBladeForm({ data: item });
form.setBaseline();

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "apply",
    title: t("CATALOG.TOOLBAR.APPLY"),
    icon: "lucide-check",
    disabled: !valid.value || !(form.isModified.value || isNew),
    clickHandler() {
      if (canEditExpression.value) item.value.sortExpression = expression.value;
      options.value?.onApply({ ...item.value });
      form.setBaseline();
      closeSelf();
    },
  },
]);
</script>
