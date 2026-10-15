import * as blades from "./pages";
import * as locales from "./locales";
import { defineAppModule, registerExternalWidget, useExtensionPoint } from "@vc-shell/framework";
import { defineComponent, h, markRaw, type App } from "vue";
import StoreWidget from "./components/StoreWidget.vue";
import ExportFilterField from "./components/ExportFilterField.vue";
import { registerProductSearch } from "./search";
import "./styles.scss";

// Widgets the legacy catalog registered into the Store details blade ("storeDetail")
for (const [id, icon, title, key] of [
  ["CatalogStoreBrands", "lucide-award", "Brands", "CATALOG.STORE.BRANDS"],
  ["CatalogStoreSearch", "lucide-sliders-horizontal", "Search configuration", "CATALOG.STORE.SEARCH"],
]) {
  registerExternalWidget({
    id,
    title,
    component: markRaw(defineComponent({ name: `${id}Widget`, setup: () => () => h(StoreWidget, { icon, title: key, blade: id }) })),
    targetBlades: ["StoreDetails"],
    // not on the unsaved "New store" blade
    isVisible: (blade) => !!blade?.param,
  });
}

// filter fields of the catalog export types in the Export module (legacy metafields "<type>ExportFilter")
const field = (kind: string, priority: number) => ({
  id: `catalog-${kind}`,
  component: markRaw(ExportFilterField),
  props: { kind },
  priority,
});
const productFilter = useExtensionPoint("export-filter:VirtoCommerce.CatalogModule.Core.Model.Export.ExportableProduct");
for (const [kind, priority] of [["catalogs", 10], ["categories", 20], ["searchInVariations", 30], ["searchInChildren", 40]] as const)
  productFilter.add(field(kind, priority));
useExtensionPoint("export-filter:VirtoCommerce.CatalogModule.Core.Model.Export.ExportableCatalogFull").add(field("catalogs", 10));

const appModule = defineAppModule({
  blades,
  locales,
});

export default {
  install(app: App) {
    appModule.install(app);
    // products in the Admin Shell's global search
    registerProductSearch(app);
  },
};

export * from "./pages";
