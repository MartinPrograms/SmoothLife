#version 450 core

out vec4 color;

in vec2 texCoord;

uniform sampler2D tex;

// Some color manipulation can be done here

uniform float hue;
uniform float saturation;
uniform float brightness;

uniform vec3 blackLevel;
uniform vec3 whiteLevel;

vec3 RGBtoHSV(vec3 color) {
    vec3 hsv;
    float cmax = max(color.r, max(color.g, color.b));
    float cmin = min(color.r, min(color.g, color.b));
    float delta = cmax - cmin;
    if (delta == 0.0) {
        hsv.x = 0.0;
    } else if (cmax == color.r) {
        hsv.x = 60.0 * mod((color.g - color.b) / delta, 6.0);
    } else if (cmax == color.g) {
        hsv.x = 60.0 * ((color.b - color.r) / delta + 2.0);
    } else {
        hsv.x = 60.0 * ((color.r - color.g) / delta + 4.0);
    }
    if (cmax == 0.0) {
        hsv.y = 0.0;
    } else {
        hsv.y = delta / cmax;
    }
    hsv.z = cmax;
    return hsv;
}

vec3 HSVtoRGB(vec3 color) {
    vec3 rgb;
    float c = color.z * color.y;
    float x = c * (1.0 - abs(mod(color.x / 60.0, 2.0) - 1.0));
    float m = color.z - c;
    if (color.x < 60.0) {
        rgb = vec3(c, x, 0.0);
    } else if (color.x < 120.0) {
        rgb = vec3(x, c, 0.0);
    } else if (color.x < 180.0) {
        rgb = vec3(0.0, c, x);
    } else if (color.x < 240.0) {
        rgb = vec3(0.0, x, c);
    } else if (color.x < 300.0) {
        rgb = vec3(x, 0.0, c);
    } else {
        rgb = vec3(c, 0.0, x);
    }
    return rgb + m;
}

void main() {
    // Apply the texture
    vec4 texColor = texture(tex, texCoord);
    
    // Apply the color manipulation
    // First black and white levels
    if (texColor.r < blackLevel.r) {
        texColor.r = blackLevel.r;
    } else if (texColor.r > whiteLevel.r) {
        texColor.r = whiteLevel.r;
    }
    if (texColor.g < blackLevel.g) {
        texColor.g = blackLevel.g;
    } else if (texColor.g > whiteLevel.g) {
        texColor.g = whiteLevel.g;
    }
    if (texColor.b < blackLevel.b) {
        texColor.b = blackLevel.b;
    } else if (texColor.b > whiteLevel.b) {
        texColor.b = whiteLevel.b;
    }
    
    vec3 hsv = RGBtoHSV(texColor.rgb);
    hsv.x += hue;
    hsv.y *= saturation;
    hsv.z *= brightness;
    vec3 rgb = HSVtoRGB(hsv);
    color = vec4(rgb, texColor.a);
}