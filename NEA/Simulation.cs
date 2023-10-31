using GLFW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static MathsOperations;
using ImGuiNET;


internal class Simulation
{
    private Window window;
    private int screenWidth;
    private int screenHeight;
    private SpaceRenderer renderer;
    private List<Body> bodies;
    private Camera camera;
    private ImGuiController UIController;

    private MouseCallback mouseCallback;

    private List<int> collisions;
    private int focussedBodyIndex;
    private Keys[] keysToCheck = new Keys[] { Keys.Escape, Keys.Left, Keys.Right };
    private Dictionary<Keys,bool> currentKeys, prevKeys;
    private MouseButton[] mouseButtonsToCheck = new MouseButton[] { MouseButton.Left, MouseButton.Right, MouseButton.Middle };
    private Dictionary<MouseButton,bool> currentMouseButtons, prevMouseButtons;
    private double cursorX, cursorY, prevCursorX, prevCursorY;
    private float currentTime, prevTime, deltaTime;
    private int frameNumber = 0;
    public Simulation(ref Window simulationWindow, int width, int height)
    {
        window = simulationWindow;
        screenWidth = width;
        screenHeight = height;
        renderer = new SpaceRenderer(0, 0, 800, screenHeight);
        bodies = new List<Body>();
        camera = new Camera(new vec3(0, 0, 0), 0, DegreesToRadians(89), 20, 0.2f, 0.001f);
        mouseCallback = new MouseCallback(OnMouseScroll);
        Glfw.SetScrollCallback(window, mouseCallback);
        collisions = new List<int>();

        var context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);
        UIController = new ImGuiController(ref simulationWindow, screenWidth, screenHeight);

        currentKeys = new Dictionary<Keys, bool>();
        prevKeys = new Dictionary<Keys, bool>();
        foreach (Keys key in keysToCheck)
        {
            currentKeys.Add(key, false);
            prevKeys.Add(key, false);
        }
        currentMouseButtons = new Dictionary<MouseButton, bool>() { { MouseButton.Left, false }, { MouseButton.Right, false }, { MouseButton.Middle, false } };
        prevMouseButtons = new Dictionary<MouseButton, bool>() { { MouseButton.Left, false }, { MouseButton.Right, false }, { MouseButton.Middle, false } };

        bodies.Add(new Body(new vec3(0f, 1f, 0f), new vec3(1f, -0.1f, 0f), new vec3(1f, 1f, 1f), 1000f, true));
        bodies.Add(new Body(new vec3(-6f, 1f, 0f), new vec3(0f, 1f, 3f), new vec3(0.2f, 1f, 1f), 100f));
        bodies.Add(new Body(new vec3(7f, 0f, 0f), new vec3(0f, 0f, -3f), new vec3(0.8f, 0.1f, 0.2f), 100f));
    }
    public void Update()
    {
        currentTime = (float)Glfw.Time;
        deltaTime = currentTime - prevTime;
        if (deltaTime > 0.1) deltaTime = 0;

        UIController.NewFrame();
        UIController.ProcessEvent();
        ImGui.NewFrame();

        ProcessKeyboardInput();

        ProcessMouseInput();

        ProcessUI();

        camera.ChangeTarget(bodies[focussedBodyIndex].Pos);

        foreach (Body body in bodies)
        {
            body.UpdatePos(deltaTime * 2);
            if (frameNumber % 60 == 1) body.UpdatePath();
        }
        foreach (Body body in bodies) body.UpdateVelAndAcc(ref bodies, deltaTime * 2);
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

        ImGui.Render();
        UIController.Render(ImGui.GetDrawData());

        frameNumber++;
        prevTime = currentTime;
        Glfw.SwapBuffers(window);
        Glfw.PollEvents();
    }
    private void OnMouseScroll(IntPtr window, double x, double y)
    {
        camera.ChangeRadius(x, y);
        UIController.ImGuiMouseScroll(y);
    }
    private void ProcessKeyboardInput()
    {
        foreach (Keys key in keysToCheck)
        {
            prevKeys[key] = currentKeys[key];
            currentKeys[key] = Glfw.GetKey(window, key) == InputState.Press;
        }
        
        if (currentKeys[Keys.Escape])
        {
            Glfw.SetWindowShouldClose(window, true);
        }
        if (currentKeys[Keys.Left] && !prevKeys[Keys.Left])
        {
            focussedBodyIndex--;
            if (focussedBodyIndex < 0) focussedBodyIndex = bodies.Count - 1;
        }
        if (currentKeys[Keys.Right] && !prevKeys[Keys.Right])
        {
            focussedBodyIndex++;
            if (focussedBodyIndex >= bodies.Count) focussedBodyIndex = 0;
        }
    }
    private void ProcessMouseInput()
    {
        foreach (MouseButton button in mouseButtonsToCheck)
        {
            prevMouseButtons[button] = currentMouseButtons[button];
            currentMouseButtons[button] = Glfw.GetMouseButton(window, button) == InputState.Press;
        }
        prevCursorX = cursorX;
        prevCursorY = cursorY;
        Glfw.GetCursorPosition(window, out cursorX, out cursorY);

        if (currentMouseButtons[MouseButton.Right])
        {
            Glfw.SetInputMode(window, InputMode.Cursor, (int)CursorMode.Disabled);
            if (prevCursorX != cursorX || prevCursorY != cursorY)
            {
                camera.ChangeAngles((float)(cursorX - prevCursorX), (float)(prevCursorY - cursorY));
            }
        }
        else if (prevMouseButtons[MouseButton.Right])
        {
            Glfw.SetInputMode(window, InputMode.Cursor, (int)CursorMode.Normal);
            Glfw.SetCursorPosition(window, screenWidth / 2, screenHeight / 2);
        }
    }
    private void ProcessUI()
    {
        if (ImGui.Button("Next body"))
        {
            focussedBodyIndex++;
            if (focussedBodyIndex >= bodies.Count) focussedBodyIndex = 0;
        }
    }
}