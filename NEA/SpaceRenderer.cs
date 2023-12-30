using System;
using static OpenGL.GL;
using static MathsOperations;
using NEA;
using System.Reflection;
using System.Xml.Schema;

internal class SpaceRenderer
{
    private int WindowX, WindowY, Width, Height;
    private mat4 Projection, View;
    private List<Body> Stars, Planets, Paths, VelocityMarkers, ForceMarkers;
    private Shader StandardShader, PlanetShader, GridShader;
    private GLVertexArray SphereVertexArray, CircleVertexArray;

    private float GridWidth;
    private int GridSize;
    private bool ShouldDrawGrid;
    private vec3 GridPos;

    private vec3 OrbitCentrePos;
    private OrbitalElements OrbitData;
    private bool ShouldDrawOrbit;

    public int BodyViewScale;
    public float VelocityMarkerScale;
    public float ForceMarkerScale;
    public SpaceRenderer(int windowX, int windowY, int width, int height)
    {
        WindowX = windowX;
        WindowY = windowY;
        Width = width;
        Height = height;

        StandardShader = new Shader(ShaderCode.vertexStandard, ShaderCode.fragmentStandard);
        PlanetShader = new Shader(ShaderCode.vertexPlanet, ShaderCode.fragmentPlanet);
        GridShader = new Shader(ShaderCode.vertexGrid, ShaderCode.fragmentGrid);

        Projection = GeneratePerspectiveMatrix(DegreesToRadians(45f), (float)width / height, 0.01f, 1000f);

        Stars = new List<Body>();
        Planets = new List<Body>();
        Paths = new List<Body>();
        VelocityMarkers = new List<Body>();
        ForceMarkers = new List<Body>();

        ShouldDrawGrid = false;
        ShouldDrawOrbit = false;

        VelocityMarkerScale = 5f;
        ForceMarkerScale = 2f;
        BodyViewScale = 25;

        Mesh sphereMesh = GenerateSphereMesh(20, 10);
        SphereVertexArray = new GLVertexArray(sphereMesh.GetFloats(), new int[] { 3, 3 });

        CircleVertexArray = new GLVertexArray(GenerateCircleVertices(25), new int[] { 3 });
    }
    public void Update()
    {
        if (View == null)
        {
            throw new Exception("No view matrix has been set");
        }
        glViewport(WindowX, WindowY, Width, Height);

        glEnable(GL_BLEND);
        glBlendEquation(GL_FUNC_ADD);
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        glEnable(GL_DEPTH_TEST);
        glDisable(GL_SCISSOR_TEST);
        glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);

        glClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

        if (ShouldDrawGrid)
            RenderGrid();

        if (ShouldDrawOrbit)
            RenderOrbit();

        RenderPaths();

        RenderBodies();

        RenderMarkers();

