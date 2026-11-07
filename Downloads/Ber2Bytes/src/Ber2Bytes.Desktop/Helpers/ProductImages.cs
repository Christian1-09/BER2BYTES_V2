using Ber2Bytes.Infrastructure.Storage;

namespace Ber2Bytes.Desktop.Helpers;

public static class ProductImages
{
    /// <summary>URL the WebView uses to show a product photo (null when there is none).
    /// MainWindow maps https://productimages.local/ to the images folder.</summary>
    public static string? Url(string? fileName) =>
        string.IsNullOrEmpty(fileName) ? null : $"https://{AppPaths.ImageHost}/{fileName}";
}
