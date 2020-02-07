using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Player.Models;
using System;
using System.Diagnostics;

enum ECameraMode
{
    CAMERA_NONE, CAMERA_TRACK, CAMERA_DOLLY, CAMERA_ORBIT
}

namespace Player
{
    public class OpenGLWindow : GameWindow
    {
        int PointCount;
        int LineCount;

        VertexC4ubV3f[] VBO;
        float PointSize = 0.0f;
        ECameraMode CameraMode = ECameraMode.CAMERA_NONE;

        bool IsFullscreen = false;

        // Controls
        static float MOUSE_ORBIT_SPEED = 0.30f;     // 0 = SLOWEST, 1 = FASTEST
        static float MOUSE_DOLLY_SPEED = 0.2f;     // same as above...but much more sensitive
        static float MOUSE_TRACK_SPEED = 0.003f;    // same as above...but much more sensitive

        float g_heading;
        float g_pitch;
        float g_yaw;
        float dx = 0.0f;
        float dy = 0.0f;

        byte brightnessModifier = 0;

        Vector2 MousePrevious = new Vector2();
        Vector2 MouseCurrent = new Vector2();
        float[] cameraPosition = new float[3];
        float[] targetPosition = new float[3];

        // Render data
        public Frame frame = new Frame();
        public AffineTransform[] cameraPoses;

        bool bDrawMarkings = true;

        struct VertexC4ubV3f
        {
            public byte R, G, B, A;
            public Vector3 Position;

            public static int SizeInBytes = 16;
        }

        uint VBOHandle;

        public OpenGLWindow() : base(800, 600, GraphicsMode.Default, "Show Live")
        {
            this.VSync = VSyncMode.Off;
            MouseUp += new EventHandler<MouseButtonEventArgs>(OnMouseButtonUp);
            MouseDown += new EventHandler<MouseButtonEventArgs>(OnMouseButtonDown);
            MouseMove += new EventHandler<MouseMoveEventArgs>(OnMouseMove);
            MouseWheel += new EventHandler<MouseWheelEventArgs>(OnMouseWheelChanged);

            KeyDown += new EventHandler<KeyboardKeyEventArgs>(OnKeyDown);
            
            cameraPosition[0] = 0;
            cameraPosition[1] = 0;
            cameraPosition[2] = 1.0f;
            targetPosition[0] = 0;
            targetPosition[1] = 0;
            targetPosition[2] = 0;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Debug.WriteLine("Unloading GL Window and stopping workers");
            var main = MainForm.Instance;
            main.CancelUpdateWorkers();
            base.OnClosing(e);
        }

        public void ToggleFullscreen()
        {
            if (IsFullscreen)
            {
                WindowBorder = WindowBorder.Resizable;
                WindowState = WindowState.Normal;
                ClientSize = new Size(800, 600);
                CursorVisible = true;
            }
            else
            {
                CursorVisible = false;
                WindowBorder = WindowBorder.Hidden;
                WindowState = WindowState.Fullscreen;
            }
            IsFullscreen = !IsFullscreen;
        }

        void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            var keyboard = e.Keyboard;

            // exit
            if (keyboard[Key.Escape])
            {
                Exit();
            }

            //zoom
            if (keyboard[Key.Plus])
            {
                PointSize += 0.1f;
                GL.PointSize(PointSize);
            }
            if (keyboard[Key.Minus])
            {
                if (PointSize != 0)
                {
                    PointSize -= 0.1f;
                }
                GL.PointSize(PointSize);
            }

            //fullscreen
            if (keyboard[Key.F])
                ToggleFullscreen();

            // brightness
            if (keyboard[Key.O])
                brightnessModifier = (byte)Math.Max(0, brightnessModifier - 10);
            if (keyboard[Key.P])
                brightnessModifier = (byte)Math.Min(255, brightnessModifier + 10);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(0, 0, 0,0);
            GL.Enable(EnableCap.DepthTest);
            GL.PointSize(PointSize);
            GL.Enable(EnableCap.PointSmooth);
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.EnableClientState(ArrayCap.VertexArray);         
            GL.GenBuffers(1, out VBOHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOHandle);
            GL.ColorPointer(4, ColorPointerType.UnsignedByte, VertexC4ubV3f.SizeInBytes, (IntPtr)0);
            GL.VertexPointer(3, VertexPointerType.Float, VertexC4ubV3f.SizeInBytes, (IntPtr)(4 * sizeof(byte)));

