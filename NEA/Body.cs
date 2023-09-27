﻿using System;
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
    public vec3 Pos { get; }
    public vec3 Colour { get; }
    public float Mass { get; }
    public bool IsStar { get; }
    public float Radius { get { return (float)Math.Cbrt(Mass) / 4.6416f; } }
    private vec3 Vel;
    private vec3 Acc;
    public Body(vec3 pos, vec3 vel, vec3 colour, float mass, bool isStar = false)
    {
        Pos = pos;
        Vel = vel;
        Mass = mass;
        Colour = colour;
        Acc = new vec3(0f,0f,0f);
        IsStar = isStar;
        id = nextId;
        nextId++;
    }
    public void UpdatePos(float deltaTime)
    {
        for (int i = 0; i < 3; i++) Pos[i] += Vel[i]*deltaTime + 0.5f*Acc[i]*deltaTime*deltaTime;
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