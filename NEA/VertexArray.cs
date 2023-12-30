using System;
using static OpenGL.GL;

internal class GLVertexArray
{
    private uint vaoID;
    private uint vboID;
    public int Length { get; private set; }
    public GLVertexArray(float[] vertices, int[] attributeLengths)
    {
        Length = vertices.Length / attributeLengths.Sum();
        vaoID = glGenVertexArray();
        vboID = glGenBuffer();
        glBindVertexArray(vaoID);
        glBindBuffer(GL_ARRAY_BUFFER, vboID);
        unsafe
        {
            fixed (float* ptr = &vertices[0])
            {
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, ptr, GL_STATIC_DRAW);
                int offset = 0;
                for (uint i = 0; i < attributeLengths.Length; i++)
                {
                    glVertexAttribPointer(i, attributeLengths[i], GL_FLOAT, false, attributeLengths.Sum() * sizeof(float), (void*)(offset*sizeof(float)));
                    glEnableVertexAttribArray(i);
                    offset += attributeLengths[i];
                }
            }
        }
    }
    public void Bind()
    {
        glBindVertexArray(vaoID);
    }
    public void Delete()
    {
        glDeleteVertexArray(vaoID);
        glDeleteBuffer(vboID);
    }
}