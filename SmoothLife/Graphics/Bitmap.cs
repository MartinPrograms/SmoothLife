using SkiaSharp;

namespace SmoothLife.Graphics;

public class Bitmap
{
    private int width;
    private int height;
    private float[] data;
    
    public Bitmap(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    public void SetData(float[] data)
    {
        this.data = data;
    }

    public void Save(string s)
    {
        SKBitmap bitmap = new SKBitmap(width, height);
        SKImageInfo info = new SKImageInfo(width, height);
        SKColor[] colors = new SKColor[width * height];
        
        for (int i = 0; i < data.Length; i += 4)
        {
            colors[i / 4] = new SKColor((byte)(data[i] * 255), (byte)(data[i + 1] * 255), (byte)(data[i + 2] * 255), (byte)(data[i + 3] * 255));
        }
        
        bitmap.Pixels = colors;
        var stream = File.Create(s);
        bitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream);
        
        stream.Close();
    }
}