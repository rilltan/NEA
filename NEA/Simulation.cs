using GLFW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static MathsOperations;


internal class Simulation
{
    private Window window;
    private int screenWidth;
    private int screenHeight;
    private SpaceRenderer renderer;
    private List<Body> bodies;
    private Camera camera;

    private MouseCallback mouseCallback;

    private List<int> collisions;
    private int focussedBodyIndex;
    private Keys[] prevKeys = new Keys[20];
    private double cursorX, cursorY, prevCursorX, prevCursorY;
    private float currentTime, prevTime, deltaTime;
    private int pathUpdate = 0;
    public Simulation(ref Window simulationWindow, int width, int height)
    {
        window = simulationWindow;
        screenWidth = width;
        screenHeight = height;
        renderer = new SpaceRenderer(0, 0, screenWidth, screenHeight);
        bodies = new List<Body>();
        camera = new Camera(new vec3(0, 0, 0), 0, DegreesToRadians(89), 20, 0.2f, 0.001f);
        mouseCallback = new MouseCallback(camera.ChangeRadius);
        Glfw.SetScrollCallback(window, mouseCallback);
        collisions = new List<int>();

        bodies.Add(new Body(new vec3(0f, 1f, 0f), new vec3(1f, -0.1f, 0f), new vec3(1f, 1f, 1f), 1000f, true));
        bodies.Add(new Body(new vec3(-6f, 1f, 0f), new vec3(0f, 1f, 3f), new vec3(0.2f, 1f, 1f), 100f));
        bodies.Add(new Body(new vec3(7f, 0f, 0f), new vec3(0f, 0f, -3f), new vec3(0.8f, 0.1f, 0.2f), 100f));
    }
    public void Update()
    {
        currentTime = (float)Glfw.Time;
        deltaTime = currentTime - prevTime;
        if (deltaTime > 0.1) deltaTime = 0;

        if (Glfw.GetKey(window, Keys.Escape) == InputState.Press) Glfw.SetWindowShouldClose(window, true);
        if (Glfw.GetKey(window, Keys.Left) == InputState.Press)
        {
            focussedBodyIndex--;
            if (focussedBodyIndex < 0) focussedBodyIndex = bodies.Count - 1;
        }

        camera.ChangeTarget(bodies[focussedBodyIndex].Pos);
        prevCursorX = cursorX;
        prevCursorY = cursorY;
        Glfw.GetCursorPosition(window, out cursorX, out cursorY);
        if (Glfw.GetMouseButton(window, MouseButton.Right) == InputState.Press)
        {
            Glfw.SetInputMode(window, InputMode.Cursor, (int)CursorMode.Disabled);
            if (prevCursorX != cursorX || prevCursorY != cursorY) camera.ChangeAngles((float)(cursorX - prevCursorX), (float)(prevCursorY - cursorY));
        }

        else if (Glfw.GetInputMode(window, InputMode.Cursor) == (int)CursorMode.Disabled)
        {
            Glfw.SetInputMode(window, InputMode.Cursor, (int)CursorMode.Normal);
            Glfw.SetCursorPosition(window, screenWidth / 2, screenHeight / 2);
        }

        foreach (Body body in bodies) { body.UpdatePos(deltaTime * 5); if (pathUpdate == 1) body.UpdatePath(); }
        foreach (Body body in bodies) body.UpdateVelAndAcc(ref bodies, deltaTime * 5);
        collisions.Clear();
        foreach (Body body in bodies) collisions.Add(body.GetCollidingBody(ref bodies));
        bodies.RemoveAll(body => collisions.Contains(body.id));

        renderer.SetViewMatrix(camera.GetViewMatrix());
        foreach (Body body in bodies)
        {
            if (body.IsStar) renderer.DrawStar(body);
            else renderer.DrawPlanet(body);
            renderer.DrawPath(body);
        }
        renderer.DrawGrid(camera.GetTarget(), 30, 2f);
        renderer.Update();

        if (pathUpdate == 100)
        {
            pathUpdate = 0;
        }
        pathUpdate++;

        prevTime = currentTime;
        Glfw.SwapBuffers(window);
        Glfw.PollEvents();
    }
}