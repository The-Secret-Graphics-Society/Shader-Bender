// MIT License. © Stefan Gustavson, Munrocket
float fade(float t) {
    return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
}

// Hash function to generate a pseudo-random gradient index from a 2D cell
// On generating random numbers, with help of y= [(a+x)sin(bx)] mod 1", W.J.J. Rey, 22nd European Meeting of Statisticians 1998
float hash(float2 p) {
    float n = sin(dot(p, float2(127.1, 311.7)));
    return frac((n * 43758.5453));
}

// Gradient function: picks a gradient direction based on a hashed int
float2 grad2(float2 p) {
    float rnd = hash(p) * 4.0;
    float2 g;
    if (rnd < 1.0) g = float2(1.0, 0.0);
    else if (rnd < 2.0) g = float2(-1.0, 0.0);
    else if (rnd < 3.0) g = float2(0.0, 1.0);
    else g = float2(0.0, -1.0);
    return g;
}

// 2D Perlin-like Noise Function
float perlinNoise2D(float2 uv) {
    // Cell coordinates
    float2 i = floor(uv);
    float2 f = frac(uv);

    // Four corners of the cell
    float2 c00 = i + float2(0.0, 0.0);
    float2 c10 = i + float2(1.0, 0.0);
    float2 c01 = i + float2(0.0, 1.0);
    float2 c11 = i + float2(1.0, 1.0);

    // Gradient vectors for each corner
    float2 g00 = grad2(c00);
    float2 g10 = grad2(c10);
    float2 g01 = grad2(c01);
    float2 g11 = grad2(c11);

    // Compute dot products between gradient vectors and offset vectors
    float n00 = dot(g00, f - float2(0.0, 0.0));
    float n10 = dot(g10, f - float2(1.0, 0.0));
    float n01 = dot(g01, f - float2(0.0, 1.0));
    float n11 = dot(g11, f - float2(1.0, 1.0));

    // Smooth interpolation
    float u = fade(f.x);
    float v = fade(f.y);

    // Interpolate on X for each row, then interpolate those results on Y
    float nx0 = lerp(n00, n10, u);
    float nx1 = lerp(n01, n11, u);
    float nxy = lerp(nx0, nx1, v);

    // nxy is typically in [-1,1], scale and bias to [0,1]
    return nxy * 0.5 + 0.5;
}

// Attempting multiple octaves of noise
float cloudNoise(float2 uv) {
    float value = 0.0;
    float amplitude = 1.0;
    float frequency = 1.0;
    for (int i = 0; i < 4; i++) {
        value += perlinNoise2D(uv * frequency) * amplitude;
        frequency *= 2.0;
        amplitude *= 0.5;
    }
    return saturate(value);
}
