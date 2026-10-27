<template>
  <!-- A Catalog widget on the Store details blade (legacy: widgets the catalog registered into "storeDetail") -->
  <VcWidget
    :icon="icon"
    :title="$t(title)"
    :disabled="!store?.id"
    @click="open"
  />
</template>

<script setup lang="ts">
import { injectBladeContext, useBlade } from "@vc-shell/framework";
import { VcWidget } from "@vc-shell/framework/ui";
import { computed } from "vue";
import type { Store } from "../api";

const props = defineProps<{ icon: string; title: string; blade: string }>();

// the store shared by the Store blade via defineBladeContext({ item })
const ctx = injectBladeContext();
const store = computed(() => ctx.value.item as Store | undefined);
const { openBlade } = useBlade();

function open() {
  if (store.value?.id) openBlade({ name: props.blade, options: { store: store.value } });
}
</script>