        Stars.Clear();
        Planets.Clear();
        Paths.Clear();
        VelocityMarkers.Clear();
        ForceMarkers.Clear();
        ShouldDrawGrid = false;
        ShouldDrawOrbit = false;
    }
    private void RenderGrid()
    {
        GLVertexArray GridVertexArray = new GLVertexArray(GenerateGridVertices(GridSize), new int[] { 3 });
        GridVertexArray.Bind();

        GridShader.Use();
        GridShader.SetMatrix4("view", View);
        GridShader.SetMatrix4("projection", Projection);

        mat4 model = Scale(new mat4(1f), new vec3(GridSize * GridWidth / 2));
        model = Translate(model, new vec3(GridPos[0] - (GridPos[0] % GridWidth), 0f, GridPos[2] - (GridPos[2] % GridWidth)));
        GridShader.SetMatrix4("model", model);

        GridShader.SetVector3("gridCentre", GridPos);
        GridShader.SetFloat("max", GridSize * GridWidth / 2);

        glDrawArrays(GL_LINES, 0, GridVertexArray.Length);

        GridVertexArray.Delete();
    }
    private void RenderPaths()
    {
        StandardShader.Use();
        StandardShader.SetMatrix4("view", View);
        StandardShader.SetMatrix4("projection", Projection);
        StandardShader.SetMatrix4("model", new mat4(1.0f));

        GLVertexArray PathVertexArray;
        foreach (Body body in Paths)
        {
            float[] pathVertices = new float[body.Path.CurrentSize * 3];
            for (int i = 0; i < body.Path.CurrentSize; i++)
            {
                pathVertices[i * 3] = SimDistanceToRenderDistance(body.Path[i][0]);
                pathVertices[i * 3 + 1] = SimDistanceToRenderDistance(body.Path[i][1]);
                pathVertices[i * 3 + 2] = SimDistanceToRenderDistance(body.Path[i][2]);
            }

            PathVertexArray = new GLVertexArray(pathVertices, new int[] { 3 });
            PathVertexArray.Bind();

            StandardShader.SetVector4("colour", new vec4(body.Colour[0] / 1.3f, body.Colour[1] / 1.3f, body.Colour[2] / 1.3f, 1f));

            glDrawArrays(GL_LINE_STRIP, 0, PathVertexArray.Length);

            PathVertexArray.Delete();
        }
    }
    private void RenderBodies()
    {
        SphereVertexArray.Bind();
        mat4 model;

        StandardShader.Use();
        StandardShader.SetMatrix4("view", View);
        StandardShader.SetMatrix4("projection", Projection);

        foreach (Body body in Stars)
        {
            model = Scale(new mat4(1f), new vec3(SimDistanceToRenderDistance(body.Radius)));
            model = Scale(model, new vec3(BodyViewScale));
            model = Translate(model, SimPosToRenderPos(body.Pos));
            StandardShader.SetMatrix4("model", model);

            StandardShader.SetVector4("colour", new vec4(body.Colour[0], body.Colour[1], body.Colour[2], 1f));

            glDrawArrays(GL_TRIANGLES, 0, SphereVertexArray.Length);
        }


        PlanetShader.Use();
        PlanetShader.SetInt("numOfLights", Stars.Count);
        for (int i = 0; i < Stars.Count; i++)
        {
            PlanetShader.SetVector3("lights[" + i + "].pos", SimPosToRenderPos(Stars[i].Pos));
            PlanetShader.SetVector3("lights[" + i + "].colour", Stars[i].Colour);
        }
        PlanetShader.SetMatrix4("view", View);
        PlanetShader.SetMatrix4("projection", Projection);

        foreach (Body body in Planets)
        {
            model = Scale(new mat4(1f), new vec3(SimDistanceToRenderDistance(body.Radius)));
            model = Scale(model, new vec3(BodyViewScale));
            model = Translate(model, SimPosToRenderPos(body.Pos));
            PlanetShader.SetMatrix4("model", model);

            PlanetShader.SetVector4("objectColour", new vec4(body.Colour[0], body.Colour[1], body.Colour[2], 1f));

            glDrawArrays(GL_TRIANGLES, 0, SphereVertexArray.Length);
        }
    }
    private void RenderMarkers()
    {
        glDisable(GL_DEPTH_TEST);

        StandardShader.Use();
        StandardShader.SetMatrix4("view", View);
        StandardShader.SetMatrix4("projection", Projection);
        StandardShader.SetMatrix4("model", new mat4(1.0f));

        float[] markerVertices = new float[6];
        foreach (Body body in VelocityMarkers)
        {
            for (int i = 0; i < 3; i++)
                markerVertices[i] = SimDistanceToRenderDistance(body.Pos[i]);
            for (int i = 0; i < 3; i++)
                markerVertices[i + 3] = SimDistanceToRenderDistance(body.Pos[i] + body.Vel[i] * VelocityMarkerScale * 100000f);

            GLVertexArray markerVertexArray = new GLVertexArray(markerVertices, new int[] { 3 });
            markerVertexArray.Bind();

            StandardShader.SetVector4("colour", new vec4(1f, 0f, 0f, 1f));

            glDrawArrays(GL_LINES, 0, markerVertexArray.Length);

            markerVertexArray.Delete();
        }

        foreach (Body body in ForceMarkers)
        {
            for (int i = 0; i < 3; i++)
                markerVertices[i] = SimDistanceToRenderDistance(body.Pos[i]);
            for (int i = 0; i < 3; i++)
                markerVertices[i + 3] = SimDistanceToRenderDistance(body.Pos[i] + body.Acc[i] * body.Mass * ForceMarkerScale / 1E13f);

            GLVertexArray markerVertexArray = new GLVertexArray(markerVertices, new int[] { 3 });
            markerVertexArray.Bind();

            StandardShader.SetVector4("colour", new vec4(0f, 1f, 0f, 1f));

            glDrawArrays(GL_LINES, 0, markerVertexArray.Length);

            markerVertexArray.Delete();
        }
    }
    private void RenderOrbit()
    {
        CircleVertexArray.Bind();

        mat4 model = Scale(new mat4(1.0f), new vec3(1f, 1f, (float)Math.Sin(Math.Acos(OrbitData.Eccentricity))));
        model = Translate(model, new vec3(-OrbitData.Eccentricity, 0f, 0f));
        model = Scale(model, new vec3(SimDistanceToRenderDistance(OrbitData.SemiMajorAxis)));
        model = Rotate(model, OrbitData.PeriapsisArgument, new vec3(0f, 1f, 0f));
        model = Rotate(model, OrbitData.Inclination, new vec3(1f, 0f, 0f));
        model = Rotate(model, OrbitData.AscendingNodeLongitude, new vec3(0f, 1f, 0f));
        model = Translate(model, OrbitCentrePos);

        StandardShader.Use();
        StandardShader.SetMatrix4("view", View);
        StandardShader.SetMatrix4("projection", Projection);
        StandardShader.SetMatrix4("model", model);
        StandardShader.SetVector4("colour", new vec4(0f, 1f, 0f, 1f));
        glDrawArrays(GL_LINE_STRIP, 0, CircleVertexArray.Length);
    }
    public void AddPlanet(Body planet)
    {
        Planets.Add(planet);
    }
    public void AddStar(Body star)
    {
        Stars.Add(star);
    }
    public void AddPath(Body body)
    {
        if (body.Path.CurrentSize != 0)
            Paths.Add(body);
    }
    public void AddGrid(vec3 pos, int numberOfRows, float rowWidth)
    {
        ShouldDrawGrid = true;
        GridPos = SimPosToRenderPos(pos);
        GridSize = numberOfRows;
        GridWidth = SimDistanceToRenderDistance(rowWidth);
    }
    public void AddVelocityMarker(Body body)
    {
        VelocityMarkers.Add(body);
    }
    public void AddForceMarker(Body body)
    {
        ForceMarkers.Add(body);
    }
    public void AddOrbit(OrbitalElements orbit, Body primary)
    {
        ShouldDrawOrbit = true;
        OrbitData = orbit;
        OrbitCentrePos = SimPosToRenderPos(primary.Pos);
    }
    public void SetViewMatrix(mat4 view)
    {
        View = view;
    }
}