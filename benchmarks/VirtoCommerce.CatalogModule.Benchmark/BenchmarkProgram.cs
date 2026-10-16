using System;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace VirtoCommerce.CatalogModule.Benchmark;

/// <summary>
/// Runs the benchmarks in an assembly, adding an opt-in before/after mode: <c>--baseline-src &lt;path&gt;</c>
/// pairs a <c>before</c> job, which rebuilds the catalog projects from the baseline checkout via
/// <c>/p:BaselineSrc=&lt;path&gt;</c>, with an <c>after</c> job on the current source, so one run prints
/// <c>Ratio</c> and <c>Alloc Ratio</c> columns.
/// </summary>
/// <remarks>
/// Ported from <c>BenchmarkProgram</c> in the <c>VirtoCommerce.XCart.Benchmark.Core</c> package, which the other
/// module benchmarks reference. Catalog cannot reference that package: it depends on the catalog packages, while
/// this benchmark builds catalog from source. Keep the two in step.
/// </remarks>
public static class BenchmarkProgram
{
    public static void Run(Assembly benchmarkAssembly, string[] args)
    {
        var (baselineSrc, rest) = ExtractOption(args, "--baseline-src");

        var config = ManualConfig.Create(DefaultConfig.Instance).AddColumn(CategoriesColumn.Default);
        if (baselineSrc is not null)
        {
            // A double quote would end the quoted value early, and a semicolon would let the rest of the path
            // be read as another MSBuild property.
            if (baselineSrc.AsSpan().ContainsAny('"', ';'))
            {
                throw new ArgumentException("--baseline-src cannot contain a double quote or a semicolon; " +
                    $"either would truncate the value or inject an MSBuild property. Got '{baselineSrc}'.");
            }

            // The job is consumed here and applied to both halves; passing it on to BenchmarkDotNet as well
            // would add a third, unpaired job.
            var (jobName, restAfterJob) = ExtractOption(rest, "--job");
            rest = restAfterJob;

            var leftoverJob = Array.Find(rest, x => x is not null
                && (x == "--job" || x.StartsWith("--job=", StringComparison.Ordinal) || x.StartsWith("-j", StringComparison.Ordinal)));
            if (leftoverJob is not null)
            {
                throw new ArgumentException(
                    "--baseline-src takes the job as `--job <Dry|Short|Default>` or `--job=<...>`; " +
                    $"'{leftoverJob}' would reach BenchmarkDotNet and add a third, unpaired job.");
            }

            var normalized = (jobName ?? "dry").ToLowerInvariant();
            var baselineJob = normalized switch
            {
                "dry" => Job.Dry,
                "short" => Job.ShortRun,
                "default" or "measured" => Job.Default,
                _ => throw new ArgumentException($"--job must be Dry|Short|Default with --baseline-src; got '{jobName}'."),
            };
            if (normalized is not ("default" or "measured"))
            {
                Console.Error.WriteLine($"// --baseline-src on --job {normalized}: Alloc Ratio is exact; the time " +
                    "Ratio is directional only (not a verdict) — re-run with `--job Default` for a trustworthy Mean.");
            }
            config = config
                .AddJob(baselineJob.WithMsBuildArguments($"/p:BaselineSrc={QuoteForCommandLine(baselineSrc)}").WithId("before").AsBaseline())
                .AddJob(baselineJob.WithId("after"));
        }

        BenchmarkSwitcher.FromAssembly(benchmarkAssembly).Run(rest, config);
    }

    private static (string, string[]) ExtractOption(string[] args, string name)
    {
        var prefix = name + "=";
        var index = Array.FindIndex(args, x => x is not null && (x == name || x.StartsWith(prefix, StringComparison.Ordinal)));
        if (index < 0)
        {
            return (null, args);
        }

        if (args[index].StartsWith(prefix, StringComparison.Ordinal))
        {
            var inlineValue = args[index][prefix.Length..];
            if (inlineValue.Length == 0)
            {
                throw new ArgumentException($"{name} requires a value.");
            }

            return (inlineValue, args.Where((_, i) => i != index).ToArray());
        }

        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"{name} requires a value.");
        }

        var value = args[index + 1];
        var rest = args.Where((_, i) => i != index && i != index + 1).ToArray();

        return (value, rest);
    }

    // The command-line parser reads a backslash run before a closing quote as escapes, so a path ending in
    // a backslash would swallow the quote; doubling the trailing run keeps it literal.
    private static string QuoteForCommandLine(string value)
    {
        var trailingBackslashes = value.Length - value.TrimEnd('\\').Length;

        return $"\"{value}{new string('\\', trailingBackslashes)}\"";
    }
}
