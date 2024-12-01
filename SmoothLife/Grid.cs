namespace SmoothLife;

public static class Grid
{
    /*
     * vec2 GridToScreen(vec2 gridPos) {
    return (gridPos - vec2(width, height) / 2.0) / zoom + pos;
}

vec2 ScreenToGrid(vec2 screenPos) {
    return (screenPos - pos) * zoom + vec2(width, height) / 2.0;
}
     */
    
    public static Vector2 ScreenToGrid(Vector2 gridPos, Vector2 pos, float zoom, int slWidth, int slHeight, int width, int height)
    {
        var asp = (float)width / height;
        Vector2 zoomed = new Vector2(asp, 1.0f) * zoom;
        
        Vector2 result = (gridPos - new Vector2(slWidth, slHeight) / 2.0f) / zoomed + pos;
        
        result.Y = slHeight - result.Y;
        
        return result;
    }
    
    public static Vector2 GridToScreen(Vector2 screenPos, Vector2 pos, float zoom, int slWidth, int slHeight, int width, int height)
    {
        var asp = (float)width / height;
        Vector2 zoomed = new Vector2(asp, 1.0f) * zoom;
        
        Vector2 result = (screenPos - pos) * zoomed + new Vector2(slWidth, slHeight) / 2.0f;
        
        // Invert the mouse y
        result.Y = slHeight - result.Y;
        
        return result;
    }
    
    public static Vector2 FromNumerics(this System.Numerics.Vector2 vec)
    {
        return new Vector2(vec.X, vec.Y);
    }

    public static int GetIndex(int gridPosX, int gridPosY, int slWidth)
    {
        return gridPosY * slWidth + gridPosX;
    }
}