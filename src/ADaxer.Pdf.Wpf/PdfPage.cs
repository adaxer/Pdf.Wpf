using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using PDFiumCore;

namespace ADaxer.Pdf.Wpf;

[DebuggerDisplay("Page: {Number}, Zoom: {_scale}")]
public class PdfPage(FpdfDocumentT document, int number, double width, double height) : INotifyPropertyChanged, IDisposable
{
    const double GlobalResolution = 288;
    const double ThumbnailResolution = 48;
    private double _scale = 1.0;
    private ImageSource _thumbnail = default!;
    private ImageSource _fullImage = default!;

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
            if (_thumbnail == null)
            {
                var imageStream = document.RenderImage(Number - 1, (int)ThumbnailResolution);
                _thumbnail = GetImageSource(imageStream);
                imageStream.Dispose();
            }
            return _thumbnail;

        }
    }

    public ImageSource FullImage => _fullImage ?? Thumbnail;

    public override string ToString() => $"Page: {Number}, Zoom: {_scale}";

    private static BitmapImage GetImageSource(Stream imageStream)
    {
        BitmapImage bitmapImage = new();
        bitmapImage.BeginInit();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;  // Cache the image to avoid keeping the stream open
        bitmapImage.StreamSource = imageStream;
        bitmapImage.EndInit();
        return bitmapImage;
    }

    public void RenderFullImage()
    {
        if (_fullImage == null)
        {
            var imageStream = document.RenderImage(Number - 1, (int)GlobalResolution);
            _fullImage = GetImageSource(imageStream);
            imageStream.Dispose();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FullImage)));
        }
    }

    public void Dispose()
    {
        _fullImage = default!;
        _thumbnail = default!;
        GC.SuppressFinalize(this);
    }
}
