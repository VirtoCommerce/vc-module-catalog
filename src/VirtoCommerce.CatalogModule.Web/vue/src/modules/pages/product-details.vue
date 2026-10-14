<template>
  <VcBlade
    :title="title"
    :toolbar-items="toolbar"
    :loading="loading"
    width="70%"
  >
    <VcContainer>
      <VcForm class="catalog-form">
        <VcBanner
          v-if="item.id && item.parentCategoryIsActive === false"
          variant="warning"
          icon="lucide-eye-off"
        >
          {{ $t("CATALOG.CATEGORY.PARENT_HIDDEN") }}
        </VcBanner>
        <ImageGallery
          :entity="item"
          :languages="languages"
        />
        <VcRow class="tw-gap-4">
          <VcCol :size="2">
            <VcInput
              v-model="item.name"
              :label="$t('CATALOG.FIELDS.NAME')"
              name="name"
              rules="required"
              required
            />
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="item.productType"
              :label="$t('CATALOG.FIELDS.PRODUCT_TYPE')"
              :options="productTypes"
              :clearable="false"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="item.code"
              :label="$t('CATALOG.FIELDS.SKU')"
              name="code"
              rules="required"
              required
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model="item.gtin"
              :label="$t('CATALOG.FIELDS.GTIN')"
              maxlength="14"
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model="item.manufacturerPartNumber"
              :label="$t('CATALOG.FIELDS.MPN')"
              maxlength="70"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcSwitch
              v-model="item.isActive"
              :label="$t('CATALOG.FIELDS.VISIBLE')"
            />
          </VcCol>
          <VcCol>
            <VcSwitch
              v-model="item.isBuyable"
              :label="$t('CATALOG.FIELDS.BUYABLE')"
            />
          </VcCol>
          <VcCol>
            <VcSwitch
              v-model="item.trackInventory"
              :label="$t('CATALOG.FIELDS.TRACK_INVENTORY')"
            />
          </VcCol>
        </VcRow>

        <VcCard
          :header="$t('CATALOG.PRODUCT.SALES')"
          is-collapsable
        >
          <div class="catalog-card">
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcInput
                  v-model.number="priority"
                  type="number"
                  :label="virtualLink ? $t('CATALOG.PRODUCT.LINK_PRIORITY') : $t('CATALOG.FIELDS.PRIORITY')"
                />
              </VcCol>
              <VcCol>
                <VcInput
                  v-model.number="item.minQuantity"
                  type="number"
                  :label="$t('CATALOG.FIELDS.MIN_QUANTITY')"
                />
              </VcCol>
              <VcCol>
                <VcInput
                  v-model.number="item.maxQuantity"
                  type="number"
                  :label="$t('CATALOG.FIELDS.MAX_QUANTITY')"
                />
              </VcCol>
              <VcCol>
                <VcInput
                  v-model.number="item.packSize"
                  type="number"
                  :label="$t('CATALOG.FIELDS.PACK_SIZE')"
                />
              </VcCol>
            </VcRow>
            <div
              v-if="quantityError"
              class="catalog-card__error"
            >
              {{ quantityError }}
            </div>
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcDatePicker
                  v-model="item.startDate"
                  :label="$t('CATALOG.FIELDS.START_DATE')"
                />
              </VcCol>
              <VcCol>
                <VcDatePicker
                  v-model="item.endDate"
                  :label="$t('CATALOG.FIELDS.END_DATE')"
                />
              </VcCol>
            </VcRow>
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcSelect
                  v-model="item.vendor"
                  :label="$t('CATALOG.FIELDS.VENDOR')"
                  :options="searchVendors"
                  option-value="id"
                  option-label="name"
                  searchable
                />
              </VcCol>
              <VcCol>
                <VcSelect
                  v-model="item.taxType"
                  :label="$t('CATALOG.FIELDS.TAX_TYPE')"
                  :options="taxTypes"
                />
              </VcCol>
            </VcRow>
          </div>
        </VcCard>

        <VcCard
          v-if="item.productType === 'Digital'"
          :header="$t('CATALOG.PRODUCT.DIGITAL')"
          is-collapsable
        >
          <div class="catalog-card">
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcSelect
                  v-model="item.downloadType"
                  :label="$t('CATALOG.FIELDS.DOWNLOAD_TYPE')"
                  :options="downloadTypes"
                />
              </VcCol>
              <VcCol>
                <VcInput
                  v-model.number="item.maxNumberOfDownload"
                  type="number"
                  :label="$t('CATALOG.FIELDS.MAX_DOWNLOADS')"
                />
              </VcCol>
            </VcRow>
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcDatePicker
                  v-model="item.downloadExpiration"
                  :label="$t('CATALOG.FIELDS.DOWNLOAD_EXPIRATION')"
                />
              </VcCol>
              <VcCol>
                <VcSwitch
                  v-model="item.hasUserAgreement"
                  :label="$t('CATALOG.FIELDS.USER_AGREEMENT')"
                />
              </VcCol>
            </VcRow>
          </div>
        </VcCard>

        <VcCard
          v-if="item.productType !== 'Digital'"
          :header="$t('CATALOG.PRODUCT.DIMENSIONS')"
          is-collapsable
        >
          <div class="catalog-card">
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcInput
                  v-model.number="item.weight"
                  type="number"
                  :label="$t('CATALOG.FIELDS.WEIGHT')"
                />
              </VcCol>
              <VcCol>
                <VcSelect
                  v-model="item.weightUnit"
                  :label="$t('CATALOG.FIELDS.WEIGHT_UNIT')"
                  :options="weightUnits"
                />
              </VcCol>
              <VcCol>
                <VcSelect
                  v-model="item.packageType"
                  :label="$t('CATALOG.FIELDS.PACKAGE_TYPE')"
                  :options="packageTypes"
                  option-value="id"
                  option-label="name"
                >
                  <!-- legacy item-dimensions pencil: package types are Core's (core_admin) -->
                  <template
                    v-if="hasAccess('platform:setting:update')"
                    #append
                  >
                    <VcButton
                      icon="lucide-pencil"
                      icon-size="s"
                      text
                      :aria-label="$t('CATALOG.PRODUCT.EDIT_PACKAGE_TYPES')"
                      @click="openBlade({ name: 'CorePackageTypes', onClose: loadPackageTypes })"
                    />
                  </template>
                </VcSelect>
              </VcCol>
              <VcCol>
                <VcSwitch
                  :model-value="usePackageType"
                  :label="$t('CATALOG.PRODUCT.USE_PACKAGE_TYPE')"
                  :disabled="!item.packageType"
                  @update:model-value="(on?: boolean) => togglePackageType(!!on)"
                />
              </VcCol>
            </VcRow>
            <VcRow
              v-if="!usePackageType"
              class="tw-gap-4"
            >
              <VcCol>
                <VcInput
                  v-model.number="item.length"
                  type="number"
                  :label="$t('CATALOG.FIELDS.LENGTH')"
                />
              </VcCol>
              <VcCol>
                <VcInput
                  v-model.number="item.width"
                  type="number"
                  :label="$t('CATALOG.FIELDS.WIDTH')"
                />
              </VcCol>
              <VcCol>
                <VcInput
                  v-model.number="item.height"
                  type="number"
                  :label="$t('CATALOG.FIELDS.HEIGHT')"
                />
              </VcCol>
              <VcCol>
                <VcSelect
                  v-model="item.measureUnit"
                  :label="$t('CATALOG.FIELDS.MEASURE_UNIT')"
                  :options="measureUnits"
                />
              </VcCol>
            </VcRow>
          </div>
        </VcCard>

        <LocalizedNames
          :entity="item"
          :languages="languages"
        />

        <PropertyValues
          :entity="item"
          :types="['Product', 'Variation']"
          :header="$t('CATALOG.WIDGETS.PROPERTIES')"
          :languages="languages"
          :default-language="catalog?.defaultLanguage?.languageCode"
          allow-custom
          custom-type="Product"
          :edit-handler="editProperty"
        />

        <VcCard
          :header="$t('CATALOG.PRODUCT.SYSTEM')"
          is-collapsable
          is-collapsed
        >
          <div class="catalog-card">
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcField
                  :label="$t('CATALOG.FIELDS.ID')"
                  :model-value="item.id"
                  copyable
                />
              </VcCol>
              <VcCol>
                <VcInput
                  v-model="item.outerId"
                  :label="$t('CATALOG.FIELDS.OUTER_ID')"
                />
              </VcCol>
            </VcRow>
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcField
                  :label="$t('CATALOG.FIELDS.CATALOG')"
                  :model-value="catalog?.name"
                />
              </VcCol>
              <VcCol>
                <VcField
                  :label="$t('CATALOG.FIELDS.CATEGORY')"
                  :model-value="item.path"
                />
              </VcCol>
            </VcRow>
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcField
                  :label="$t('CATALOG.FIELDS.CREATED')"
                  :model-value="item.createdDate"
                  type="date"
                />
              </VcCol>
              <VcCol>
                <VcField
                  :label="$t('CATALOG.FIELDS.MODIFIED')"
                  :model-value="item.modifiedDate"
                  type="date"
                />
              </VcCol>
            </VcRow>
          </div>
        </VcCard>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import {
  defineBladeContext,
  useBlade,
  useBladeForm,
  useBladeWidgets,
  usePermissions,
  usePopup,
  type IBladeToolbar,
} from "@vc-shell/framework";
import {
  VcBanner,
  VcBlade,
  VcButton,
  VcCard,
  VcCol,
  VcContainer,
  VcDatePicker,
  VcField,
  VcForm,
  VcInput,
  VcRow,
  VcSelect,
  VcSwitch,
} from "@vc-shell/framework/ui";
import { computed, onMounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { catalogLanguages, clone, api, http, options, settingOptions } from "../api";
import PropertyValues from "../components/PropertyValues.vue";
import ImageGallery from "../components/ImageGallery.vue";
import LocalizedNames from "../components/LocalizedNames.vue";
import type { Catalog, CatalogProduct, Property } from "../../api_client/virtocommerce.catalog";

defineBlade({ url: "/catalog-product", name: "CatalogProductDetails" });

const { t } = useI18n({ useScope: "global" });
const { param, options: bladeOptions, openBlade, callParent, closeSelf, replaceWith, exposeToChildren } = useBlade<{
  catalogId?: string;
  categoryId?: string;
  productType?: string;
  /** a clone (cloneProduct) or a new variation (getNewVariation) to create */
  product?: CatalogProduct;
  /** opened from a virtual catalog/category */
  linkCatalogId?: string;
  linkCategoryId?: string;
}>();
const { showConfirmation } = usePopup();
const { hasAccess } = usePermissions();
const configSections = ref(0);
// widget badges that need their own request (by id: on open they load alongside the product)
async function loadBadges(productId = item.value.id) {
  if (!productId) return;
  if (hasAccess("catalog:configurations:read"))
    configSections.value =
      (await api.configurations.searchConfigurations({ productId, take: 1 }).catch(() => undefined))?.results?.[0]?.sections?.length ?? 0;
}

const item = ref<CatalogProduct>({});
const catalog = ref<Catalog>();
const loading = ref(false);
const taxTypes = ref<{ id: string; title: string }[]>([]);
const weightUnits = ref<{ id: string; title: string }[]>([]);
const measureUnits = ref<{ id: string; title: string }[]>([]);
const productTypes = options(["Physical", "Digital", "BillOfMaterials"]);
interface PackageType {
  id?: string;
  name?: string;
  measureUnit?: string;
}
const packageTypes = ref<PackageType[]>([]);
const loadPackageTypes = async () =>
  (packageTypes.value = (await http<PackageType[]>("GET", "/api/packageTypes").catch(() => [])) ?? []);
// legacy item-dimensions: a package type can stand for the dimensions (then measure unit and sizes are dropped)
const usePackageType = ref(false);
function togglePackageType(on: boolean) {
  usePackageType.value = on;
  if (on) Object.assign(item.value, { measureUnit: undefined, height: undefined, width: undefined, length: undefined });
  // back to own dimensions: start from the package type's unit (legacy)
  else item.value.measureUnit ??= packageTypes.value.find((p) => p.id === item.value.packageType)?.measureUnit;
}
watch(
  () => item.value.packageType,
  (type) => !type && (usePackageType.value = false),
);
const initPackageType = () =>
  (usePackageType.value = !!item.value.packageType && !(item.value.measureUnit && item.value.height && item.value.width && item.value.length));

// opened from a virtual catalog: the priority is the one of the product's link there (legacy _priority)
const virtualLink = computed(() => {
  const o = bladeOptions.value;
  if (!o?.linkCatalogId) return undefined;
  return item.value.links?.find((l) => l.catalogId === o.linkCatalogId && (!o.linkCategoryId || l.categoryId === o.linkCategoryId));
});
const priority = computed({
  get: () => (virtualLink.value ? virtualLink.value.priority : item.value.priority),
  set: (v) => {
    if (virtualLink.value) virtualLink.value.priority = v;
    else item.value.priority = v;
  },
});
const downloadTypes = options(["Standard Product", "Software", "Music"]);

const languages = computed(() => catalogLanguages(catalog.value));
const isVariation = computed(() => !!item.value.mainProductId);
const title = computed(
  () => item.value.name || t(isVariation.value ? "CATALOG.PRODUCT.NEW_VARIATION" : "CATALOG.PRODUCT.NEW"),
);

const form = useBladeForm({ data: item, closeConfirmMessage: computed(() => t("CATALOG.ALERTS.CLOSE_CONFIRM")) });

// legacy item-detail isValidQuantity
const quantityError = computed(() => {
  const { minQuantity: min = 0, maxQuantity: max = 0, packSize } = item.value;
  if ((min ?? 0) < 0 || (max ?? 0) < 0) return t("CATALOG.PRODUCT.QTY_NEGATIVE");
  if (!packSize || packSize < 1) return t("CATALOG.PRODUCT.QTY_PACK");
  if (max && (min ?? 0) > max) return t("CATALOG.PRODUCT.QTY_MIN_MAX");
  if (max && packSize > max) return t("CATALOG.PRODUCT.QTY_PACK_MAX");
  return "";
});


async function searchVendors(keyword?: string, skip = 0, ids?: string[]) {
  return http<{ results?: { id: string; name: string }[]; totalCount?: number }>("POST", "/api/members/search", {
    memberType: "Vendor",
    keyword,
    objectIds: ids,
    sort: "name:asc",
    skip,
    take: 20,
  }).catch(() => ({ results: [], totalCount: 0 }));
}

async function load() {
  loading.value = true;
  try {
    const o = bladeOptions.value;
    if (param.value) item.value = await api.products.getProductById(param.value);
    else if (o?.product) item.value = clone(o.product);
    else {
      item.value = o?.categoryId
        ? await api.products.getNewProductByCatalogAndCategory(o.catalogId!, o.categoryId)
        : await api.products.getNewProductByCatalog(o!.catalogId!);
      item.value.productType = o?.productType ?? "Physical";
      item.value.packSize ||= 1;
    }
    if (!item.value.id) {
      // what the server saves for unset flags; digital and BOM products track no inventory (legacy setTrackInventory)
      item.value.isActive ??= true;
      item.value.isBuyable ??= true;
      item.value.trackInventory ??= !["Digital", "BillOfMaterials"].includes(item.value.productType ?? "");
    }
    if (item.value.catalogId && catalog.value?.id !== item.value.catalogId)
      catalog.value = await api.catalogs.getCatalog(item.value.catalogId);
    initPackageType();
  } finally {
    loading.value = false;
  }
  form.setBaseline();
}

async function save() {
  loading.value = true;
  try {
    const isNew = !item.value.id;
    // variations are saved by their own blades; sending them back makes the server re-insert them
    // (legacy item-detail loads without them: respGroup 2015). A clone's first save creates its copies.
    const body: CatalogProduct = isNew ? item.value : { ...item.value, variations: undefined };
    const saved = await api.products.saveProduct(body);
    form.setBaseline();
    callParent("reload");
    if (isNew) {
      // reopen as the saved product so the url, widgets and child blades use its id
      await replaceWith({ name: "CatalogProductDetails", param: saved.id });
      return;
    }
    item.value = await api.products.getProductById(saved.id!);
    form.setBaseline();
  } finally {
    loading.value = false;
  }
}

function editProperty(property: Property) {
  openBlade({ name: "CatalogPropertyDetails", param: property.id, options: { languages: languages.value } });
}

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "save",
    title: t("CATALOG.TOOLBAR.SAVE"),
    icon: "lucide-save",
    permissions: item.value.id ? ["catalog:update", "catalog:products:update"] : ["catalog:create", "catalog:products:create"],
    disabled: !form.canSave.value || !item.value.name || !item.value.code || !!quantityError.value,
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
    id: "clone",
    title: t("CATALOG.PRODUCT.CLONE"),
    icon: "lucide-copy",
    isVisible: !!item.value.id,
    disabled: form.isModified.value,
    permissions: ["catalog:create", "catalog:products:create"],
    async clickHandler() {
      const copy = await api.products.cloneProduct(item.value.id!);
      openBlade({ name: "CatalogProductDetails", options: { product: copy } });
    },
  },
  {
    id: "delete",
    title: t("CATALOG.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    isVisible: !!item.value.id,
    permissions: ["catalog:delete", "catalog:products:delete"],
    async clickHandler() {
      if (!(await showConfirmation(t("CATALOG.ALERTS.DELETE_PRODUCT", { name: item.value.name })))) return;
      await api.products.deleteProduct([item.value.id!]);
      form.setBaseline();
      callParent("reload");
      closeSelf();
    },
  },
]);

