namespace Hodl.Utils;

public static class ImageHelper
{
    /// <summary>
    /// Creates an Image object from the Base64 encoded string.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static Image ImageDataFromBase64(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        // First split the data type info from the input
        // Example: data:image/png;base64,
        string[] parts = input.Split("base64,");
        try
        {
            byte[] bytes = Convert.FromBase64String(parts.Last());
            return Image.Load(bytes);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("The input is not a valid base64 string of an image", ex);
        }
    }

    /// <summary>
    /// Resizes and converts the Image to PNG and returns the byte array to 
    /// save in the data models.
    /// </summary>
    /// <param name="img"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static byte[] ConvertToPng(Image img, Size size)
    {
        if (img == null) return null;

        if (img.Width < size.Width || img.Height < size.Height)
        {
            throw new ArgumentException($"Image dimensions do not match the minimum size ({size.Width}, {size.Height})");
        }
        img.Mutate(x => x.Resize(size.Width, size.Height));

        //var resizeOptions = new ResizeOptions { Size = size, Mode = ResizeMode.Stretch };
        //var resizedImage = img.Clone(x => x.Resize(resizeOptions).BackgroundColor(Rgba32.White));

        using MemoryStream ms = new();

        img.SaveAsPng(ms);

        return ms.ToArray();
    }
}
