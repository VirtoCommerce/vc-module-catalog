using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace VirtoCommerce.CatalogModule.Data.ExportImport;

/// <summary>
/// Writes the catalog payload and its binary sidecars to a nested package. The Platform export
/// contract exposes one opaque module stream named with a .json suffix, so sidecars cannot be
/// added to the outer archive without changing the Platform contract.
/// </summary>
internal sealed class CatalogExportPackage : IDisposable
{
    private readonly ZipArchive _archive;
    private readonly FileStream _catalogStream;
    private readonly HashSet<string> _binaryDataEntries = new(StringComparer.Ordinal);
    private readonly HashSet<string> _failedBinaryDataEntries = new(StringComparer.Ordinal);
    private bool _isCompleted;

    private CatalogExportPackage(Stream outputStream, bool includeBinaryData)
    {
        if (includeBinaryData)
        {
            var catalogStream = TemporaryFileStream.Create();
            try
            {
                _archive = new ZipArchive(outputStream, ZipArchiveMode.Create, leaveOpen: true);
            }
            catch
            {
                catalogStream.Dispose();
                throw;
            }

            _catalogStream = catalogStream;
            CatalogStream = _catalogStream;
        }
        else
        {
            CatalogStream = outputStream;
        }
    }

    public Stream CatalogStream { get; }

    public static CatalogExportPackage Create(Stream outputStream, bool includeBinaryData)
    {
        ArgumentNullException.ThrowIfNull(outputStream);

        return new CatalogExportPackage(outputStream, includeBinaryData);
    }

    public async Task<string> WriteBinaryDataAsync(string sourceUrl, Func<Task<Stream>> openSourceStream, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceUrl);
        ArgumentNullException.ThrowIfNull(openSourceStream);
        cancellationToken.ThrowIfCancellationRequested();

        if (_archive == null)
        {
            throw new InvalidOperationException("The catalog export package is not configured to include binary data.");
        }

        var entryName = CatalogPackageFormat.CreateBinaryDataReference(sourceUrl);
        if (_failedBinaryDataEntries.Contains(entryName))
        {
            throw new InvalidDataException($"Binary data entry '{entryName}' could not be written earlier in this export.");
        }

        if (!_binaryDataEntries.Add(entryName))
        {
            return entryName;
        }

        try
        {
            await using var sourceStream = await openSourceStream();
            var entry = _archive.CreateEntry(entryName, CompressionLevel.NoCompression);

            await using var entryStream = entry.Open();
            await sourceStream.CopyToAsync(entryStream, cancellationToken);
        }
        catch
        {
            _failedBinaryDataEntries.Add(entryName);
            throw;
        }

        return entryName;
    }

    public async Task CompleteAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_isCompleted)
        {
            throw new InvalidOperationException("The catalog export package has already been completed.");
        }

        if (_archive != null)
        {
            await _catalogStream.FlushAsync(cancellationToken);
            _catalogStream.Position = 0;

            var entry = _archive.CreateEntry(CatalogPackageFormat.CatalogEntryName, CompressionLevel.NoCompression);
            await using (var entryStream = entry.Open())
            {
                await _catalogStream.CopyToAsync(entryStream, cancellationToken);
            }

            var manifestEntry = _archive.CreateEntry(CatalogPackageFormat.ManifestEntryName, CompressionLevel.NoCompression);
            await using var manifestStream = manifestEntry.Open();
            await manifestStream.WriteAsync(CatalogPackageFormat.CreateManifest(), cancellationToken);
        }

        _isCompleted = true;
    }

    public void Dispose()
    {
        try
        {
            _archive?.Dispose();
        }
        finally
        {
            _catalogStream?.Dispose();
        }
    }
}
