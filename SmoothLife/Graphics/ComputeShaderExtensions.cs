namespace SmoothLife.Graphics;

public class ComputeShaderExtensions
{
    public static int GetNumWorkGroups(int numElements, int workGroupSize)
    {
        return (numElements + workGroupSize - 1) / workGroupSize;
    }
}