using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ADaxer.Pdf.Wpf.DemoApp;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        pdf.PdfBytes = File.ReadAllBytes("C:\\temp\\mittel.pdf");
    }


    private void SetMode(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb)
        {
            pdf.ViewMode = (PdfViewModes)(Enum.Parse(typeof(PdfViewModes), (rb.Content.ToString()!)));
        }

    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        try
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && ((string[])e.Data.GetData(DataFormats.FileDrop))?.FirstOrDefault() is String file)
            {
                pdf.PdfBytes = File.ReadAllBytes(file);
            }
        }
        catch (Exception ex)
        {
            Trace.TraceError($"{ex}");
        }

    }

    private void OnLoad(object sender, TextChangedEventArgs e)
    {
        if (File.Exists((sender as TextBox)?.Text) && pdf != null)
        {
            pdf.PdfBytes = File.ReadAllBytes((sender as TextBox)?.Text!);
        }

    }
}