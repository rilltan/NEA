using System;
using GLFW;
using static OpenGL.GL;

class Program
{
    private const string TITLE = "Gravity Simulator";
    static void Main(string[] args)
    {
        int screenWidth = 1200;
        int screenHeight = 800;
        
        Glfw.WindowHint(Hint.ContextVersionMajor, 3);
        Glfw.WindowHint(Hint.ContextVersionMinor, 3);
        Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
        Glfw.WindowHint(Hint.Resizable, false);
        Window window = Glfw.CreateWindow(screenWidth, screenHeight, TITLE, GLFW.Monitor.None, GLFW.Window.None);
        Glfw.MakeContextCurrent(window);
        Import(Glfw.GetProcAddress);

        Simulation sim = new Simulation(ref window, screenWidth, screenHeight);

        while (!Glfw.WindowShouldClose(window))
        {
            sim.Update();
        }

        Glfw.Terminate();
    }
}