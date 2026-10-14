<template>
  <VcBlade
    :title="item.name || $t('CATALOG.PROPERTY.NEW')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcContainer>
      <VcForm class="catalog-form">
        <VcInput
          v-model="item.name"
          :label="$t('CATALOG.FIELDS.NAME')"
          name="name"
          rules="required"
          required
          :error="!!nameError"
          :error-message="nameError"
        />
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcSelect
              v-model="item.type"
              :label="$t('CATALOG.PROPERTY.APPLIES_TO')"
              :options="propertyTypes"
              :clearable="false"
              :disabled="!isNew"
            />
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="item.valueType"
              :label="$t('CATALOG.PROPERTIES.VALUE_TYPE')"
              :options="valueTypes"
              :clearable="false"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcSelect
              v-if="item.valueType === 'Measure'"
              v-model="item.measureId"
              :label="$t('CATALOG.PROPERTY.MEASURE')"
              :options="measures"
              option-value="id"
              option-label="name"
              required
            />
            <VcSelect
              v-else
              v-model="item.propertyGroupId"
              :label="$t('CATALOG.PROPERTY.GROUP')"
              :options="groups"
              option-value="id"
              option-label="name"
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model.number="item.displayOrder"
              type="number"
              :label="$t('CATALOG.FIELDS.DISPLAY_ORDER')"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcSwitch
              v-model="item.required"
              :label="$t('CATALOG.PROPERTY.REQUIRED')"
            />
          </VcCol>
          <VcCol>
            <VcSwitch
              v-model="item.dictionary"
              :label="$t('CATALOG.PROPERTY.DICTIONARY')"
              :disabled="!allowed.dictionary"
            />
          </VcCol>
          <VcCol>
            <VcSwitch
              v-model="item.multivalue"
              :label="$t('CATALOG.PROPERTY.MULTIVALUE')"
              :disabled="!allowed.multivalue"
            />
          </VcCol>
          <VcCol>
            <VcSwitch
              v-model="item.multilanguage"
              :label="$t('CATALOG.PROPERTY.MULTILANGUAGE')"
              :disabled="!allowed.multilanguage"
            />
          </VcCol>
          <VcCol>
            <VcSwitch
              v-model="item.hidden"
              :label="$t('CATALOG.PROPERTY.HIDDEN')"
            />
          </VcCol>
        </VcRow>

        <VcCard
          :header="$t('CATALOG.PROPERTY.DISPLAY_NAMES')"
          is-collapsable
        >
          <div class="catalog-card">
            <VcInput
              v-for="lang in languages"
              :key="lang"
              :model-value="displayName(lang)"
              :label="lang"
              @update:model-value="(v) => setDisplayName(lang, String(v ?? ''))"
            />
          </div>
        </VcCard>

        <VcCard
          v-if="isText"
          :header="$t('CATALOG.PROPERTY.VALIDATION')"
          is-collapsable
        >
          <div class="catalog-card">
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcInput
                  v-model.number="rule.charCountMin"
                  type="number"
                  :label="$t('CATALOG.PROPERTY.MIN_CHARS')"
                />
              </VcCol>
              <VcCol>
                <VcInput
                  v-model.number="rule.charCountMax"
                  type="number"
                  :label="$t('CATALOG.PROPERTY.MAX_CHARS')"
                />
              </VcCol>
            </VcRow>
            <VcInput
              v-model="rule.regExp"
              :label="$t('CATALOG.PROPERTY.PATTERN')"
              :placeholder="'^[a-zA-Z0-9]+$'"
            />
          </div>
        </VcCard>

        <VcCard
          :header="$t('CATALOG.PROPERTY.ATTRIBUTES')"
          is-collapsable
        >
          <div class="catalog-card">
            <VcRow
              v-for="(attr, i) in item.attributes ?? []"
              :key="i"
              class="tw-gap-4 tw-items-end"
            >
              <VcCol>
                <VcInput
                  v-model="attr.name"
                  :label="$t('CATALOG.FIELDS.NAME')"
                />
              </VcCol>
              <VcCol>
                <VcInput
                  v-model="attr.value"
                  :label="$t('CATALOG.FIELDS.VALUE')"
                />
              </VcCol>
              <VcButton
                icon="lucide-trash-2"
                text
                :aria-label="$t('CATALOG.TOOLBAR.DELETE')"
                @click="item.attributes!.splice(i, 1)"
              />
            </VcRow>
            <div>
              <VcButton
                icon="lucide-plus"
                variant="secondary"
                size="sm"
                @click="(item.attributes ??= []).push({ name: '', value: '', propertyId: item.id })"
              >
                {{ $t("CATALOG.PROPERTY.ADD_ATTRIBUTE") }}
              </VcButton>
            </div>
          </div>
        </VcCard>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, useBladeWidgets, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import {
  VcBlade,
  VcButton,
  VcCard,
  VcCol,
  VcContainer,
  VcForm,
  VcInput,
  VcPopup,
  VcRow,
  VcSelect,
  VcSwitch,
} from "@vc-shell/framework/ui";
import { computed, h, onMounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { api, options } from "../api";
import {
  PropertyValueType,
  type Measure,
  type Property,
  type PropertyGroup,
  type PropertyValidationRule,
} from "../../api_client/virtocommerce.catalog";

defineBlade({ url: "/catalog-property", name: "CatalogPropertyDetails" });

const { t } = useI18n({ useScope: "global" });
const { param, options: bladeOptions, openBlade, callParent, closeSelf, replaceWith } = useBlade<{
  catalogId?: string;
  categoryId?: string;
  type?: string;
}>();
// legacy deleteProperty-dialog: remove only the metadata, delete with all values, or cancel (undefined)
let settleDelete: ((withValues?: boolean) => void) | undefined;
const deletePrompt = usePopup({
  component: VcPopup,
  props: { title: t("CATALOG.TOOLBAR.DELETE"), variant: "warning" },
  emits: { onClose: () => settleDelete?.() },
  slots: {
    content: () => t("CATALOG.PROPERTY.DELETE_VALUES_CONFIRM", { name: item.value.name }),
    footer: () =>
      h("div", { class: "vc-popup-base__actions vc-popup-base__actions--confirm" }, [
        h(VcButton, { text: true, class: "vc-popup-base__confirm", onClick: () => settleDelete?.(false) }, () =>
          t("CATALOG.PROPERTY.DELETE_SHALLOW"),
        ),
        h(VcButton, { text: true, class: "vc-popup-base__confirm", onClick: () => settleDelete?.(true) }, () =>
          t("CATALOG.PROPERTY.DELETE_FULL"),
        ),
        h(VcButton, { onClick: () => settleDelete?.() }, () => t("COMPONENTS.ORGANISMS.VC_POPUP.CANCEL")),
      ]),
  },
});
const askDelete = () =>
  new Promise<boolean | undefined>((resolve) => {
    settleDelete = (withValues) => {
      settleDelete = undefined;
      deletePrompt.close();
      resolve(withValues);
    };
    deletePrompt.open();
  });

const item = ref<Property>({});
const loading = ref(false);
const languages = ref<string[]>([]);
const defaultLanguage = ref<string>();
const groups = ref<PropertyGroup[]>([]);
const measures = ref<Measure[]>([]);
const nameError = ref("");
const isNew = computed(() => !!item.value.isNew || !param.value);
const propertyTypes = options(["Product", "Variation", "Category", "Catalog"]);
// Measure values exist only on products and variations (legacy initializeBlade)
const valueTypes = computed(() =>
  options(Object.values(PropertyValueType)).filter(
    (v) => v.id !== "Measure" || item.value.type === "Product" || item.value.type === "Variation",
  ),
);
// legacy property-detail: the flags each value type allows
const allowed = computed(() => {
  const type = item.value.valueType ?? "";
  const none = ["DateTime", "Boolean", "Measure"].includes(type);
  return {
    multivalue: !none && type !== "Html",
    dictionary: !none && !["Integer", "GeoPoint", "Number", "LongText", "Html"].includes(type),
    multilanguage: !none && !["Integer", "GeoPoint", "Number"].includes(type),
  };
});
// a value type change clears the flags it does not allow (not on load)
watch(
  () => item.value.valueType,
  (type, previous) => {
    if (!previous) return;
    for (const flag of ["multivalue", "dictionary", "multilanguage"] as const) if (!allowed.value[flag]) item.value[flag] = false;
  },
);
const isText = computed(() => item.value.valueType === "ShortText" || item.value.valueType === "LongText");

const form = useBladeForm({ data: item, closeConfirmMessage: computed(() => t("CATALOG.ALERTS.CLOSE_CONFIRM")) });

// the legacy validation-rules blade edits validationRules[0] (normalized in load)
const rule = computed<PropertyValidationRule>(() => item.value.validationRules?.[0] ?? {});
const originalName = ref<string>();

const displayName = (lang: string) => item.value.displayNames?.find((d) => d.languageCode === lang)?.name ?? "";
function setDisplayName(lang: string, name: string) {
  const list = (item.value.displayNames ??= []);
  const found = list.find((d) => d.languageCode === lang);
  if (found) found.name = name;
  else list.push({ languageCode: lang, name });
}

let loadedCatalogId: string | undefined;
async function load() {
  loading.value = true;
  try {
    if (param.value) item.value = await api.properties.getProperty(param.value);
    else {
      const o = bladeOptions.value;
      item.value = o?.categoryId
        ? await api.properties.getNewCategoryProperty(o.categoryId)
        : await api.properties.getNewCatalogProperty(o!.catalogId!);
      // product properties are the common case (legacy property-type-list asks first)
      item.value.type = (o?.type ?? "Product") as Property["type"];
      item.value.valueType ??= PropertyValueType.ShortText;
    }
    if (!item.value.validationRules?.length) item.value.validationRules = [{ propertyId: item.value.id }];
    originalName.value = isNew.value ? undefined : item.value.name;
    // the catalog's languages and groups: read again only for another catalog (not on save or reset)
    const catalogId = item.value.catalogId;
    if (catalogId && catalogId !== loadedCatalogId) {
      loadedCatalogId = catalogId;
      const [catalog, g] = await Promise.all([
        api.catalogs.getCatalog(catalogId),
        api.propertyGroups.search({ catalogId, take: 1000 }),
      ]);
      languages.value = (catalog.languages ?? []).map((l) => l.languageCode!);
      defaultLanguage.value = catalog.defaultLanguage?.languageCode;
      groups.value = g.results ?? [];
    }
  } finally {
    loading.value = false;
  }
  form.setBaseline();
}

async function save() {
  nameError.value = "";
  loading.value = true;
  try {
    // property names must be unique within the category tree (legacy doValidateNameAsync)
    if (item.value.name !== originalName.value) {
      const check = await api.properties.validatePropertyName({
        propertyName: item.value.name,
        propertyType: item.value.type,
        propertyValueType: item.value.valueType,
        categoryId: item.value.categoryId,
        catalogId: item.value.catalogId,
      });
      if (check && check.isValid === false) {
        nameError.value = (check.errors ?? []).map((e) => e.errorMessage).join("; ") || t("CATALOG.PROPERTY.NAME_TAKEN");
        return;
      }
    }
    const body: Property = { ...item.value };
    delete (body as { validationRule?: unknown }).validationRule;
    if (!isText.value || !body.validationRules?.some((r) => r.charCountMin || r.charCountMax || r.regExp))
      body.validationRules = [];
    await api.properties.saveProperty(body);
    form.setBaseline();
    callParent("reload");
    if (isNew.value && body.id) await replaceWith({ name: "CatalogPropertyDetails", param: body.id });
    else if (isNew.value) await closeSelf();
    else await load();
  } finally {
    loading.value = false;
  }
}

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "save",
    title: t("CATALOG.TOOLBAR.SAVE"),
    icon: "lucide-save",
    permissions: "catalog:metadata-property:edit",
    disabled: !(isNew.value || form.canSave.value) || !item.value.name,
    clickHandler: save,
  },
  {
    id: "reset",
    title: t("CATALOG.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    disabled: !form.isModified.value,
    clickHandler: load,
  },
  {
    id: "delete",
    title: t("CATALOG.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    isVisible: !isNew.value && item.value.isManageable !== false,
    permissions: "catalog:metadata-property:edit",
    async clickHandler() {
      const withValues = await askDelete();
      if (withValues === undefined) return;
      await api.properties.deleteProperty(item.value.id, withValues);
      form.setBaseline();
      callParent("reload");
      closeSelf();
    },
  },
]);

useBladeWidgets([
  {
    id: "CatalogPropertyDictionary",
    icon: "lucide-book-open",
    title: "CATALOG.WIDGETS.DICTIONARY",
    isVisible: computed(() => !!item.value.dictionary && !isNew.value),
    onClick: () =>
      openBlade({
        name: "CatalogPropertyDictionary",
        options: { property: item.value, languages: languages.value, defaultLanguage: defaultLanguage.value },
      }),
  },
]);

// the measures only fill a select: they load alongside the property
onMounted(async () => {
  await Promise.all([api.measures.searchMeasures({ take: 1000 }).then((r) => (measures.value = r.results ?? [])), load()]);
});
</script>
