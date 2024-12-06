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

float sigmoid(float x) {
    return 1.0 / (1.0 + exp(-x));
}

uniform float smoothness; // 50 default.
uniform float threshold_u0_1; // 0.50 default.
uniform float threshold_u0_2; // 0.25 default.
uniform float threshold_u1; // 0.5 default.
uniform float threshold_u0_3; // 0.43 default.
uniform float threshold_u0_4; // 0.26 default.

float upper(float u0, float u1){
    float smooth_u0_1 = sigmoid((u0 - threshold_u0_2) * smoothness);
    float smooth_u0_2 = sigmoid((threshold_u0_1 - u0) * smoothness);
    float smooth_u1 = sigmoid((u1 - threshold_u1) * smoothness);
    return smooth_u1 * smooth_u0_1 * smooth_u0_2;
}

float lower(float u0, float u1){
    float smooth_u0_3 = sigmoid((u0 - threshold_u0_4) * smoothness);
    float smooth_u0_4 = sigmoid((threshold_u0_3 - u0) * smoothness);
    u1 = 1.0 - u1;
    float smooth_u1 = sigmoid((u1 - threshold_u1) * smoothness);
    return smooth_u1 * smooth_u0_3 * smooth_u0_4;
}

float growth(float u0, float u1) {
    float outputval = upper(u0, u1);
    outputval += lower(u0, u1);
    return outputval;
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
            
            // Update u0
            u0 += weight * valueAtPosition;
        }
    }
    
    for (int i = 0; i < internalKernelRadius; i++){ // X
        for (int j = 0; j < internalKernelRadius; j++){
            // Get the weight at this position
            float weight = getWeight(i, j);
            
            // Get the value at this position
            float valueAtPosition = 0.0;
            int offsetX = int(x) - internalKernelRadius / 2 + i;
            int offsetY = int(y) - internalKernelRadius / 2 + j;
            if (offsetX >= 0 && offsetX < width && offsetY >= 0 && offsetY < height){
                valueAtPosition = inputBuffer[offsetY * width + offsetX];
            }
            else{
                // wrap around
                offsetX = (offsetX + width) % width;
                offsetY = (offsetY + height) % height;
                
                valueAtPosition = inputBuffer[offsetY * width + offsetX];
            }
            
            // Update u1
            u1 += weight * valueAtPosition;
        }
    }
    
    // Normalize u0 and u1
    u0 /= squaredRadiusF;
    u1 /= internalKernelRadiusF;
    
    
    // Compute the growth function
    float g = growth(u0, u1);
    
    // Update the output buffer
    outputBuffer[index] = g;
}
