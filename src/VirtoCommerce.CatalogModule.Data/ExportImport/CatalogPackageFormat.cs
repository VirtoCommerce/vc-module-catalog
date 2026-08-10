using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VirtoCommerce.CatalogModule.Data.ExportImport;

internal static class CatalogPackageFormat
{
    public const int Version = 1;
    public const string CatalogEntryName = "catalog.json";
    public const string ManifestEntryName = "package.json";
    public const string BinaryDataDirectory = "assets/";
    public const int SignatureLength = 4;
    public const int MaximumManifestLength = 4096;
    public const int CopyBufferSize = 81920;

    // These are abuse-prevention ceilings, not operational catalog-size limits.
    public const int MaximumEntryCount = 1_000_000;
    public const long MaximumPackageLength = 1L << 40;

    public static string CreateBinaryDataReference(string sourceUrl)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceUrl);

        var reference = $"{BinaryDataDirectory}{sourceUrl}";
        if (!IsValidBinaryDataReference(reference))
        {
            throw new ArgumentException($"The source URL '{sourceUrl}' cannot be represented as a safe package path.", nameof(sourceUrl));
        }

        return reference;
    }

    public static bool IsValidBinaryDataReference(string reference)
    {
        if (string.IsNullOrEmpty(reference)
            || !reference.StartsWith(BinaryDataDirectory, StringComparison.Ordinal)
            || reference.Contains('\\'))
        {
            return false;
        }

        var relativePath = reference[BinaryDataDirectory.Length..];
        if (relativePath.Length == 0)
        {
            return false;
        }

        return relativePath.Split('/').All(IsValidPathSegment);
    }

    public static bool IsAllowedEntryName(string entryName)
    {
        return entryName == CatalogEntryName
            || entryName == ManifestEntryName
            || IsValidBinaryDataReference(entryName);
    }

    public static byte[] CreateManifest()
    {
        var manifest = new JObject
        {
            ["formatVersion"] = Version,
            ["catalogEntry"] = CatalogEntryName,
            ["binaryDataDirectory"] = BinaryDataDirectory,
        };

        return Encoding.UTF8.GetBytes(manifest.ToString(Formatting.None));
    }

    private static bool IsValidPathSegment(string segment)
    {
        if (segment.Length == 0 || segment is "." or "..")
        {
            return false;
        }

        return segment.All(character => !char.IsControl(character));
    }
}
