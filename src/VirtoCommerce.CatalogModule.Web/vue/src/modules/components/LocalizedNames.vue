<template>
  <VcCard
    v-if="languages.length"
    :header="header ?? $t('CATALOG.FIELDS.LOCALIZED_NAME')"
    is-collapsable
    :is-collapsed="collapsed"
  >
    <div class="catalog-card">
      <template
        v-for="lang in languages"
        :key="lang"
      >
        <VcTextarea
          v-if="multiline"
          :model-value="values[lang]"
          :label="lang"
          @update:model-value="(v) => set(lang, v)"
        />
        <VcInput
          v-else
          :model-value="values[lang]"
          :label="lang"
          maxlength="1024"
          @update:model-value="(v) => set(lang, v)"
        />
      </template>
    </div>
  </VcCard>
</template>

<script setup lang="ts">
import { VcCard, VcInput, VcTextarea } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import type { LocalizedString } from "../../api_client/virtocommerce.catalog";

// legacy "localized name" inputs: one per catalog language, stored in localizedName.values
const props = defineProps<{
  entity: Record<string, unknown>;
  languages: string[];
  field?: string;
  header?: string;
}>();

const key = computed(() => props.field ?? "localizedName");
// descriptions are unbounded multi-line text (legacy textarea); names are single-line, up to 1024 chars
const multiline = computed(() => key.value === "localizedDescription");
const values = computed(() => ((props.entity[key.value] as LocalizedString | undefined)?.values ?? {}) as Record<string, string>);
const hasValues = computed(() => Object.values(values.value).some(Boolean));
// starts collapsed when empty and opens once values arrive (e.g. after a load), but never collapses while typing
const collapsed = ref(true);
watch(hasValues, (v) => v && (collapsed.value = false), { immediate: true });

function set(lang: string, value: string | number | Date | null | undefined) {
  const next = { ...values.value, [lang]: value ? String(value) : "" };
  if (!next[lang]) delete next[lang];
  props.entity[key.value] = { values: next };
}
</script>
