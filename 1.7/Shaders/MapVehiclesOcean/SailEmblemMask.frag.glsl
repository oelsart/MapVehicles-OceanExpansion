#version 450

layout(location = 1) in vec2 vUv;

layout(location = 0) out vec4 outColor;

layout(set = 2, binding = 1) uniform sampler2D _MainTex;

vec2 flipV(vec2 c) { return vec2(c.x, 1.0 - c.y); }

void main()
{
    if (texture(_MainTex, flipV(vUv)).r > 0.5)
        discard;
    outColor = vec4(0.0);
}
