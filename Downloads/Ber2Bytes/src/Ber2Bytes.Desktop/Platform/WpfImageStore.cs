using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ber2Bytes.Application.Abstractions;
using Ber2Bytes.Infrastructure.Storage;

namespace Ber2Bytes.Desktop.Platform;

/// <summary>Saves product photos as small JPEG files using WPF imaging (Windows only).</summary>
public class WpfImageStore(IAppLogger log) : IImageStore
{
    /// <summary>Resizes the photo to max 500px on a white background, saves it as JPEG and returns the file name.</summary>
    public string Save(byte[] data)
    {
        // WPF imaging must run on the UI (STA) thread.
        return System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            using var input = new MemoryStream(data);
            var frame = BitmapFrame.Create(input, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            const int max = 500;
            double scale = Math.Min(1.0, (double)max / Math.Max(frame.PixelWidth, frame.PixelHeight));
            int w = Math.Max(1, (int)Math.Round(frame.PixelWidth * scale));
            int h = Math.Max(1, (int)Math.Round(frame.PixelHeight * scale));

            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, w, h));
                dc.DrawImage(frame, new Rect(0, 0, w, h));
            }
            var bmp = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(visual);

            var encoder = new JpegBitmapEncoder { QualityLevel = 85 };
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            var name = $"{Guid.NewGuid():N}.jpg";
            using var output = File.Create(Path.Combine(AppPaths.ImagesFolder, name));
            encoder.Save(output);
            return name;
        });
    }

    public void Delete(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return;
        try
        {
            var path = Path.Combine(AppPaths.ImagesFolder, Path.GetFileName(fileName));
            if (File.Exists(path)) File.Delete(path);
        }
        catch (Exception ex) { log.Error("Could not delete image " + fileName, ex); }
    }
}
