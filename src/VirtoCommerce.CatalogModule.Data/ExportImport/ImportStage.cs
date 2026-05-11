using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.ExportImport;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    /// <summary>
    /// Strategy applied when a stage's <c>SaveChangesAsync</c> throws.
    /// </summary>
    public enum OnImportError
    {
        /// <summary>
        /// Re-throw a decorated <see cref="ImportStageException"/> and let the platform's
        /// per-module catch abort the rest of this module's import.
        /// Identical effective behaviour to the pre-helper code, but with stage and ids
        /// attached for triage.
        /// </summary>
        Stop = 0,

        /// <summary>
        /// Log the whole batch as failed, then continue with the next batch and stage.
        /// </summary>
        SkipBatch = 1,

        /// <summary>
        /// On batch failure, retry items one-by-one to isolate the bad row(s),
        /// log each failing id, keep the survivors and continue.
        /// </summary>
        SkipItem = 2,
    }

    /// <summary>
    /// Per-stage context for <see cref="ImportStage.RunBatchAsync"/>. Build once per
    /// stage at the call site so error messages get a stable Module/Stage/EntityType prefix.
    /// </summary>
    public sealed class ImportStageContext
    {
        public required string ModuleId { get; init; }
        public required string Stage { get; init; }
        public required string EntityType { get; init; }
        public OnImportError ErrorPolicy { get; init; } = OnImportError.Stop;
        public required ExportImportProgressInfo ProgressInfo { get; init; }
        public required Action<ExportImportProgressInfo> ProgressCallback { get; init; }
    }

    /// <summary>
    /// Decorated exception thrown by <see cref="ImportStage.RunBatchAsync"/> under the
    /// <see cref="OnImportError.Stop"/> policy. Carries the structured stage / entity
    /// context so callers and log scrapers don't have to parse the message string.
    /// </summary>
    public sealed class ImportStageException : Exception
    {
        public string ModuleId { get; }
        public string Stage { get; }
        public string EntityType { get; }
        public IReadOnlyList<string> EntityIds { get; }

        public ImportStageException(ImportStageContext ctx, IReadOnlyList<string> entityIds, Exception inner)
            : base(BuildMessage(ctx, entityIds, inner), inner)
        {
            ModuleId = ctx.ModuleId;
            Stage = ctx.Stage;
            EntityType = ctx.EntityType;
            EntityIds = entityIds;
        }

        private static string BuildMessage(ImportStageContext ctx, IReadOnlyList<string> ids, Exception inner)
        {
            return $"[{ctx.ModuleId}/{ctx.Stage}] {FormatIds(ctx.EntityType, ids)}: {inner.Message}";
        }

        internal static string FormatIds(string entityType, IReadOnlyList<string> ids)
        {
            return ids.Count switch
            {
                0 => "<no ids>",
                1 => $"{entityType} '{ids[0]}'",
                _ => $"{ids.Count} {entityType}(s) [{string.Join(", ", ids.Take(5))}{(ids.Count > 5 ? ", …" : "")}]",
            };
        }
    }

    /// <summary>
    /// Wraps <c>SaveChangesAsync</c>-style batch saves with stage-aware error handling
    /// and structured progress reporting. Emitted error lines are tagged
    /// <c>[ModuleId/Stage] EntityType 'id': &lt;inner&gt;</c> so the platform's restore-blade
    /// JS fallback retro-attaches them to the failing module's timeline item by name match.
    /// </summary>
    public static class ImportStage
    {
        /// <summary>
        /// Runs <paramref name="saveBatchAsync"/> over <paramref name="batch"/> within the
        /// given stage context. On failure, applies the context's <see cref="ImportStageContext.ErrorPolicy"/>:
        /// <list type="bullet">
        ///   <item><description><see cref="OnImportError.Stop"/> — throws <see cref="ImportStageException"/>.</description></item>
        ///   <item><description><see cref="OnImportError.SkipBatch"/> — logs the batch as failed, returns 0.</description></item>
        ///   <item><description><see cref="OnImportError.SkipItem"/> — retries items one-at-a-time, logs each failure, returns the count of survivors.</description></item>
        /// </list>
        /// </summary>
        /// <returns>Number of items considered successfully saved (used to feed back into progress counters).</returns>
        public static async Task<int> RunBatchAsync<T>(
            ImportStageContext ctx,
            IList<T> batch,
            Func<IList<T>, Task> saveBatchAsync,
            Func<T, string> getId)
            where T : class
        {
            ArgumentNullException.ThrowIfNull(ctx);
            ArgumentNullException.ThrowIfNull(saveBatchAsync);
            ArgumentNullException.ThrowIfNull(getId);

            if (batch is null || batch.Count == 0)
            {
                return 0;
            }

            try
            {
                await saveBatchAsync(batch);
                return batch.Count;
            }
            catch (Exception ex) when (ctx.ErrorPolicy == OnImportError.SkipBatch)
            {
                var ids = batch.Select(getId).ToList();
                EmitError(ctx, ids, ex, outcome: $"BATCH SKIPPED ({ids.Count} item(s) not imported)");
                return 0;
            }
            catch (Exception ex) when (ctx.ErrorPolicy == OnImportError.SkipItem)
            {
                // EF rolled the whole batch back. Retry one-at-a-time to isolate failing rows.
                var saved = 0;
                foreach (var item in batch)
                {
                    try
                    {
                        await saveBatchAsync([item]);
                        saved++;
                    }
                    catch (Exception itemEx)
                    {
                        EmitError(ctx, [getId(item)], itemEx, outcome: "SKIPPED");
                    }
                }
                if (saved == 0)
                {
                    EmitError(ctx, batch.Select(getId).ToList(), ex,
                        outcome: $"BATCH SKIPPED ({batch.Count} item(s) not imported)",
                        note: "All items also failed individually.");
                }
                return saved;
            }
            catch (Exception ex) // OnImportError.Stop
            {
                throw new ImportStageException(ctx, batch.Select(getId).ToList(), ex);
            }
        }

        /// <summary>Convenience wrapper for a single-entity save.</summary>
        public static Task<int> RunOneAsync<T>(
            ImportStageContext ctx,
            T item,
            Func<T, Task> saveAsync,
            Func<T, string> getId)
            where T : class
        {
            ArgumentNullException.ThrowIfNull(saveAsync);
            return RunBatchAsync(ctx, [item], batch => saveAsync(batch[0]), getId);
        }

        private static void EmitError(
            ImportStageContext ctx,
            IReadOnlyList<string> ids,
            Exception ex,
            string outcome = null,
            string note = null)
        {
            // Format: "[Stage] EntityType 'id' — OUTCOME: error message (note)"
            //   - The Stage prefix lets the platform UI scope the error to the right module/stage row.
            //   - The OUTCOME marker (e.g. SKIPPED, BATCH SKIPPED) is the human-readable signal that the
            //     importer continued past this row. Without it, callers had to infer the skip from the
            //     error policy and the absence of a thrown exception.
            //   - `note` carries optional context (e.g. "All items also failed individually.").
            var subject = ImportStageException.FormatIds(ctx.EntityType, ids);
            var head = string.IsNullOrEmpty(outcome)
                ? $"[{ctx.Stage}] {subject}: {ex.Message}"
                : $"[{ctx.Stage}] {subject} — {outcome}: {ex.Message}";
            var message = note is null ? head : $"{head} ({note})";

            // Push to the legacy errors collection — this drives the platform's "Completed with errors"
            // banner and the JS-side fallback that retro-attaches each line to the failing module's
            // timeline item by matching the ModuleId substring in the message prefix.
            ctx.ProgressInfo.Errors ??= new List<string>();
            if (!ctx.ProgressInfo.Errors.Contains(message))
            {
                ctx.ProgressInfo.Errors.Add(message);
            }
            ctx.ProgressInfo.Description = message;

            ctx.ProgressCallback(ctx.ProgressInfo);
        }
    }
}
