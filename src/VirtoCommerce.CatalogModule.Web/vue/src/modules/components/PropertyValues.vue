<template>
  <VcCard
    :header="header"
    is-collapsable
  >
    <template #actions>
      <div @click.stop>
        <div class="property-values__actions">
          <VcSelect
            v-if="languages.length > 1"
            v-model="currentLanguage"
            class="property-values__lang"
            size="small"
            :options="options(languages)"
            :clearable="false"
            :aria-label="$t('CATALOG.FIELDS.LANGUAGE')"
          />
          <VcButton
            v-if="manageHandler"
            size="sm"
            variant="secondary"
            icon="lucide-settings-2"
            @click="manageHandler"
          >
            {{ $t("CATALOG.PROPERTIES.MANAGE") }}
          </VcButton>
        </div>
      </div>
    </template>
    <div class="property-values">
      <div
        v-if="all.length > 1"
        class="property-values__tools"
      >
        <VcSelect
          v-model="shown"
          class="tw-flex-1"
          :label="$t('CATALOG.PROPERTIES.FILTER')"
          :placeholder="$t('CATALOG.PROPERTIES.FILTER_ALL')"
          :options="options(all.map((p) => p.name!))"
          multiple
          searchable
        />
        <VcSwitch
          v-model="hideEmpty"
          :label="$t('CATALOG.PROPERTIES.HIDE_EMPTY')"
        />
      </div>
      <div
        v-if="!visible.length && !customAllowed"
        class="property-values__empty"
      >
        {{ $t("CATALOG.PROPERTIES.EMPTY") }}
      </div>
      <div
        v-for="property in visible"
        :key="(property.id ?? property.name) + currentLanguage"
        class="property-values__row"
        :data-property="property.name"
      >
        <div
          v-if="renaming === property"
          class="property-values__rename"
        >
          <VcInput
            v-model="newName"
            class="tw-flex-1"
            :label="$t('CATALOG.PROPERTIES.RENAME')"
          />
          <VcButton
            icon="lucide-check"
            variant="secondary"
            :disabled="!newName || (newName !== property.name && entity.properties?.some((p) => p.name === newName))"
            @click="rename(property)"
          >
            {{ $t("CATALOG.TOOLBAR.APPLY") }}
          </VcButton>
        </div>
        <VcDynamicProperty
          v-else
          class="property-values__control"
          :property="property"
          :model-value="valueOf(property)"
          :options-getter="loadDictionaries"
          :measurements-getter="loadMeasurements"
          :required="!!property.required"
          :multivalue="property.multivalue"
          :multilanguage="property.multilanguage"
          :current-language="currentLanguage"
          :value-type="property.valueType ?? 'ShortText'"
          :dictionary="property.dictionary"
          :name="property.name ?? ''"
          :display-names="property.displayNames"
          :rules="{
            min: property.validationRule?.charCountMin,
            max: property.validationRule?.charCountMax,
            regex: property.validationRule?.regExp,
          }"
          :disabled="disabled || property.isReadOnly || (!property.id && !canEditCustom)"
          @update:model-value="(ev: unknown) => onChange(property, ev)"
        />
        <VcButton
          v-if="property.id && editHandler"
          class="property-values__edit"
          icon="lucide-pencil"
          icon-size="s"
          text
          :aria-label="$t('CATALOG.PROPERTIES.EDIT', { name: property.name })"
          @click="editHandler(property)"
        />
        <VcButton
          v-if="!property.id && customAllowed && renaming !== property"
          class="property-values__edit"
          icon="lucide-pencil"
          icon-size="s"
          text
          :aria-label="$t('CATALOG.PROPERTIES.RENAME')"
          @click="startRename(property)"
        />
        <VcButton
          v-if="!property.id && customAllowed"
          class="property-values__edit"
          icon="lucide-x"
          icon-size="s"
          text
          :aria-label="$t('CATALOG.PROPERTIES.REMOVE', { name: property.name })"
          @click="removeCustom(property)"
        />
      </div>
      <div
        v-if="customAllowed && !disabled"
        class="catalog-inline-add"
      >
        <VcInput
          v-model="customName"
          :label="$t('CATALOG.PROPERTIES.CUSTOM_NAME')"
          class="tw-flex-1"
        />
        <VcSelect
          v-model="customType"
          :label="$t('CATALOG.PROPERTIES.VALUE_TYPE')"
          :options="valueTypes"
          :clearable="false"
          class="tw-flex-1"
        />
        <VcButton
          icon="lucide-plus"
          variant="secondary"
          :disabled="!customName || entity.properties?.some((p) => p.name === customName)"
          @click="addCustom"
        >
          {{ $t("CATALOG.PROPERTIES.ADD_CUSTOM") }}
        </VcButton>
      </div>
    </div>
  </VcCard>
