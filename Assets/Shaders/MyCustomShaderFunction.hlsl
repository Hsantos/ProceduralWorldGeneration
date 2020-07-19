#ifndef MYCUSTOMSHADERFUNCTION_INCLUDED
#define MYCUSTOMSHADERFUNCTION_INCLUDED

void MyFunction_float(
    float heightPercent, 
    float4 color01,
    float4 color02,
    float4 color03,
    float4 color04,
    float baseHeight01,
    float baseHeight02,
    float baseHeight03,
    float baseHeight04,
    float3 inAlbedo,
    out float3 outAlbedo)
{
    const static int baseArrayCount = 4;

    float4 baseColours[baseArrayCount];
    baseColours[0] = color01;
    baseColours[1] = color02;
    baseColours[2] = color03;
    baseColours[3] = color04;

    float baseHeights[baseArrayCount];
    baseHeights[0] = baseHeight01;
    baseHeights[1] = baseHeight02;
    baseHeights[2] = baseHeight03;
    baseHeights[3] = baseHeight04;

    for(int i = 0; i < baseArrayCount; i++)
    {
        float drawStrength = saturate(sign(heightPercent - baseHeights[i]));
        outAlbedo = baseColours[i] * drawStrength;
    }
}

#endif