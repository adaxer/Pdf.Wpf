using PDFiumCore;
using System.Diagnostics;
using System.IO;

namespace ADaxer.Pdf.Wpf;
public static class PdfiumExtensions
{
    private static bool _isPdfiumInitialized = false;
    private static void EnsureInitialized()
    {
        if (!_isPdfiumInitialized)
        {
            fpdfview.FPDF_InitLibrary();
            _isPdfiumInitialized = true;
        }
    }

    public static unsafe FpdfDocumentT ToFpdfDocument(this byte[] pdfBytes)
    {
        EnsureInitialized();
        fixed (void* ptr = pdfBytes)
        {
            var document = fpdfview.FPDF_LoadMemDocument(new IntPtr(ptr), pdfBytes.Length, null);
            return document;
        }
    }

    public static IEnumerable<PdfPage> ToPages(this byte[] pdfBytes)
    {
        var document = pdfBytes.ToFpdfDocument();
        var pageCount = fpdfview.FPDF_GetPageCount(document);
        for (int i = 0; i < pageCount; i++)
        {
            var page = fpdfview.FPDF_LoadPage(document, i);
            yield return new PdfPage(document, i + 1, fpdfview.FPDF_GetPageWidthF(page), fpdfview.FPDF_GetPageHeightF(page));
        }
    }

    public static Stream RenderImage(this FpdfDocumentT document, int pageNo, int dpi)
    {
        EnsureInitialized();
        float scale = (float)dpi / 72;
        // White color.
        uint color = uint.MaxValue;

        var page = fpdfview.FPDF_LoadPage(document, pageNo);
        FS_SIZEF_ size = new FS_SIZEF_();
        fpdfview.FPDF_GetPageSizeByIndexF(document, 0, size);

        double pageWidth = size.Width * scale;
        var pageHeight = size.Height * scale;

        var bitmap = fpdfview.FPDFBitmapCreateEx(
                (int)pageWidth,
                (int)pageHeight,
                (int)FPDFBitmapFormat.BGRA,
                IntPtr.Zero,
                0);

        if (bitmap == null)
            throw new Exception("failed to create a bitmap object");

        // Leave out if you want to make the background transparent.
        fpdfview.FPDFBitmapFillRect(bitmap, 0, 0, (int)pageWidth, (int)pageHeight, color);

        // |          | a b 0 |
        // | matrix = | c d 0 |
        // |          | e f 1 |
        using var matrix = new FS_MATRIX_();
        using var clipping = new FS_RECTF_();

        matrix.A = scale;
        matrix.B = 0;
        matrix.C = 0;
        matrix.D = scale;
        matrix.E = 0;
        matrix.F = 0;

        clipping.Left = 0;
        clipping.Right = (float)pageWidth;
        clipping.Bottom = 0;
        clipping.Top = (float)pageHeight;

        fpdfview.FPDF_RenderPageBitmapWithMatrix(bitmap, page, matrix, clipping, (int)RenderFlags.RenderAnnotations);


        using var imageWrapper = new PdfImageWrapper(
            bitmap,
            (int)(pageWidth),
            (int)(pageHeight));

        var stream = imageWrapper.GetPngStream();
        return stream;
    }
}
