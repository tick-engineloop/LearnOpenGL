#version 420 core

// shader outputs
layout (location = 0) out vec4 frag;

// color accumulation buffer
layout (binding = 0) uniform sampler2D accum;

// revealage threshold buffer
layout (binding = 1) uniform sampler2D reveal;

// epsilon number
const float EPSILON = 0.00001f;

// calculate floating point numbers equality accurately
bool isApproximatelyEqual(float a, float b)
{
	return abs(a - b) <= (abs(a) < abs(b) ? abs(b) : abs(a)) * EPSILON;
}

// get the max value between three values
float max3(vec3 v) 
{
	return max(max(v.x, v.y), v.z);
}

void main()
{
	// fragment coordination
	ivec2 coords = ivec2(gl_FragCoord.xy);
	
	// fragment revealage
	float revealage = texelFetch(reveal, coords, 0).r;
	
	// save the blending and color texture fetch cost if there is not a transparent fragment
	if (isApproximatelyEqual(revealage, 1.0f)) 
		discard;
 
	// fragment color
	vec4 accumulation = texelFetch(accum, coords, 0);
	
	// suppress overflow
	if (isinf(max3(abs(accumulation.rgb)))) 
		accumulation.rgb = vec3(accumulation.a);

	// prevent floating point precision bug
    // 假设一个片段被 3 个半透明物体覆盖，第一个物体用OC1(r,g,b,a,w)表示，第二个物体用OC2(r,g,b,a,w)表示，
    // 第三个物体用OC3(r,g,b,a,w)表示，其中 w 表示透明通道中计算的权重，那么有：
    // accumulation.rgb = OC1.rgb*OC1.a*OC1.w + OC2.rgb*OC2.a*OC2.w + OC3.rgb*OC3.a*OC3.w
    // accumulation.a = OC1.a*OC1.w + OC2.a*OC2.w + OC3.a*OC3.w
	vec3 average_color = accumulation.rgb / max(accumulation.a, EPSILON);

	// blend pixels
	frag = vec4(average_color, 1.0f - revealage);   // 一个片段上覆盖的半透明物体越多，revealage 值越小，1.0f - revealage 越大，越不透明
}