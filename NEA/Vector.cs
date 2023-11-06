using System;

internal class vec3
{
    private float[] data;
    public vec3()
    {
        data = new float[3];
    }
    public vec3(float val)
    {
        data = new float[3];
        for (int i = 0; i < 3; i++) data[i] = val;
    }
    public vec3(float f1, float f2, float f3)
    {
        data = new float[3];
        data[0] = f1;
        data[1] = f2;
        data[2] = f3;
    }
    public vec3(float[] vals)
    {
        data = new float[3];
        for (int i = 0; i < 3; i++) data[i] = vals[i];
    }
    public vec3(vec3 vector)
    {
        data = new float[3];
        for (int i = 0; i < 3; i++) data[i] = vector[i];
    }
    public vec3(System.Numerics.Vector3 vector)
    {
        data = new float[3];
        data[0] = vector.X;
        data[1] = vector.Y;
        data[2] = vector.Z;
    }
    public float this[int i]
    {
        get { return data[i]; }
        set { data[i] = value; }
    }
    public float GetMagnitude()
    {
        return (float)Math.Sqrt(data[0] * data[0] + data[1] * data[1] + data[2] * data[2]);
    }
    public System.Numerics.Vector3 GetNumericsVector3()
    {
        return new System.Numerics.Vector3(data[0], data[1], data[2]);
    }
    public static vec3 operator +(vec3 x, vec3 y)
    {
        vec3 result = new vec3();
        for (int i = 0; i < 3; i++) result[i] = x[i] + y[i];
        return result;
    }
    public static vec3 operator -(vec3 x, vec3 y)
    {
        vec3 result = new vec3();
        for (int i = 0; i < 3; i++) result[i] = x[i] - y[i];
        return result;
    }
}
internal class vec4
{
    private float[] data;
    public vec4()
    {
        data = new float[4];
    }
    public vec4(float val)
    {
        data = new float[4];
        for (int i = 0; i < 4; i++) data[i] = val;
    }
    public vec4(float f1, float f2, float f3, float f4)
    {
        data = new float[4];
        data[0] = f1;
        data[1] = f2;
        data[2] = f3;
        data[3] = f4;
    }
    public vec4(float[] vals)
    {
        data = new float[4];
        for (int i = 0; i < 4; i++) data[i] = vals[i];
    }
    public vec4(vec4 vector)
    {
        data = new float[4];
        for (int i = 0; i < 4; i++) data[i] = vector[i];
    }
    public float this[int i]
    {
        get { return data[i]; }
        set { data[i] = value; }
    }
}