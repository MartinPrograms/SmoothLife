#version 450 core
#define PI 3.14159265359

layout(local_size_x = 16, local_size_y = 16) in;

layout(std430, binding = 0) buffer Input {
    float inputBuffer[];
};

layout(std430, binding = 1) buffer Output {
    float outputBuffer[];
};

uniform int width;
uniform int height;

// Kernel radius for SmoothLife
const int kernelRadius = 16; // Adjust as needed, circular kernel
const float kernelRadiusF = float(kernelRadius);

// Because its a circular kernel, we need to compute the number of pixels
const int kernelNumPixels = (2 * kernelRadius + 1) * (2 * kernelRadius + 1);

const int internalRadius = 3; // 3x3 kernel
const float internalRadiusF = float(internalRadius);
const int internalNumPixels = (2 * internalRadius + 1) * (2 * internalRadius + 1);

float fastSigmoid(float x, float x0, float sigma) {
    float s = (x - x0) / sigma;
    return 1.0 / (1.0 + exp(-s));
}

float growth(float u0, float u1) {
    float s_u1 = fastSigmoid(u1, 0.25, 0.03);
    float t1 = fastSigmoid(u0, 0.238, 0.03) * (1.0 - fastSigmoid(u0, 0.44, 0.03));
    float t2 = fastSigmoid(u0, 0.26, 0.03) * (1.0 - fastSigmoid(u0, 0.9, 0.03));
    return s_u1 * t1 + (1.0 - s_u1) * t2;
}
void main() {
    uint x = gl_GlobalInvocationID.x;
    uint y = gl_GlobalInvocationID.y;

    if (x >= width || y >= height) {
        return;
    }

    uint index = y * width + x;
    float value = inputBuffer[index];
    
    // Compute SmoothLife
    
    // First calculate u0 and u1
    float u0 = 0.0;
    float u1 = 0.0;
    
    // The kernel is circular
    for (int i = -kernelRadius; i <= kernelRadius; i++) {
        for (int j = -kernelRadius; j <= kernelRadius; j++) {
            float dist = sqrt(float(i * i + j * j));
            
            if (dist <= kernelRadiusF) {
                int xIndex = int(x) + i;
                int yIndex = int(y) + j;
                
                // If within the internal radius, compute u1
                if (dist <= internalRadiusF) {
                    if (xIndex >= 0 && xIndex < width && yIndex >= 0 && yIndex < height) {
                        uint index = uint(yIndex) * uint(width) + uint(xIndex);
                        float value = inputBuffer[index];
                        
                        u1 += value;
                    }
                }
                
                if (xIndex >= 0 && xIndex < width && yIndex >= 0 && yIndex < height) {
                    uint index = uint(yIndex) * uint(width) + uint(xIndex);
                    float value = inputBuffer[index];
                    
                    u0 += value;
                }
            }
        }
    }
    
    // Normalize u0 and u1
    u0 /= kernelNumPixels;
    u1 /= internalNumPixels;
    
    
    // Compute the growth function
    float g = growth(u0, u1);
    
    // Update the output buffer
    outputBuffer[index] = g;
}
