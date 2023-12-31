﻿using System;

internal static class MathsOperations
{
    public static mat4 Translate(mat4 matrix, vec3 translationVector)
    {
        mat4 result = new mat4(1f);
        result[3] = translationVector[0];
        result[7] = translationVector[1];
        result[11] = translationVector[2];
        return result * matrix;
    }
    public static mat4 Scale(mat4 matrix, vec3 scaleVector)
    {
        mat4 result = new mat4(1f);
        result[0] = scaleVector[0];
        result[5] = scaleVector[1];
        result[10] = scaleVector[2];
        return result * matrix;
    }
    public static mat4 Rotate(mat4 matrix, float angle, vec3 rotationAxis)
    {
        mat4 result = new mat4();
        rotationAxis = Normalize(rotationAxis);
        float x = rotationAxis[0];
        float y = rotationAxis[1];
        float z = rotationAxis[2];
        float ca = (float)Math.Cos(angle);
        float cb = 1 - ca;
        float s = (float)Math.Sin(angle);

        result[0] = x * x * cb + ca;
        result[1] = x * y * cb - z * s;
        result[2] = x * z * cb + y * s;
        result[3] = 0;
        result[4] = y * x * cb + z * s;
        result[5] = y * y * cb + ca;
        result[6] = y * z * cb - x * s;
        result[7] = 0;
        result[8] = z * x * cb - y * s;
        result[9] = z * y * cb + x * s;
        result[10] = z * z * cb + ca;
        result[11] = 0;
        result[12] = 0;
        result[13] = 0;
        result[14] = 0;
        result[15] = 1;

        return result * matrix;
    }
    public static mat4 GeneratePerspectiveMatrix(float fieldOfView, float aspectRatio, float near, float far)
    {
        float top = near * (float)Math.Tan(0.5f * fieldOfView);
        float right = aspectRatio * top;
        mat4 result = new mat4(0f);
        result[0] = near / right;
        result[5] = near / top;
        result[10] = (-near - far) / (far - near);
        result[11] = -2f * far * near / (far - near);
        result[14] = -1f;
        return result;
    }
    public static float DegreesToRadians(float degrees)
    {
        return degrees * (float)Math.PI / 180f;
    }
    public static vec3 Normalize(vec3 vector)
    {
        vec3 result = new vec3();

        float mag = vector.GetMagnitude();
        for (int i = 0; i < 3; i++)
            result[i] = vector[i] / mag;

        return result;
    }
    public static vec3 Cross(vec3 x, vec3 y)
    {
        return new vec3(x[1] * y[2] - x[2] * y[1], x[2] * y[0] - x[0] * y[2], x[0] * y[1] - x[1] * y[0]);
    }
    public static vec3 SphericalToXYZ(float anglex, float angley, float radius = 1)
    {
        vec3 result = new vec3(
            (float)(Math.Sin(anglex) * Math.Cos(angley) * radius),
            (float)(Math.Sin(angley) * radius),
            (float)(Math.Cos(anglex) * Math.Cos(angley) * radius));

        return result;
    }
    public static Mesh GenerateSphereMesh(int columns, int rows)
    {
        Mesh result = new Mesh();

        vec3[,] vertexLookup = new vec3[columns, rows - 1];
        for (int y = 0; y < rows - 1; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                vertexLookup[x, y] = SphericalToXYZ((float)x / columns * (float)Math.PI * 2, (0.5f - (float)(y + 1) / rows) * (float)Math.PI);
            }
        }
        vec3 topVertex = new vec3(0, 1, 0);
        vec3 bottomVertex = new vec3(0, -1, 0);

        for (int x = 0; x < columns; x++)
        {
            result.Add(new Tri(
                new Vertex(topVertex),
                new Vertex(vertexLookup[(x + 1) % columns, 0]),
                new Vertex(vertexLookup[x, 0])));

            result.Add(new Tri(
                new Vertex(vertexLookup[x, rows - 2]),
                new Vertex(vertexLookup[(x + 1) % columns, rows - 2]),
                new Vertex(bottomVertex)));
        }

        for (int y = 0; y < rows - 2; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                result.Add(new Tri(
                    new Vertex(vertexLookup[x, y]),
                    new Vertex(vertexLookup[(x + 1) % columns, y]),
                    new Vertex(vertexLookup[(x + 1) % columns, y + 1])));
                result.Add(new Tri(
                    new Vertex(vertexLookup[(x + 1) % columns, y + 1]),
                    new Vertex(vertexLookup[x, y + 1]),
                    new Vertex(vertexLookup[x, y])));
            }
        }

        return result;
    }
    public static float[] GenerateCircleVertices(int numberOfSides)
    {
        float[] result = new float[(numberOfSides + 1) * 3];
        vec3 vertex;
        for (int i = 0; i <= numberOfSides; i++)
        {
            vertex = SphericalToXYZ((float)i / numberOfSides * 2f * (float)Math.PI, 0f);
            result[i * 3] = vertex[0];
            result[i * 3 + 1] = vertex[1];
            result[i * 3 + 2] = vertex[2];
        }
        return result;
    }
    public static float[] GenerateGridVertices(int rows)
    {
        float[] result = new float[12 * (rows - 1)];
        float x;
        float z;
        for (int i = 0; i < rows - 1; i++)
        {
            z = 1f - 2f * (i + 1f) / (rows);
            result[6 * i] = 1f;
            result[6 * i + 1] = 0f;
            result[6 * i + 2] = z;

            result[6 * i + 3] = -1f;
            result[6 * i + 4] = 0f;
            result[6 * i + 5] = z;
        }

        for (int i = 0; i < rows - 1; i++)
        {
            x = 1f - 2f * (i + 1f) / (rows);
            result[(rows - 1) * 6 + 6 * i] = x;
            result[(rows - 1) * 6 + 6 * i + 1] = 0f;
            result[(rows - 1) * 6 + 6 * i + 2] = 1f;

            result[(rows - 1) * 6 + 6 * i + 3] = x;
            result[(rows - 1) * 6 + 6 * i + 4] = 0f;
            result[(rows - 1) * 6 + 6 * i + 5] = -1f;
        }
        return result;
    }
    public static float SimDistanceToRenderDistance(float x)
    {
        //1*10^10 meters (10 million km) in the simulation converts to 1 render unit
        return x / 1E10f;
    }
    public static vec3 SimPosToRenderPos(vec3 pos)
    {
        return new vec3(SimDistanceToRenderDistance(pos[0]), SimDistanceToRenderDistance(pos[1]), SimDistanceToRenderDistance(pos[2]));
    }
}