            PointCount = 0;
            LineCount = 12;
            VBO = new VertexC4ubV3f[PointCount + 2 * LineCount];
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.DeleteBuffers(1, ref VBOHandle);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);

            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 p = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Width / (float)Height, 0.1f, 50.0f);
            GL.LoadMatrix(ref p);

            GL.MatrixMode(MatrixMode.Modelview);
            Matrix4 mv = Matrix4.LookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY);
            GL.LoadMatrix(ref mv);
        }

        void OnMouseWheelChanged(object sender, MouseWheelEventArgs e)
        {
            dy = e.Delta * MOUSE_DOLLY_SPEED;
            cameraPosition[2] -= dy;
        }

        void OnMouseMove(object sender, MouseMoveEventArgs e)
        {
            MouseCurrent.X = e.Mouse.X;
            MouseCurrent.Y = e.Mouse.Y;
            switch (CameraMode)
            {
                case ECameraMode.CAMERA_TRACK:
                    dx = MouseCurrent.X - MousePrevious.X;
                    dx *= MOUSE_TRACK_SPEED;

                    dy = MouseCurrent.Y - MousePrevious.Y;
                    dy *= MOUSE_TRACK_SPEED;

                    cameraPosition[0] -= dx;
                    cameraPosition[1] += dy;
                    break;

                case ECameraMode.CAMERA_DOLLY:
                    dy = MouseCurrent.Y - MousePrevious.Y;
                    dy *= MOUSE_ORBIT_SPEED;
                    g_pitch += dy;

                    dx = MouseCurrent.X - MousePrevious.X;
                    dx *= MOUSE_ORBIT_SPEED;
                    g_yaw += dx;

                    break;

                case ECameraMode.CAMERA_ORBIT:
                    dx = MouseCurrent.X - MousePrevious.X;
                    dx *= MOUSE_ORBIT_SPEED;

                    dy = MouseCurrent.Y - MousePrevious.Y;
                    dy *= MOUSE_ORBIT_SPEED;

                    g_heading += dx;
                    g_pitch += dy;

                    break;
            }
            MousePrevious.X = MouseCurrent.X;
            MousePrevious.Y = MouseCurrent.Y;
        }

        void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            CameraMode = ECameraMode.CAMERA_NONE;
        }

        void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButton.Left:
                    CameraMode = ECameraMode.CAMERA_ORBIT;
                    break;
                case MouseButton.Middle:
                    CameraMode = ECameraMode.CAMERA_DOLLY;
                    break;
                case MouseButton.Right:
                    CameraMode = ECameraMode.CAMERA_TRACK;
                    break;
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            this.Title = $"PLAYER DEMO";

            PointCount = frame.Vertices.Length / 3;
            LineCount = 0;
            if (bDrawMarkings)
            {
                LineCount += 12;
                LineCount += cameraPoses.Length * 3;
            }

            VBO = new VertexC4ubV3f[PointCount + 2 * LineCount];

            for (int i = 0; i < PointCount; i++)
            {
                VBO[i].R = (byte)Math.Max(0, Math.Min(255, (frame.Colors[i * 3] + brightnessModifier)));
                VBO[i].G = (byte)Math.Max(0, Math.Min(255, (frame.Colors[i * 3 + 1] + brightnessModifier)));
                VBO[i].B = (byte)Math.Max(0, Math.Min(255, (frame.Colors[i * 3 + 2] + brightnessModifier)));
                VBO[i].A = 255;
                VBO[i].Position.X = frame.Vertices[i * 3];
                VBO[i].Position.Y = frame.Vertices[i * 3 + 1];
                VBO[i].Position.Z = frame.Vertices[i * 3 + 2];
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {        
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.PushMatrix();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.Translate(-cameraPosition[0], -cameraPosition[1], -cameraPosition[2]);
            GL.Rotate(g_pitch, 1.0f, 0.0f, 0.0f);
            GL.Rotate(g_heading, 0.0f, 1.0f, 0.0f);
            GL.Rotate(g_yaw, 0.0f, 0.0f, 1.0f);

            // Tell OpenGL to discard old VBO when done drawing it and reserve memory _now_ for a new buffer.
            // without this, GL would wait until draw operations on old VBO are complete before writing to it
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(VertexC4ubV3f.SizeInBytes * (PointCount + 2 * LineCount)), IntPtr.Zero, BufferUsageHint.StreamDraw);
            // Fill newly allocated buffer
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(VertexC4ubV3f.SizeInBytes * (PointCount + 2 * LineCount)), VBO, BufferUsageHint.StreamDraw);

            GL.DrawArrays(PrimitiveType.Points, 0, PointCount);
            GL.DrawArrays(PrimitiveType.Lines, PointCount, 2 * LineCount);

            GL.PopMatrix();

            SwapBuffers();
        }
    }
}

