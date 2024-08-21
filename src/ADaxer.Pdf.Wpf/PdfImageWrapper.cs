using PDFiumCore;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.IO;
using System.Runtime.InteropServices;

namespace ADaxer.Pdf.Wpf;
public class PdfImageWrapper : IDisposable
{
    private readonly GCHandle _pinnedArrayHandle;

    private Image<Bgra32> _imageData { get; }

    internal PdfImageWrapper(FpdfBitmapT pdfBitmap, int width, int height)
    {
        var _buffer = fpdfview.FPDFBitmapGetBuffer(pdfBitmap);
        var stride = fpdfview.FPDFBitmapGetStride(pdfBitmap);
        var bufferSize = stride * height;

        // Copy data from unmanaged buffer to managed array
        byte[] managedArray = new byte[bufferSize];
        Marshal.Copy(_buffer, managedArray, 0, bufferSize);

        // Pin the managed array in memory to prevent GC from moving it
        _pinnedArrayHandle = GCHandle.Alloc(managedArray, GCHandleType.Pinned);

        // Wrap the pinned memory in an ImageSharp Image object
        _imageData = Image.LoadPixelData<Bgra32>(managedArray, width, height);
    }

    public void Dispose()
    {
        _imageData?.Dispose();
        _pinnedArrayHandle.Free();
        // No need to free the buffer itself as PDFium manages that
    }

    public Stream GetPngStream()
    {
        var stream = new MemoryStream();
        _imageData.SaveAsPng(stream);
        return stream;
    }
}
