using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

struct Vertex
{
    public vec3 Pos;
    public vec3 Normal;
    public Vertex(vec3 pos)
    {
        Pos = pos;
        Normal = pos;
    }
    public Vertex(vec3 pos, vec3 normal)
    {
        Pos = pos;
        Normal = normal;
    }
}
struct Tri
{
    public Vertex[] Vertices = new Vertex[3];
    public Tri(Vertex v1, Vertex v2, Vertex v3)
    {
        Vertices[0] = v1;
        Vertices[1] = v2;
        Vertices[2] = v3;
    }
    public Vertex this[int i]
    {
        get { return Vertices[i]; }
        set { Vertices[i] = value; }
    }
}
internal class Mesh
{
    private List<Tri> Tris;
    public Mesh()
    {
        Tris = new List<Tri>();
    }
    public void Add(Tri tri)
    {
        Tris.Add(tri);
    }
    public float[] GetFloats()
    {
        float[] result = new float[Tris.Count * 18];
        for (int tri = 0; tri < Tris.Count; tri++)
        {
            for (int vertex = 0; vertex < 3; vertex++)
            {
                for (int i = 0; i < 3; i++)
                {
                    result[tri * 18 + vertex * 6 + i] = Tris[tri][vertex].Pos[i];
                    result[tri * 18 + vertex * 6 + i + 3] = Tris[tri][vertex].Normal[i];
                }
            }
        }
        return result;
    }
}
