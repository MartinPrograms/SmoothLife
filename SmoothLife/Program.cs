global using OpenTK.Windowing.GraphicsLibraryFramework;
using ImGuiNET;
using SmoothLife;
using SmoothLife.Graphics;
using Window = SmoothLife.Window;

var window = new Window(800, 800, "SmoothLife");
window.ClearColor = Color4.CornflowerBlue;

Shader? smoothLifeShader = null;
Shader? smoothLifeToTextureShader = null;

Shader? outputShader = null;
Texture? texture = null;
Mesh square = null;

Ssbo<float> slInput = null;
Ssbo<float> slOutput = null;

int slWidth = 2000;
int slHeight = 2000;

System.Numerics.Vector2 pos = new(slWidth/2, slHeight/2);
float zoom = 1.0f;
float targetZoom = 1.0f;
float zoomSmoothness = 5;
bool dragging = false;
float radius = 2;


window.KeyDown += key =>
{
    if (key == Keys.Escape)
    {
        window.Close();
    }
};

window.MouseDown += button =>
{
    if (button == MouseButton.Right)
    {
        dragging = true;
    }
};

window.MouseUp += button =>
{
    if (button == MouseButton.Right)
    {
        dragging = false;
    }
};

window.Scroll += offset =>
{
    targetZoom += offset.Y * 0.1f * targetZoom;
    
    targetZoom = Math.Clamp(targetZoom, 0.1f, 50.0f);
};

window.Load += () =>
{
    // Load the ./Graphics/SmoothLife.glsl compute shader
    smoothLifeShader =
        new Shader(new ShaderStage(SLShaderType.Compute, File.ReadAllText("./Graphics/Shaders/SmoothLife.glsl")));
    smoothLifeToTextureShader = new Shader(new ShaderStage(SLShaderType.Compute,
        File.ReadAllText("./Graphics/Shaders/SmoothLifeToTexture.glsl")));
    
    // Create the input and output buffers
    slInput = new Ssbo<float>(new float[slWidth * slHeight],4, 0);
    slOutput = new Ssbo<float>(new float[slWidth * slHeight],4, 1);

    smoothLifeShader.AddBuffer(slInput);
    smoothLifeShader.AddBuffer(slOutput); // Output buffer
    smoothLifeToTextureShader.AddBuffer(slInput); // Input buffer
    
    // Load the ./Graphics/Shaders/Texture.frag and ./Graphics/Shaders/Texture.vert shaders
    outputShader = new Shader(new ShaderStage(SLShaderType.Vertex, File.ReadAllText("./Graphics/Shaders/Texture.vert")), new ShaderStage(SLShaderType.Fragment, File.ReadAllText("./Graphics/Shaders/Texture.frag")));
    
    // Create a texture with the same dimensions as the window
    texture = new Texture(window.Width, window.Height, new float[window.Width * window.Height * 4]);
    square = Mesh.CreateSquare();
};

window.Resize += size =>
{
    texture!.Resize(size.X, size.Y);
};

void Step()
{
    // Run the SmoothLife shader
    smoothLifeShader!.Use();
    smoothLifeShader.SetInt("width", slWidth);
    smoothLifeShader.SetInt("height", slHeight);
    GL.DispatchCompute(ComputeShaderExtensions.GetNumWorkGroups(slWidth, 16), ComputeShaderExtensions.GetNumWorkGroups(slHeight, 16), 1);
    GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
    
    // We need to copy the output buffer to the texture
    slInput!.From(slOutput!);
}

