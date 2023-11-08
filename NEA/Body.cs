using NEA;
using System;
using static MathsOperations;

internal class Body
{
    private static double G = 6.6743E-11;
    private static int nextId = 0;

    public int id { get; private set; }
    public CircularArray<vec3> Path { get; private set; }
    public vec3 Acc { get; }
    public vec3 Colour { get; set; }
    public float Mass { get; set; }
    public string Name { get; set; }
    public vec3 Pos { get; set; }
    public bool IsStar { get; set; }
    public float Radius { get; set; }
    public vec3 Vel { get; set; }
    public Body(vec3 pos, vec3 vel, vec3 colour, float mass, float radius, string name, bool isStar = false)
    {
        Pos = pos;
        Name = name;
        Vel = vel;
        Mass = mass;
        Radius = radius;
        Colour = colour;
        Acc = new vec3(0f,0f,0f);
        IsStar = isStar;
        id = nextId;
        nextId++;
        Path = new CircularArray<vec3>(2000);
    }
    public void UpdatePos(float deltaTime)
    {
        for (int i = 0; i < 3; i++) Pos[i] += Vel[i]*deltaTime + 0.5f*Acc[i]*deltaTime*deltaTime;
    }
    public void UpdatePath()
    {
        Path.AddItem(new vec3(Pos));
    }
    public void UpdateVelAndAcc(ref List<Body> bodies, float deltaTime)
    {
        double[] forceVecDouble = new double[3];
        double forceMagnitude;
        vec3 distanceVec;
        float distanceMagnitude;
        foreach (Body body in bodies)
        {
            if (body.id != id)
            {
                distanceVec = body.Pos - Pos;
                distanceMagnitude = distanceVec.GetMagnitude();
                forceMagnitude = G * Mass * body.Mass / (distanceMagnitude*distanceMagnitude);
                for (int i = 0; i < 3; i++) forceVecDouble[i] += forceMagnitude * (distanceVec[i]/distanceMagnitude);
            }
        }

        for (int i = 0; i < 3; i++)
        {
            Vel[i] += 0.5f * (Acc[i] + (float)(forceVecDouble[i] / Mass)) * deltaTime;
        }
        for (int i = 0; i < 3; i++)
        {
            Acc[i] = (float)(forceVecDouble[i] / Mass);
        }
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
    public void SetOrbit(Body primary, float eccentricity, float semiMajorAxis, float inclination, float ascendingNodeLongitude, float argumentOfPeriapsis)
    {
        vec3 orbitalPos = new vec3(1f, 0f, 0f);
        float orbitalSpeedY = (float)Math.Sqrt(G * primary.Mass * semiMajorAxis) / (semiMajorAxis * (1 - eccentricity)) * (float)Math.Sqrt(1 - eccentricity * eccentricity);
        vec3 orbitalVel = new vec3(0f, orbitalSpeedY, 0f);
    }
}