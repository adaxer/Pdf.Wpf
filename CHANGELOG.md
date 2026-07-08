# Changelog

All notable changes to this project will be documented in this file.

The format is inspired by Keep a Changelog and follows Semantic
Versioning.

## \[Unreleased\]

### Added

-   Placeholder for future changes.

------------------------------------------------------------------------

## \[1.2.0\] - 2026-07-08

### Changed

-   Removed the dependency on SixLabors.ImageSharp.
-   PDF pages are now rendered directly to WPF `BitmapSource`.
-   Introduced internal `PdfDocument` lifetime management.
-   Improved PDFium document and page resource handling.
-   Reduced memory usage when switching PDF documents.
-   Simplified package dependencies (PDFiumCore only).
-   Updated NuGet package metadata.
-   Updated project documentation and README.

### Fixed

-   Proper disposal of PDFium document and page handles.
-   Improved lazy page rendering.
-   Reduced unnecessary memory retention.

------------------------------------------------------------------------

## \[1.1.2\] - 2026-06-18

### Changed

-   Maintenance release.
-   Internal improvements and bug fixes.
-   Updated dependencies.

------------------------------------------------------------------------

## \[1.1.1\] - Initial public release

### Added

-   WPF `PdfView` control.
-   Display PDF documents from `byte[]`.
-   Single Page, Double Page and Scrolling view modes.
-   Zoom In / Zoom Out.
-   Fit to Width / Fit to Height.
-   Page navigation.
-   Lazy page rendering.
-   Rendering based on PDFiumCore.
