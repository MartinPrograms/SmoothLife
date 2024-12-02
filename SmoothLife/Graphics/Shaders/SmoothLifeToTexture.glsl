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

vec3 HSVtoRGB(vec3 hsv) {
    float c = hsv.z * hsv.y;
    float x = c * (1.0 - abs(mod(hsv.x * 6.0, 2.0) - 1.0));
    float m = hsv.z - c;

    vec3 rgb = vec3(0.0, 0.0, 0.0);
    if (hsv.x < 1.0) {
        rgb = vec3(c, x, 0.0);
    } else if (hsv.x < 2.0) {
        rgb = vec3(x, c, 0.0);
    } else if (hsv.x < 3.0) {
        rgb = vec3(0.0, c, x);
    } else if (hsv.x < 4.0) {
        rgb = vec3(0.0, x, c);
    } else if (hsv.x < 5.0) {
        rgb = vec3(x, 0.0, c);
    } else {
        rgb = vec3(c, 0.0, x);
    }

    return rgb + vec3(m, m, m);
}

vec3 RGBtoHSV(vec3 rgb) {
    float cmax = max(rgb.r, max(rgb.g, rgb.b));
    float cmin = min(rgb.r, min(rgb.g, rgb.b));
    float delta = cmax - cmin;

    vec3 hsv = vec3(0.0, 0.0, 0.0);
    if (delta == 0.0) {
        hsv.x = 0.0;
    } else if (cmax == rgb.r) {
        hsv.x = mod((rgb.g - rgb.b) / delta, 6.0);
    } else if (cmax == rgb.g) {
        hsv.x = (rgb.b - rgb.r) / delta + 2.0;
    } else {
        hsv.x = (rgb.r - rgb.g) / delta + 4.0;
    }

    hsv.x *= 60.0;
    if (hsv.x < 0.0) {
        hsv.x += 360.0;
    }

    if (cmax == 0.0) {
        hsv.y = 0.0;
    } else {
        hsv.y = delta / cmax;
    }

    hsv.z = cmax;

    return hsv;
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
        vec3 hsv = vec3(0.0, 0.5, log(value + 1.0) / log(2.0));
        hsv.x = 100 * value;
        
        result += HSVtoRGB(hsv);

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