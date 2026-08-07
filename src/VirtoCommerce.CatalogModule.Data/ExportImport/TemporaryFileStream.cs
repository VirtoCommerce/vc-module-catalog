using System.IO;

namespace VirtoCommerce.CatalogModule.Data.ExportImport;

internal static class TemporaryFileStream
{
    public static FileStream Create()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var options = new FileStreamOptions
        {
            Access = FileAccess.ReadWrite,
            BufferSize = CatalogPackageFormat.CopyBufferSize,
            Mode = FileMode.CreateNew,
            Options = FileOptions.Asynchronous | FileOptions.DeleteOnClose,
            Share = FileShare.None,
        };

        return new FileStream(path, options);
    }
}
