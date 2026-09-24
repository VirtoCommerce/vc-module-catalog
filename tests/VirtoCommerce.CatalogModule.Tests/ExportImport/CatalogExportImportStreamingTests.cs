using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.ExportImport;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests.ExportImport;

public class CatalogExportImportStreamingTests
{
    private const int MaximumExpectedCopyBufferSize = 128 * 1024;

    [Fact]
    public async Task DoExportAsync_NonSeekableStreams_CopiesBinaryInBoundedChunks()
    {
        // Arrange
        const string url = "catalog/large-export.bin";
        var bytes = CreateBytes(2 * 1024 * 1024);
        var source = new NonSeekableReadStream(bytes, maxChunkSize: 4096);
        var output = new NonSeekableWriteStream();
        var fixture = new CatalogExportImportTestFixture();
        fixture.BlobReadFactories[url] = () => source;
        fixture.SetProductExportResults(CreateProduct("product", url));

        // Act
        await fixture.CreateSut().DoExportAsync(
            output,
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        var package = CatalogPackageTestHelper.Read(output.ToArray());
        var reference = CatalogPackageTestHelper.CreateReference(url);
        package.Entries[reference].Should().Equal(bytes);
        source.MaxRequestedReadSize.Should().BeLessThanOrEqualTo(MaximumExpectedCopyBufferSize);
        source.IsDisposed.Should().BeTrue();
        output.IsDisposed.Should().BeFalse("the caller owns the export stream");
    }

    [Fact]
    public async Task DoImportAsync_NonSeekablePackageStream_SpoolsWithBoundedReadsAndRestoresBinary()
    {
        // Arrange
        const string url = "catalog/large-import.bin";
        var bytes = CreateBytes(2 * 1024 * 1024);
        var reference = CatalogPackageTestHelper.CreateReference(url);
        var packageBytes = CatalogPackageTestHelper.Build(
            CreateCatalog(CreateProduct("product", url, reference)),
            (reference, bytes));
        var input = new NonSeekableReadStream(packageBytes, maxChunkSize: 4096);
        var fixture = new CatalogExportImportTestFixture();

        // Act
        await fixture.CreateSut().DoImportAsync(
            input,
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        fixture.WrittenBlobs[url].Should().Equal(bytes);
        input.MaxRequestedReadSize.Should().BeLessThanOrEqualTo(MaximumExpectedCopyBufferSize);
        input.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task DoExportAsync_CancelledDuringBinaryCopy_PropagatesCancellationAndDisposesSource()
    {
        // Arrange
        const string url = "catalog/cancel-export.bin";
        using var cancellation = new CancellationTokenSource();
        var bytes = CreateBytes(2 * 1024 * 1024);
        var source = new NonSeekableReadStream(
            bytes,
            maxChunkSize: 4096,
            afterRead: totalRead =>
            {
                if (totalRead >= 32 * 1024)
                {
                    cancellation.Cancel();
                }
            });
        var fixture = new CatalogExportImportTestFixture();
        fixture.BlobReadFactories[url] = () => source;
        fixture.SetProductExportResults(CreateProduct("product", url));

        // Act
        var act = () => fixture.CreateSut().DoExportAsync(
            new NonSeekableWriteStream(),
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            cancellation.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        source.IsDisposed.Should().BeTrue();
        fixture.Progress.SelectMany(x => x.Errors).Should().NotContain(x => x.Contains("cancel", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task DoImportAsync_CancelledWhileSpoolingNonSeekablePackage_PropagatesCancellation()
    {
        // Arrange
        const string url = "catalog/cancel-import.bin";
        using var cancellation = new CancellationTokenSource();
        var bytes = CreateBytes(2 * 1024 * 1024);
        var reference = CatalogPackageTestHelper.CreateReference(url);
        var packageBytes = CatalogPackageTestHelper.Build(
            CreateCatalog(CreateProduct("product", url, reference)),
            (reference, bytes));
        var input = new NonSeekableReadStream(
            packageBytes,
            maxChunkSize: 4096,
            afterRead: totalRead =>
            {
                if (totalRead >= 32 * 1024)
                {
                    cancellation.Cancel();
                }
            });
        var fixture = new CatalogExportImportTestFixture();

        // Act
        var act = () => fixture.CreateSut().DoImportAsync(
            input,
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            cancellation.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        input.IsDisposed.Should().BeTrue();
        fixture.BlobWriteOpenCounts.Should().BeEmpty();
    }

    [Fact]
    public async Task DoImportAsync_CancelledDuringSidecarUpload_PropagatesCancellationAndDisposesStreams()
    {
        // Arrange
        const string url = "catalog/cancel-sidecar.bin";
        using var cancellation = new CancellationTokenSource();
        var reference = CatalogPackageTestHelper.CreateReference(url);
        var packageBytes = CatalogPackageTestHelper.Build(
            CreateCatalog(CreateProduct("product", url, reference)),
            (reference, CreateBytes(2 * 1024 * 1024)));
        var input = new NonSeekableReadStream(packageBytes, maxChunkSize: 4096);
        var target = new CancellingWriteStream(cancellation);
        var fixture = new CatalogExportImportTestFixture();
        fixture.BlobWriteFactories[url] = () => target;

        // Act
        var act = () => fixture.CreateSut().DoImportAsync(
            input,
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            cancellation.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        target.IsDisposed.Should().BeTrue();
        input.IsDisposed.Should().BeTrue();
        fixture.Progress.SelectMany(x => x.Errors).Should().NotContain(x => x.Contains("cancel", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task DoImportAsync_SeekableInputAboveAbuseCeiling_RejectsBeforeReading()
    {
        // Arrange
        const long maximumPackageLength = 1L << 40;
        var input = new ReportedLengthReadStream(maximumPackageLength + 1);
        var fixture = new CatalogExportImportTestFixture();

        // Act
        var act = () => fixture.CreateSut().DoImportAsync(
            input,
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidDataException>().WithMessage("*maximum supported length*");
        input.ReadCount.Should().Be(0);
        input.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task DoExportAsync_LargeGeneratedBinary_WritesPackageWhileSourceIsStillBeingRead()
    {
        // Arrange
        const string url = "catalog/generated-large.bin";
        const long length = 8L * 1024 * 1024;
        var output = new NonSeekableWriteStream();
        var source = new InterleavingGeneratedReadStream(length, () => output.BytesWritten);
        var fixture = new CatalogExportImportTestFixture();
        fixture.BlobReadFactories[url] = () => source;
        fixture.SetProductExportResults(CreateProduct("product", url));

        // Act
        await fixture.CreateSut().DoExportAsync(
            output,
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        source.ObservedOutputGrowthWhileReading.Should().BeTrue(
            "a full-binary buffer would finish reading the source before writing the side-car entry");
        source.MaxRequestedReadSize.Should().BeLessThanOrEqualTo(MaximumExpectedCopyBufferSize);

        using var packageStream = new MemoryStream(output.ToArray(), writable: false);
        using var archive = new ZipArchive(packageStream, ZipArchiveMode.Read);
        archive.GetEntry(CatalogPackageTestHelper.CreateReference(url))!.Length.Should().Be(length);
    }

    private static CatalogProduct CreateProduct(string id, string url, string reference = null)
    {
        return new CatalogProduct
        {
            Id = id,
            CatalogId = "catalog",
            Code = id,
            Name = id,
            Images = new List<Image>
            {
                new()
                {
                    Id = $"{id}-image",
                    Name = $"{id}-image",
                    Url = url,
                    RelativeUrl = url,
                    BinaryDataReference = reference,
                },
            },
            Assets = new List<Asset>(),
        };
    }

    private static JObject CreateCatalog(params CatalogProduct[] products)
    {
        return JObject.FromObject(new { Products = products });
    }

    private static byte[] CreateBytes(int length)
    {
        var result = new byte[length];
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = unchecked((byte)((i * 31) ^ (i >> 7)));
        }

        return result;
    }

    private sealed class InterleavingGeneratedReadStream : Stream
    {
        private const int ChunkSize = 16 * 1024;
        private readonly long _length;
        private readonly Func<long> _getOutputLength;
        private long _position;
        private long _initialOutputLength = -1;

        public InterleavingGeneratedReadStream(long length, Func<long> getOutputLength)
        {
            _length = length;
            _getOutputLength = getOutputLength;
        }

        public bool ObservedOutputGrowthWhileReading { get; private set; }
        public int MaxRequestedReadSize { get; private set; }

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

        public override int Read(byte[] buffer, int offset, int count) => ReadCore(buffer.AsSpan(offset, count));
        public override int Read(Span<byte> buffer) => ReadCore(buffer);

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

        private int ReadCore(Span<byte> buffer)
        {
            MaxRequestedReadSize = Math.Max(MaxRequestedReadSize, buffer.Length);
            if (_initialOutputLength < 0)
            {
                _initialOutputLength = _getOutputLength();
            }
            else if (_position < _length && _getOutputLength() > _initialOutputLength)
            {
                ObservedOutputGrowthWhileReading = true;
            }

            var count = (int)Math.Min(Math.Min(buffer.Length, ChunkSize), _length - _position);
            if (count <= 0)
            {
                return 0;
            }

            for (var i = 0; i < count; i++)
            {
                var position = _position + i;
                buffer[i] = unchecked((byte)((position * 31) ^ (position >> 7)));
            }

            _position += count;
            return count;
        }
    }

    private sealed class CancellingWriteStream : Stream
    {
        private readonly CancellationTokenSource _cancellation;

        public CancellingWriteStream(CancellationTokenSource cancellation)
        {
            _cancellation = cancellation;
        }

        public bool IsDisposed { get; private set; }
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _cancellation.Cancel();
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            _cancellation.Cancel();
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.CompletedTask;
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class ReportedLengthReadStream : Stream
    {
        private readonly long _length;

        public ReportedLengthReadStream(long length)
        {
            _length = length;
        }

        public int ReadCount { get; private set; }
        public bool IsDisposed { get; private set; }
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _length;
        public override long Position { get; set; }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ReadCount++;
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}
