global using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SmoothLife.Graphics;

namespace SmoothLife;

public class Window
{
    private GameWindow window;
    public Color4 ClearColor = Color4.Black;
    private ImGuiController imguiController;
    
    public Window(int width, int height, string title)
    {
        window = new GameWindow(new GameWindowSettings
        {
            UpdateFrequency = 144, 
        }, new NativeWindowSettings
        {
            ClientSize = new Vector2i(width, height),
            Title = title
        });
        
        window.Title = title;
        
        window.KeyDown +=(sender) =>
        {
            KeyDown?.Invoke(sender.Key);
        };
        window.KeyUp +=(sender) => KeyUp?.Invoke(sender.Key);
        window.MouseDown +=(sender) => MouseDown?.Invoke(sender.Button);
        window.MouseUp +=(sender) => MouseUp?.Invoke(sender.Button);
        window.MouseMove +=(sender) => MouseMove?.Invoke(sender.Position);
        window.MouseWheel += (sender) =>
        {
            Scroll?.Invoke(sender.Offset);
        };
        
        window.RenderFrame += (dt) =>
        {
            MousePosition = window.MousePosition;
            GL.Viewport(0, 0, window.FramebufferSize.X, window.FramebufferSize.Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(ClearColor);
            
            imguiController.Update(window, (float)dt.Time);
            
            Render?.Invoke(dt, window.MouseState, window.KeyboardState);
          
            Time += (float)dt.Time;
            
            imguiController.Render();
            
            window.SwapBuffers();
            
            _prevMousePosition = MousePosition;
        };
        
        window.Load += () =>
        {
            imguiController = new ImGuiController(Width, Height);
            
            Load?.Invoke();
        };
        
        window.Resize += (size) =>
        {
            imguiController.WindowResized(size.Width, size.Height);
            Resize?.Invoke(size.Size);
        };
    }
    
    public Action<Keys> KeyDown;
    public Action<Keys> KeyUp;
    public Action<MouseButton> MouseDown;
    public Action<MouseButton> MouseUp;
    public Action<Vector2> MouseMove;
    public Action<Vector2> Scroll;
    
    public Action<FrameEventArgs,MouseState, KeyboardState> Render;
    public Action Load;

    public Action<Vector2i> Resize;

    public float DeltaTime => (float)window.UpdateTime;
    public float Time { get; private set; }
    
    public int Width => window.FramebufferSize.X;
    public int Height => window.FramebufferSize.Y;
    public Vector2 MousePosition;
    private Vector2 _prevMousePosition;
    public Vector2 MouseDelta => MousePosition - _prevMousePosition;

    public void Run()
    {
        window.Run();
    }

    public void Close()
    {
        window.Close();
    }
}