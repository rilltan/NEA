using NEA;
using System;
using System.Runtime.ConstrainedExecution;
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
        Path = new CircularArray<vec3>(3000);
    }
    public void UpdatePos(float deltaTime)
    {
        for (int i = 0; i < 3; i++)
            Pos[i] += Vel[i]*deltaTime + 0.5f*Acc[i]*deltaTime*deltaTime;
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
                for (int i = 0; i < 3; i++)
                    forceVecDouble[i] += forceMagnitude * (distanceVec[i]/distanceMagnitude);
            }
        }

        for (int i = 0; i < 3; i++)
            Vel[i] += 0.5f * (Acc[i] + (float)(forceVecDouble[i] / Mass)) * deltaTime;
        for (int i = 0; i < 3; i++)
            Acc[i] = (float)(forceVecDouble[i] / Mass);
    }
    public int GetCollidingBodyID(ref List<Body> bodies)
    {
        float distance;
        foreach (Body body in bodies)
        {
            if (body.id != this.id)
            {
                distance = (body.Pos - this.Pos).GetMagnitude();
                if (distance < body.Radius + this.Radius)
                {
                    if (this.Mass < body.Mass)
                        return this.id;
                    else
                        return body.id;
                }
            }
        }
        return -1;
    }
    public void SetOrbit(Body primary, OrbitalElements elements)
    {
        float sma = elements.SemiMajorAxis;
        float ecc = elements.Eccentricity;

        vec4 orbitalPos = new vec4(sma * (1 - ecc), 0f, 0f, 1f);
        float orbitalSpeedZ = (float)Math.Sqrt(G * primary.Mass * sma) / (sma * (1f - ecc)) * (float)Math.Sqrt(1 - ecc * ecc);
        vec4 orbitalVel = new vec4(0f, 0f, orbitalSpeedZ, 1f);

        mat4 orbitalToInertial = Rotate(new mat4(1f), elements.PeriapsisArgument, new vec3(0f, 1f, 0f));
        orbitalToInertial = Rotate(orbitalToInertial, elements.Inclination, new vec3(1f, 0f, 0f));
        orbitalToInertial = Rotate(orbitalToInertial, elements.AscendingNodeLongitude, new vec3(0f, 1f, 0f));

        vec4 cartesianPos = orbitalToInertial * orbitalPos;
        cartesianPos = Translate(new mat4(1f), primary.Pos) * cartesianPos;
        vec4 cartesianVel = orbitalToInertial * orbitalVel;
        cartesianVel = Translate(new mat4(1f), primary.Vel) * cartesianVel;

        for (int i = 0; i < 3; i++)
        {
            Pos[i] = cartesianPos[i];
            Vel[i] = cartesianVel[i];
        }
        Path.Clear();
    }
}