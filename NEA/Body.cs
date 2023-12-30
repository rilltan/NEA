using NEA;
using System;
using System.Runtime.ConstrainedExecution;
using static MathsOperations;

internal class Body
{
    private static float G = 6.6743E-11f;
    private static int nextId = 0;

    public int id { get; private set; }
    public CircularArray<vec3> Path { get; private set; }
    public vec3 Acc { get; private set; }
    public vec3 Colour;
    public float Mass;
    public string Name;
    public vec3 Pos;
    public bool IsStar;
    public float Radius;
    public vec3 Vel;
    public Body(vec3 pos, vec3 vel, vec3 colour, float mass, float radius, string name, bool isStar = false)
    {
        Pos = pos;
        Name = name;
        Vel = vel;
        Mass = mass;
        Radius = radius;
        Colour = colour;
        Acc = new vec3(0f, 0f, 0f);
        IsStar = isStar;
        id = nextId;
        nextId++;
        Path = new CircularArray<vec3>(3000);
    }
    public void UpdatePos(float deltaTime)
    {
        for (int i = 0; i < 3; i++)
            Pos[i] += Vel[i] * deltaTime + 0.5f * Acc[i] * deltaTime * deltaTime;
    }
    public void UpdatePath()
    {
        Path.AddItem(new vec3(Pos));
    }
    public void UpdateVelAndAcc(ref List<Body> bodies, float deltaTime)
    {
        vec3 newAcc = new vec3(0f);
        float accMagnitude;
        vec3 distanceVec;
        float distance;
        foreach (Body body in bodies)
        {
            if (body.id != id)
            {
                distanceVec = body.Pos - Pos;
                distance = distanceVec.GetMagnitude();
                accMagnitude = G * body.Mass / (distance * distance);
                for (int i = 0; i < 3; i++)
                    newAcc[i] += accMagnitude * (distanceVec[i] / distance);
            }
        }

        for (int i = 0; i < 3; i++)
            Vel[i] += 0.5f * (Acc[i] + newAcc[i]) * deltaTime;
        for (int i = 0; i < 3; i++)
            Acc[i] = newAcc[i];
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