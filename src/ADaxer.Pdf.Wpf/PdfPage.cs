using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Media;

using PDFiumCore;

namespace ADaxer.Pdf.Wpf;

[DebuggerDisplay("Page: {Number}, Zoom: {_scale}")]
public class PdfPage(PdfDocument document, int number, double width, double height)
    : INotifyPropertyChanged, IDisposable
{
    private const double GlobalResolution = 144;
    private const double ThumbnailResolution = 8;

    private double _scale = 1.0;
    private ImageSource? _thumbnail;
    private ImageSource? _fullImage;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Number { get; } = number;

    public double Width { get; set; } = width;

    public double Height { get; set; } = height;

    public double UIWidth => Width * _scale;

    public double UIHeight => Height * _scale;

    public void Rescale(double newScale) => _scale = newScale / Height;

    public ImageSource Thumbnail
    {
        get
        {
            Trace.TraceInformation($"Thumbnail called for {this}");

            return _thumbnail ??= document.Handle.RenderImageSource(Number - 1, (int)ThumbnailResolution);
        }
    }

    public ImageSource FullImage => _fullImage ?? Thumbnail;

    public override string ToString() => $"Page: {Number}, Zoom: {_scale}";

    public void RenderFullImage()
    {
        if (_fullImage != null)
            return;

        _fullImage = document.Handle.RenderImageSource(Number - 1, (int)GlobalResolution);

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FullImage)));
    }
    public void UnloadFullImage()
    {
        if (_fullImage == null)
            return;

        _fullImage = null;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FullImage)));
    }

    public void Dispose()
    {
        UnloadFullImage();
        _thumbnail = null;
        GC.SuppressFinalize(this);
    }
}