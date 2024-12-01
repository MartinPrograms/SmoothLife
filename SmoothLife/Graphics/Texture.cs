namespace SmoothLife.Graphics;

public class Texture : IDisposable
{
    // This texture does NOT support loading from file, only from memory
    public int Handle;
    public int Width;
    public int Height;
    
    public Texture(int width, int height, float[] data)
    {
        Width = width;
        Height = height;
        
        Handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Handle);
        
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, width, height, 0, PixelFormat.Rgba, PixelType.Float, data);
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
    }
    
    public void Bind(int unit)
    {
        GL.ActiveTexture(TextureUnit.Texture0 + unit);
        GL.BindTexture(TextureTarget.Texture2D, Handle);
    }
    
    public void SetData(float[] data)
    {
        GL.BindTexture(TextureTarget.Texture2D, Handle);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, Width, Height, 0, PixelFormat.Rgba, PixelType.Float, data);
    }
    
    public void Dispose()
    {
        GL.DeleteTexture(Handle);
    }
    
    public void Unbind()
    {
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Resize(int sizeX, int sizeY)
    {
        Width = sizeX;
        Height = sizeY;
        
        GL.BindTexture(TextureTarget.Texture2D, Handle);
        
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, sizeX, sizeY, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
    }

    public void BindCompute(int i, TextureAccess access = TextureAccess.ReadWrite)
    {
        GL.BindImageTexture(i, Handle, 0, false, 0, access, SizedInternalFormat.Rgba32f);
    }
}