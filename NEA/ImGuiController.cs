using System;
using System.Collections.Generic;
using System.Buffers;
using System.Numerics;
using System.Reflection;
using System.IO;
using System.Runtime.CompilerServices;
using ImGuiNET;
using GLFW;
using System.Runtime.InteropServices;
using System.Text;
using static OpenGL.GL;
using NEA;

internal unsafe class ImGuiController
{
    private Vector2 mousePosition;
    private Vector2 displaySize;
    private float delta;
    private bool isKeyCtrl;
    private bool isKeyShift;
    private bool isKeyAlt;
    private bool isKeySuper;
    private Window window;
    private CharCallback charCallback;
    private Shader UIShader;
    private int screenWidth, screenHeight;

    static double g_Time = 0.0;
    static bool g_UnloadAtlas = false;
    static uint g_AtlasTexID = 0;

    public ImGuiController(ref Window simulationWindow, int width, int height)
    {
        window = simulationWindow;
        screenWidth = width;
        screenHeight = height;
        Init();
    }
    private void Init()
    {
        var io = ImGui.GetIO();

        io.KeyMap[(int)ImGuiKey.Tab] = (int)Keys.Tab;
        io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Keys.Left;
        io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Keys.Right;
        io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Keys.Up;
        io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Keys.Down;
        io.KeyMap[(int)ImGuiKey.PageUp] = (int)Keys.PageUp;
        io.KeyMap[(int)ImGuiKey.PageDown] = (int)Keys.PageDown;
        io.KeyMap[(int)ImGuiKey.Home] = (int)Keys.Home;
        io.KeyMap[(int)ImGuiKey.End] = (int)Keys.End;
        io.KeyMap[(int)ImGuiKey.Insert] = (int)Keys.Insert;
        io.KeyMap[(int)ImGuiKey.Delete] = (int)Keys.Delete;
        io.KeyMap[(int)ImGuiKey.Backspace] = (int)Keys.Backspace;
        io.KeyMap[(int)ImGuiKey.Space] = (int)Keys.Space;
        io.KeyMap[(int)ImGuiKey.Enter] = (int)Keys.Enter;
        io.KeyMap[(int)ImGuiKey.Escape] = (int)Keys.Escape;
        io.KeyMap[(int)ImGuiKey.KeypadEnter] = (int)Keys.NumpadEnter;
        io.KeyMap[(int)ImGuiKey.A] = (int)Keys.A;
        io.KeyMap[(int)ImGuiKey.C] = (int)Keys.C;
        io.KeyMap[(int)ImGuiKey.V] = (int)Keys.V;
        io.KeyMap[(int)ImGuiKey.X] = (int)Keys.X;
        io.KeyMap[(int)ImGuiKey.Y] = (int)Keys.Y;
        io.KeyMap[(int)ImGuiKey.Z] = (int)Keys.Z;

        mousePosition = new Vector2(0, 0);
        io.MousePos = mousePosition;

        charCallback = new CharCallback(CharPressedCallback);
        Glfw.SetCharCallback(window, charCallback);

        UIShader = new Shader(ShaderCode.vertexUI, ShaderCode.fragmentUI);

        // Use this space to add more fonts
        LoadDefaultFontAtlas();
    }

    public void Shutdown()
    {
        if (g_UnloadAtlas)
        {
            ImGui.GetIO().Fonts.ClearFonts();
        }
        g_Time = 0.0;
    }

    private void UpdateMousePosAndButtons()
    {
        var io = ImGui.GetIO();

        if (io.WantSetMousePos)
           Glfw.SetCursorPosition(window, (int)io.MousePos.X, (int)io.MousePos.Y);

        io.MouseDown[0] = Glfw.GetMouseButton(window, MouseButton.Left) == InputState.Press;
        io.MouseDown[1] = Glfw.GetMouseButton(window, MouseButton.Right) == InputState.Press;
        io.MouseDown[2] = Glfw.GetMouseButton(window, MouseButton.Middle) == InputState.Press;

        double mouseX, mouseY;
        Glfw.GetCursorPosition(window, out mouseX, out mouseY);
        mousePosition = new Vector2((float)mouseX, (float)mouseY);
        io.MousePos = mousePosition;
    }

