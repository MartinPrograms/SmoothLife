global using OpenTK.Graphics.OpenGL4;

namespace SmoothLife.Graphics;

public class Shader : IDisposable
{
    public int Handle;
    private List<ShaderStage> Stages = new();
    private List<ISsbo> Buffers = new();
    
    public Shader(params ShaderStage[] stages)
    {
        Compile(stages);
    }
    
    public void AddBuffer(ISsbo buffer)
    {
        Buffers.Add(buffer);
    }

    private void Compile(ShaderStage[] stages)
    {
        Handle = GL.CreateProgram();

        foreach (var stage in stages)
        {
            var shader = GL.CreateShader(stage.Type switch
            {
                SLShaderType.Vertex => ShaderType.VertexShader,
                SLShaderType.Fragment => ShaderType.FragmentShader,
                SLShaderType.Compute => ShaderType.ComputeShader,
                _ => throw new ArgumentOutOfRangeException()
            });

            GL.ShaderSource(shader, stage.Source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out var result);
            if (result != 1)
            {
                var info = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({stage.Type}): {info}");
            }

            GL.AttachShader(Handle, shader);
        }

        GL.LinkProgram(Handle);

        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out var code);
        if (code != 1)
        {
            var info = GL.GetProgramInfoLog(Handle);
            throw new Exception($"Error occurred whilst linking Shader: {info}");
        }

        foreach (var stage in stages)
        {
            var shader = GL.CreateShader(stage.Type switch
            {
                SLShaderType.Vertex => ShaderType.VertexShader,
                SLShaderType.Fragment => ShaderType.FragmentShader,
                SLShaderType.Compute => ShaderType.ComputeShader,
                _ => throw new ArgumentOutOfRangeException()
            });

            GL.DetachShader(Handle, shader);
            GL.DeleteShader(shader);
        }
    }

    public void Use()
    {
        GL.UseProgram(Handle);
        
        foreach (var buffer in Buffers)
        {
            buffer.Bind();
        }
    }

    public void SetInt(string name, int data)
    {
        GL.Uniform1(GL.GetUniformLocation(Handle, name), data);
    }

    public void SetFloat(string name, float data)
    {
        GL.Uniform1(GL.GetUniformLocation(Handle, name), data);
    }

    public void SetVector2(string name, Vector2 data)
    {
        GL.Uniform2(GL.GetUniformLocation(Handle, name), data);
    }

    public void SetVector3(string name, Vector3 data)
    {
        GL.Uniform3(GL.GetUniformLocation(Handle, name), data);
    }
    public void SetVector3(string name, System.Numerics.Vector3 data)
    {
        GL.Uniform3(GL.GetUniformLocation(Handle, name), new Vector3(data.X, data.Y, data.Z));
    }

    public void SetVector4(string name, Vector4 data)
    {
        GL.Uniform4(GL.GetUniformLocation(Handle, name), data);
    }

    public void SetMatrix4(string name, Matrix4 data)
    {
        GL.UniformMatrix4(GL.GetUniformLocation(Handle, name), false, ref data);
    }

    private void ReleaseUnmanagedResources()
    {
        GL.DeleteProgram(Handle);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Shader()
    {
        ReleaseUnmanagedResources();
    }
}

public enum SLShaderType
{
    Vertex,
    Fragment,
    Compute
}

public class ShaderStage{
    public SLShaderType Type;
    public string Source;
    
    public ShaderStage(SLShaderType type, string source)
    {
        Type = type;
        Source = source;
    }
}