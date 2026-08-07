using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.ExportImport;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogModule.Tests.ExportImport;

internal sealed class CatalogExportImportTestFixture
{
    public CatalogExportImportTestFixture()
    {
        SetupEmptySearchResults();
        SetupSettings();

        ItemService
            .Setup(x => x.SaveChangesAsync(It.IsAny<IList<CatalogProduct>>()))
            .Returns(Task.CompletedTask);

        CategoryService
            .Setup(x => x.SaveChangesAsync(It.IsAny<IList<Category>>()))
            .Returns(Task.CompletedTask);

        BlobStorageProvider
            .Setup(x => x.OpenReadAsync(It.IsAny<string>()))
            .Returns((string url) =>
            {
                Increment(BlobReadOpenCounts, url);
                if (!BlobReadFactories.TryGetValue(url, out var factory))
                {
                    return Task.FromException<Stream>(new FileNotFoundException($"No test blob is registered for '{url}'.", url));
                }

                return Task.FromResult(factory());
            });

        BlobStorageProvider
            .Setup(x => x.OpenWriteAsync(It.IsAny<string>()))
            .Returns((string url) =>
            {
                Increment(BlobWriteOpenCounts, url);
                if (BlobWriteFactories.TryGetValue(url, out var factory))
                {
                    return Task.FromResult(factory());
                }

                return Task.FromResult<Stream>(new CallbackMemoryStream(bytes => WrittenBlobs[url] = bytes));
            });
    }

    public Mock<ICatalogService> CatalogService { get; } = new();
    public Mock<ICatalogSearchService> CatalogSearchService { get; } = new();
    public Mock<IProductSearchService> ProductSearchService { get; } = new();
    public Mock<ICategorySearchService> CategorySearchService { get; } = new();
    public Mock<ICategoryService> CategoryService { get; } = new();
    public Mock<IItemService> ItemService { get; } = new();
    public Mock<IPropertyService> PropertyService { get; } = new();
    public Mock<IPropertySearchService> PropertySearchService { get; } = new();
    public Mock<IPropertyDictionaryItemSearchService> PropertyDictionarySearchService { get; } = new();
    public Mock<IPropertyDictionaryItemService> PropertyDictionaryService { get; } = new();
    public Mock<IBlobStorageProvider> BlobStorageProvider { get; } = new();
    public Mock<IAssociationService> AssociationService { get; } = new();
    public Mock<IProductConfigurationService> ConfigurationService { get; } = new();
    public Mock<IProductConfigurationSearchService> ConfigurationSearchService { get; } = new();
    public Mock<IMeasureService> MeasureService { get; } = new();
    public Mock<IMeasureSearchService> MeasureSearchService { get; } = new();
    public Mock<IPropertyGroupService> PropertyGroupService { get; } = new();
    public Mock<IPropertyGroupSearchService> PropertyGroupSearchService { get; } = new();
    public Mock<ISettingsManager> SettingsManager { get; } = new();

    public IDictionary<string, Func<Stream>> BlobReadFactories { get; } = new Dictionary<string, Func<Stream>>(StringComparer.Ordinal);
    public IDictionary<string, Func<Stream>> BlobWriteFactories { get; } = new Dictionary<string, Func<Stream>>(StringComparer.Ordinal);
    public IDictionary<string, byte[]> WrittenBlobs { get; } = new Dictionary<string, byte[]>(StringComparer.Ordinal);
    public IDictionary<string, int> BlobReadOpenCounts { get; } = new Dictionary<string, int>(StringComparer.Ordinal);
    public IDictionary<string, int> BlobWriteOpenCounts { get; } = new Dictionary<string, int>(StringComparer.Ordinal);
    public IList<ProgressSnapshot> Progress { get; } = new List<ProgressSnapshot>();

    public int BatchSize { get; set; } = 50;
    public OnImportError ErrorPolicy { get; set; } = OnImportError.SkipItem;

