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

        #region texture
        //float[] textureCoords = { 0f, 0f, 1f, 0f, 0.5f, 1f };
        //Bitmap textureBitmap = new Bitmap(textureFile);
        //textureBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
        //int width = textureBitmap.Width;
        //int height = textureBitmap.Height;
        //BitmapData textureData = textureBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

        //uint texture = glGenTexture();
        //glBindTexture(GL_TEXTURE_2D, texture);
        //glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, width, height, 0, GL_BGR, GL_UNSIGNED_BYTE, textureData.Scan0);
        //glGenerateMipmap(GL_TEXTURE_2D);
        //textureBitmap.UnlockBits(textureData);
        #endregion
        while (!Glfw.WindowShouldClose(window))
        {
            sim.Update();
        }

        Glfw.Terminate();
    }
}