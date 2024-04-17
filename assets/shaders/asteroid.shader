HEADER
{
	Description = "Asteroid Shader for Aluminum Acrobat";
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
	float g_FresnelScale < Default( 0.2 ); Attribute( "g_FresnelScale"); >;
	float g_FresnelPower < Default( 2 ); Attribute( "g_FresnelPower" ); >;
	float g_FresnelBrightness < Default( 1.5 ); Attribute( "g_FresnelBrightness" ); >;

	float Fresnel( PixelInput i, float bias, float scale, float power )
	{
		float3 fDot = dot(g_vCameraDirWs, i.vNormalWs);
		return bias + scale * pow(1 + fDot, power );
	}

	float4 AddHighlight( PixelInput i, float4 c )
	{
		float fresnel = Fresnel( i, g_FresnelBias, g_FresnelScale, g_FresnelPower );
		// Highlight fades with distance from camera.
		float depth = ( 1 - Depth::GetNormalized( i.vPositionSs ) ) * 1.5;
		float4 highlight = float4( 1, 1, 0.8, 0 ) * g_FresnelBrightness;
		// Get the fully highlighted color...
		float4 o = lerp( c, highlight, depth );
		// ...and only show it if we're on the rim.
		return lerp( c, o, saturate(fresnel) );
	}

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::From( i );
		// Apply tint color
		m.Albedo *= i.vVertexColor.rgb;
		float4 o = ShadingModelStandard::Shade( i, m );
		o = AddHighlight( i, o );
		return o;
	}
}