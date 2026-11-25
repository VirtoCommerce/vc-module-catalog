using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Settings;
using CatalogSettings = VirtoCommerce.CatalogModule.Core.ModuleConstants.Settings;
using SearchSettings = VirtoCommerce.SearchModule.Core.ModuleConstants.Settings;

namespace VirtoCommerce.CatalogModule.Data.MCP.ProductSetup;

// How a saved product reaches the storefront on this installation, which reads products from the search index: indexing
// on change, the scheduled indexing job, or neither. Every answer that creates or changes products says it, and so does
// the fix of a product that is missing from the index.
public class ProductIndexingTexts(ISettingsManager settingsManager)
{
    // The back office's Search index screen, where an index is built.
    public const string SearchIndexRoute = "/#!/workspace/searchIndex";

    public const string SearchIndexScreen = "Search index";

    // The default schedule of the indexing job.
    public const string DefaultSchedule = "0/5 * * * *";

    // What happens to a saved product: "The storefront shows it after the search index updates, usually within a minute."
    // searchIndexLink: the Search index screen's link, or null.
    public virtual async Task<string> GetIndexingSentenceAsync(string searchIndexLink = null)
    {
        if (await settingsManager.GetValueAsync<bool>(CatalogSettings.General.EventBasedIndexation))
        {
            return "The storefront shows it after the search index updates, usually within a minute.";
        }

        if (await settingsManager.GetValueAsync<bool>(SearchSettings.IndexingJobs.Enable))
        {
            var schedule = await settingsManager.GetValueAsync<string>(SearchSettings.IndexingJobs.CronExpression);

            return $"Indexing on change is switched off: the scheduled indexing job adds the change ({GetScheduleText(schedule)}).";
        }

        return $"Indexing is switched off: build the index on the {GetScreenText(searchIndexLink)} screen.";
    }

    // "every 5 minutes" for the default schedule, else the job's cron expression.
    protected virtual string GetScheduleText(string schedule)
    {
        return string.IsNullOrWhiteSpace(schedule) || schedule.Trim() == DefaultSchedule
            ? "schedule: every 5 minutes"
            : $"schedule: {schedule.Trim()}";
    }

    protected virtual string GetScreenText(string searchIndexLink)
    {
        return string.IsNullOrEmpty(searchIndexLink) ? SearchIndexScreen : $"{SearchIndexScreen} ({searchIndexLink})";
    }
}
