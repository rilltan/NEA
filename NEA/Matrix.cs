using System;

internal class mat4
{
    private float[] data;
    public mat4()
    {
        data = new float[16];
    }
    public mat4(float diagonal)
    {
        data = new float[16];
        for (int i = 0; i < 16; i++) data[i] = 0f;
        data[0] = diagonal;
        data[5] = diagonal;
        data[10] = diagonal;
        data[15] = diagonal;
    }
    public mat4(float[] vals)
    {
        if (vals.Length != 16) throw new ArgumentException("Array of numbers must be of length 16");
        data = new float[16];
        for (int i = 0; i < 16; i++) data[i] = vals[i];
    }
    public mat4(mat4 matrix)
    {
        data = new float[16];
        for (int i = 0; i < 16; i++) data[i] = matrix[i];
    }
    public float this[int i]
    {
        get { return data[i]; }
        set { data[i] = value; }
    }
    public static mat4 operator +(mat4 x, mat4 y)
    {
        mat4 result = new mat4();
        for (int i = 0; i < 16; i++) result[i] = x[i] + y[i];
        return result;
    }
    public static mat4 operator -(mat4 x, mat4 y)
    {
        mat4 result = new mat4();
        for (int i = 0; i < 16; i++) result.data[i] = x[i] - y[i];
        return result;
    }
    public static mat4 operator *(mat4 x, mat4 y)
    {
        mat4 result = new mat4();
        float val;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                val = 0f;
                for (int k = 0; k < 4; k++)
                {
                    val += x.data[4 * i + k] * y.data[j + 4 * k];
                }
                result.data[4 * i + j] = val;
            }
        }
        return result;
    }
    public unsafe float* GetPointer()
    {
        fixed (float* ptr = &data[0])
        {
            return ptr;
        }
    }
}