</template>

<script setup lang="ts">
import { useDynamicProperties, usePermissions } from "@vc-shell/framework";
import { VcButton, VcCard, VcDynamicProperty, VcInput, VcSelect, VcSwitch } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { api, options } from "../api";
import { PropertyValueType, type Measure, type Property } from "../../api_client/virtocommerce.catalog";

const props = defineProps<{
  entity: { properties?: Property[] };
  /** property types whose values are edited here (e.g. Product + Variation) */
  types: string[];
  header: string;
  languages: string[];
  defaultLanguage?: string;
  disabled?: boolean;
  /** products only: add/remove custom (unmanaged) properties, legacy item-property-detail */
  allowCustom?: boolean;
  customType?: string;
  editHandler?: (property: Property) => void;
  manageHandler?: () => void;
}>();

// custom (unmanaged) properties need their own permission, or the whole save is refused (legacy property-list)
const { hasAccess } = usePermissions();
const canEditCustom = hasAccess("catalog:custom-property:edit");
const customAllowed = computed(() => !!props.allowCustom && canEditCustom);

const currentLanguage = ref(props.defaultLanguage ?? props.languages[0] ?? "en-US");
watch(
  () => props.defaultLanguage,
  (l) => l && (currentLanguage.value = l),
);

const all = computed(() =>
  (props.entity.properties ?? [])
    .filter((p) => props.types.includes(p.type ?? ""))
    .sort((a, b) => (a.displayOrder ?? 0) - (b.displayOrder ?? 0) || (a.name ?? "").localeCompare(b.name ?? "")),
);

// legacy property-list filter (kept per browser) and "hide empty values" (required and Boolean properties stay)
const FILTER_KEY = "catalog.propertyFilter";
const shown = ref<string[]>(readFilter());
const hideEmpty = ref(false);
function readFilter(): string[] {
  try {
    return JSON.parse(localStorage.getItem(FILTER_KEY) ?? "[]");
  } catch {
    return [];
  }
}
watch(shown, (names) => {
  try {
    localStorage.setItem(FILTER_KEY, JSON.stringify(names));
  } catch {
    // per-browser convenience only
  }
});
const isEmpty = (p: Property) => !(p.values ?? []).some((v) => v.value !== undefined && v.value !== null && v.value !== "");
const visible = computed(() => {
  const names = new Set(shown.value);
  // a filter that matches none of these properties shows them all
  const filtered = all.value.some((x) => names.has(x.name!));
  return all.value.filter(
    (p) => (!filtered || names.has(p.name!)) && (!hideEmpty.value || p.required || p.valueType === "Boolean" || !isEmpty(p)),
  );
});

// rows remount on a language switch or a filter change: each measure is read once per editor (units are localized per call)
const measures = new Map<string, Promise<Measure>>();
function getMeasure(id: string) {
  if (!measures.has(id))
    measures.set(
      id,
      api.measures.getMeasureById(id).catch((e) => {
        measures.delete(id);
        throw e;
      }),
    );
  return measures.get(id)!;
}

