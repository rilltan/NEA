﻿using System;
using System.IO;
using static OpenGL.GL;

internal unsafe class Shader
{
    public uint ID;
    public Shader(string vertexPath, string shaderPath)
    {
        Console.WriteLine(vertexPath + ", " + shaderPath + ":");
        string vertexCode = "";
        string fragmentCode = "";
        try
        {
            vertexCode = File.ReadAllText(vertexPath);
            fragmentCode = File.ReadAllText(shaderPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        uint vertex = glCreateShader(GL_VERTEX_SHADER);
        int success = 0;
        glShaderSource(vertex, vertexCode);
        glCompileShader(vertex);
        glGetShaderiv(vertex, GL_COMPILE_STATUS, & success);
        if (success == GL_TRUE) Console.WriteLine("vertex shader success");
        else { Console.WriteLine("vertex shader failure"); Console.WriteLine(glGetShaderInfoLog(vertex)); }

        uint fragment = glCreateShader(GL_FRAGMENT_SHADER);
        success = 0;
        glShaderSource(fragment, fragmentCode);
        glCompileShader(fragment);
        glGetShaderiv(fragment, GL_COMPILE_STATUS, &success);
        if (success == GL_TRUE) Console.WriteLine("fragment shader success");
        else { Console.WriteLine("fragment shader failure"); Console.WriteLine(glGetShaderInfoLog(fragment)); }

        ID = glCreateProgram();
        success = 0;
        glAttachShader(ID, vertex);
        glAttachShader(ID, fragment);
        glLinkProgram(ID);
        glGetProgramiv(ID, GL_LINK_STATUS, &success);
        if (success == GL_TRUE) Console.WriteLine("shader program success");
        else { Console.WriteLine("shader program failure"); Console.WriteLine(glGetProgramInfoLog(ID)); }

        Console.WriteLine();

        glDeleteShader(vertex);
        glDeleteShader(fragment);
    }
    public void Use()
    {
        glUseProgram(ID);
    }
    public void SetBool(string name, bool value)
    {
        int val = 0;
        if (value) val = 1;
        glUniform1i(glGetUniformLocation(ID, name), val);
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