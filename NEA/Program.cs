using System;
using GLFW;
using static OpenGL.GL;

class Program
{
    static void Main(string[] args)
    {
        int screenWidth = 1200;
        int screenHeight = 800;
        string title = "Gravity Simulator";
        int targetFPS = 60;
        
        Glfw.WindowHint(Hint.ContextVersionMajor, 3);
        Glfw.WindowHint(Hint.ContextVersionMinor, 3);
        Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
        Glfw.WindowHint(Hint.Resizable, false);
        Window window = Glfw.CreateWindow(screenWidth, screenHeight, title, GLFW.Monitor.None, GLFW.Window.None);
        Glfw.MakeContextCurrent(window);
        Import(Glfw.GetProcAddress);

        Simulation sim = new Simulation(ref window, screenWidth, screenHeight);

        float endTime;
        float startTime;
        float updateInterval = 0.666f / targetFPS;
        while (!Glfw.WindowShouldClose(window))
        {
            startTime = (float)Glfw.Time;
            sim.Update();
            endTime = (float)Glfw.Time;
            if (updateInterval - (endTime - startTime) > 0)
                Thread.Sleep((int)((updateInterval - (endTime - startTime))*1000f));
        }

        Glfw.Terminate();
    }
}