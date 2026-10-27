<template>
  <VcBlade
    :title="item.name || $t('CATALOG.CONFIG.NEW_SECTION')"
    :subtitle="$t('CATALOG.CONFIG.TITLE')"
    :toolbar-items="toolbar"
    width="60%"
  >
    <VcContainer>
      <VcForm class="catalog-form">
        <VcRow class="tw-gap-4">
          <VcCol :size="2">
            <VcInput
              v-model="item.name"
              :label="$t('CATALOG.FIELDS.NAME')"
              required
            />
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="item.type"
              :label="$t('CATALOG.FIELDS.TYPE')"
              :tooltip="$t('CATALOG.CONFIG.TYPE_HINT')"
              :options="types"
              :clearable="false"
              :disabled="typeLocked"
              required
            />
          </VcCol>
        </VcRow>
        <VcTextarea
          v-model="item.description"
          :label="$t('CATALOG.IMAGE.DESCRIPTION')"
        />
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcSwitch
              v-model="item.isRequired"
              :label="$t('CATALOG.PROPERTY.REQUIRED')"
            />
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="item.dependsOnSectionId"
              :label="$t('CATALOG.CONFIG.DEPENDS_ON')"
              :options="dependsOn"
              option-value="id"
              option-label="name"
            />
          </VcCol>
        </VcRow>
        <VcRow
          v-if="item.type === 'Text'"
          class="tw-gap-4"
        >
          <VcCol>
            <VcSwitch
              :model-value="item.allowCustomText"
              :label="$t('CATALOG.CONFIG.CUSTOM_TEXT')"
              @update:model-value="(v?: boolean) => setTextMode('allowCustomText', !!v)"
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model.number="item.maxLength"
              type="number"
              :label="$t('CATALOG.CONFIG.MAX_LENGTH')"
              :disabled="!item.allowCustomText"
            />
          </VcCol>
          <VcCol>
            <VcSwitch
              :model-value="item.allowPredefinedOptions"
              :label="$t('CATALOG.CONFIG.PREDEFINED')"
              @update:model-value="(v?: boolean) => setTextMode('allowPredefinedOptions', !!v)"
            />
          </VcCol>
        </VcRow>

        <VcCard
          v-if="showOptions"
          :header="$t('CATALOG.CONFIG.OPTIONS')"
        >
          <template #actions>
            <div @click.stop>
              <div class="property-values__actions">
                <VcButton
                  v-if="item.type === 'Product'"
                  size="sm"
                  variant="secondary"
                  icon="lucide-plus"
                  @click="addProducts"
                >
                  {{ $t("CATALOG.CONFIG.ADD_PRODUCTS") }}
                </VcButton>
                <VcButton
                  size="sm"
                  variant="secondary"
                  icon="lucide-trash-2"
                  :disabled="!selected.length"
                  @click="removeSelected"
                >
                  {{ $t("CATALOG.TOOLBAR.DELETE") }}
                </VcButton>
              </div>
            </div>
          </template>
          <div class="catalog-card">
            <div
              v-if="item.type === 'Text'"
              class="catalog-inline-add"
            >
              <VcInput
                v-model="newText"
                class="tw-flex-1"
                :label="$t('CATALOG.CONFIG.OPTION_TEXT')"
              />
              <VcButton
                icon="lucide-plus"
                variant="secondary"
                :disabled="!newText"
                @click="addText"
              >
                {{ $t("CATALOG.CONFIG.ADD_OPTION") }}
              </VcButton>
            </div>
            <VcDataTable
              v-model:selection="selected"
              selection-mode="multiple"
              edit-mode="cell"
              :items="optionRows"
              :state-key="'CATALOG_CONFIG_OPTIONS'"
              :row-actions="optionActions"
              :empty-state="{ icon: 'lucide-list', title: $t('CATALOG.CONFIG.NO_OPTIONS') }"
              @cell-edit-complete="onOptionEdit"
            >
              <VcColumn
                v-if="item.type === 'Product'"
                id="productImageUrl"
                :title="$t('CATALOG.FIELDS.IMAGE')"
                type="image"
                width="84px"
              />
              <VcColumn
                v-if="item.type === 'Product'"
                id="productName"
                :title="$t('CATALOG.FIELDS.NAME')"
                always-visible
              />
              <VcColumn
                v-else
                id="text"
                :title="$t('CATALOG.CONFIG.OPTION_TEXT')"
                always-visible
                editable
              />
              <VcColumn
                v-if="item.type === 'Product'"
                id="quantity"
                :title="$t('CATALOG.ASSOCIATIONS.QUANTITY')"
                type="number"
                editable
              />
              <VcColumn
                id="isDefault"
                :title="$t('CATALOG.MEASURES.DEFAULT')"
              >
                <template #body="{ data }">
                  <div
                    v-if="data.isDefault"
                    class="tw-inline-flex"
                  >
                    <VcStatus variant="success">{{ $t("CATALOG.MEASURES.DEFAULT") }}</VcStatus>
                  </div>
                </template>
              </VcColumn>
            </VcDataTable>
          </div>
        </VcCard>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import {
  VcBlade,
  VcButton,
  VcCard,
  VcCol,
  VcColumn,
  VcContainer,
  VcDataTable,
  VcForm,
  VcInput,
  VcRow,
  VcSelect,
  VcStatus,
  VcSwitch,
  VcTextarea,
} from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { assetUrl, clone, options as toOptions, searchAllEntries, withoutRows } from "../api";
import type { PickOptions } from "./browse.vue";
import type { ProductConfigurationOption, ProductConfigurationSection } from "../../api_client/virtocommerce.catalog";