void DrawState()
{
    // Run the SmoothLifeToTexture shader
    smoothLifeToTextureShader!.Use();
    smoothLifeToTextureShader.SetInt("width", window.Width);
    smoothLifeToTextureShader.SetInt("height", window.Height);
    smoothLifeToTextureShader.SetInt("slWidth", slWidth); // The width of the SmoothLife simulation
    smoothLifeToTextureShader.SetInt("slHeight", slHeight);
    
    // Now for movement stuff, because its boring to just have a static image
    smoothLifeToTextureShader.SetFloat("zoom", zoom);
    smoothLifeToTextureShader.SetVector2("pos", new Vector2(pos.X, pos.Y));
    smoothLifeToTextureShader.SetVector2("mousePos",
        new Vector2(window.MousePosition.X, window.MousePosition.Y * -1 + window.Height));
    smoothLifeToTextureShader.SetFloat("radius", radius);
    smoothLifeToTextureShader.SetBool("drawOnMouse", window.MouseState.IsButtonDown(MouseButton.Left) && !ImGui.IsAnyItemActive());
    smoothLifeToTextureShader.SetBool("shift", window.KeyboardState.IsKeyDown(Keys.LeftShift));
    
    // Bind the texture to the output buffer (1)
    texture!.BindCompute(1, TextureAccess.WriteOnly);
    smoothLifeToTextureShader.SetInt("outputBuffer", 1);
    
    GL.DispatchCompute(ComputeShaderExtensions.GetNumWorkGroups(window.Width, 16), ComputeShaderExtensions.GetNumWorkGroups(window.Height, 16), 1);
    GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit | MemoryBarrierFlags.TextureUpdateBarrierBit);
    
    // Unbind the texture
    texture!.Unbind();
    
    slInput!.Read();
}

float rate = 10; // 10 steps per second
float lastStep = 0;
bool playing = false;

float hue = 0;
float saturation = 0.5f;
float brightness = 1;

System.Numerics.Vector3 blackLevel = new(0, 0, 0);
System.Numerics.Vector3 whiteLevel = new(1, 1, 1);

bool export = false;
int export_index = 0;

window.Render += (dt, ms, ks) =>
{
    lastStep += (float)dt.Time;
    

    DrawState();
    
    if (playing && lastStep >= 1.0f / rate)
    {
        Step();
        lastStep = 0;
        
        if (export)
        {
            // We're exporting the texture to a file
            var data = new float[window.Width * window.Height * 4];
            texture!.Bind(0);
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.Float, data);
            texture!.Unbind();
            
            var bmp = new Bitmap(window.Width, window.Height);
            bmp.SetData(data);
            if (!Directory.Exists("./Exports"))
            {
                Directory.CreateDirectory("./Exports");
            }
            bmp.Save($"./Exports/export{export_index}.png");
            
            export_index++;
        }
    }
    
    // Render the texture
    outputShader!.Use();
    texture!.Bind(0);
    outputShader.SetInt("tex", 0);
    
    /*
     * uniform float hue;
uniform float saturation;
uniform float brightness;

uniform vec3 blackLevel;
uniform vec3 whiteLevel;

     */
    
    outputShader.SetFloat("hue", hue);
    outputShader.SetFloat("saturation", saturation);
    outputShader.SetFloat("brightness", brightness);
    
    outputShader.SetVector3("blackLevel", blackLevel);
    outputShader.SetVector3("whiteLevel", whiteLevel);
    
    square!.Render();
    
    
    ImGui.Begin("SmoothLife");
    ImGui.Text($"FPS: {1 / window.DeltaTime}");
    if (ImGui.Button(playing ? "Pause" : "Play"))
    {
        playing = !playing;
    }
    ImGui.SliderFloat("Rate", ref rate, 1, 60);
    if (ImGui.Button("Step"))
    {
        Step();
    }
    
    if (ImGui.Button("Export"))
    {
        export = true;
    }
    
    ImGui.End();
    
    if (ImGui.Begin("Controls"))
    {
        ImGui.DragFloat2("Position", ref pos, 0.01f);
        ImGui.DragFloat("Zoom", ref zoom, 0.01f);
        ImGui.DragFloat("Radius", ref radius, 0.01f);
    }
    
    ImGui.End();
    
    if (ImGui.Begin("Color"))
    {
        ImGui.DragFloat("Hue", ref hue, 0.01f);
        ImGui.DragFloat("Saturation", ref saturation, 0.01f);
        ImGui.DragFloat("Brightness", ref brightness, 0.01f);
        
        ImGui.DragFloat3("Black Level", ref blackLevel, 0.01f);
        ImGui.DragFloat3("White Level", ref whiteLevel, 0.01f);
    }
    
    ImGui.End();
    
    if (dragging && !ImGui.IsAnyItemActive())
    {
        pos += new System.Numerics.Vector2(-window.MouseDelta.X, window.MouseDelta.Y) / zoom;
    }
    
    zoom = MathHelper.Lerp(zoom, targetZoom, zoomSmoothness * window.DeltaTime);
};

window.Run();