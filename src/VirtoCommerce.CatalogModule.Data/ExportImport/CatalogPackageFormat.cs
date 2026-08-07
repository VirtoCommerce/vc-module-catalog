using System;
using System.IO;
using System.Security.Cryptography;
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
    public const int Sha256HexLength = 64;
    public const int CopyBufferSize = 81920;

    // These are abuse-prevention ceilings, not operational catalog-size limits.
    public const int MaximumEntryCount = 1_000_000;
    public const long MaximumPackageLength = 1L << 40;

    public static string CreateBinaryDataReference(string sourceUrl)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceUrl);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(sourceUrl));
        return $"{BinaryDataDirectory}{Convert.ToHexString(hash).ToLowerInvariant()}.bin";
    }

    public static bool IsValidBinaryDataReference(string reference)
    {
        if (string.IsNullOrEmpty(reference)
            || !reference.StartsWith(BinaryDataDirectory, StringComparison.Ordinal)
            || reference.Contains('\\'))
        {
            return false;
        }

        var segments = reference.Split('/');
        return segments.Length == 2
            && segments[1].Length == Sha256HexLength + ".bin".Length
            && segments[1].EndsWith(".bin", StringComparison.Ordinal)
            && IsLowerHex(segments[1].AsSpan(0, Sha256HexLength));
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

    private static bool IsLowerHex(ReadOnlySpan<char> value)
    {
        foreach (var character in value)
        {
            var isDigit = character is >= '0' and <= '9';
            var isLowerHexLetter = character is >= 'a' and <= 'f';
            if (!isDigit && !isLowerHexLetter)
            {
                return false;
            }
        }

        return true;
    }
}