// legacy configuration-section-detail + configuration-options-list + option-product/text-detail;
// applied to the configuration blade, saved with it
defineBlade({ url: "/catalog-configuration-section", name: "CatalogConfigurationSection", routable: false });

type Option = ProductConfigurationOption & { productName?: string; productImageUrl?: string; productType?: string };
type OptionRow = Option & { _index: number; id: string };

const { t } = useI18n({ useScope: "global" });
const { options, openBlade, closeSelf } = useBlade<{
  section?: ProductConfigurationSection;
  otherSections: ProductConfigurationSection[];
  onApply: (s: ProductConfigurationSection) => void;
}>();

const isNew = !options.value?.section;
const item = ref<ProductConfigurationSection>(clone(options.value?.section) ?? { type: "Product", isRequired: false, options: [] });
item.value.options ??= [];
const types = toOptions(["Product", "Variation", "Text", "File"]);
const dependsOn = computed(() => options.value?.otherSections ?? []);
const selected = ref<OptionRow[]>([]);
const newText = ref("");
const form = useBladeForm({ data: item });

// the type is fixed once the section is saved or has options (legacy isTypeChangeDisabled)
const typeLocked = computed(() => !!item.value.id || !!item.value.options?.length);
const showOptions = computed(() => item.value.type === "Product" || (item.value.type === "Text" && !!item.value.allowPredefinedOptions));
const optionRows = computed<OptionRow[]>(() =>
  (item.value.options as Option[]).map((o, i) => ({
    ...o,
    _index: i,
    id: o.id ?? `${o.productId ?? o.text}-${i}`,
    productImageUrl: assetUrl(o.productImageUrl),
  })),
);

// Text sections: at least one of custom text / predefined options stays on; custom text defaults to 255 chars
function setTextMode(flag: "allowCustomText" | "allowPredefinedOptions", on: boolean) {
  item.value[flag] = on;
  const other = flag === "allowCustomText" ? "allowPredefinedOptions" : "allowCustomText";
  if (!on && !item.value[other]) item.value[other] = true;
  if (item.value.allowCustomText && !item.value.maxLength) item.value.maxLength = 255;
}
// a Text section needs an input mode, also when a new section is switched to Text
watch(
  () => item.value.type,
  (type) => {
    if (type === "Text" && !item.value.allowCustomText && !item.value.allowPredefinedOptions) setTextMode("allowCustomText", true);
  },
  { immediate: true },
);
form.setBaseline();

const setOptions = (list: Option[]) => (item.value.options = list);

function addProducts() {
  openBlade({
    name: "CatalogBrowse",
    options: {
      pick: {
        title: t("CATALOG.CONFIG.ADD_PRODUCTS"),
        mode: "entries",
        types: ["product"],
        onPick: async (picked, _folder, all) => {
          // "select all" covers every page of the list, not just the rows shown
          const entries = all ? (await searchAllEntries(all)).filter((e) => e.type === "product") : picked;
          const have = new Set((item.value.options ?? []).map((o) => o.productId));
          setOptions([
            ...(item.value.options as Option[]),
            ...entries
              .filter((e) => !have.has(e.id))
              .map((e) => ({ productId: e.id, productName: e.name, productImageUrl: e.imageUrl, productType: e.productType, quantity: 1 })),
          ]);
        },
      } satisfies PickOptions,
    },
  });
}

function addText() {
  setOptions([...(item.value.options as Option[]), { text: newText.value, isDefault: false }]);
  newText.value = "";
}

function removeSelected() {
  setOptions(withoutRows(item.value.options as Option[], selected.value));
  selected.value = [];
}

function onOptionEdit(e: { data: OptionRow; field: string; newValue: unknown }) {
  const value = e.field === "quantity" ? Number(e.newValue) : String(e.newValue ?? "");
  if (e.field === "quantity" && (Number.isNaN(value) || (value as number) < 1)) return;
  setOptions((item.value.options as Option[]).map((o, i) => (i === e.data._index ? { ...o, [e.field]: value } : o)));
}

// a single default option (legacy toggleDefaultOption)
const optionActions = (row: OptionRow) => [
  {
    icon: "lucide-star",
    title: t(row.isDefault ? "CATALOG.CONFIG.UNSET_DEFAULT" : "CATALOG.MEASURES.MAKE_DEFAULT"),
    type: "info" as const,
    clickHandler: () =>
      setOptions((item.value.options as Option[]).map((o, i) => ({ ...o, isDefault: !row.isDefault && i === row._index }))),
  },
];

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "apply",
    title: t("CATALOG.TOOLBAR.APPLY"),
    icon: "lucide-check",
    disabled: !item.value.name || !item.value.type || !(form.isModified.value || isNew),
    clickHandler() {
      const section = { ...item.value };
      // only Product and predefined-text sections have options
      if (!showOptions.value) section.options = [];
      options.value?.onApply(section);
      form.setBaseline();
      closeSelf();
    },
  },
]);
</script>