    private void UpdateMouseCursor()
    {
        var io = ImGui.GetIO();
        if (io.ConfigFlags.HasFlag(ImGuiConfigFlags.NoMouseCursorChange))
            return;

        var imgui_cursor = ImGui.GetMouseCursor();
        if (io.MouseDrawCursor || imgui_cursor == ImGuiMouseCursor.None)
        {
            Glfw.SetInputMode(window, InputMode.Cursor, (int)CursorMode.Hidden);
        }
        else
        {
            Glfw.SetInputMode(window, InputMode.Cursor, Glfw.GetInputMode(window, InputMode.Cursor));
        }
    }

    public void ImGuiMouseScroll(double y)
    {
        var io = ImGui.GetIO();
        if (y < 0)
        {
            io.MouseWheel -= 1;
        }
        else if (y > 0)
        {
            io.MouseWheel += 1;
        }
    }
    public void NewFrame()
    {
        var io = ImGui.GetIO();

        int displayX, displayY;
        Glfw.GetWindowSize(window, out displayX, out displayY);
        displaySize = new Vector2((float)displayX, (float)displayY);
        io.DisplaySize = displaySize;
        
        double current_time = Glfw.Time;
        delta = g_Time > 0.0 ? (float)(current_time - g_Time) : 1.0f / 60.0f;
        io.DeltaTime = delta;

        isKeyCtrl = Glfw.GetKey(window, Keys.RightControl) == InputState.Press || Glfw.GetKey(window, Keys.LeftControl) == InputState.Press;
        isKeyShift = Glfw.GetKey(window, Keys.RightShift) == InputState.Press || Glfw.GetKey(window, Keys.LeftShift) == InputState.Press;
        isKeyAlt = Glfw.GetKey(window, Keys.RightAlt) == InputState.Press || Glfw.GetKey(window, Keys.LeftAlt) == InputState.Press;
        isKeySuper = Glfw.GetKey(window, Keys.RightSuper) == InputState.Press || Glfw.GetKey(window, Keys.LeftSuper) == InputState.Press;

        io.KeyCtrl = isKeyCtrl;
        io.KeyAlt = isKeyAlt;
        io.KeyShift = isKeyShift;
        io.KeySuper = isKeySuper;

        UpdateMousePosAndButtons();
        UpdateMouseCursor();
    }

