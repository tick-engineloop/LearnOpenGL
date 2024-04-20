#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;

out vec3 FragPos;
out vec2 TexCoords;
out vec3 Normal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    // 应用平移、旋转、缩放，并将顶点位置从局部空间变换到世界空间
    vec4 worldPos = model * vec4(aPos, 1.0);
    FragPos = worldPos.xyz; 
    
    TexCoords = aTexCoords;
    
    // 把法线从模型空间变换到世界空间
    // 旋转、非均匀缩放等空间变换会导致顶点与对应的法线不再匹配，
    // 需要对法线应用法线变换矩阵，以匹配上面的空间变换操作
    mat3 normalMatrix = transpose(inverse(mat3(model)));
    Normal = normalMatrix * aNormal;

    gl_Position = projection * view * worldPos;
}