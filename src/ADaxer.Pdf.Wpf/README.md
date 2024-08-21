# WPF PDF Viewer Control

_A free PDF viewer control for WPF applications._ 

To the best of my knowledge, there is currently no other free PDF viewer available for WPF, so I created one, as I needed it in a recent customer project.

## Overview

`PdfView` is a custom WPF control designed to easily display PDF documents within WPF applications. It features:

- **Zooming**: Easily zoom in and out of PDF pages.
- **Navigation**: Navigate between pages using commands for next and previous pages.
- **View Modes**: Switch between different view modes, including single-page, double-page, and scrolling modes.
- **Fit to Width/Height**: Commands to automatically adjust the zoom level to fit the width or height of the control.

## Demo Application

A demo WPF application to demonstrate how to use the `PdfView` control is provided, so check out the ADaxer.Pdf.Wpf.DemoApp (which uses the wonderful [MaterialDesignInXaml toolkit](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)).

## Getting started

To use the `PdfView` control in your WPF project, follow these simple steps:

1. **Add it to Your Project** 
- by copying the source code or by adding it as a project reference.

- Or by adding the **NuGet Package**:

   ```sh
   dotnet add package PdfViewerControl --version 1.0.0
   ```

   You can find the package [here](https://www.nuget.org/packages/PdfViewerControl).

3. **XAML Usage**: Add the `PdfView` control to your XAML file.

   ```xml
   <Window x:Class="YourNamespace.MainWindow"
           xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
           xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
           xmlns:adaxer="http://adaxer.de/pdf/wpf"
           Title="PDF Viewer" >
       <Grid>
           <adaxer:PdfView x:Name="pdfViewer"
                          PdfBytes="{Binding PdfBytes}"
                          Zoom="{Binding ZoomLevel}"
                          ViewMode="{Binding SelectedViewMode}"
                          CurrentPage="{Binding CurrentPage}" />
       </Grid>
   </Window>
   ```

4. **Bind the Properties**: Bind the properties (`PdfBytes`, `Zoom`, `ViewMode`, `CurrentPage`) to your view model or code-behind as needed.

5. **Load PDF Data**: Load a PDF file into the `PdfBytes` property as a byte array.

   ```csharp
   byte[] pdfData = File.ReadAllBytes("path/to/your/file.pdf");
   pdfViewer.PdfBytes = pdfData;
   ```

6. **Use Commands**: Use the commands provided by the control to navigate and interact with the PDF.

   ```csharp
   // Zoom in
   pdfViewer.ZoomInCommand.Execute(null);

   // Move to the next page
   pdfViewer.NextPageCommand.Execute(null);
   ```

## External dependencies
The image processing of the Pdf is done via [PdfiumCore](https://github.com/Dtronix/PDFiumCore) and uses [ImageSharp](https://github.com/SixLabors/ImageSharp), apart from that there are no dependencies.

## Contributing

Contributions are welcome! If you find any issues or have suggestions for improvements, please feel free to open an issue or submit a pull request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
