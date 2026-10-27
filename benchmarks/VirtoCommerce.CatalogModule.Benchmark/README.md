# VirtoCommerce.CatalogModule Benchmarks

Microbenchmarks for catalog code paths, with all I/O mocked at the leaves. A tool for **local
development and code analysis**: run it while changing catalog code to see the time and allocation
effect.

The metric to trust is **allocations** (`[MemoryDiagnoser]`) measured at **`--job Short` or above**.
Wall-clock `Mean` is a complementary signal, meaningful only within a single controlled run; it is the
primary signal only where the subject is a pause rather than an allocation, as with the forced
collection below.

## Subject

| Benchmark | Subject | Harness |
|---|---|---|
| `ProductIndexingCacheBenchmarks` | one product indexing batch → `ProductDocumentBuilder.GetDocumentsAsync` (`BatchSize` × `CachedProducts`) | real builder, real `MemoryCache` with `ItemCacheRegion` change tokens, mocked `IItemService` / `IProductSearchService` |
| `ProductCacheExpirationBenchmarks` | the expiration step alone: `ItemCacheRegion.ExpireTokenForKey` per batch product vs `ItemCacheRegion.ExpireRegion()` (`BatchSize` × `CachedProducts`) | real `MemoryCache` with `ItemCacheRegion` change tokens |

The measured compute is document creation for the batch plus the cache expiration the builder does at
the end of every batch. The cache holds `CachedProducts` products, two entries each, tied to the
product's key token the way `ItemService.ConfigureCache` ties them, so a region-wide expiration and a
per-product expiration both do their real work. The job runs on Server GC, like the platform host, so a
forced `GC.Collect()` costs what it costs there.

The builder removes products from the cache, so each iteration refills the cache in
`[IterationSetup]`, outside the measured region.

### What this does NOT measure

Database and search-provider time, Redis backplane traffic, the later scan that evicts expired entries
from `MemoryCache`, and the cache misses other requests pay after their products were evicted. A
region-wide expiration makes every other request reload its products; that cost lands outside the
indexing batch and needs a running platform to observe (cache metrics, SQL logging).

## Prerequisites

- .NET 10 SDK

## Running

```bash
cd benchmarks/VirtoCommerce.CatalogModule.Benchmark

# validate first — compiles + executes each case once, no measurement
dotnet run -c Release -- --filter "*" --job Dry

# run
dotnet run -c Release -- --filter "*" --noOverwrite > benchmark.log 2>&1
```

Results are written to `BenchmarkDotNet.Artifacts/`; read the `*-report-github.md` summary.

## Comparing before/after a change

1. **Two runs, git-switched.** Run into separate `--artifacts` dirs (current vs changed code) and diff
   the `Allocated` columns — run both sides at `--job Short`, where allocations reproduce across
   separate runs; they do not at `--job Dry`.
2. **Single-process side-by-side (`--baseline-src`).** Point the benchmark at a baseline checkout of the
   source for `Ratio` / `Alloc Ratio` columns in one run:
   ```bash
   git worktree add /tmp/catalog-before <baseline-ref>
   dotnet run -c Release -- --filter "*" --baseline-src /tmp/catalog-before/src --job Short
   git worktree remove /tmp/catalog-before
   ```
   The `before` job (the baseline) rebuilds `CatalogModule.Data` from that source via
   `/p:BaselineSrc=<path>` (a `ProjectReference` swap, so the full package graph still restores). Valid
   only when the change keeps the benchmarked public API stable. Without `--job`, `--baseline-src`
   defaults to `Dry`, which is only good for checking that both sides build and run. The time `Ratio` at
   `Dry` or `Short` is directional; pass `--job Default` for a trustworthy `Mean`. Spell the job as
   `--job <name>` or `--job=<name>`; short forms are rejected under `--baseline-src`.

   The baseline must build on its own: a baseline whose package graph fails NuGet audit (`NU1903` is an
   error under `TreatWarningsAsErrors`) never produces a `before` row.

### Reading allocations: use `--job Short`

`--job Dry` runs one cold invocation, so the first call's JIT and static initialisation land inside the
measured region. Read allocations at `--job Short` or above, and treat a Short-to-Short difference under
~1 % as noise. Reserve Dry for "does it still run".

The subject does not survive repeated invocation — each batch empties part of the cache — so a number is
only real if each invocation got a freshly filled cache. `[IterationSetup]` supplies one per iteration,
and at `Short` and above BenchmarkDotNet pins `InvocationCount=1` because of it. **The tell is `1 op` in
each `WorkloadActual` row.** Setting `InvocationCount` above 1 or `UnrollFactor` on its own breaks that
while still printing a plausible number.

### Recognising a run that measured nothing

BenchmarkDotNet exits **0** whether or not a single case produced a figure. Look for `NA` in the `Mean`
column, `There are not any results runs`, a `Benchmarks with issues:` block, and exceptions in
`BenchmarkDotNet.Artifacts/BenchmarkRun-*.log`. Always pass `--filter`: without it BenchmarkDotNet asks
interactively, and a non-interactive caller exits **0** having measured nothing.

### Before you trust a green result, show the arm can go red

A benchmark has no assertions, so "no regression" and "this run never touched your change" print the
same number. Before reading a `1.00` ratio as good news, perturb the code you changed — revert it, or add
an obvious allocation — and confirm the number moves.