const { getPropertyValue, setPropertyValue, loadDictionaries, loadMeasurements } = useDynamicProperties({
  searchDictionary: async (criteria) =>
    (await api.dictionary.searchPropertyDictionaryItems({ ...criteria, take: criteria.take ?? 100 })).results,
  searchMeasurements: async (measureId: string, locale?: string) => {
    const measure = await getMeasure(measureId);
    return (measure.units ?? []).map((u) => ({
      ...u,
      displayName: (locale && u.localizedName?.values?.[locale]) || u.name,
      displaySymbol: (locale && u.localizedSymbol?.values?.[locale]) || u.symbol || u.code,
    }));
  },
});

// VcMultivalue (non-dictionary multivalue) labels its chips by `title`, the property values carry `value`
const isFreeMultivalue = (p: Property) => !!p.multivalue && !p.dictionary;
function valueOf(property: Property) {
  const value = getPropertyValue(property, currentLanguage.value);
  return isFreeMultivalue(property) && Array.isArray(value)
    ? value.map((v) => ({ ...(v as object), title: (v as { value?: unknown }).value }))
    : value;
}

function onChange(property: Property, ev: unknown) {
  const change = (ev && typeof ev === "object" && "value" in ev ? { ...ev } : { value: ev }) as { value: unknown };
  if (isFreeMultivalue(property) && Array.isArray(change.value))
    change.value = change.value.map(({ title, ...v }: { title?: unknown; value?: unknown }) => ({ ...v, value: v.value ?? title }));
  setPropertyValue({ property, ...change } as Parameters<typeof setPropertyValue>[0]);
  // an edited value overrides the inherited one; the server skips inherited values on save (legacy property2)
  property.values?.forEach((v) => (v.isInherited = false));
  // a new array so the blade form's deep watcher always sees nested value changes
  props.entity.properties = [...(props.entity.properties ?? [])];
}

const valueTypes = options(Object.values(PropertyValueType));
const customName = ref("");
const customType = ref<string>(PropertyValueType.ShortText);
function addCustom() {
  props.entity.properties = [
    ...(props.entity.properties ?? []),
    { name: customName.value, valueType: customType.value as PropertyValueType, type: props.customType as Property["type"], values: [] },
  ];
  customName.value = "";
}
// legacy item-property-detail: a custom (unmanaged) property can be renamed
const renaming = ref<Property>();
const newName = ref("");
function startRename(property: Property) {
  renaming.value = property;
  newName.value = property.name ?? "";
}
function rename(property: Property) {
  const name = newName.value;
  props.entity.properties = (props.entity.properties ?? []).map((p) =>
    p === property ? { ...p, name, values: (p.values ?? []).map((v) => ({ ...v, propertyName: name })) } : p,
  );
  renaming.value = undefined;
}

function removeCustom(property: Property) {
  props.entity.properties = (props.entity.properties ?? []).filter((p) => p !== property);
}
</script>

<style lang="scss">
.property-values {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  &__row {
    display: flex;
    align-items: flex-start;
    gap: 4px;
  }
  &__control {
    flex: 1;
    min-width: 0;
  }
  &__edit {
    margin-top: 28px;
  }
  /* rename + remove side by side: keep their 44px hit areas apart */
  &__edit + &__edit {
    margin-left: 20px;
  }
  &__empty {
    color: var(--neutrals-500);
    font-size: 14px;
  }
  &__tools {
    display: flex;
    align-items: flex-end;
    gap: 16px;
    padding-bottom: 12px;
    border-bottom: 1px solid var(--neutrals-200);
  }
  &__rename {
    flex: 1;
    display: flex;
    align-items: flex-end;
    gap: 12px;
  }
  &__actions {
    display: flex;
    align-items: center;
    gap: 8px;
  }
  &__lang {
    width: 120px;
  }
}
</style>
