using System;
using GLFW;
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

    private MouseCallback mouseScrollCallback;

    private int focussedBodyID;
    private MouseButton[] mouseButtonsToCheck = new MouseButton[] { MouseButton.Left, MouseButton.Right, MouseButton.Middle };
    private Dictionary<MouseButton,bool> currentMouseButtons, prevMouseButtons;
    private double cursorX, cursorY, prevCursorX, prevCursorY;
    private float currentTime, prevTime, deltaTime;
    private int frameNumber;
    private bool renderVelocityMarkers, renderForceMarkers, renderPaths, renderGrid;
    private float simulationSpeed;
    private bool simulationPaused;
    private string saveLoadResult;
    private float timeWhenSaveLoad;
    private int currentOrbitAroundIndex;
    private OrbitalElements currentOrbitalElements;
    public Simulation(ref Window simulationWindow, int width, int height)
    {
        window = simulationWindow;
        screenWidth = width;
        screenHeight = height;

        renderer = new SpaceRenderer(0, 0, width - 400, screenHeight);
        bodies = new List<Body>();
        camera = new Camera(new vec3(0, 0, 0), 0, DegreesToRadians(89), 5E11f);
        mouseScrollCallback = new MouseCallback(OnMouseScroll);
        Glfw.SetScrollCallback(window, mouseScrollCallback);

        var context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);
        UIController = new ImGuiController(ref simulationWindow, screenWidth, screenHeight);

        ImGuiStylePtr style = ImGui.GetStyle();
        style.Colors[(int)ImGuiCol.TabActive] = new System.Numerics.Vector4(0f, 0.631f, 0.745f, 1f);
        style.Colors[(int)ImGuiCol.TabHovered] = new System.Numerics.Vector4(0f, 0.836f, 1f, 1f);
        style.Colors[(int)ImGuiCol.Tab] = new System.Numerics.Vector4(0.219f, 0.18f, 0.418f, 1f);
        style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new System.Numerics.Vector4(0f, 0f, 0f, 0f);
        style.Colors[(int)ImGuiCol.PopupBg] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 1f);

        renderPaths = true;
        renderGrid = true;
        renderVelocityMarkers = true;
        renderForceMarkers = true;

        frameNumber = 0;
        simulationSpeed = 262144f;
        simulationPaused = true;

        currentMouseButtons = new Dictionary<MouseButton, bool>() { { MouseButton.Left, false }, { MouseButton.Right, false }, { MouseButton.Middle, false } };
        prevMouseButtons = new Dictionary<MouseButton, bool>() { { MouseButton.Left, false }, { MouseButton.Right, false }, { MouseButton.Middle, false } };

        saveLoadResult = "";

        focussedBodyID = -1;
        currentOrbitAroundIndex = -1;
    }
    public void Update()
    {
        currentTime = (float)Glfw.Time;
        deltaTime = currentTime - prevTime;
        if (deltaTime > 0.1)
            deltaTime = 0;

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
                if (frameNumber % 5 == 1)
                    body.UpdatePath();
            }

            foreach (Body body in bodies)
                body.UpdateVelAndAcc(ref bodies, deltaTime * simulationSpeed);

            List<int> bodiesToDelete = new List<int>();
            foreach (Body body in bodies)
                bodiesToDelete.Add(body.GetCollidingBodyID(ref bodies));
            foreach (int id in bodiesToDelete)
                DeleteBodyByID(id);
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
            if (body.IsStar)
                renderer.AddStar(body);
            else
                renderer.AddPlanet(body);

            if (renderPaths)
                renderer.AddPath(body);

            if (renderVelocityMarkers)
                renderer.AddVelocityMarker(body);

            if (renderForceMarkers)
                renderer.AddForceMarker(body);
        }

        if (renderGrid)
            renderer.AddGrid(camera.GetTarget(), 60, 1E10f);

        if (currentOrbitAroundIndex != -1)
            renderer.AddOrbit(currentOrbitalElements, bodies[currentOrbitAroundIndex]);

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
            focussedBodyID = -1;
        bodies.RemoveAt(index);
    }
    private void DeleteBodyByID(int id)
    {
        if (id == focussedBodyID)
            focussedBodyID = -1;
        bodies.RemoveAll(body => body.id == id);
    }
    private void OnMouseScroll(IntPtr window, double x, double y)
    {
        camera.ChangeRadius(y);
        UIController.ImGuiMouseScroll(y);
    }
    private void ProcessKeyboardInput()
    {
        if (Glfw.GetKey(window, Keys.Escape) == InputState.Press)
            Glfw.SetWindowShouldClose(window, true);
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
                camera.ChangeAngles((float)(cursorX - prevCursorX), (float)(prevCursorY - cursorY));
        }
        else if (prevMouseButtons[MouseButton.Right])
        {
            Glfw.SetInputMode(window, InputMode.Cursor, (int)CursorMode.Normal);
            Glfw.SetCursorPosition(window, screenWidth / 2, screenHeight / 2);
        }
    }
    private string SaveSimulationAsText()
    {
        string result = "";
        foreach (Body body in bodies)
        {
            for (int i = 0; i < 3; i++)
                result += body.Pos[i] + ",";

            for (int i = 0; i < 3; i++)
                result += body.Vel[i] + ",";

            for (int i = 0; i < 3; i++)
                result += body.Colour[i] + ",";

            result += body.Mass + ",";
            result += body.Radius + ",";
            result += body.Name + ",";
            result += body.IsStar;
            result += "\n";
        }
        return result;
    }
    private void LoadSimulationFromText(string data)
    {
        bodies.Clear();
        simulationPaused = true;
        string[] lines = data.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            string[] bodyDataStrings = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
            float[] bodyData = new float[bodyDataStrings.Length - 2];
            for (int i = 0; i < bodyDataStrings.Length - 2; i++)
                bodyData[i] = float.Parse(bodyDataStrings[i]);

            bodies.Add(new Body(
                new vec3(bodyData[0], bodyData[1], bodyData[2]),
                new vec3(bodyData[3], bodyData[4], bodyData[5]),
                new vec3(bodyData[6], bodyData[7], bodyData[8]),
                bodyData[9],
                bodyData[10],
                bodyDataStrings[11],
                bool.Parse(bodyDataStrings[12])));
        }
    }
    private void ProcessUI()
    {
        ImGui.SetNextWindowPos(new System.Numerics.Vector2(screenWidth - 400, 0), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, screenHeight), ImGuiCond.Always);

        ImGui.Begin("Controls", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar);

        ImGui.NewLine();

        UISaveAndLoad();
        ImGui.NewLine();

        UICameraFocus();
        ImGui.NewLine();

        UIInfoToggles();
        ImGui.NewLine();

        UISimulationSpeed();
        ImGui.NewLine();

        UIEditBodies();

        ImGui.End();
    }
    private void UITooltip(string text)
    {
        ImGui.SameLine();
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal | ImGuiHoveredFlags.AllowWhenDisabled))
            ImGui.SetTooltip(text);
    }
    private void UISaveAndLoad()
    {
        if (ImGui.Button("Load inner solar system"))
            LoadSimulationFromText("0,0,0,0,0,0,0.9190636,0.9362745,0.40847272,1.9728E+30,696342000,Sun,True\n1.48E+11,0,0,0,0,29784,0,0.38235307,1,5.97E+24,6378000,Earth,False\n5.8E+10,0,0,0,0,47000,0.2647059,0.2647059,0.2647059,3.285E+23,2439700,Mercury,False\n1.082E+11,0,0,0,0,35021,0.9607843,0.48039216,0,4.867E+24,6051800,Venus,False\n2.3E+11,0,0,0,0,24077,0.9558824,0.014057107,0.014057107,6.4171E+23,3389500,Mars,False\n");

        if (ImGui.Button("Save simulation"))
        {
            timeWhenSaveLoad = currentTime;
            try
            {
                ImGui.SetClipboardText(SaveSimulationAsText());
                saveLoadResult = "Simulation data successfully copied to clipboard";
            }
            catch (System.Exception e)
            {
                saveLoadResult = "Save failed: " + e.Message;
            }
        }
        ImGui.SameLine();
        if (ImGui.Button("Load simulation from clipboard"))
        {
            timeWhenSaveLoad = currentTime;
            try
            {
                LoadSimulationFromText(ImGui.GetClipboardText());
                saveLoadResult = "Simulation successfully loaded";
            }
            catch (System.Exception e)
            {
                saveLoadResult = "Load failed: " + e.Message;
            }
        }
        if (currentTime < timeWhenSaveLoad + 3)
            ImGui.TextWrapped(saveLoadResult);
        else
            ImGui.NewLine();
    }
    private void UICameraFocus()
    {
        string[] cameraFocusOptions = new string[bodies.Count + 1];
        cameraFocusOptions[bodies.Count] = "[none]";

        int currentFocusOption = bodies.FindIndex(body => body.id == focussedBodyID);
        if (currentFocusOption == -1)
            currentFocusOption = bodies.Count;

        for (int i = 0; i < bodies.Count; i++)
            cameraFocusOptions[i] = bodies[i].Name;
        ImGui.Combo("Camera focus", ref currentFocusOption, cameraFocusOptions, cameraFocusOptions.Length);

        if (currentFocusOption == bodies.Count)
            focussedBodyID = -1;
        else
            focussedBodyID = bodies[currentFocusOption].id;
    }
    private void UIInfoToggles()
    {
        ImGui.Checkbox("Paths", ref renderPaths);
        ImGui.Checkbox("Grid", ref renderGrid);

        ImGui.Checkbox("Velocity markers", ref renderVelocityMarkers);
        if (renderVelocityMarkers)
        {
            ImGui.SameLine();
            ImGui.SetNextItemWidth(ImGui.GetFontSize() * 10f);
            float uivelocityscale = renderer.VelocityMarkerScale;
            ImGui.SliderFloat("Velocity scale", ref uivelocityscale, 0.1f, 50f, "%.3f", ImGuiSliderFlags.Logarithmic);
            renderer.VelocityMarkerScale = uivelocityscale;
        }

        ImGui.Checkbox("Force markers", ref renderForceMarkers);
        if (renderForceMarkers)
        {
            ImGui.SameLine();
            ImGui.SetNextItemWidth(ImGui.GetFontSize() * 10f);
            float uiforcescale = renderer.ForceMarkerScale;
            ImGui.SliderFloat("Force scale", ref uiforcescale, 0.1f, 50f, "%.3f", ImGuiSliderFlags.Logarithmic);
            renderer.ForceMarkerScale = uiforcescale;
        }

        int uibodyscale = renderer.BodyViewScale;
        ImGui.SliderInt("Body view scale", ref uibodyscale, 1, 100, $"{uibodyscale}x");
        renderer.BodyViewScale = uibodyscale;
    }
    private void UISimulationSpeed()
    {
        if (ImGui.Button("Pause / Resume"))
            simulationPaused = !simulationPaused;

        if (simulationPaused) ImGui.BeginDisabled();

        int simSpeedExponent = (int)Math.Log2(simulationSpeed);
        ImGui.SliderInt("Speed", ref simSpeedExponent, 13, 24, $"{simulationSpeed}x");
        simulationSpeed = (float)Math.Pow(2, simSpeedExponent);

        UITooltip("Can only be adjusted when the simulation is running");

        if (simulationPaused)
            ImGui.EndDisabled();
    }
    private void UIEditBodies()
    {
        if (!simulationPaused)
            ImGui.BeginDisabled();

        if (ImGui.Button("Add body"))
        {
            bodies.Add(new Body(
                new vec3(0f, 0f, 0f),
                new vec3(0f, 0f, 0f),
                new vec3(1f, 1f, 1f),
                1E24f,
                1E7f,
                $"body #{bodies.Count}"));
        }
        UITooltip("Can only be used when the simulation is paused");

        if (!simulationPaused)
            ImGui.EndDisabled();

        if (ImGui.BeginTabBar("tab_bar", ImGuiTabBarFlags.AutoSelectNewTabs | ImGuiTabBarFlags.FittingPolicyScroll))
        {
            for (int i = 0; i < bodies.Count; i++)
            {
                if (ImGui.BeginTabItem($"{bodies[i].Name}###ImGuiTabID{bodies[i].id}"))
                {
                    ImGui.Text($"ID: {bodies[i].id}");

                    string uiname = bodies[i].Name;
                    ImGui.InputText("Name", ref uiname, 30);
                    if (ImGui.IsItemDeactivatedAfterEdit()) bodies[i].Name = uiname;

                    bool uistar = bodies[i].IsStar;
                    ImGui.Checkbox("Star", ref uistar);
                    bodies[i].IsStar = uistar;
                    UITooltip("This just determines whether it emits light or not");

                    float uimass = bodies[i].Mass;
                    ImGui.SliderFloat("Mass", ref uimass, 1E22f, 1E31f, "%.5e kg", ImGuiSliderFlags.Logarithmic);
                    bodies[i].Mass = uimass;

                    float uiradius = bodies[i].Radius;
                    ImGui.SliderFloat("Radius", ref uiradius, 1E6f, 1E9f, "%.2e m");
                    bodies[i].Radius = uiradius;

                    System.Numerics.Vector3 uicolour = bodies[i].Colour.GetNumericsVector3();
                    ImGui.ColorEdit3("Colour", ref uicolour);
                    bodies[i].Colour = new vec3(uicolour);

                    if (!simulationPaused)
                        ImGui.BeginDisabled();

                    System.Numerics.Vector3 uivelocity = bodies[i].Vel.GetNumericsVector3();
                    ImGui.DragFloat3("Velocity", ref uivelocity, 1000f, -100000, 100000f, "%.2e m/s");
                    bodies[i].Vel = new vec3(uivelocity);
                    UITooltip("Can only be adjusted when the simulation is paused");

                    System.Numerics.Vector3 uiposition = bodies[i].Pos.GetNumericsVector3();
                    ImGui.DragFloat3("Position", ref uiposition, 1E9f, -1E12f, 1E12f, "%.2e m");
                    bodies[i].Pos = new vec3(uiposition);
                    UITooltip("Can only be adjusted when the simulation is paused");

                    if (ImGui.Button("Set orbit"))
                    {
                        ImGui.OpenPopup("Set Orbit");
                        currentOrbitalElements = new OrbitalElements(1E11f, 0f, 0f, 0f, 0f);
                        currentOrbitAroundIndex = -1;
                    }
                    UITooltip("Can only be used when the simulation is paused");
                    UISetOrbit(bodies[i]);

                    if (!simulationPaused)
                        ImGui.EndDisabled();

                    if (ImGui.Button("Delete"))
                        DeleteBodyByListIndex(i);

                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }
    }
    private void UISetOrbit(Body orbitingBody)
    {
        bool isOpen = true;
        ImGui.SetNextWindowPos(new System.Numerics.Vector2(screenWidth - 200, screenHeight / 2), ImGuiCond.Always, new System.Numerics.Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(350, screenHeight/2), ImGuiCond.Always);
        if (ImGui.BeginPopupModal("Set Orbit", ref isOpen , ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar))
        {
            string[] orbitAroundOptions = new string[bodies.Count];
            for (int i = 0; i < bodies.Count; i++)
                orbitAroundOptions[i] = bodies[i].Name;
            ImGui.Combo("Orbit around", ref currentOrbitAroundIndex, orbitAroundOptions, orbitAroundOptions.Length);
            if (currentOrbitAroundIndex == bodies.IndexOf(orbitingBody))
                currentOrbitAroundIndex = -1;

            ImGui.Text("Semi-major axis");
            ImGui.SliderFloat("###SemiMajorAxis", ref currentOrbitalElements.SemiMajorAxis, 0f, 1E12f, "%.4e m");
            ImGui.Text("Eccentricity");
            ImGui.SliderFloat("###Eccentricity", ref currentOrbitalElements.Eccentricity, 0f, 0.99f);
            ImGui.Text("Inclination");
            ImGui.SliderFloat("###Inclination", ref currentOrbitalElements.Inclination, 0f, (float)Math.Tau);
            ImGui.Text("Longitude of the ascending node");
            ImGui.SliderFloat("###AscendingNodeLongitude", ref currentOrbitalElements.AscendingNodeLongitude, 0f, (float)Math.Tau);
            ImGui.Text("Argument of periapsis");
            ImGui.SliderFloat("###PeriapsisArgument", ref currentOrbitalElements.PeriapsisArgument, 0f, (float)Math.Tau);

            if (currentOrbitAroundIndex == -1)
                ImGui.BeginDisabled();

            if (ImGui.Button("Confirm"))
            {
                orbitingBody.SetOrbit(bodies[currentOrbitAroundIndex], currentOrbitalElements);
                currentOrbitAroundIndex = -1;
                ImGui.CloseCurrentPopup();
            }

            if (currentOrbitAroundIndex == -1)
                ImGui.EndDisabled();

            ImGui.SameLine();

            if (ImGui.Button("Cancel"))
            {
                currentOrbitAroundIndex = -1;
                ImGui.CloseCurrentPopup();
            }

            ImGui.TextWrapped("This will only be accurate if the body being orbited around is stationary and has a much greater mass than the orbiting body");

            ImGui.EndPopup();
        }
    }
}