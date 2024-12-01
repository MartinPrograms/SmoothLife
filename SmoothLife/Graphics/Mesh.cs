using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SmoothLife.Graphics;

public class Mesh
{
    public int Handle;
    public int VertexCount;
    public int IndexCount;
    public int VertexArray;
    public int IndexBuffer;
    public int VertexBuffer;
    
    public Mesh(Vertex[] vertices, uint[] indices)
    {
        int vertexSize = 5; // 3 for position, 2 for texCoords
        
        VertexCount = vertices.Length;
        IndexCount = indices.Length;
        
        VertexBuffer = GL.GenBuffer();
        IndexBuffer = GL.GenBuffer();
        VertexArray = GL.GenVertexArray();
        
        GL.BindVertexArray(VertexArray);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
        int vertexDataSize = vertexSize * VertexCount;
        float[] vertexData = new float[vertexDataSize];
        
        for (int i = 0; i < VertexCount; i++)
        {
            int offset = i * vertexSize;
            vertexData[offset] = vertices[i].Position.X;
            vertexData[offset + 1] = vertices[i].Position.Y;
            vertexData[offset + 2] = vertices[i].Position.Z;
            vertexData[offset + 3] = vertices[i].TexCoords.X;
            vertexData[offset + 4] = vertices[i].TexCoords.Y;
        }
        
        GL.BufferData(BufferTarget.ArrayBuffer, vertexDataSize * sizeof(float), vertexData, BufferUsageHint.StaticDraw);
        
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
        
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize * sizeof(float), 0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, vertexSize * sizeof(float), 3 * sizeof(float));

        GL.EnableVertexAttribArray(0);
        GL.EnableVertexAttribArray(1);
        
        GL.BindVertexArray(0);
        
    }
    
    public void Dispose()
    {
        GL.DeleteBuffer(VertexBuffer);
        GL.DeleteBuffer(IndexBuffer);
        GL.DeleteVertexArray(VertexArray);
    }

    public static Mesh CreateSquare()
    {
        Vertex[] vertices = new Vertex[]
        {
            new Vertex(new Vector3(-1f, -1f, 0.0f), new Vector2(0.0f, 0.0f)),
            new Vertex(new Vector3(1f, -1f, 0.0f), new Vector2(1.0f, 0.0f)),
            new Vertex(new Vector3(1f, 1f, 0.0f), new Vector2(1.0f, 1.0f)),
            new Vertex(new Vector3(-1f, 1f, 0.0f), new Vector2(0.0f, 1.0f))
        };
        
        uint[] indices = new uint[]
        {
            0, 1, 2,
            2, 3, 0
        };
        
        return new Mesh(vertices, indices);
    }

    public void Render()
    {
        GL.BindVertexArray(VertexArray);
        GL.DrawElements(PrimitiveType.Triangles, IndexCount, DrawElementsType.UnsignedInt, 0);
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct Vertex
{
    public Vector3 Position;
    public Vector2 TexCoords;
    
    public Vertex(Vector3 position, Vector2 texCoords)
    {
        Position = position;
        TexCoords = texCoords;
    }
}