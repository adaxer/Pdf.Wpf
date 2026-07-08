# ADaxer.Pdf.Wpf

A free, reusable PDF viewer control for WPF applications.

> **Note**
>
> To the best of my knowledge, there is currently no freely available,
> easy-to-use WPF PDF viewer control that supports modern PDF rendering
> and integrates naturally into WPF applications. This project was
> created to fill that gap.
>
> If you know of a comparable open-source alternative, I'd be happy to
> hear about it.

## Overview

ADaxer.Pdf.Wpf is a lightweight WPF control for displaying PDF documents
directly from a `byte[]`.

### Features

-   Display PDF documents directly from memory (`byte[]`)
-   Single Page, Double Page and Scrolling view modes
-   Zoom In / Zoom Out
-   Fit to Width / Fit to Height
-   Page navigation
-   Lazy page rendering
-   Optimized rendering and memory management
-   Direct rendering to WPF `BitmapSource`
-   Minimal external dependencies

## Demo

A demo application is included in the repository and demonstrates all
supported view modes, zooming and page navigation.

## Getting Started

Install the NuGet package:

``` bash
dotnet add package ADaxer.Pdf.Wpf
```

Reference the control:

``` xml
<pdf:PdfView PdfBytes="{Binding PdfBytes}" />
```

Load a document:

``` csharp
PdfBytes = File.ReadAllBytes("Sample.pdf");
```

Bind the properties (`PdfBytes`, `Zoom`, `ViewMode`, `CurrentPage`) to your view model or code-behind as needed.

Use the commands provided by the control to navigate and interact with the PDF.

   ```csharp
   // Zoom in
   pdfViewer.ZoomInCommand.Execute(null);

   // Move to the next page
   pdfViewer.NextPageCommand.Execute(null);
   ```


## External dependencies

Rendering is performed using **PDFiumCore**.

Starting with **Version 1.2.0**, the dependency on **ImageSharp** has
been removed. PDF pages are rendered directly to WPF `BitmapSource`
objects, reducing both dependencies and memory usage.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for a complete history of changes.

## Contributing

Contributions, bug reports and feature requests are welcome.

If you encounter a problem or have an idea for an improvement, please
open an issue or submit a pull request.

## License

This project is licensed under the MIT License.
