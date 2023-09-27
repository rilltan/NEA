using System;
using GLFW;
using static OpenGL.GL;
using static MathsOperations;
using System.Drawing;
using System.Drawing.Imaging;

class Program
{
    private const string TITLE = "Orbital Simulator";
    static void Main(string[] args)
    {
        string textureFile = "bongocat.jpg";
        int screenWidth = 800;
        int screenHeight = 800;

        Glfw.WindowHint(Hint.ContextVersionMajor, 3);
        Glfw.WindowHint(Hint.ContextVersionMinor, 3);
        Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
        Glfw.WindowHint(Hint.Resizable, false);
        Window window = Glfw.CreateWindow(screenWidth, screenHeight, TITLE, GLFW.Monitor.None, GLFW.Window.None);
        Glfw.MakeContextCurrent(window);
        Import(Glfw.GetProcAddress);

        SpaceRenderer renderer = new SpaceRenderer(0, 0, 800, 800);

        Random rand = new Random();
        List<Body> bodies = new List<Body>();
        //for (int i = 0; i < 5; i++)
        //{
        //    bodies.Add(new Body(new vec3(rand.NextSingle() * 20 - 10, rand.NextSingle() * 20 - 10, rand.NextSingle() * 20 - 10), new vec3(0f, 0f, 0f), new vec3(rand.NextSingle(), rand.NextSingle(), rand.NextSingle()), 100f));
        //}
        bodies.Add(new Body(new vec3(0f, 0f, 0f), new vec3(1f, 0f, 0f), new vec3(1f, 1f, 1f), 1000f, true));
        bodies.Add(new Body(new vec3(9f, 0f, 0f), new vec3(0f, 0f, 3f), new vec3(0.2f, 1f, 1f), 100f));
        bodies.Add(new Body(new vec3(-7f, 0f, 0f), new vec3(0f, 0f, -3f), new vec3(0.8f, 0.1f, 0.2f), 100f));
        bodies.Add(new Body(new vec3(0f, 0f, 7f), new vec3(-3f, 0f, 0f), new vec3(0.1f, 1f, 1f), 100f));
        bodies.Add(new Body(new vec3(0f, 0f, -7f), new vec3(3f, 0f, 0f), new vec3(0.8f, 0.2f, 0.1f), 100f));

        #region texture
        float[] textureCoords = { 0f, 0f, 1f, 0f, 0.5f, 1f };
        Bitmap textureBitmap = new Bitmap(textureFile);
        textureBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
        int width = textureBitmap.Width;
        int height = textureBitmap.Height;
        BitmapData textureData = textureBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

        uint texture = glGenTexture();
        glBindTexture(GL_TEXTURE_2D, texture);
        glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, width, height, 0, GL_BGR, GL_UNSIGNED_BYTE, textureData.Scan0);
        glGenerateMipmap(GL_TEXTURE_2D);
        textureBitmap.UnlockBits(textureData);
        #endregion

        Camera camera = new Camera(new vec3(0, 0, 0), 0, DegreesToRadians(89), 20, 0.2f, 0.001f);
        Glfw.SetScrollCallback(window, new MouseCallback(camera.ChangeRadius));


        int focussedBodyIndex = 0;
        float timeValue = (float)Glfw.Time;
        float lastTimeValue;
        float deltaTime;
        double cursorX;
        double cursorY;
        Glfw.GetCursorPosition(window, out cursorX, out cursorY);
        double lastCursorX;
        double lastCursorY;
        List<int> collisions = new List<int>();
        while (!Glfw.WindowShouldClose(window))
        {
            lastTimeValue = timeValue;
            timeValue = (float)Glfw.Time;
            deltaTime = timeValue - lastTimeValue;
            if (deltaTime > 0.1) deltaTime = 0;

            if (Glfw.GetKey(window, Keys.Escape) == InputState.Press) Glfw.SetWindowShouldClose(window, true);
            if (Glfw.GetKey(window, Keys.Left) == InputState.Press)
            {
                focussedBodyIndex--;
                if (focussedBodyIndex < 0) focussedBodyIndex = bodies.Count - 1;
            }

            camera.ChangeTarget(bodies[focussedBodyIndex].Pos);
            lastCursorX = cursorX;
            lastCursorY = cursorY;
            Glfw.GetCursorPosition(window, out cursorX, out cursorY);
            if (Glfw.GetMouseButton(window, MouseButton.Right) == InputState.Press)
            {
                Glfw.SetInputMode(window, InputMode.Cursor, (int)CursorMode.Disabled);
                if (lastCursorX != cursorX || lastCursorY != cursorY) camera.ChangeAngles((float)(cursorX - lastCursorX), (float)(lastCursorY - cursorY));
            }

            else if (Glfw.GetInputMode(window, InputMode.Cursor) == (int)CursorMode.Disabled)
            {
                Glfw.SetInputMode(window, InputMode.Cursor, (int)CursorMode.Normal);
                Glfw.SetCursorPosition(window, screenWidth / 2, screenHeight / 2);
            }

            foreach (Body body in bodies) body.UpdatePos(deltaTime * 2);
            foreach (Body body in bodies) body.UpdateVelAndAcc(ref bodies, deltaTime * 2);
            collisions.Clear();
            foreach (Body body in bodies) collisions.Add(body.GetCollidingBody(ref bodies));
            bodies.RemoveAll(body => collisions.Contains(body.id));


            glClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

            renderer.SetViewMatrix(camera.GetViewMatrix());
            foreach (Body body in bodies)
            {
                if (body.IsStar) renderer.DrawStar(body);
                else renderer.DrawPlanet(body);
            }
            renderer.DrawGrid(camera.GetTarget(), 30, 2f);
            renderer.Update();

            Glfw.SwapBuffers(window);
            Glfw.PollEvents();
        }

        Glfw.Terminate();
    }
}