using System;
using static OpenGL.GL;

internal unsafe class Shader
{
    public uint ID;
    public Shader(string vertexCode, string fragmentCode)
    {
        uint vertex = glCreateShader(GL_VERTEX_SHADER);
        int success = 0;
        glShaderSource(vertex, vertexCode);
        glCompileShader(vertex);
        glGetShaderiv(vertex, GL_COMPILE_STATUS, & success);
        if (success == GL_FALSE)
        {
            Console.WriteLine("Vertex shader failure:");
            Console.WriteLine(glGetShaderInfoLog(vertex));
        }

        uint fragment = glCreateShader(GL_FRAGMENT_SHADER);
        success = 0;
        glShaderSource(fragment, fragmentCode);
        glCompileShader(fragment);
        glGetShaderiv(fragment, GL_COMPILE_STATUS, &success);
        if (success == GL_FALSE)
        {
            Console.WriteLine("Fragment shader failure:");
            Console.WriteLine(glGetShaderInfoLog(fragment));
        }

        ID = glCreateProgram();
        success = 0;
        glAttachShader(ID, vertex);
        glAttachShader(ID, fragment);
        glLinkProgram(ID);
        glGetProgramiv(ID, GL_LINK_STATUS, &success);
        if (success == GL_FALSE)
        {
            Console.WriteLine("Shader program failure:");
            Console.WriteLine(glGetShaderInfoLog(ID));
        }

        glDeleteShader(vertex);
        glDeleteShader(fragment);
    }
    public void Use()
    {
        glUseProgram(ID);
    }
    public void SetInt(string name, int value)
    {
        glUniform1i(glGetUniformLocation(ID, name), value);
    }
    public void SetFloat(string name, float value)
    {
        glUniform1f(glGetUniformLocation(ID, name), value);
    }
    public void SetVector3(string name, vec3 vector)
    {
        glUniform3f(glGetUniformLocation(ID, name), vector[0], vector[1], vector[2]);
    }
    public void SetVector4(string name, vec4 vector)
    {
        glUniform4f(glGetUniformLocation(ID, name), vector[0], vector[1], vector[2], vector[3]);
    }
    public void SetMatrix4(string name, mat4 matrix)
    {
        glUniformMatrix4fv(glGetUniformLocation(ID, name), 1, true, matrix.GetPointer());
    }
}