using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class Body
{
    private static float G = 0.05f;
    private static int nextId = 0;

    public int id { get; }
    public vec3 Colour { get; set; }
    public float Mass { get; set; }
    public string Name { get; set; }
    public vec3 Pos { get; set; }
    public bool IsStar { get; set; }
    public float Radius { get { return (float)Math.Cbrt(Mass) / 4.6416f; } }
    public List<vec3> Path { get; }
    public vec3 Vel { get; set; }
    public vec3 Acc { get; }
    public Body(vec3 pos, vec3 vel, vec3 colour, float mass, string name, bool isStar = false)
    {
        Pos = pos;
        Name = name;
        Vel = vel;
        Mass = mass;
        Colour = colour;
        Acc = new vec3(0f,0f,0f);
        IsStar = isStar;
        id = nextId;
        nextId++;
        Path = new List<vec3>();
    }
    public void UpdatePos(float deltaTime)
    {
        for (int i = 0; i < 3; i++) Pos[i] += Vel[i]*deltaTime + 0.5f*Acc[i]*deltaTime*deltaTime;
    }
    public void UpdatePath()
    {
        Path.Add(new vec3(Pos));
    }
    public void UpdateVelAndAcc(ref List<Body> bodies, float deltaTime)
    {
        vec3 forceVec = new vec3(0f);
        float forceMagnitude;
        vec3 distanceVec;
        float distanceMagnitude;
        foreach (Body body in bodies)
        {
            if (body.id != id)
            {
                distanceVec = body.Pos - Pos;
                distanceMagnitude = distanceVec.GetMagnitude();
                forceMagnitude = G * Mass * body.Mass / (distanceMagnitude*distanceMagnitude);
                for (int i = 0; i < 3; i++) forceVec[i] += forceMagnitude * (distanceVec[i]/distanceMagnitude);
            }
        }

        for (int i = 0; i < 3; i++) Vel[i] += 0.5f * (Acc[i] + forceVec[i] / Mass) * deltaTime;
        for (int i = 0; i < 3; i++) Acc[i] = forceVec[i] / Mass;
    }
    public int GetCollidingBody(ref List<Body> bodies)
    {
        float distance;
        foreach (Body body in bodies)
        {
            if (body.id != this.id)
            {
                distance = (body.Pos - this.Pos).GetMagnitude();
                if (distance < body.Radius + this.Radius)
                {
                    if (this.Mass < body.Mass) return this.id;
                    else return body.id;
                }
            }
        }
        return -1;
    }
}