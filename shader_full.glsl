#version 330

uniform vec4 axisRange; // (xLo, xHi, yLo, yHi)

in  vec2 fragTexCoord;
in  vec4 fragColor;
out vec4 finalColor;

float plot(float x, float y)
{
    float z = y - x;
    z = y - sin(x);
    return z;
}

void main()
{
    vec2 p = fragTexCoord;

    float x = axisRange.x + (axisRange.y - axisRange.x) * p.x;
    float y = axisRange.z + (axisRange.w - axisRange.z) * (1.0 - p.y);

    // get uv deltas for neighbouring pixels
    float dx = dFdx(x);
    float dy = dFdy(y);

    float z = plot(x,y);
    vec2  z_lo = vec2(plot(x - dx, y), plot(x, y - dy));
    vec2  z_hi = vec2(plot(x + dx, y), plot(x, y + dy));

    vec2 z_delta = 0.5 * (z_hi - z_lo);
    float dist = abs(z / length(z_delta));

    float alpha = clamp(2.0 - dist, 0.0, 1.0);

    finalColor = vec4(1.0, 0.0, 0.0, alpha);
}
