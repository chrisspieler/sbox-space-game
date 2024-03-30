HEADER
{
	Description = "Template Shader for S&box";
}

FEATURES
{
    #include "common/features.hlsl"
}

COMMON
{
	#include "common/shared.hlsl"
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );
		// Add your vertex manipulation functions here
		return FinalizeVertex( o );
	}
}

//=========================================================================================================================

PS
{
    #include "common/pixel.hlsl"

	float g_FresnelBias < Default( 0 ); Attribute( "g_FresnelBias"); >;
	float g_FresnelScale < Default( 0.5 ); Attribute( "g_FresnelScale"); >;
	float g_FresnelPower < Default( 3 ); Attribute( "g_FresnelPower" ); >;
	float g_FresnelBrightness < Default( 2 ); Attribute( "g_FresnelBrightness" ); >;

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::From( i );
		m.Albedo *= i.vVertexColor.rgb;
		float4 o = ShadingModelStandard::Shade( i, m );
		float3 fDot = dot(g_vCameraDirWs, i.vNormalWs);
		float fresnel = g_FresnelBias + g_FresnelScale * pow(1 + fDot, g_FresnelPower );
		o = lerp( o, saturate( o * g_FresnelBrightness ), saturate( fresnel ) );
		return o;
	}
}
