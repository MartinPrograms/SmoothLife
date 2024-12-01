#version 450 core

// Compute shader for smooth life

layout(local_size_x = 16, local_size_y = 16) in;

// 1 float per pixel
layout(std430, binding = 0) buffer Input {
    float inputBuffer[];
};

layout(rgba32f, binding = 1) uniform image2D outputBuffer;
 
// If value = 1, output white, if value = 0, output black

uniform int width;
uniform int height;

uniform int slWidth; // Width and height of the input buffer
uniform int slHeight;

uniform vec2 pos;
uniform float zoom; // Higher = more zoomed in

uniform vec2 mousePos;
uniform float radius;
uniform bool drawOnMouse;
uniform bool shift;

// msaa samples
const vec2[] samples = {
    vec2(-0.5, -0.5),
    vec2(0.5, -0.5),
    vec2(-0.5, 0.5),
    vec2(0.5, 0.5)
};

void SetPixel(uint x, uint y, float r, float g, float b) {
    imageStore(outputBuffer, ivec2(x, y), vec4(r, g, b, 1.0));
}

vec2 ScreenToGrid(vec2 gridPos) {
    return (gridPos - vec2(width, height) / 2.0) / zoom + pos;
}

vec2 GridToScreen(vec2 screenPos) {
    return (screenPos - pos) * zoom + vec2(width, height) / 2.0;
}

void main() {
    uint x = gl_GlobalInvocationID.x;
    uint y = gl_GlobalInvocationID.y;

    if (x >= width || y >= height) {
        return;
    }

    vec3 result = vec3(0.0, 0.0, 0.0);

    for (int i = 0; i < 4; i++) {
        float value = 0.0;

        float offsetX = (float(x) - float(width) / 2.0) / zoom + pos.x + samples[i].x / zoom;
        float offsetY = (float(y) - float(height) / 2.0) / zoom + pos.y + samples[i].y / zoom;

        // get the value from the input buffer
        if (offsetX >= 0.0 && offsetX < float(slWidth) && offsetY >= 0.0 && offsetY < float(slHeight)) {
            uint index = uint(offsetY) * uint(slWidth) + uint(offsetX);
            value = inputBuffer[index];
        }

        // Now we display the value
        result += vec3(value, value, value);

        // In a 8x8 gridspace circle around the mouse, we set the value on red to 0.2    
        if (distance(ScreenToGrid(mousePos), ScreenToGrid(vec2(x, y))) < radius) {
            result += vec3(0.2, 0.0, 0.0);
        }

        // Draw a grid, in grid space, if zoomed in enough
        if (zoom > 3) {
            vec2 gridPos = ScreenToGrid(vec2(x, y));
            if (mod(gridPos.x, 1.0) < 0.1 || mod(gridPos.y, 1.0) < 0.1) {
                result = vec3(0.2, 0.2, 0.2);
            }
        }
    }
    result /= 4.0;

    SetPixel(x, y, result.r, result.g, result.b);

    if (drawOnMouse){
    if (distance(ScreenToGrid(mousePos), ScreenToGrid(vec2(x, y))) < radius) {
        float offsetX = (float(x) - float(width) / 2.0) / zoom + pos.x;
        float offsetY = (float(y) - float(height) / 2.0) / zoom + pos.y;

        uint index = uint(offsetY) * uint(slWidth) + uint(offsetX);
        inputBuffer[index] = shift ? 0.0 : 1.0;
    }
}
}