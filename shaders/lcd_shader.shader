//=========================================================================================================================
// Optional
//=========================================================================================================================
HEADER
{
    CompileTargets = ( IS_SM_50 && ( PC || VULKAN ) );
    Description = "LCD Screen Shader";
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
FEATURES
{
    #include "common/features.hlsl"
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
MODES
{
    VrForward();
    Depth( "vr_depth_only.vfx" );
    ToolsVis( S_MODE_TOOLS_VIS );
    ToolsWireframe( "vr_tools_wireframe.vfx" );
    ToolsShadingComplexity( "vr_tools_shading_complexity.vfx" );
}

//=========================================================================================================================

COMMON
{
    #include "common/shared.hlsl"
}

//=========================================================================================================================

struct VertexInput
{
    #include "common/vertexinput.hlsl"
};

//=========================================================================================================================

struct PixelInput
{
    #include "common/pixelinput.hlsl"
};

//=========================================================================================================================

VS
{
    #include "common/vertex.hlsl"
    //
    // Main
    // 	
    PixelInput MainVs( INSTANCED_SHADER_PARAMS( VS_INPUT i ) )
    {
        PixelInput o = ProcessVertex( i );
        return FinalizeVertex( o );
    }
}

//=========================================================================================================================

PS
{
    #define CUSTOM_TEXTURE_FILTERING
    SamplerState TextureFiltering < Filter( POINT ); AddressU( REPEAT ); AddressV( REPEAT ); AddressW( REPEAT ); MaxAniso( 8 ); >;

    #include "common/pixel.hlsl"

    //=====================================================================================================================
    // Properties
    //=====================================================================================================================

    //
    // Appearance
    //
    float g_flBrightnessMultiplier< UiGroup( "LCD Screen/Appearance" ); UiType( Slider ); Default( 8.0f ); Range( 0.1f, 16.0f ); >;
    FloatAttribute( g_flBrightnessMultiplier, true );

    float2 g_vScreenResolution< UiGroup( "LCD Screen/Appearance" ); UiType( Slider ); Default2( 256.0f, 256.0f ); Range2( 0.0f, 0.0f, 1024.0f, 1024.0f ); >;
    Float2Attribute( g_vScreenResolution, true );

    //
    // Subpixels
    //
    CreateInputTexture2D( PixelTexture, Srgb, 8, "", "_color", "LCD Screen/Subpixels", Default3( 1.0, 1.0, 1.0 ) );
    CreateTexture2DWithoutSampler( g_tPixel ) < Channel( RGB, Box( PixelTexture ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); >;

    float2 g_vPixelSize< UiGroup( "LCD Screen/Subpixels" ); UiType( Slider ); Default2( 1.0f, 1.0f ); >;
    Float2Attribute( g_vPixelSize, true );

    float g_flPixelOpacityMin< UiGroup( "LCD Screen/Subpixels" ); UiType( Slider ); Default( 0.1f ); Range( 0.0f, 1.0f ); >;
    FloatAttribute( g_flPixelOpacityMin, true );

    float g_flPixelOpacityMax< UiGroup( "LCD Screen/Subpixels" ); UiType( Slider ); Default( 1.0f ); Range( 0.0f, 1.0f ); >;
    FloatAttribute( g_flPixelOpacityMax, true );

    float g_flPixelBlendOffset< UiGroup( "LCD Screen/Subpixels" ); UiType( Slider ); Default( 0.0f ); Range( -32, 32.0f ); >;
    FloatAttribute( g_flPixelBlendOffset, true );

    float g_flPixelBlendDistance< UiGroup( "LCD Screen/Subpixels" ); UiType( Slider ); Default( 8.0f ); Range( 4.0f, 1024.0f ); >;
    FloatAttribute( g_flPixelBlendDistance, true );

    //
    // Viewing Angles
    //
    float g_flFresnelReflectance< UiGroup( "LCD Screen/Viewing Angles" ); UiType( Slider ); Default( 0.01f ); Range( 0.0f, 1.0f ); >;
    FloatAttribute( g_flFresnelReflectance, true );

    float g_flFresnelExponent< UiGroup( "LCD Screen/Viewing Angles" ); UiType( Slider ); Default( 5.0f ); Range( 0.0f, 10.0f ); >;
    FloatAttribute( g_flFresnelExponent, true );

    //
    // animation
    //
    float2 g_vAnimationScroll< UiGroup( "LCD Screen/Animation" ); UiType( Slider ); Default2( 0.0f, 0.0f ); Range2( -1.0f, -1.0f, 1.0f, 1.0f ); >;
    Float2Attribute( g_vAnimationScroll, true );

    //=====================================================================================================================

    float4 MainPs( PixelInput i ) : SV_Target0
    {
        Material m = Material::From( i );
        
        //
        // UVs
        //
        float2 vSnappedUvs = round( i.vTextureCoords.xy * g_vScreenResolution.xy + 0.5 ) / g_vScreenResolution.xy;
        vSnappedUvs += g_vAnimationScroll * g_flTime;
        
        float2 vLcdUvs = i.vTextureCoords.xy * g_vScreenResolution.xy / g_vPixelSize.xy;

        //
        // Sample textures
        //
        float4 vAlbedoSample = Tex2DS( g_tColor, TextureFiltering, vSnappedUvs );
        float4 vLcdSample = Tex2DS( g_tPixel, TextureFiltering, vLcdUvs );

        // Make the individual subpixels match the albedo sample
        vLcdSample *= vAlbedoSample;

        //
        // Invert using fresnel
        //
        float4 inverse = 1.0 - vAlbedoSample;
        // Increase saturation
        inverse = pow( inverse, 4 );
        // Decrease strength
        inverse = lerp( vAlbedoSample, inverse, 0.25 );
        inverse.a = 1.0;

        float3 vPositionWs = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
        float fresnel = CalculateNormalizedFresnel( g_flFresnelReflectance, g_flFresnelExponent, vPositionWs, i.vNormalWs );
        vAlbedoSample = lerp( vAlbedoSample, inverse, fresnel );
        
        //
        // LCD subpixel blending
        //
        float fBlendAmt = ddx( vLcdUvs.x ) + ddy( vLcdUvs.y );
        fBlendAmt *= g_flPixelBlendDistance;
        fBlendAmt = 1.0f - ( fBlendAmt - g_flPixelBlendOffset );
        fBlendAmt = clamp( fBlendAmt, g_flPixelOpacityMin, g_flPixelOpacityMax );

        //
        // Output
        //
        float4 o = lerp( vAlbedoSample, vLcdSample, fBlendAmt );
        o *= g_flBrightnessMultiplier;

        return o;
    }
}
