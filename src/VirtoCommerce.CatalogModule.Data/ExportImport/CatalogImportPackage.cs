using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.Platform.Core.ExportImport;

namespace VirtoCommerce.CatalogModule.Data.ExportImport;

internal sealed class CatalogImportPackage : IDisposable
{
    private const int ManifestPropertyCount = 3;

    private static readonly byte[] ZipLocalFileHeader = [0x50, 0x4B, 0x03, 0x04];

    private readonly Stream _inputStream;
    private readonly FileStream _temporaryStream;
    private readonly ZipArchive _archive;
    private readonly IImportBinaryDataReader _binaryDataReader;
    private readonly HashSet<(string Reference, string DestinationUrl)> _importedBinaryData = [];

    private CatalogImportPackage(Stream inputStream, Stream catalogStream, IImportBinaryDataReader binaryDataReader, FileStream temporaryStream = null, ZipArchive archive = null)
    {
        _inputStream = inputStream;
        _temporaryStream = temporaryStream;
        _archive = archive;
        _binaryDataReader = binaryDataReader;
        CatalogStream = catalogStream;
    }

    public Stream CatalogStream { get; }

    public static Task<CatalogImportPackage> OpenAsync(Stream inputStream, CancellationToken cancellationToken)
    {
        return OpenAsync(inputStream, null, cancellationToken);
    }

    public static Task<CatalogImportPackage> OpenAsync(Stream inputStream, IImportBinaryDataReader binaryDataReader, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(inputStream);
        cancellationToken.ThrowIfCancellationRequested();

        return OpenInternalAsync(inputStream, binaryDataReader, cancellationToken);
    }

    private static async Task<CatalogImportPackage> OpenInternalAsync(Stream inputStream, IImportBinaryDataReader binaryDataReader, CancellationToken cancellationToken)
    {
        FileStream temporaryStream = null;
        ZipArchive archive = null;

        try
        {
            ValidateSeekableInputLength(inputStream);

            var signature = new byte[CatalogPackageFormat.SignatureLength];
            var signatureLength = await ReadSignatureAsync(inputStream, signature, cancellationToken);

            if (!IsZipArchive(signature, signatureLength))
            {
                var catalogStream = new PrefixReadStream(signature, signatureLength, inputStream, leaveOpen: true);
                return new CatalogImportPackage(inputStream, catalogStream, binaryDataReader);
            }

            temporaryStream = TemporaryFileStream.Create();
            await CopyPackageAsync(inputStream, temporaryStream, signature, signatureLength, cancellationToken);
            await temporaryStream.FlushAsync(cancellationToken);
            temporaryStream.Position = 0;

            archive = new ZipArchive(temporaryStream, ZipArchiveMode.Read, leaveOpen: true);
            ValidateArchive(archive);
            await ValidateManifestAsync(archive, cancellationToken);

            var catalogEntry = archive.GetEntry(CatalogPackageFormat.CatalogEntryName)
                ?? throw new InvalidDataException($"The catalog export package does not contain '{CatalogPackageFormat.CatalogEntryName}'.");

            return new CatalogImportPackage(inputStream, catalogEntry.Open(), binaryDataReader, temporaryStream, archive);
        }
        catch
        {
            try
            {
                archive?.Dispose();
            }
            finally
            {
                try
                {
                    temporaryStream?.Dispose();
                }
                finally
                {
                    inputStream.Dispose();
                }
            }

            throw;
        }
    }

    public async Task<Stream> OpenBinaryDataAsync(string reference, CancellationToken cancellationToken)
    {
        if (!CatalogPackageFormat.IsValidBinaryDataReference(reference))
        {
            throw new InvalidDataException($"The binary data reference '{reference}' is invalid.");
        }

        if (_archive != null)
        {
            var entry = _archive.GetEntry(reference)
                ?? throw new InvalidDataException($"The catalog export package does not contain binary data entry '{reference}'.");

            return entry.Open();
        }

        if (_binaryDataReader != null)
        {
            return await _binaryDataReader.OpenReadAsync(reference, cancellationToken)
                ?? throw new InvalidDataException($"The catalog export package does not contain binary data entry '{reference}'.");
        }

        throw new InvalidDataException("The catalog export is JSON and does not contain side-car binary data.");
    }

    public bool IsBinaryDataImported(string reference, string destinationUrl)
    {
        return _importedBinaryData.Contains((reference, destinationUrl));
    }

    public bool IsBinaryDataReferenceForDestination(string reference, string destinationUrl)
    {
        var expectedReference = CatalogPackageFormat.CreateBinaryDataReference(destinationUrl);

        // Packages produced before source URLs became visible used assets/<SHA256>.bin. Accept that
        // URL-hash shape only inside a validated nested package; external sidecars must keep
        // the strict source URL mapping so one top-level asset cannot be substituted for another.
        return string.Equals(reference, expectedReference, StringComparison.Ordinal)
            || _archive != null && CatalogPackageFormat.IsLegacyBinaryDataReference(reference, destinationUrl);
    }

    public void MarkBinaryDataImported(string reference, string destinationUrl)
    {
        _importedBinaryData.Add((reference, destinationUrl));
    }

    public void Dispose()
    {
        try
        {
            CatalogStream.Dispose();
        }
        finally
        {
            try
            {
                _archive?.Dispose();
            }
            finally
            {
                try
                {
                    _temporaryStream?.Dispose();
                }
                finally
                {
                    _inputStream.Dispose();
                }
            }
        }
    }