    public CatalogExportImport CreateSut()
    {
        var serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        });

        return new CatalogExportImport(
            CatalogService.Object,
            CatalogSearchService.Object,
            ProductSearchService.Object,
            CategorySearchService.Object,
            CategoryService.Object,
            ItemService.Object,
            PropertyService.Object,
            PropertySearchService.Object,
            PropertyDictionarySearchService.Object,
            PropertyDictionaryService.Object,
            serializer,
            BlobStorageProvider.Object,
            AssociationService.Object,
            ConfigurationService.Object,
            ConfigurationSearchService.Object,
            MeasureService.Object,
            MeasureSearchService.Object,
            PropertyGroupService.Object,
            PropertyGroupSearchService.Object,
            SettingsManager.Object);
    }

    public void CaptureProgress(VirtoCommerce.Platform.Core.ExportImport.ExportImportProgressInfo progress)
    {
        Progress.Add(new ProgressSnapshot(
            progress.Description,
            progress.Errors?.ToArray() ?? Array.Empty<string>()));
    }

    public void AddBlob(string url, byte[] bytes)
    {
        BlobReadFactories[url] = () => new MemoryStream(bytes, writable: false);
    }

    public void SetProductExportResults(params CatalogProduct[] products)
    {
        ProductSearchService
            .Setup(x => x.SearchAsync(It.IsAny<ProductSearchCriteria>(), It.IsAny<bool>()))
            .Returns((ProductSearchCriteria criteria, bool _) => Task.FromResult(new ProductSearchResult
            {
                TotalCount = products.Length,
                Results = products.Skip(criteria.Skip).Take(criteria.Take).ToList(),
            }));
    }

    public void SetCategoryExportResults(params Category[] categories)
    {
        CategorySearchService
            .Setup(x => x.SearchAsync(It.IsAny<CategorySearchCriteria>(), It.IsAny<bool>()))
            .Returns((CategorySearchCriteria criteria, bool _) => Task.FromResult(new CategorySearchResult
            {
                TotalCount = categories.Length,
                Results = categories.Skip(criteria.Skip).Take(criteria.Take).ToList(),
            }));
    }

    private void SetupEmptySearchResults()
    {
        CatalogSearchService
            .Setup(x => x.SearchAsync(It.IsAny<CatalogSearchCriteria>(), It.IsAny<bool>()))
            .ReturnsAsync(new CatalogSearchResult());

        ProductSearchService
            .Setup(x => x.SearchAsync(It.IsAny<ProductSearchCriteria>(), It.IsAny<bool>()))
            .ReturnsAsync(new ProductSearchResult());

        CategorySearchService
            .Setup(x => x.SearchAsync(It.IsAny<CategorySearchCriteria>(), It.IsAny<bool>()))
            .ReturnsAsync(new CategorySearchResult());

        PropertySearchService
            .Setup(x => x.SearchPropertiesAsync(It.IsAny<PropertySearchCriteria>()))
            .ReturnsAsync(new PropertySearchResult());

        PropertyDictionarySearchService
            .Setup(x => x.SearchAsync(It.IsAny<PropertyDictionaryItemSearchCriteria>(), It.IsAny<bool>()))
            .ReturnsAsync(new PropertyDictionaryItemSearchResult());

        ConfigurationSearchService
            .Setup(x => x.SearchAsync(It.IsAny<ProductConfigurationSearchCriteria>()))
            .ReturnsAsync(new ProductConfigurationSearchResult());

        MeasureSearchService
            .Setup(x => x.SearchAsync(It.IsAny<MeasureSearchCriteria>()))
            .ReturnsAsync(new MeasureSearchResult());

        PropertyGroupSearchService
            .Setup(x => x.SearchAsync(It.IsAny<PropertyGroupSearchCriteria>()))
            .ReturnsAsync(new PropertyGroupSearchResult());
    }

    private void SetupSettings()
    {
        SettingsManager
            .Setup(x => x.GetObjectSettingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string name, string _, string _) => new ObjectSettingEntry
            {
                Name = name,
                Value = name switch
                {
                    "Catalog.BackupRestore.BatchSize" => BatchSize,
                    "Catalog.BackupRestore.ErrorPolicy" => ErrorPolicy.ToString(),
                    _ => null,
                },
            });
    }

    private static void Increment(IDictionary<string, int> counts, string key)
    {
        counts[key] = counts.TryGetValue(key, out var value) ? value + 1 : 1;
    }
}

internal sealed record ProgressSnapshot(string Description, IReadOnlyList<string> Errors);

internal sealed class CallbackMemoryStream : MemoryStream
{
    private readonly Action<byte[]> _onDispose;
    private bool _disposed;

    public CallbackMemoryStream(Action<byte[]> onDispose)
    {
        _onDispose = onDispose;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _disposed = true;
            _onDispose(ToArray());
        }

        base.Dispose(disposing);
    }
}

internal class NonSeekableReadStream : Stream
{
    private readonly byte[] _bytes;
    private readonly int _maxChunkSize;
    private readonly Action<long> _afterRead;
    private int _position;

