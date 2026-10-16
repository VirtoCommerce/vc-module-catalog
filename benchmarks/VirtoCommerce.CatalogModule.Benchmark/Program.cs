using System.Reflection;
using VirtoCommerce.CatalogModule.Benchmark;

// The entry-point plumbing — the --baseline-src before/after pairing and the --job it consumes for both
// halves — lives in BenchmarkProgram.
BenchmarkProgram.Run(Assembly.GetExecutingAssembly(), args);
