// Products in the Admin Shell's global search (the header box, Ctrl+K): this module's provider, registered with the
// shell's registry (app.config.globalProperties.$globalSearch; see the shell's docs/global-search.md). A result opens the
// product over the catalogs list; "Show all" opens the catalogs' own search across all catalogs with the query.
import type { App } from "vue";
import type { ListEntryBase } from "../api_client/virtocommerce.catalog";

/** The part of the shell's global search registry this module uses */
interface GlobalSearch {
  registerProvider(provider: object): void;
  hasPermission(permission: string | string[]): boolean;
  translate(keyOrText: string): string;
  rankItems<T>(items: T[], query: string, fields: (item: T) => unknown[], take?: number): T[];
  openBlade(options: {
    route: string;
    workspace: string;
    blades?: { name: string; param?: string; options?: Record<string, unknown> }[];
  }): Promise<void>;
}

// a product entry of the catalog search (the one the catalog's own lists use)
type ProductEntry = ListEntryBase & { productType?: string };

/** Registers the Products search provider; nothing in a host without global search */
export function registerProductSearch(app: App) {
  const search = app.config.globalProperties.$globalSearch as GlobalSearch | undefined;
  if (!search) return;

  const open = (blades: { name: string; param?: string; options?: Record<string, unknown> }[]) =>
    search.openBlade({ route: "/catalog", workspace: "CatalogList", blades });

  search.registerProvider({
    id: "catalog.products",
    title: "CATALOG.SEARCH.PRODUCTS",
    icon: "lucide-package",
    order: 120,
    permission: "catalog:read",
    isAvailable: () => search.hasPermission("catalog:access"),
    minLength: 2,
    delay: 250,
    aliases: ["product", "products", "sku"],
    search: async (query: string, context: { take: number; signal: AbortSignal }) => {
      const response = await fetch("/api/catalog/listentries", {
        method: "POST",
        signal: context.signal,
        headers: { Accept: "application/json", "Content-Type": "application/json" },
        body: JSON.stringify({
          keyword: query,
          take: context.take,
          objectTypes: ["CatalogProduct"],
          searchInChildren: true,
          // with variations the server pages before it folds them in, and returns fewer rows than asked for
          searchInVariations: false,
          responseGroup: "withProducts",
        }),
      });
      if (!response.ok) throw new Error(`Products search failed: ${response.status}`);
      const result: { totalCount?: number; listEntries?: ProductEntry[] } = await response.json();
      // ranked by the query first, then the rest in the server's order
      const products = result.listEntries ?? [];
      const ranked = search.rankItems(products, query, (p) => [p.name, p.code], products.length);
      return {
        total: result.totalCount,
        // the catalogs list's own "search in all catalogs"
        showAll: () => open([{ name: "CatalogBrowse", options: { global: true, keyword: query } }]),
        items: [...ranked, ...products.filter((p) => !ranked.includes(p))].map((product) => ({
          id: product.id,
          title: product.name ?? product.code ?? product.id,
          // the catalog (the first step of its path): a product copied into another catalog keeps its name and SKU
          description: [product.code, product.path?.[0], product.productType].filter(Boolean).join(" · "),
          icon: product.imageUrl ? undefined : "lucide-package",
          iconUrl: product.imageUrl,
          badge: product.isActive === false ? search.translate("CATALOG.SEARCH.INACTIVE") : undefined,
          execute: () => open([{ name: "CatalogProductDetails", param: product.id }]),
        })),
      };
    },
  });
}