    public NonSeekableReadStream(byte[] bytes, int maxChunkSize = 4096, Action<long> afterRead = null)
    {
        _bytes = bytes;
        _maxChunkSize = maxChunkSize;
        _afterRead = afterRead;
    }

    public int MaxRequestedReadSize { get; private set; }
    public long TotalBytesRead => _position;
    public bool IsDisposed { get; private set; }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return ReadCore(buffer.AsSpan(offset, count));
    }

    public override int Read(Span<byte> buffer)
    {
        return ReadCore(buffer);
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(ReadCore(buffer.AsSpan(offset, count)));
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(ReadCore(buffer.Span));
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        IsDisposed = true;
        base.Dispose(disposing);
    }

    private int ReadCore(Span<byte> buffer)
    {
        MaxRequestedReadSize = Math.Max(MaxRequestedReadSize, buffer.Length);
        var count = Math.Min(Math.Min(buffer.Length, _maxChunkSize), _bytes.Length - _position);
        if (count <= 0)
        {
            return 0;
        }

        _bytes.AsSpan(_position, count).CopyTo(buffer);
        _position += count;
        _afterRead?.Invoke(_position);
        return count;
    }
}

internal sealed class NonSeekableWriteStream : Stream
{
    private readonly MemoryStream _stream = new();

    public long BytesWritten => _stream.Length;
    public bool IsDisposed { get; private set; }
    public byte[] ToArray() => _stream.ToArray();

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void Flush() => _stream.Flush();
    public override Task FlushAsync(CancellationToken cancellationToken) => _stream.FlushAsync(cancellationToken);
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);
    public override void Write(ReadOnlySpan<byte> buffer) => _stream.Write(buffer);
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        _stream.WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) =>
        _stream.WriteAsync(buffer, cancellationToken);

    protected override void Dispose(bool disposing)
    {
        IsDisposed = true;
        base.Dispose(disposing);
    }
}

internal static class CatalogPackageTestHelper
{
    public const string ManifestJson = "{\"formatVersion\":1,\"catalogEntry\":\"catalog.json\",\"binaryDataDirectory\":\"assets/\"}";

    public static string CreateReference(string relativeUrl)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(relativeUrl));
        return $"assets/{Convert.ToHexString(hash).ToLowerInvariant()}.bin";
    }

    public static CatalogPackageContents Read(byte[] packageBytes)
    {
        using var stream = new MemoryStream(packageBytes, writable: false);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        var entries = new Dictionary<string, byte[]>(StringComparer.Ordinal);

        foreach (var entry in archive.Entries)
        {
            using var entryStream = entry.Open();
            using var output = new MemoryStream();
            entryStream.CopyTo(output);
            entries.Add(entry.FullName, output.ToArray());
        }

        var catalog = JObject.Parse(Encoding.UTF8.GetString(entries["catalog.json"]));
        var manifest = JObject.Parse(Encoding.UTF8.GetString(entries["package.json"]));
        return new CatalogPackageContents(catalog, manifest, entries);
    }

    public static byte[] Build(JObject catalog, params (string Name, byte[] Bytes)[] binaryEntries)
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(archive, "package.json", Encoding.UTF8.GetBytes(ManifestJson));
            WriteEntry(archive, "catalog.json", Encoding.UTF8.GetBytes(catalog.ToString(Formatting.None)));

            foreach (var binaryEntry in binaryEntries)
            {
                WriteEntry(archive, binaryEntry.Name, binaryEntry.Bytes);
            }
        }

        return stream.ToArray();
    }

    public static byte[] BuildWithManifest(JObject catalog, string manifestJson, CompressionLevel compressionLevel = CompressionLevel.NoCompression)
    {
        return BuildEntries(
            ("package.json", Encoding.UTF8.GetBytes(manifestJson), compressionLevel),
            ("catalog.json", Encoding.UTF8.GetBytes(catalog.ToString(Formatting.None)), compressionLevel));
    }

    public static byte[] BuildEntries(params (string Name, byte[] Bytes, CompressionLevel Compression)[] entries)
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var entry in entries)
            {
                WriteEntry(archive, entry.Name, entry.Bytes, entry.Compression);
            }
        }

        return stream.ToArray();
    }

    private static void WriteEntry(ZipArchive archive, string name, byte[] bytes, CompressionLevel compressionLevel = CompressionLevel.NoCompression)
    {
        var entry = archive.CreateEntry(name, compressionLevel);
        using var entryStream = entry.Open();
        entryStream.Write(bytes);
    }
}

internal sealed record CatalogPackageContents(
    JObject Catalog,
    JObject Manifest,
    IReadOnlyDictionary<string, byte[]> Entries);
