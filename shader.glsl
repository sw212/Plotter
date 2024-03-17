#version 330

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
    vec2 p = fragTexCoord - 0.5;
    p *= 5.0;

    float x = p.x;
    float y = -p.y;
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
