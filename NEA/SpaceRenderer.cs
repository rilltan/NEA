using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenGL.GL;
using static MathsOperations;
using System.Collections;
using System.Reflection;

internal class SpaceRenderer
{
    private int X, Y, Width, Height;
    private mat4 Projection;
    private List<Body> Stars;
    private List<Body> Planets;
    private Shader StarShader;
    private Shader PlanetShader;
    private Shader GridShader;
    private GLVertexArray SphereVertexArray;
    private GLVertexArray GridVertexArray;
    private Camera Cam;
    private float GridWidth = 2f;
    private int GridSize = 20;
    public bool DrawGrid;
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
        StarShader = new Shader("vertex_shader_star.txt", "fragment_shader_star.txt");
        PlanetShader = new Shader("vertex_shader_planet.txt", "fragment_shader_planet.txt");
        GridShader = new Shader("vertex_shader_grid.txt", "fragment_shader_grid.txt");


        Projection = Perspective(DegreesToRadians(45f), (float)width / height, 0.1f, 1000f);
        Stars = new List<Body>();
        Planets = new List<Body>();

        Mesh sphereMesh = GenerateSphereShadeSmooth(20, 10);
        SphereVertexArray = new GLVertexArray(sphereMesh.GetFloats(), new int[] { 3, 3 });

        GridVertexArray = new GLVertexArray(GenerateGridVertices(GridSize), new int[] { 3 });
        DrawGrid = true;
    }
    public void Update()
    {
        glViewport(X, Y, Width, Height);

        mat4 model;

        if (DrawGrid)
        {
            GridVertexArray.Bind();

            GridShader.Use();
            GridShader.SetMatrix4("view", Cam.GetViewMatrix());
            GridShader.SetMatrix4("projection", Projection);
            model = Scale(new mat4(1f), new vec3(GridSize * GridWidth / 2));
            vec3 target = Cam.GetTarget();
            model = Translate(model, new vec3(target[0] - (target[0] % GridWidth), target[1], target[2] - (target[2] % GridWidth)));
            GridShader.SetMatrix4("model", model);
            GridShader.SetVector3("camTargetPos", Cam.GetTarget());
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
        PlanetShader.SetMatrix4("view", Cam.GetViewMatrix());
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
        StarShader.SetMatrix4("view", Cam.GetViewMatrix());
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
    }
    public void SetCamera(ref Camera camera)
    {
        Cam = camera;
    }
    public void DrawPlanet(Body planet)
    {
        Planets.Add(planet);
    }
    public void DrawStar(Body star)
    {
        Stars.Add(star);
    }
}