const saved = computed(() => !!item.value.id);
// item is a getter: save and reset replace the product, and open children must follow (legacy x.refresh(blade.item))
const open = (name: string, extra: Record<string, unknown> = {}) =>
  openBlade({
    name,
    options: {
      get item() {
        return item.value;
      },
      kind: "product",
      catalog: catalog.value,
      languages: languages.value,
      ...extra,
    },
  });
useBladeWidgets([
  {
    id: "CatalogDescriptions",
    icon: "lucide-file-text",
    title: "CATALOG.WIDGETS.DESCRIPTIONS",
    badge: computed(() => item.value.reviews?.length ?? 0),
    onClick: () => open("CatalogDescriptions"),
  },
  {
    id: "CatalogVariations",
    icon: "lucide-git-branch",
    title: "CATALOG.WIDGETS.VARIATIONS",
    badge: computed(() => item.value.variations?.length ?? 0),
    isVisible: computed(() => saved.value && !isVariation.value),
    onClick: () => open("CatalogVariations"),
  },
  {
    id: "CatalogAssociations",
    icon: "lucide-link-2",
    title: "CATALOG.WIDGETS.ASSOCIATIONS",
    badge: computed(() => item.value.associations?.length ?? 0),
    isVisible: saved,
    onClick: () => open("CatalogAssociations"),
  },
  {
    id: "CatalogBom",
    icon: "lucide-list-checks",
    title: "CATALOG.WIDGETS.BOM",
    badge: computed(() => item.value.associations?.filter((a) => a.type === "BillOfMaterials").length ?? 0),
    isVisible: computed(() => saved.value && item.value.productType === "BillOfMaterials"),
    onClick: () => open("CatalogAssociations", { bom: true }),
  },
  {
    id: "CatalogSeo",
    icon: "lucide-globe",
    title: "CATALOG.WIDGETS.SEO",
    badge: computed(() => item.value.seoInfos?.length ?? 0),
    onClick: () => open("CatalogSeo", { objectType: "CatalogProduct" }),
  },
  {
    id: "CatalogLinks",
    icon: "lucide-folder-tree",
    title: "CATALOG.WIDGETS.LINKS",
    badge: computed(() => item.value.links?.length ?? 0),
    isVisible: saved,
    onClick: () => open("CatalogLinks"),
  },
  {
    id: "CatalogAssets",
    icon: "lucide-paperclip",
    title: "CATALOG.WIDGETS.ASSETS",
    badge: computed(() => item.value.assets?.length ?? 0),
    onClick: () => open("CatalogAssets"),
  },
  {
    id: "CatalogVideos",
    icon: "lucide-video",
    title: "CATALOG.WIDGETS.VIDEOS",
    isVisible: saved,
    onClick: () => open("CatalogVideos"),
  },
  {
    id: "CatalogConfiguration",
    icon: "lucide-puzzle",
    title: "CATALOG.WIDGETS.CONFIGURATION",
    badge: computed(() => configSections.value),
    isVisible: computed(() => saved.value && hasAccess("catalog:configurations:read")),
    onClick: () => open("CatalogConfiguration"),
  },
]);

// the product, for widgets other modules add to this blade (Prices, Inventory, ...)
defineBladeContext({ item });

// the lookups only fill the selects: they load alongside the product and its badges
onMounted(async () => {
  await Promise.all([
    settingOptions("VirtoCommerce.Core.General.TaxTypes").then((x) => (taxTypes.value = x)),
    settingOptions("VirtoCommerce.Core.General.WeightUnits").then((x) => (weightUnits.value = x)),
    settingOptions("VirtoCommerce.Core.General.MeasureUnits").then((x) => (measureUnits.value = x)),
    loadPackageTypes(),
    load(),
    loadBadges(param.value),
  ]);
});

exposeToChildren({
  /** child blades that save on the server (variations, associations, links, videos) refresh the product */
  async reload() {
    if (!item.value.id) return;
    const wasModified = form.isModified.value;
    const fresh = await api.products.getProductById(item.value.id);
    item.value.variations = fresh.variations;
    item.value.associations = fresh.associations;
    item.value.links = fresh.links;
    if (!wasModified) form.setBaseline();
    await loadBadges();
    callParent("reload");
  },
});
</script>
