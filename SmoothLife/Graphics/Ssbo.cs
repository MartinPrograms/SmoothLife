using System.Runtime.CompilerServices;

namespace SmoothLife.Graphics;

public class Ssbo<T> : ISsbo where T : struct
{
    public T[] Data;
    public int Size { get; set; }
    public int Handle { get; set; }
    public int Index { get; set; }
    
    public Ssbo(T[] data, int elementSize, int index)
    {
        Data = data;
        Size = data.Length * elementSize;
     
        Index = index;
        
        Handle = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Handle);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, Size, data, BufferUsageHint.DynamicDraw);
        
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, Index, Handle);
    }
    
    public void Update()
    {
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Handle);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, Size, Data, BufferUsageHint.DynamicDraw);
    }
    
    public void Dispose()
    {
        GL.DeleteBuffer(Handle);
    }

    public void Bind()
    {
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, Index, Handle);
    }
    
    public void Unbind()
    {
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
    }
    
    public void SetData(T[] data)
    {
        Data = data;
        Size = data.Length * Unsafe.SizeOf<T>();
        
        Update();
    }
    
    public void SetData(int index, T data)
    {
        Data[index] = data;
    }

    public void Read()
    {
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Handle);
        GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, Size, Data);
    }

    public void From(ISsbo slOutput)
    {
        GL.BindBuffer(BufferTarget.CopyReadBuffer, slOutput.Handle);
        GL.BindBuffer(BufferTarget.CopyWriteBuffer, Handle);
        
        GL.CopyBufferSubData(BufferTarget.CopyReadBuffer, BufferTarget.CopyWriteBuffer, IntPtr.Zero, IntPtr.Zero, Size);
        
        GL.BindBuffer(BufferTarget.CopyReadBuffer, 0);
        GL.BindBuffer(BufferTarget.CopyWriteBuffer, 0);
    }
}

public interface ISsbo
{
    int Handle { get; set; } // For example, 0 would be at binding point 0
    int Size { get; set; }
    int Index { get; set; }
    void Bind();
    void Unbind();
    void Dispose();
    void Read();
    void From(ISsbo slOutput);
}