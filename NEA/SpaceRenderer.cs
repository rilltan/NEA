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
    private List<Body> Stars, Planets, Paths, VelocityMarkers, ForceMarkers;
    private Shader StandardShader, PlanetShader, GridShader;
    private GLVertexArray SphereVertexArray;
    private float GridWidth;
    private int GridSize;
    private bool ShouldDrawGrid;
    private vec3 GridPos;

    public float VelocityMarkerScale, ForceMarkerScale;
    public SpaceRenderer(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        
        StandardShader = new Shader(ShaderCode.vertexStandard, ShaderCode.fragmentStandard);
        PlanetShader = new Shader(ShaderCode.vertexPlanet, ShaderCode.fragmentPlanet);
        GridShader = new Shader(ShaderCode.vertexGrid, ShaderCode.fragmentGrid);


        Projection = Perspective(DegreesToRadians(45f), (float)width / height, 0.1f, 1000f);
        Stars = new List<Body>();
        Planets = new List<Body>();
        Paths = new List<Body>();
        VelocityMarkers = new List<Body>();
        ForceMarkers = new List<Body>();

        VelocityMarkerScale = 2f;
        ForceMarkerScale = 2f;

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

        glEnable(GL_BLEND);
        glBlendEquation(GL_FUNC_ADD);
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        glEnable(GL_DEPTH_TEST);
        glDisable(GL_SCISSOR_TEST);
        glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);

        glClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

        mat4 model;


        if (ShouldDrawGrid)
        {
            GLVertexArray GridVertexArray = new GLVertexArray(GenerateGridVertices(GridSize), new int[] { 3 });
            GridVertexArray.Bind();

            GridShader.Use();
            GridShader.SetMatrix4("view", View);
            GridShader.SetMatrix4("projection", Projection);
            model = Scale(new mat4(1f), new vec3(GridSize * GridWidth / 2));
            model = Translate(model, new vec3(GridPos[0] - (GridPos[0] % GridWidth), 0f, GridPos[2] - (GridPos[2] % GridWidth)));
            GridShader.SetMatrix4("model", model);
            GridShader.SetVector3("gridCentre", GridPos);
            GridShader.SetFloat("max", GridSize * GridWidth / 2);
            glDrawArrays(GL_LINES, 0, GridVertexArray.Length);

            GridVertexArray.Delete();
        }


        StandardShader.Use();
        StandardShader.SetMatrix4("view", View);
        StandardShader.SetMatrix4("projection", Projection);
        StandardShader.SetMatrix4("model", new mat4(1.0f));
        GLVertexArray PathVertexArray;
        foreach (Body body in Paths)
        {
            float[] pathVertices = new float[body.Path.Count * 3];
            for (int i = 0; i < body.Path.Count; i++)
            {
                pathVertices[i * 3] = body.Path[i][0];
                pathVertices[i * 3 + 1] = body.Path[i][1];
                pathVertices[i * 3 + 2] = body.Path[i][2];
            }
            PathVertexArray  = new GLVertexArray(pathVertices, new int[] { 3 });
            PathVertexArray.Bind();
            StandardShader.SetVector3("colour", new vec3(body.Colour[0] / 2, body.Colour[1] / 2, body.Colour[2] / 2));
            glDrawArrays(GL_LINE_STRIP, 0, PathVertexArray.Length);

            PathVertexArray.Delete();
        }


        SphereVertexArray.Bind();

        StandardShader.Use();
        StandardShader.SetMatrix4("view", View);
        StandardShader.SetMatrix4("projection", Projection);
        foreach (Body body in Stars)
        {
            model = Scale(new mat4(1f), new vec3(body.Radius));
            model = Translate(model, body.Pos);
            StandardShader.SetMatrix4("model", model);
            StandardShader.SetVector3("colour", body.Colour);
            glDrawArrays(GL_TRIANGLES, 0, SphereVertexArray.Length);
        }

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

        glDisable(GL_DEPTH_TEST);
        StandardShader.Use();
        StandardShader.SetMatrix4("view", View);
        StandardShader.SetMatrix4("projection", Projection);
        StandardShader.SetMatrix4("model", new mat4(1.0f));
        float[] markerVertices = new float[6];
        foreach (Body body in VelocityMarkers)
        {
            for (int i = 0; i < 3; i++) markerVertices[i] = body.Pos[i];
            for (int i = 0; i < 3; i++) markerVertices[i + 3] = body.Pos[i] + body.Vel[i] * VelocityMarkerScale;
            GLVertexArray markerVertexArray = new GLVertexArray(markerVertices, new int[] { 3 });
            markerVertexArray.Bind();
            StandardShader.SetVector3("colour", new vec3(1f,0f,0f));
            glDrawArrays(GL_LINES, 0, markerVertexArray.Length);
            markerVertexArray.Delete();
        }
        foreach (Body body in ForceMarkers)
        {
            for (int i = 0; i < 3; i++) markerVertices[i] = body.Pos[i];
            for (int i = 0; i < 3; i++) markerVertices[i + 3] = body.Pos[i] + body.Acc[i] * ForceMarkerScale;
            GLVertexArray markerVertexArray = new GLVertexArray(markerVertices, new int[] { 3 });
            markerVertexArray.Bind();
            StandardShader.SetVector3("colour", new vec3(0f, 1f, 0f));
            glDrawArrays(GL_LINES, 0, markerVertexArray.Length);
            markerVertexArray.Delete();
        }


        Stars.Clear();
        Planets.Clear();
        Paths.Clear();
        VelocityMarkers.Clear();
        ForceMarkers.Clear();
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
    public void DrawPath(Body body)
    {
        if (body.Path.Count != 0) Paths.Add(body);
    }
    public void DrawGrid(vec3 pos, int numberOfRows, float rowWidth)
    {
        ShouldDrawGrid = true;
        GridPos = pos;
        GridSize = numberOfRows;
        GridWidth = rowWidth;
    }
    public void DrawVelocityMarker(Body body)
    {
        VelocityMarkers.Add(body);
    }
    public void DrawForceMarker(Body body)
    {
        ForceMarkers.Add(body);
    }
    public void SetViewMatrix(mat4 view)
    {
        View = view;
    }
}