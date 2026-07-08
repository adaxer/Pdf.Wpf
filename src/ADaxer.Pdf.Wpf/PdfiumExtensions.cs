using PDFiumCore;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ADaxer.Pdf.Wpf;

public static class PdfiumExtensions
{
    private static bool _isPdfiumInitialized;

    internal static void EnsureInitialized()
    {
        if (_isPdfiumInitialized)
            return;

        fpdfview.FPDF_InitLibrary();
        _isPdfiumInitialized = true;
    }

    public static IReadOnlyList<PdfPage> ToPages(this PdfDocument document)
    {
        var pages = new List<PdfPage>(document.PageCount);

        for (var i = 0; i < document.PageCount; i++)
        {
            var page = fpdfview.FPDF_LoadPage(document.Handle, i);

            try
            {
                pages.Add(new PdfPage(
                    document,
                    i + 1,
                    fpdfview.FPDF_GetPageWidthF(page),
                    fpdfview.FPDF_GetPageHeightF(page)));
            }
            finally
            {
                fpdfview.FPDF_ClosePage(page);
            }
        }

        return pages;
    }

    public static BitmapSource RenderImageSource(this FpdfDocumentT document, int pageNo, int dpi)
    {
        EnsureInitialized();

        var scale = (float)dpi / 72f;

        var page = fpdfview.FPDF_LoadPage(document, pageNo);

        if (page == null)
            throw new InvalidOperationException($"Failed to load PDF page {pageNo}.");

        try
        {
            using var size = new FS_SIZEF_();
            fpdfview.FPDF_GetPageSizeByIndexF(document, pageNo, size);

            var pixelWidth = Math.Max(1, (int)Math.Ceiling(size.Width * scale));
            var pixelHeight = Math.Max(1, (int)Math.Ceiling(size.Height * scale));

            var bitmap = fpdfview.FPDFBitmapCreateEx(
                pixelWidth,
                pixelHeight,
                (int)FPDFBitmapFormat.BGRA,
                IntPtr.Zero,
                0);

            if (bitmap == null)
                throw new InvalidOperationException("Failed to create PDFium bitmap.");

            try
            {
                fpdfview.FPDFBitmapFillRect(
                    bitmap,
                    0,
                    0,
                    pixelWidth,
                    pixelHeight,
                    uint.MaxValue);

                using var matrix = new FS_MATRIX_();
                using var clipping = new FS_RECTF_();

                matrix.A = scale;
                matrix.B = 0;
                matrix.C = 0;
                matrix.D = scale;
                matrix.E = 0;
                matrix.F = 0;

                clipping.Left = 0;
                clipping.Right = pixelWidth;
                clipping.Bottom = 0;
                clipping.Top = pixelHeight;

                fpdfview.FPDF_RenderPageBitmapWithMatrix(
                    bitmap,
                    page,
                    matrix,
                    clipping,
                    (int)RenderFlags.RenderAnnotations);

                var buffer = fpdfview.FPDFBitmapGetBuffer(bitmap);
                var stride = fpdfview.FPDFBitmapGetStride(bitmap);
                var bufferSize = stride * pixelHeight;

                var pixels = new byte[bufferSize];
                Marshal.Copy(buffer, pixels, 0, bufferSize);

                var source = BitmapSource.Create(
                    pixelWidth,
                    pixelHeight,
                    dpi,
                    dpi,
                    PixelFormats.Bgra32,
                    null,
                    pixels,
                    stride);

                source.Freeze();
                return source;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error rendering image source: {ex}");
                throw;
            }
            finally
            {
                fpdfview.FPDFBitmapDestroy(bitmap);
            }
        }
        finally
        {
            fpdfview.FPDF_ClosePage(page);
        }
    }
}