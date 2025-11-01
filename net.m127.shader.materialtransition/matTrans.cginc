
float gradientPoint(float3 restPos, float3 src, float3 bbmin, float3 bbmax)
{
    float grad = length(restPos - src);
    float ndist = length(src - clamp(src, bbmin, bbmax));
    float mdist = 0;
    [unroll] for (int i = 0; i < 8; i++)
    {
        mdist = max(mdist, length(src - lerp(bbmin, bbmax, float3(i % 2, (i / 2) % 2, (i / 4) % 2))));
    }
    grad -= ndist;
    return grad / (mdist - ndist);
}

float gradientDir(float3 restPos, float3 dir, float3 bbmin, float3 bbmax)
{
    float3 npos = smoothstep(bbmin, bbmax, restPos);
    float grad = dot(lerp(npos, 1 - npos, step(_SourceVector, 0)), abs(_SourceVector));
    return grad / dot(abs(_SourceVector), 1);
}

float distLerp(float grad, float pt, float range)
{
    float sign = 1, offset = 0;
    if(pt > 1) {
        sign = -1;
        offset = 1;
        pt -= 1;
    }    
    grad *= 1 - 2 * range;
    grad += range;
    const float min = pt - range, max = pt + range;
    return sign * smoothstep(min, max, grad) + offset;
}