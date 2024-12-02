#version 450 core
#define PI 3.14159265359

layout(local_size_x = 32, local_size_y = 32) in;

layout(std430, binding = 0) buffer Input {
    float inputBuffer[];
};

layout(std430, binding = 1) buffer Output {
    float outputBuffer[];
};

layout(rgba32f, binding = 2) uniform image2D weightsBuffer;

// There is only one (1) weight. It is of size kernelRadius * kernelRadius. The weights are stored in a 1D array, so we can use the formula weights[y * kernelRadius + x] to access the weight at (x, y).
uniform int kernelRadius;
uniform float kernelRadiusF;
uniform float squaredRadiusF;

uniform int internalKernelRadius;
uniform float internalKernelRadiusF;
uniform float squaredInternalKernelRadiusF;


float getWeight(int x, int y) {
    return imageLoad(weightsBuffer, ivec2(x, y)).r;
}

uniform int width;
uniform int height;

float fastSigmoid(float x, float x0, float sigma) {
    float s = (x - x0) / sigma;
    return 1.0 / (1.0 + exp(-s));
}

uniform float sig; // 0.03 by default
uniform float s_u1u; // 0.25 by default
uniform float t1au; // 0.238 by default
uniform float t1bu; // 0.44 by default
uniform float t2au; // 0.26 by default
uniform float t2bu; // 0.9 by default

float growth(float u0, float u1) {
    float s = fastSigmoid(u0, 0.5, sig);
    float t1 = t1au * u0 + t1bu;
    float t2 = t2au * u0 + t2bu;
    return s * (1.0 - s) * (s_u1u * u1 + (1.0 - s_u1u) * (t1 - t2));
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
    
    // The kernel is a 2D texture, with a value corresponding to the weight at that position [0, 1]
    // The size of this texture is kernelRadius and kernelRadius
    for (int i = 0; i < kernelRadius; i++){ // X
        for (int j = 0; j < kernelRadius; j++){
            // Get the weight at this position
            float weight = getWeight(i, j);
            
            // Get the value at this position
            float valueAtPosition = 0.0;
            int offsetX = int(x) - kernelRadius / 2 + i;
            int offsetY = int(y) - kernelRadius / 2 + j;
            if (offsetX >= 0 && offsetX < width && offsetY >= 0 && offsetY < height){
                valueAtPosition = inputBuffer[offsetY * width + offsetX];
            }
            else{
                // wrap around
                offsetX = (offsetX + width) % width;
                offsetY = (offsetY + height) % height;
                
                valueAtPosition = inputBuffer[offsetY * width + offsetX];
            }
            
            // Update u0 and u1
            u0 += weight * valueAtPosition;
            u1 += weight * valueAtPosition * valueAtPosition;
        }
    }
    
    // Normalize u0 and u1
    u0 /= squaredRadiusF;
    u1 /= squaredInternalKernelRadiusF;
    
    
    // Compute the growth function
    float g = growth(u0, u1);
    
    // Update the output buffer
    outputBuffer[index] = g;
}
