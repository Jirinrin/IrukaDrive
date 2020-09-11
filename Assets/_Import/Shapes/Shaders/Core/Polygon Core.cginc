// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
UNITY_DEFINE_INSTANCED_PROP( float4, _Color)
UNITY_DEFINE_INSTANCED_PROP( float4, _ColorEnd)
UNITY_DEFINE_INSTANCED_PROP( int, _FillType)
UNITY_DEFINE_INSTANCED_PROP( int, _FillSpace)
UNITY_DEFINE_INSTANCED_PROP( float4, _FillStart) // xyz = pos, w = radius
UNITY_DEFINE_INSTANCED_PROP( float3, _FillEnd) // xyz = pos
UNITY_INSTANCING_BUFFER_END(Props)

struct VertexInput {
    float4 vertex : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
    float4 pos : SV_POSITION;
    float3 fillCoords : TEXCOORD0; // relative to start
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

float3 GetFillCoords( float3 localPos ){
	int fillType = UNITY_ACCESS_INSTANCED_PROP(Props, _FillType);
    if( fillType != FILL_TYPE_NONE ){
        // need coords
        int fillSpace = UNITY_ACCESS_INSTANCED_PROP(Props, _FillSpace);
        float3 absoluteCoord = fillSpace == FILL_SPACE_LOCAL ? localPos : LocalToWorldPos( localPos );
        float4 start = UNITY_ACCESS_INSTANCED_PROP(Props, _FillStart);
        float3 relativeCoord = absoluteCoord - start.xyz;
        
        if( fillType == FILL_TYPE_RADIAL ){
            // has to send full coordinates
            return relativeCoord; 
        } else {
            // linear needs only the interpolator
            float3 end = UNITY_ACCESS_INSTANCED_PROP(Props, _FillEnd); // todo: do distance in vert shader?
			half3 gradVec = end - start.xyz;
			half t = dot(gradVec, relativeCoord ) / dot(gradVec, gradVec);
            return float3( t, 0, 0 );
        }
    }
    return float3(0,0,0);
}

half GetGradientT( float3 coords, int fillType ){
	float t = 0;
	switch( fillType ){
		case FILL_TYPE_LINEAR:
			t = saturate(coords.x); // interpolation is done in the vertex shader so shrug~
			break;
		case FILL_TYPE_RADIAL:
			float4 start = UNITY_ACCESS_INSTANCED_PROP(Props, _FillStart);
			half radius = start.w;
			t = saturate( length( coords ) / radius ); // start.w = radius
			break;
	}
	return t;
}

half4 GetColor( VertexOutput i ){
	half4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
	int fillType = UNITY_ACCESS_INSTANCED_PROP(Props, _FillType);
	if( fillType == FILL_TYPE_NONE ){
		return color;
	} else {
		half4 colorEnd = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorEnd);
		half t = GetGradientT( i.fillCoords, fillType );
		return lerp( color, colorEnd, t );
	}
}

VertexOutput vert (VertexInput v) {
    UNITY_SETUP_INSTANCE_ID(v);
    VertexOutput o = (VertexOutput)0;
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    o.pos = UnityObjectToClipPos( v.vertex );
    o.fillCoords = GetFillCoords( v.vertex.xyz );
    return o;
}

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
    UNITY_SETUP_INSTANCE_ID(i);
    return ShapesOutput( GetColor( i ), 1 );
}