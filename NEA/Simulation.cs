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
using NEA;

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
    private int focussedBodyID;
    private Keys[] keysToCheck = new Keys[] { Keys.Escape, Keys.Left, Keys.Right };
    private Dictionary<Keys,bool> currentKeys, prevKeys;
    private MouseButton[] mouseButtonsToCheck = new MouseButton[] { MouseButton.Left, MouseButton.Right, MouseButton.Middle };
    private Dictionary<MouseButton,bool> currentMouseButtons, prevMouseButtons;
    private double cursorX, cursorY, prevCursorX, prevCursorY;
    private float currentTime, prevTime, deltaTime;
    private int frameNumber;
    private bool renderVelocityMarkers, renderForceMarkers, renderPaths, renderGrid;
    private float simulationSpeed;
    private bool simulationPaused;
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
        ImGuiStylePtr style = ImGui.GetStyle();
        style.Colors[(int)ImGuiCol.TabActive] = new System.Numerics.Vector4(0f, 0.631f, 0.745f, 1f);
        style.Colors[(int)ImGuiCol.TabHovered] = new System.Numerics.Vector4(0f, 0.836f, 1f, 1f);
        style.Colors[(int)ImGuiCol.Tab] = new System.Numerics.Vector4(0.219f, 0.18f, 0.418f, 1f);

        renderPaths = true;
        renderGrid = true;
        renderVelocityMarkers = true;
        renderForceMarkers = true;

        frameNumber = 0;
        simulationSpeed = 1f;
        simulationPaused = true;

        currentKeys = new Dictionary<Keys, bool>();
        prevKeys = new Dictionary<Keys, bool>();
        foreach (Keys key in keysToCheck)
        {
            currentKeys.Add(key, false);
            prevKeys.Add(key, false);
        }
        currentMouseButtons = new Dictionary<MouseButton, bool>() { { MouseButton.Left, false }, { MouseButton.Right, false }, { MouseButton.Middle, false } };
        prevMouseButtons = new Dictionary<MouseButton, bool>() { { MouseButton.Left, false }, { MouseButton.Right, false }, { MouseButton.Middle, false } };

        //bodies.Add(new Body(new vec3(0f, 1f, 0f), new vec3(1f, -0.1f, 0f), new vec3(1f, 1f, 1f), 1000f, "star", true));
        //bodies.Add(new Body(new vec3(-6f, 1f, 0f), new vec3(0f, 1f, 3f), new vec3(0.2f, 1f, 1f), 100f, "planet 1"));
        //bodies.Add(new Body(new vec3(7f, 0f, 0f), new vec3(0f, 0f, -3f), new vec3(0.8f, 0.1f, 0.2f), 100f, "planet two"));
        focussedBodyID = -1;
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

        if (!simulationPaused)
        {
            foreach (Body body in bodies)
            {
                body.UpdatePos(deltaTime * simulationSpeed);
                if (frameNumber % 10 == 1) body.UpdatePath();
            }
            foreach (Body body in bodies) body.UpdateVelAndAcc(ref bodies, deltaTime * simulationSpeed);
            collisions.Clear();
            foreach (Body body in bodies) collisions.Add(body.GetCollidingBody(ref bodies));
            foreach (int id in collisions) DeleteBodyByID(id);
        }
        
        if (focussedBodyID != -1)
        {
            var focussedBody = bodies.Find(body => body.id == focussedBodyID);
            if (focussedBody != null)
            {
                camera.ChangeTarget(focussedBody.Pos);
            }
        }
        renderer.SetViewMatrix(camera.GetViewMatrix());
        foreach (Body body in bodies)
        {
            if (body.IsStar) renderer.DrawStar(body);
            else renderer.DrawPlanet(body);
            if (renderPaths) renderer.DrawPath(body);
            if (renderVelocityMarkers) renderer.DrawVelocityMarker(body);
            if (renderForceMarkers) renderer.DrawForceMarker(body);
        }
        if (renderGrid) renderer.DrawGrid(camera.GetTarget(), 30, 2f);
        renderer.Update();

        ImGui.Render();
        UIController.Render(ImGui.GetDrawData());

        frameNumber++;
        prevTime = currentTime;
        Glfw.SwapBuffers(window);
        Glfw.PollEvents();
    }
    private void DeleteBodyByListIndex(int index)
    {
        if (bodies[index].id == focussedBodyID)
        {
            focussedBodyID = -1;
        }
        bodies.RemoveAt(index);
    }
    private void DeleteBodyByID(int id)
    {
        if (id == focussedBodyID)
        {
            focussedBodyID = -1;
        }
        bodies.RemoveAll(body => body.id == id);
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
        ImGui.SetNextWindowPos(new System.Numerics.Vector2(800, 0), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 800), ImGuiCond.Always);

        ImGui.Begin("Controls", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar);
        
        ImGui.SeparatorText("Controls");

        string[] cameraFocusOptions = new string[bodies.Count + 1];
        cameraFocusOptions[bodies.Count] = "[none]";
        int currentFocusOption = bodies.FindIndex(body => body.id == focussedBodyID);
        if (currentFocusOption == -1) currentFocusOption = bodies.Count;
        for (int i = 0; i < bodies.Count; i++) cameraFocusOptions[i] = bodies[i].Name;
        ImGui.Combo("Camera focus", ref currentFocusOption, cameraFocusOptions, cameraFocusOptions.Length);
        if (currentFocusOption >= bodies.Count) focussedBodyID = -1;
        else focussedBodyID = bodies[currentFocusOption].id;

        ImGui.NewLine();
        ImGui.Checkbox("Paths", ref renderPaths);
        ImGui.Checkbox("Grid", ref renderGrid);
        ImGui.Checkbox("Velocity markers", ref renderVelocityMarkers);
        if (renderVelocityMarkers)
        {
            ImGui.SameLine();
            ImGui.SetNextItemWidth(ImGui.GetFontSize() * 10f);
            ImGui.SliderFloat("Velocity scale", ref renderer.VelocityMarkerScale, 0.1f, 10f, "%.3f", ImGuiSliderFlags.Logarithmic);
        }
        ImGui.Checkbox("Force markers", ref renderForceMarkers);
        if (renderForceMarkers)
        {
            ImGui.SameLine();
            ImGui.SetNextItemWidth(ImGui.GetFontSize() * 10f);
            ImGui.SliderFloat("Force scale", ref renderer.ForceMarkerScale, 0.1f, 10f, "%.3f", ImGuiSliderFlags.Logarithmic);
        }

        ImGui.NewLine();

        if (ImGui.Button("Pause / Resume"))
        {
            simulationPaused = !simulationPaused;
        }

        if (simulationPaused) ImGui.BeginDisabled();
        int uisimspeed = (int)Math.Log2(simulationSpeed);
        ImGui.SliderInt("Speed", ref uisimspeed, -3, 5, $"{simulationSpeed}x");
        simulationSpeed = (float)Math.Pow(2, uisimspeed);
        UITooltip("Can only be used when the simulation is running");
        if (simulationPaused) ImGui.EndDisabled();

        ImGui.NewLine();
        if (!simulationPaused) ImGui.BeginDisabled();
        if (ImGui.Button("Add body"))
        {
            bodies.Add(new Body(
                new vec3(0f, 0f, 0f),
                new vec3(0f, 0f, 0f),
                new vec3(1f, 1f, 1f),
                100f,
                $"body #{bodies.Count}"
            ));
        }
        UITooltip("Can only be used when the simulation is paused");
        if (!simulationPaused) ImGui.EndDisabled();

        if (ImGui.BeginTabBar("tab_bar"))
        {
            for (int i = 0; i < bodies.Count; i++)
            {
                if (ImGui.BeginTabItem(bodies[i].Name))
                {
                    ImGui.Text($"ID: {bodies[i].id}");
                    string uiname = bodies[i].Name;
                    ImGui.InputText("Name", ref uiname, 30);
                    if (ImGui.IsItemDeactivatedAfterEdit()) bodies[i].Name = uiname;

                    float uimass = bodies[i].Mass;
                    ImGui.SliderFloat("Mass", ref uimass, 10f, 2000f);
                    bodies[i].Mass = uimass;

                    System.Numerics.Vector3 uicolour = bodies[i].Colour.GetNumericsVector3();
                    ImGui.ColorEdit3("Colour", ref uicolour);
                    bodies[i].Colour = new vec3(uicolour);

                    bool uistar = bodies[i].IsStar;
                    ImGui.Checkbox("Star", ref uistar);
                    bodies[i].IsStar = uistar;
                    UITooltip("This just determines whether it emits light or not");

                    if (!simulationPaused) ImGui.BeginDisabled();
                    System.Numerics.Vector3 uivelocity = bodies[i].Vel.GetNumericsVector3();
                    ImGui.DragFloat3("Velocity", ref uivelocity, 0.05f);
                    bodies[i].Vel = new vec3(uivelocity);
                    UITooltip("Can only be adjusted when the simulation is paused");

                    System.Numerics.Vector3 uiposition = bodies[i].Pos.GetNumericsVector3();
                    ImGui.DragFloat3("Position", ref uiposition, 0.05f);
                    bodies[i].Pos = new vec3(uiposition);
                    UITooltip("Can only be adjusted when the simulation is paused");
                    if (!simulationPaused) ImGui.EndDisabled();

                    if (ImGui.Button("Delete"))
                    {
                        DeleteBodyByListIndex(i);
                    }

                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }
        ImGui.End();
    }
    private void UITooltip(string text)
    {
        ImGui.SameLine();
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal | ImGuiHoveredFlags.AllowWhenDisabled)) ImGui.SetTooltip(text);
    }
}