    private static void ValidateSeekableInputLength(Stream inputStream)
    {
        if (inputStream.CanSeek && inputStream.Length - inputStream.Position > CatalogPackageFormat.MaximumPackageLength)
        {
            throw new InvalidDataException("The catalog export package exceeds the maximum supported length.");
        }
    }

    private static async Task CopyPackageAsync(Stream source, FileStream destination, byte[] signature, int signatureLength, CancellationToken cancellationToken)
    {
        await destination.WriteAsync(signature.AsMemory(0, signatureLength), cancellationToken);
        var totalLength = (long)signatureLength;
        var buffer = new byte[CatalogPackageFormat.CopyBufferSize];

        while (true)
        {
            var count = await source.ReadAsync(buffer, cancellationToken);
            if (count == 0)
            {
                break;
            }

            if (count > CatalogPackageFormat.MaximumPackageLength - totalLength)
            {
                throw new InvalidDataException("The catalog export package exceeds the maximum supported length.");
            }

            await destination.WriteAsync(buffer.AsMemory(0, count), cancellationToken);
            totalLength += count;
        }
    }

    private static async Task<int> ReadSignatureAsync(Stream stream, byte[] signature, CancellationToken cancellationToken)
    {
        var read = 0;

        while (read < signature.Length)
        {
            var count = await stream.ReadAsync(signature.AsMemory(read), cancellationToken);
            if (count == 0)
            {
                break;
            }

            read += count;
        }

        return read;
    }

    private static bool IsZipArchive(byte[] signature, int length)
    {
        if (length != ZipLocalFileHeader.Length)
        {
            return false;
        }

        return signature.AsSpan(0, length).SequenceEqual(ZipLocalFileHeader);
    }

    private static void ValidateArchive(ZipArchive archive)
    {
        if (archive.Entries.Count > CatalogPackageFormat.MaximumEntryCount)
        {
            throw new InvalidDataException("The catalog export package contains too many entries.");
        }

        var entryNames = new HashSet<string>(StringComparer.Ordinal);
        var totalLength = 0L;

        foreach (var entry in archive.Entries)
        {
            if (!entryNames.Add(entry.FullName))
            {
                throw new InvalidDataException($"The catalog export package contains duplicate entry '{entry.FullName}'.");
            }

            if (!CatalogPackageFormat.IsAllowedEntryName(entry.FullName))
            {
                throw new InvalidDataException($"The catalog export package contains unsupported entry '{entry.FullName}'.");
            }

            if (entry.CompressedLength != entry.Length)
            {
                throw new InvalidDataException($"The catalog export package entry '{entry.FullName}' uses unsupported compression.");
            }

            if (entry.Length > CatalogPackageFormat.MaximumPackageLength - totalLength)
            {
                throw new InvalidDataException("The uncompressed catalog export package exceeds the maximum supported length.");
            }

            totalLength += entry.Length;
        }
    }

    private static async Task ValidateManifestAsync(ZipArchive archive, CancellationToken cancellationToken)
    {
        var entry = archive.GetEntry(CatalogPackageFormat.ManifestEntryName)
            ?? throw new InvalidDataException($"The catalog export package does not contain '{CatalogPackageFormat.ManifestEntryName}'.");

        var manifest = await ReadManifestAsync(entry, cancellationToken);
        ValidateManifest(manifest);
    }

    private static async Task<JObject> ReadManifestAsync(ZipArchiveEntry entry, CancellationToken cancellationToken)
    {
        if (entry.Length > CatalogPackageFormat.MaximumManifestLength)
        {
            throw new InvalidDataException("The catalog export package manifest is too large.");
        }

        using var stream = entry.Open();
        using var streamReader = new StreamReader(stream);
        using var jsonReader = new JsonTextReader(streamReader);
        try
        {
            var manifest = await JObject.LoadAsync(jsonReader, new JsonLoadSettings
            {
                DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error,
            }, cancellationToken);

            if (await jsonReader.ReadAsync(cancellationToken))
            {
                throw new InvalidDataException("The catalog export package manifest contains trailing content.");
            }

            return manifest;
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("The catalog export package manifest is not valid JSON.", ex);
        }
    }

    private static void ValidateManifest(JObject manifest)
    {
        if (manifest.Count != ManifestPropertyCount)
        {
            ThrowInvalidManifest();
        }

        ValidateManifestValue(manifest, "formatVersion", JTokenType.Integer, CatalogPackageFormat.Version);
        ValidateManifestValue(manifest, "catalogEntry", JTokenType.String, CatalogPackageFormat.CatalogEntryName);
        ValidateManifestValue(manifest, "binaryDataDirectory", JTokenType.String, CatalogPackageFormat.BinaryDataDirectory);
    }

    private static void ValidateManifestValue<T>(JObject manifest, string propertyName, JTokenType tokenType, T expectedValue)
    {
        var property = manifest[propertyName];
        if (property?.Type != tokenType || !EqualityComparer<T>.Default.Equals(property.Value<T>(), expectedValue))
        {
            ThrowInvalidManifest();
        }
    }

    private static void ThrowInvalidManifest()
    {
        throw new InvalidDataException("The catalog export package manifest is invalid or unsupported.");
    }
}
