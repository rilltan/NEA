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
        
        Glfw.WindowHint(Hint.ContextVersionMajor, 3);
        Glfw.WindowHint(Hint.ContextVersionMinor, 3);
        Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
        Glfw.WindowHint(Hint.Resizable, false);
        Window window = Glfw.CreateWindow(screenWidth, screenHeight, title, GLFW.Monitor.None, GLFW.Window.None);
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