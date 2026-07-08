using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ADaxer.Pdf.Wpf;

[TemplatePart(Name = "PART_ScrollViewer", Type = typeof(ScrollViewer))]
[TemplatePart(Name = "PART_Items", Type = typeof(ItemsControl))]
public class PdfView : Control, IDisposable
{
    private bool _isZoomInvalid;
    private ScrollViewer _scrollViewer = default!;
    private ItemsControl _itemsControl = default!;
    private List<PdfPage> _pages = [];
    private Panel _panel = default!;
    private PdfDocument? _document;

    static PdfView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(PdfView), new FrameworkPropertyMetadata(typeof(PdfView)));
    }

    public PdfView()
    {
        Pages = [];
    }

    #region Bindable Properties

    public static readonly DependencyProperty PdfBytesProperty =
        DependencyProperty.Register("PdfBytes", typeof(object), typeof(PdfView), new PropertyMetadata(null, propertyChangedCallback: (d, e) => (d as PdfView)?.OnPdfBytesChanged(e)));

    public object PdfBytes
    {
        get { return (object)GetValue(PdfBytesProperty); }
        set { SetValue(PdfBytesProperty, value); }
    }

    public static readonly DependencyProperty ZoomProperty =
        DependencyProperty.Register("Zoom", typeof(double), typeof(PdfView), new PropertyMetadata(1.0, propertyChangedCallback: (d, e) => (d as PdfView)?.OnZoomChanged(e)));

    public double Zoom
    {
        get { return (double)GetValue(ZoomProperty); }
        set { SetValue(ZoomProperty, value); }
    }

    public static readonly DependencyProperty ViewModeProperty =
        DependencyProperty.Register("ViewMode", typeof(PdfViewModes), typeof(PdfView), new PropertyMetadata(PdfViewModes.Single, propertyChangedCallback: (d, e) => (d as PdfView)?.OnViewModeChanged(e)));

    public PdfViewModes ViewMode
    {
        get { return (PdfViewModes)GetValue(ViewModeProperty); }
        set { SetValue(ViewModeProperty, value); }
    }

    public static readonly DependencyProperty CurrentPageProperty =
        DependencyProperty.Register("CurrentPage", typeof(int), typeof(PdfView), new PropertyMetadata(1, propertyChangedCallback: (d, e) => (d as PdfView)?.OnCurrentPageChanged(e)));

    public int CurrentPage
    {
        get { return (int)GetValue(CurrentPageProperty); }
        set { SetValue(CurrentPageProperty, value); }
    }

    public static readonly DependencyProperty PagesProperty =
        DependencyProperty.Register("Pages", typeof(ObservableCollection<PdfPage>), typeof(PdfView), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public ObservableCollection<PdfPage> Pages
    {
        get { return (ObservableCollection<PdfPage>)GetValue(PagesProperty); }
        set { SetValue(PagesProperty, value); }
    }
    #endregion

    #region Commands
    public ICommand FitWidthCommand => new PdfViewCommand(FitWidth);
    public ICommand FitHeightCommand => new PdfViewCommand(FitHeight);
    public ICommand PreviousPageCommand => new PdfViewCommand(PreviousPage);
    public ICommand NextPageCommand => new PdfViewCommand(NextPage);
    public ICommand ZoomOutCommand => new PdfViewCommand(ZoomOut);
    public ICommand ZoomInCommand => new PdfViewCommand(ZoomIn);
    #endregion

    public int PageCount => _pages == null ? 0 : _pages.Count;

    #region Overrides

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        try
        {
            _scrollViewer = (GetTemplateChild("PART_ScrollViewer") as ScrollViewer)!;
            _scrollViewer.ScrollChanged += OnScrollChanged;
            _itemsControl = (GetTemplateChild("PART_Items") as ItemsControl)!;
            _itemsControl.Loaded += (s, e) =>
            {
                _panel = (FindItemsPanel(_itemsControl) as Panel)!;
                if (_panel == null)
                {
                    throw new InvalidOperationException("The PART_Items ItemsControl must use a Panel in order for PdfView to work properly");
                }
            };
        }
        catch (Exception ex)
        {
            Trace.TraceError($"{ex}");
            throw new InvalidOperationException("Please make sure to use all Template parts", ex);
        }
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (_isZoomInvalid)
        {
            FitHeight();
            _isZoomInvalid = false;
            Dispatcher.BeginInvoke(InvalidateVisual, DispatcherPriority.Loaded);
        }
        else
        {
            _pages.ForEach(p => p.Rescale(ActualHeight * Zoom));
            base.OnRender(drawingContext);
            Redraw();
        }
    }

    // private Dictionary

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        ICommand? toExecute = (e.Key, Keyboard.Modifiers) switch
        {
            (Key.PageDown, ModifierKeys.None) => NextPageCommand,
            (Key.PageUp, ModifierKeys.None) => PreviousPageCommand,
            (Key.OemPlus, ModifierKeys.Control) => ZoomInCommand,
            (Key.OemMinus, ModifierKeys.Control) => ZoomOutCommand,
            _ => default
        };

        if (toExecute == default(ICommand))
        {
            base.OnPreviewKeyDown(e);
        }

        else
        {
            e.Handled = true;
            if (toExecute.CanExecute(this))
            {
                toExecute.Execute(this);
            }
        }
    }

    #endregion

    private static Panel FindItemsPanel(FrameworkElement element)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
        {
            var child = VisualTreeHelper.GetChild(element, i);
            if (child is Panel panel && panel.IsItemsHost)
            {
                return panel;
            }

            var foundPanel = FindItemsPanel((child as FrameworkElement)!);
            if (foundPanel != null)
            {
                return foundPanel;
            }
        }
        return default!;
    }

    private void OnViewModeChanged(DependencyPropertyChangedEventArgs _)
    {
        Trace.TraceInformation($"OnViewModeChanged: {ViewMode}");
        InvalidateVisual();
    }

    private void OnCurrentPageChanged(DependencyPropertyChangedEventArgs _)
    {
        int correctPage = Math.Min(PageCount, Math.Max(1, CurrentPage));
        Trace.TraceInformation($"OnCurrentPageChanged: {CurrentPage} (correct {correctPage})");
        if (CurrentPage == correctPage)
        {
            if (ViewMode == PdfViewModes.Scrolling)
            {
                ScrollToPage(correctPage);
            }
            InvalidateVisual();
        }
        else
        {
            CurrentPage = Math.Max(1,correctPage);
        }
    }

    private void OnZoomChanged(DependencyPropertyChangedEventArgs _)
    {
        Trace.TraceInformation($"OnZoomChanged: {Zoom}");
        InvalidateVisual();
    }

    private void OnPdfBytesChanged(DependencyPropertyChangedEventArgs e)
    {
        Trace.TraceInformation($"OnPdfBytesChanged: {e.OldValue} => {e.NewValue}");

        ClearDocument();
        foreach (var page in _pages)
            page.Dispose();

        _pages.Clear();

        _document?.Dispose();
        _document = null;

        Pages.Clear();
        CurrentPage = 1;
        _isZoomInvalid = true;

        if (PdfBytes is not byte[] pdfBytes || pdfBytes.Length == 0)
        {
            Visibility = Visibility.Collapsed;
            InvalidateVisual();
            return;
        }

        Visibility = Visibility.Visible;

        _document = new PdfDocument(pdfBytes);
        _pages = [.. _document.ToPages()];

        InvalidateVisual();
    }

    private void ClearDocument()
    {
        foreach (var page in _pages)
            page.Dispose();

        _pages.Clear();
        Pages.Clear();

        _document?.Dispose();
        _document = null;
    }

    private void ScrollToPage(int correctPage)
    {
        Dispatcher.BeginInvoke(() =>
        {
            try
            {
                double offset = 0;
                for (int i = 0; i < correctPage - 1; i++)
                {
                    if (_panel.Children[i] is FrameworkElement child)
                    {
                        offset += child.ActualHeight;
                    }
                }

                _scrollViewer.UpdateLayout();
                _scrollViewer.ScrollToVerticalOffset(offset);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"{ex}");
            }
        }, DispatcherPriority.Loaded);
    }

    private void Redraw()
    {
        if (PageCount == 0) return;

        Pages.Clear();

        var pagesToDraw = new List<int> { CurrentPage - 1 };

        switch (ViewMode)
        {
            case PdfViewModes.Double:
                int first = 2 * (((int)CurrentPage - 1) / 2);
                pagesToDraw = [.. Enumerable.Range(first, (PageCount < 2) ? 1 : 2)];
                break;
            case PdfViewModes.Scrolling:
                pagesToDraw = [.. Enumerable.Range(0, PageCount)];
                break;
            default:
                break;
        }

        Trace.TraceInformation($"Redrawing {pagesToDraw.Count} pages ({pagesToDraw.First()} - {pagesToDraw.Last()})");
        for (int i = 0; i < Math.Min(pagesToDraw.Count, PageCount); i++)
        {
            Pages.Add(_pages[pagesToDraw[i]]);
        }

        if (ViewMode == PdfViewModes.Scrolling)
        {
            ScrollToPage(CurrentPage);
        }
        else
        {
            pagesToDraw.ForEach(p => _pages[p].RenderFullImage());
        }
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        var firstVisibleIndex = GetFirstVisibleItemIndex();
        var lastVisibleIndex = GetLastVisibleItemIndex();
        LoadVisibleItems(firstVisibleIndex, lastVisibleIndex);
    }

    private int GetFirstVisibleItemIndex()
    {
        if (_scrollViewer == null || _itemsControl == null)
            return 0;

        var itemHeight = _itemsControl.ActualHeight / _itemsControl.Items.Count;
        var firstVisibleIndex = (int)(_scrollViewer.VerticalOffset / itemHeight);
        return firstVisibleIndex;
    }

    private int GetLastVisibleItemIndex()
    {
        if (_scrollViewer == null || _itemsControl == null)
            return 0;

        var itemHeight = _itemsControl.ActualHeight / _itemsControl.Items.Count;
        var lastVisibleIndex = (int)((_scrollViewer.VerticalOffset + _scrollViewer.ViewportHeight) / itemHeight);
        return lastVisibleIndex;
    }

    private const int PreloadPageMargin = 2;

    private void LoadVisibleItems(int firstVisibleIndex, int lastVisibleIndex)
    {
        var loadFrom = Math.Max(0, firstVisibleIndex - PreloadPageMargin);
        var loadTo = Math.Min(PageCount - 1, lastVisibleIndex + PreloadPageMargin);

        for (var i = loadFrom; i <= loadTo; i++)
            _pages[i].RenderFullImage();
    }
    private void FitWidth()
    {
        var page = _pages[CurrentPage - 1];
        Zoom = (ActualWidth / ActualHeight) * (page.Height / page.Width) * ((ViewMode == PdfViewModes.Double) ? .48 : 0.98);
    }

    private void FitHeight()
    {
        if (PageCount == 0) return;
        Zoom = 1.0;
    }

    private void PreviousPage()
    {
        if (CurrentPage == 1)
        {
            return;
        }
        CurrentPage--;
    }

    private void NextPage()
    {
        if (CurrentPage >= PageCount)
        {
            return;
        }
        CurrentPage++;
    }

    private void ZoomOut()
    {
        if (Zoom <= 0.1)
        {
            return;
        }
        var currentZoom = Zoom;
        Zoom = 1;
        Zoom = (Math.Round(currentZoom * 10) / 10) - 0.1;
    }

    private void ZoomIn()
    {
        var currentZoom = Zoom;
        Zoom = 1;
        Zoom = (Math.Round(currentZoom * 10) / 10) + 0.1;
    }

    public void Dispose()
    {
        foreach (var page in _pages)
            page.Dispose();

        _pages.Clear();
        Pages.Clear();

        _document?.Dispose();
        _document = null;

        if (_scrollViewer != null)
            _scrollViewer.ScrollChanged -= OnScrollChanged;

        GC.SuppressFinalize(this);
    }

    private class PdfViewCommand(Action action) : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            action();
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