    public bool ProcessEvent()
    {
        var io = ImGui.GetIO();

        io.KeysDown[(int)Keys.Apostrophe] = Glfw.GetKey(window, Keys.Apostrophe) == InputState.Press;
        io.KeysDown[(int)Keys.Comma] = Glfw.GetKey(window, Keys.Comma) == InputState.Press;
        io.KeysDown[(int)Keys.Minus] = Glfw.GetKey(window, Keys.Minus) == InputState.Press;
        io.KeysDown[(int)Keys.Period] = Glfw.GetKey(window, Keys.Period) == InputState.Press;
        io.KeysDown[(int)Keys.Slash] = Glfw.GetKey(window, Keys.Slash) == InputState.Press;
        io.KeysDown[(int)Keys.Alpha0] = Glfw.GetKey(window, Keys.Alpha0) == InputState.Press;
        io.KeysDown[(int)Keys.Alpha1] = Glfw.GetKey(window, Keys.Alpha1) == InputState.Press;
        io.KeysDown[(int)Keys.Alpha2] = Glfw.GetKey(window, Keys.Alpha2) == InputState.Press;
        io.KeysDown[(int)Keys.Alpha3] = Glfw.GetKey(window, Keys.Alpha3) == InputState.Press;
        io.KeysDown[(int)Keys.Alpha4] = Glfw.GetKey(window, Keys.Alpha4) == InputState.Press;
        io.KeysDown[(int)Keys.Alpha5] = Glfw.GetKey(window, Keys.Alpha5) == InputState.Press;
        io.KeysDown[(int)Keys.Alpha6] = Glfw.GetKey(window, Keys.Alpha6) == InputState.Press;
        io.KeysDown[(int)Keys.Alpha7] = Glfw.GetKey(window, Keys.Alpha7) == InputState.Press;
        io.KeysDown[(int)Keys.Alpha8] = Glfw.GetKey(window, Keys.Alpha8) == InputState.Press;
        io.KeysDown[(int)Keys.Alpha9] = Glfw.GetKey(window, Keys.Alpha9) == InputState.Press;
        io.KeysDown[(int)Keys.SemiColon] = Glfw.GetKey(window, Keys.SemiColon) == InputState.Press;
        io.KeysDown[(int)Keys.Equal] = Glfw.GetKey(window, Keys.Equal) == InputState.Press;
        io.KeysDown[(int)Keys.A] = Glfw.GetKey(window, Keys.A) == InputState.Press;
        io.KeysDown[(int)Keys.B] = Glfw.GetKey(window, Keys.B) == InputState.Press;
        io.KeysDown[(int)Keys.C] = Glfw.GetKey(window, Keys.C) == InputState.Press;
        io.KeysDown[(int)Keys.D] = Glfw.GetKey(window, Keys.D) == InputState.Press;
        io.KeysDown[(int)Keys.E] = Glfw.GetKey(window, Keys.E) == InputState.Press;
        io.KeysDown[(int)Keys.F] = Glfw.GetKey(window, Keys.F) == InputState.Press;
        io.KeysDown[(int)Keys.G] = Glfw.GetKey(window, Keys.G) == InputState.Press;
        io.KeysDown[(int)Keys.H] = Glfw.GetKey(window, Keys.H) == InputState.Press;
        io.KeysDown[(int)Keys.I] = Glfw.GetKey(window, Keys.I) == InputState.Press;
        io.KeysDown[(int)Keys.J] = Glfw.GetKey(window, Keys.J) == InputState.Press;
        io.KeysDown[(int)Keys.K] = Glfw.GetKey(window, Keys.K) == InputState.Press;
        io.KeysDown[(int)Keys.L] = Glfw.GetKey(window, Keys.L) == InputState.Press;
        io.KeysDown[(int)Keys.M] = Glfw.GetKey(window, Keys.M) == InputState.Press;
        io.KeysDown[(int)Keys.N] = Glfw.GetKey(window, Keys.N) == InputState.Press;
        io.KeysDown[(int)Keys.O] = Glfw.GetKey(window, Keys.O) == InputState.Press;
        io.KeysDown[(int)Keys.P] = Glfw.GetKey(window, Keys.P) == InputState.Press;
        io.KeysDown[(int)Keys.Q] = Glfw.GetKey(window, Keys.Q) == InputState.Press;
        io.KeysDown[(int)Keys.R] = Glfw.GetKey(window, Keys.R) == InputState.Press;
        io.KeysDown[(int)Keys.S] = Glfw.GetKey(window, Keys.S) == InputState.Press;
        io.KeysDown[(int)Keys.T] = Glfw.GetKey(window, Keys.T) == InputState.Press;
        io.KeysDown[(int)Keys.U] = Glfw.GetKey(window, Keys.U) == InputState.Press;
        io.KeysDown[(int)Keys.V] = Glfw.GetKey(window, Keys.V) == InputState.Press;
        io.KeysDown[(int)Keys.W] = Glfw.GetKey(window, Keys.W) == InputState.Press;
        io.KeysDown[(int)Keys.X] = Glfw.GetKey(window, Keys.X) == InputState.Press;
        io.KeysDown[(int)Keys.Y] = Glfw.GetKey(window, Keys.Y) == InputState.Press;
        io.KeysDown[(int)Keys.Z] = Glfw.GetKey(window, Keys.Z) == InputState.Press;
        io.KeysDown[(int)Keys.Space] = Glfw.GetKey(window, Keys.Space) == InputState.Press;
        io.KeysDown[(int)Keys.Escape] = Glfw.GetKey(window, Keys.Escape) == InputState.Press;
        io.KeysDown[(int)Keys.Enter] = Glfw.GetKey(window, Keys.Enter) == InputState.Press;
        io.KeysDown[(int)Keys.Tab] = Glfw.GetKey(window, Keys.Tab) == InputState.Press;
        io.KeysDown[(int)Keys.Backspace] = Glfw.GetKey(window, Keys.Backspace) == InputState.Press;
        io.KeysDown[(int)Keys.Insert] = Glfw.GetKey(window, Keys.Insert) == InputState.Press;
        io.KeysDown[(int)Keys.Delete] = Glfw.GetKey(window, Keys.Delete) == InputState.Press;
        io.KeysDown[(int)Keys.Right] = Glfw.GetKey(window, Keys.Right) == InputState.Press;
        io.KeysDown[(int)Keys.Left] = Glfw.GetKey(window, Keys.Left) == InputState.Press;
        io.KeysDown[(int)Keys.Down] = Glfw.GetKey(window, Keys.Down) == InputState.Press;
        io.KeysDown[(int)Keys.Up] = Glfw.GetKey(window, Keys.Up) == InputState.Press;
        io.KeysDown[(int)Keys.PageUp] = Glfw.GetKey(window, Keys.PageUp) == InputState.Press;
        io.KeysDown[(int)Keys.PageDown] = Glfw.GetKey(window, Keys.PageDown) == InputState.Press;
        io.KeysDown[(int)Keys.Home] = Glfw.GetKey(window, Keys.Home) == InputState.Press;
        io.KeysDown[(int)Keys.End] = Glfw.GetKey(window, Keys.End) == InputState.Press;
        io.KeysDown[(int)Keys.CapsLock] = Glfw.GetKey(window, Keys.CapsLock) == InputState.Press;
        io.KeysDown[(int)Keys.ScrollLock] = Glfw.GetKey(window, Keys.ScrollLock) == InputState.Press;
        io.KeysDown[(int)Keys.NumLock] = Glfw.GetKey(window, Keys.NumLock) == InputState.Press;
        io.KeysDown[(int)Keys.PrintScreen] = Glfw.GetKey(window, Keys.PrintScreen) == InputState.Press;
        io.KeysDown[(int)Keys.Pause] = Glfw.GetKey(window, Keys.Pause) == InputState.Press;
        io.KeysDown[(int)Keys.F1] = Glfw.GetKey(window, Keys.F1) == InputState.Press;
        io.KeysDown[(int)Keys.F2] = Glfw.GetKey(window, Keys.F2) == InputState.Press;
        io.KeysDown[(int)Keys.F3] = Glfw.GetKey(window, Keys.F3) == InputState.Press;
        io.KeysDown[(int)Keys.F4] = Glfw.GetKey(window, Keys.F4) == InputState.Press;
        io.KeysDown[(int)Keys.F5] = Glfw.GetKey(window, Keys.F5) == InputState.Press;
        io.KeysDown[(int)Keys.F6] = Glfw.GetKey(window, Keys.F6) == InputState.Press;
        io.KeysDown[(int)Keys.F7] = Glfw.GetKey(window, Keys.F7) == InputState.Press;
        io.KeysDown[(int)Keys.F8] = Glfw.GetKey(window, Keys.F8) == InputState.Press;
        io.KeysDown[(int)Keys.F9] = Glfw.GetKey(window, Keys.F9) == InputState.Press;
        io.KeysDown[(int)Keys.F10] = Glfw.GetKey(window, Keys.F10) == InputState.Press;
        io.KeysDown[(int)Keys.F11] = Glfw.GetKey(window, Keys.F11) == InputState.Press;
        io.KeysDown[(int)Keys.F12] = Glfw.GetKey(window, Keys.F12) == InputState.Press;
        io.KeysDown[(int)Keys.LeftShift] = Glfw.GetKey(window, Keys.LeftShift) == InputState.Press;
        io.KeysDown[(int)Keys.LeftControl] = Glfw.GetKey(window, Keys.LeftControl) == InputState.Press;
        io.KeysDown[(int)Keys.LeftAlt] = Glfw.GetKey(window, Keys.LeftAlt) == InputState.Press;
        io.KeysDown[(int)Keys.LeftSuper] = Glfw.GetKey(window, Keys.LeftSuper) == InputState.Press;
        io.KeysDown[(int)Keys.RightShift] = Glfw.GetKey(window, Keys.RightShift) == InputState.Press;
        io.KeysDown[(int)Keys.RightControl] = Glfw.GetKey(window, Keys.RightControl) == InputState.Press;
        io.KeysDown[(int)Keys.RightAlt] = Glfw.GetKey(window, Keys.RightAlt) == InputState.Press;
        io.KeysDown[(int)Keys.RightSuper] = Glfw.GetKey(window, Keys.RightSuper) == InputState.Press;
        io.KeysDown[(int)Keys.LeftBracket] = Glfw.GetKey(window, Keys.LeftBracket) == InputState.Press;
        io.KeysDown[(int)Keys.Backslash] = Glfw.GetKey(window, Keys.Backslash) == InputState.Press;
        io.KeysDown[(int)Keys.RightBracket] = Glfw.GetKey(window, Keys.RightBracket) == InputState.Press;
        io.KeysDown[(int)Keys.GraveAccent] = Glfw.GetKey(window, Keys.GraveAccent) == InputState.Press;
        io.KeysDown[(int)Keys.Numpad0] = Glfw.GetKey(window, Keys.Numpad0) == InputState.Press;
        io.KeysDown[(int)Keys.Numpad1] = Glfw.GetKey(window, Keys.Numpad1) == InputState.Press;
        io.KeysDown[(int)Keys.Numpad2] = Glfw.GetKey(window, Keys.Numpad2) == InputState.Press;
        io.KeysDown[(int)Keys.Numpad3] = Glfw.GetKey(window, Keys.Numpad3) == InputState.Press;
        io.KeysDown[(int)Keys.Numpad4] = Glfw.GetKey(window, Keys.Numpad4) == InputState.Press;
        io.KeysDown[(int)Keys.Numpad5] = Glfw.GetKey(window, Keys.Numpad5) == InputState.Press;
        io.KeysDown[(int)Keys.Numpad6] = Glfw.GetKey(window, Keys.Numpad6) == InputState.Press;
        io.KeysDown[(int)Keys.Numpad7] = Glfw.GetKey(window, Keys.Numpad7) == InputState.Press;
        io.KeysDown[(int)Keys.Numpad8] = Glfw.GetKey(window, Keys.Numpad8) == InputState.Press;
        io.KeysDown[(int)Keys.Numpad9] = Glfw.GetKey(window, Keys.Numpad9) == InputState.Press;
        io.KeysDown[(int)Keys.NumpadDecimal] = Glfw.GetKey(window, Keys.NumpadDecimal) == InputState.Press;
        io.KeysDown[(int)Keys.NumpadDivide] = Glfw.GetKey(window, Keys.NumpadDivide) == InputState.Press;
        io.KeysDown[(int)Keys.NumpadMultiply] = Glfw.GetKey(window, Keys.NumpadMultiply) == InputState.Press;
        io.KeysDown[(int)Keys.NumpadSubtract] = Glfw.GetKey(window, Keys.NumpadSubtract) == InputState.Press;
        io.KeysDown[(int)Keys.NumpadAdd] = Glfw.GetKey(window, Keys.NumpadAdd) == InputState.Press;
        io.KeysDown[(int)Keys.NumpadEnter] = Glfw.GetKey(window, Keys.NumpadEnter) == InputState.Press;
        io.KeysDown[(int)Keys.NumpadEqual] = Glfw.GetKey(window, Keys.NumpadEqual) == InputState.Press;
        io.KeysDown[(int)Keys.Menu] = Glfw.GetKey(window, Keys.Menu) == InputState.Press;

        return true;
    }
    private void CharPressedCallback(IntPtr window, uint codepoint)
    {
        var io = ImGui.GetIO();
        io.AddInputCharacterUTF16((ushort)codepoint);
    }

