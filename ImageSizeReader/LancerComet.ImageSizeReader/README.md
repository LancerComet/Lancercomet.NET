# ImageSizeReader

This project was ported from https://github.com/doxakis/ImageSizeReader

Compare to the original:

 - Functions are turned to static function.
 - Add webp detection.
 - Add bmp detection.

## Quick start

```csharp
var size = ImageSizeReaderUtil.GetDimensions(Some_Image_Stream);
Console.WriteLine($"Image size: {size.Width}, {size.Height}");
```

## License

MIT.
