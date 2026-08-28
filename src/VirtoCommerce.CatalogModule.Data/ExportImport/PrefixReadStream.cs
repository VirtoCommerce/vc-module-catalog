using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VirtoCommerce.CatalogModule.Data.ExportImport;

internal sealed class PrefixReadStream : Stream
{
    private readonly byte[] _prefix;
    private readonly int _prefixLength;
    private readonly Stream _stream;
    private readonly bool _leaveOpen;
    private int _prefixPosition;

    public PrefixReadStream(byte[] prefix, int prefixLength, Stream stream, bool leaveOpen = false)
    {
        _prefix = prefix;
        _prefixLength = prefixLength;
        _stream = stream;
        _leaveOpen = leaveOpen;
    }

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
        var prefixCount = ReadPrefix(buffer.AsSpan(offset, count));
        return prefixCount > 0
            ? prefixCount
            : _stream.Read(buffer, offset, count);
    }

    public override int Read(Span<byte> buffer)
    {
        var prefixCount = ReadPrefix(buffer);
        return prefixCount > 0
            ? prefixCount
            : _stream.Read(buffer);
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var prefixCount = ReadPrefix(buffer.AsSpan(offset, count));
        return prefixCount > 0
            ? Task.FromResult(prefixCount)
            : _stream.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var prefixCount = ReadPrefix(buffer.Span);
        return prefixCount > 0
            ? ValueTask.FromResult(prefixCount)
            : _stream.ReadAsync(buffer, cancellationToken);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_leaveOpen)
        {
            _stream.Dispose();
        }

        base.Dispose(disposing);
    }

    private int ReadPrefix(Span<byte> buffer)
    {
        var count = Math.Min(buffer.Length, _prefixLength - _prefixPosition);
        if (count > 0)
        {
            _prefix.AsSpan(_prefixPosition, count).CopyTo(buffer);
            _prefixPosition += count;
        }

        return count;
    }
}