    void LoadDefaultFontAtlas()
    {
        if (!g_UnloadAtlas)
        {
            var io = ImGui.GetIO();
            IntPtr pixels;
            int width, height, bpp;
            Image image;
            
            io.Fonts.GetTexDataAsRGBA32(out pixels, out width, out height, out bpp);
            var size = bpp*width*height;
            image = new Image(width, height, Marshal.AllocHGlobal(size));
            Buffer.MemoryCopy((void*)pixels, (void*)image.Pixels, size, size);

            uint texture = glGenTexture();
            glBindTexture(GL_TEXTURE_2D, texture);
            glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, image.Width, image.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, image.Pixels);
            glGenerateMipmap(GL_TEXTURE_2D);
            g_AtlasTexID = texture;
            io.Fonts.TexID = (IntPtr)g_AtlasTexID;
            Marshal.FreeHGlobal(pixels);
            Marshal.FreeHGlobal(image.Pixels);
            g_UnloadAtlas = true;
        }
    }

    public void Render(ImDrawDataPtr draw_data)
    {
        float[] vertices = new float[draw_data.TotalIdxCount * 8];
        int vertexCounter = 0;
        for (int n = 0; n < draw_data.CmdListsCount; n++)
        {
            ImDrawListPtr cmd_list = draw_data.CmdLists[n];
            uint idx_index = 0;
            for (int i = 0; i < cmd_list.CmdBuffer.Size; i++)
            {
                var pcmd = cmd_list.CmdBuffer[i];
                {
                    var ti = pcmd.TextureId;
                    for (int j = 0; j <= (pcmd.ElemCount - 3); j += 3)
                    {
                        if (pcmd.ElemCount == 0)
                        {
                            break;
                        }

                        ImDrawVertPtr vertex;
                        ushort index;

                        index = cmd_list.IdxBuffer[(int)(j + idx_index)];
                        vertex = cmd_list.VtxBuffer[index];
                        vertices[vertexCounter * 8] = vertex.pos.X / draw_data.DisplaySize.X * 2 - 1;
                        vertices[vertexCounter * 8 + 1] = 1 - vertex.pos.Y / draw_data.DisplaySize.Y * 2;
                        vertices[vertexCounter * 8 + 2] = vertex.uv.X;
                        vertices[vertexCounter * 8 + 3] = vertex.uv.Y;
                        vertices[vertexCounter * 8 + 4] = (float)((byte)(vertex.col >> 0)) / 255f;
                        vertices[vertexCounter * 8 + 5] = (float)((byte)(vertex.col >> 8)) / 255f;
                        vertices[vertexCounter * 8 + 6] = (float)((byte)(vertex.col >> 16)) / 255f;
                        vertices[vertexCounter * 8 + 7] = (float)((byte)(vertex.col >> 24)) / 255f;
                        vertexCounter++;

                        index = cmd_list.IdxBuffer[(int)(j + 2 + idx_index)];
                        vertex = cmd_list.VtxBuffer[index];

                        vertices[vertexCounter * 8] = vertex.pos.X / draw_data.DisplaySize.X * 2 - 1;
                        vertices[vertexCounter * 8 + 1] = 1 - vertex.pos.Y / draw_data.DisplaySize.Y * 2;
                        vertices[vertexCounter * 8 + 2] = vertex.uv.X;
                        vertices[vertexCounter * 8 + 3] = vertex.uv.Y;
                        vertices[vertexCounter * 8 + 4] = (float)((byte)(vertex.col >> 0)) / 255f;
                        vertices[vertexCounter * 8 + 5] = (float)((byte)(vertex.col >> 8)) / 255f;
                        vertices[vertexCounter * 8 + 6] = (float)((byte)(vertex.col >> 16)) / 255f;
                        vertices[vertexCounter * 8 + 7] = (float)((byte)(vertex.col >> 24)) / 255f;
                        vertexCounter++;

                        index = cmd_list.IdxBuffer[(int)(j + 1 + idx_index)];
                        vertex = cmd_list.VtxBuffer[index];

                        vertices[vertexCounter * 8] = vertex.pos.X / draw_data.DisplaySize.X * 2 - 1;
                        vertices[vertexCounter * 8 + 1] = 1 - vertex.pos.Y / draw_data.DisplaySize.Y * 2;
                        vertices[vertexCounter * 8 + 2] = vertex.uv.X;
                        vertices[vertexCounter * 8 + 3] = vertex.uv.Y;
                        vertices[vertexCounter * 8 + 4] = (float)((byte)(vertex.col >> 0)) / 255f;
                        vertices[vertexCounter * 8 + 5] = (float)((byte)(vertex.col >> 8)) / 255f;
                        vertices[vertexCounter * 8 + 6] = (float)((byte)(vertex.col >> 16)) / 255f;
                        vertices[vertexCounter * 8 + 7] = (float)((byte)(vertex.col >> 24)) / 255f;
                        vertexCounter++;
                    }
                }

                idx_index += pcmd.ElemCount;
            }
        }
        glViewport(0, 0, screenWidth, screenHeight);
        glEnable(GL_BLEND);
        glBlendEquation(GL_FUNC_ADD);
        glBlendFuncSeparate(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA, GL_ONE, GL_ONE_MINUS_SRC_ALPHA);
        glDisable(GL_CULL_FACE);
        glDisable(GL_DEPTH_TEST);
        glDisable(GL_STENCIL_TEST);
        glEnable(GL_SCISSOR_TEST);
        if (draw_data.TotalVtxCount > 0)
        {
            GLVertexArray UIVertexArray = new GLVertexArray(vertices, new int[] { 2, 2, 4 });
            UIVertexArray.Bind();
            UIShader.Use();
            glBindTexture(GL_TEXTURE_2D, (uint)draw_data.CmdLists[0].CmdBuffer[0].TextureId.ToInt32());
            glDrawArrays(GL_TRIANGLES, 0, UIVertexArray.Length);
            UIVertexArray.Delete();
        }
    }

}