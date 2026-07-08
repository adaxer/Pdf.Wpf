using System.Runtime.InteropServices;
using PDFiumCore;

namespace ADaxer.Pdf.Wpf;

public sealed class PdfDocument : IDisposable
{
    private readonly GCHandle _pdfBytesHandle;
    private bool _disposed;

    public PdfDocument(byte[] pdfBytes)
    {
        ArgumentNullException.ThrowIfNull(pdfBytes);

        if (pdfBytes.Length == 0)
            throw new ArgumentException("PDF bytes must not be empty.", nameof(pdfBytes));

        PdfiumExtensions.EnsureInitialized();

        _pdfBytesHandle = GCHandle.Alloc(pdfBytes, GCHandleType.Pinned);

        Handle = fpdfview.FPDF_LoadMemDocument(
            _pdfBytesHandle.AddrOfPinnedObject(),
            pdfBytes.Length,
            null);

        if (Handle == null)
        {
            _pdfBytesHandle.Free();
            throw new InvalidOperationException("PDF document could not be loaded.");
        }

        PageCount = fpdfview.FPDF_GetPageCount(Handle);
    }

    public FpdfDocumentT Handle { get; }

    public int PageCount { get; }

    public void Dispose()
    {
        if (_disposed)
            return;

        fpdfview.FPDF_CloseDocument(Handle);

        if (_pdfBytesHandle.IsAllocated)
            _pdfBytesHandle.Free();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}