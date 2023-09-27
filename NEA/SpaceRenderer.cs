﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenGL.GL;
using static MathsOperations;
using System.Collections;
using System.Reflection;
using NEA;
using System.ComponentModel;

internal class SpaceRenderer
{
    private int X, Y, Width, Height;
    private mat4 Projection, View;
    private List<Body> Stars, Planets;
    private Shader StarShader, PlanetShader, GridShader;
    private GLVertexArray SphereVertexArray, GridVertexArray;
    private float GridWidth;
    private int GridSize;
    private bool ShouldDrawGrid;
    private vec3 GridPos;
    public SpaceRenderer(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;

        glEnable(GL_DEPTH_TEST);
        glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
        glEnable(GL_BLEND);
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        
        StarShader = new Shader(ShaderCode.vertexStar, ShaderCode.fragmentStar);
        PlanetShader = new Shader(ShaderCode.vertexPlanet, ShaderCode.fragmentPlanet);
        GridShader = new Shader(ShaderCode.vertexGrid, ShaderCode.fragmentGrid);


        Projection = Perspective(DegreesToRadians(45f), (float)width / height, 0.1f, 1000f);
        Stars = new List<Body>();
        Planets = new List<Body>();

        Mesh sphereMesh = GenerateSphereShadeSmooth(20, 10);
        SphereVertexArray = new GLVertexArray(sphereMesh.GetFloats(), new int[] { 3, 3 });
    }
    public void Update()
    {
        if (View == null)
        {
            throw new Exception("No view matrix has been set");
        }
        glViewport(X, Y, Width, Height);

        mat4 model;

        if (ShouldDrawGrid)
        {
            GLVertexArray GridVertexArray = new GLVertexArray(GenerateGridVertices(GridSize), new int[] { 3 });
            GridVertexArray.Bind();

            GridShader.Use();
            GridShader.SetMatrix4("view", View);
            GridShader.SetMatrix4("projection", Projection);
            model = Scale(new mat4(1f), new vec3(GridSize * GridWidth / 2));
            model = Translate(model, new vec3(GridPos[0] - (GridPos[0] % GridWidth), GridPos[1], GridPos[2] - (GridPos[2] % GridWidth)));
            GridShader.SetMatrix4("model", model);
            GridShader.SetVector3("gridCentre", GridPos);
            GridShader.SetFloat("max", GridSize * GridWidth / 2);
            glDrawArrays(GL_LINES, 0, GridVertexArray.Length);
        }


        SphereVertexArray.Bind();

        PlanetShader.Use();
        PlanetShader.SetInt("numOfLights", Stars.Count);
        for (int i = 0; i < Stars.Count; i++)
        {
            PlanetShader.SetVector3("lights[" + i + "].pos", Stars[i].Pos);
            PlanetShader.SetVector3("lights[" + i + "].colour", Stars[i].Colour);
        }
        PlanetShader.SetMatrix4("view", View);
        PlanetShader.SetMatrix4("projection", Projection);
        foreach (Body body in Planets)
        {
            model = Scale(new mat4(1f), new vec3(body.Radius));
            model = Translate(model, body.Pos);
            PlanetShader.SetMatrix4("model", model);
            PlanetShader.SetVector3("objectColour", body.Colour);
            glDrawArrays(GL_TRIANGLES, 0, SphereVertexArray.Length);
        }

        StarShader.Use();
        StarShader.SetMatrix4("view", View);
        StarShader.SetMatrix4("projection", Projection);
        foreach (Body body in Stars)
        {
            model = Scale(new mat4(1f), new vec3(body.Radius));
            model = Translate(model, body.Pos);
            StarShader.SetMatrix4("model", model);
            StarShader.SetVector3("objectColour", body.Colour);
            glDrawArrays(GL_TRIANGLES, 0, SphereVertexArray.Length);
        }

        Stars.Clear();
        Planets.Clear();
        ShouldDrawGrid = false;
    }
    public void DrawPlanet(Body planet)
    {
        Planets.Add(planet);
    }
    public void DrawStar(Body star)
    {
        Stars.Add(star);
    }
    public void DrawGrid(vec3 pos, int numberOfRows, float rowWidth)
    {
        ShouldDrawGrid = true;
        GridPos = pos;
        GridSize = numberOfRows;
        GridWidth = rowWidth;
    }
    public void SetViewMatrix(mat4 view)
    {
        View = view;
    }
}