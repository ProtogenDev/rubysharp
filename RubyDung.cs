using System;
using System.Collections.Generic;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.OpenGL.Legacy;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using mc_c_.character;
using mc_c_.level;

namespace mc_c_
{
    public class RubyDung
    {
        private const bool FULLSCREEN_MODE = false;

        private IWindow window;
        private IInputContext input;
        private IMouse mouse;

        private int width;
        private int height;
        private float[] fogColor = new float[4];
        private Timer timer = new Timer(60.0f);
        private Level level;
        private LevelRenderer levelRenderer;
        private Player player;
        private List<Zombie> zombies = new List<Zombie>();
        private HitResult hitResult = null;

        private System.Numerics.Vector2 lastMousePos;
        private bool mouseInitialized = false;

        public void Init()
        {
            int col = 920330;
            float fr = 0.5f;
            float fg = 0.8f;
            float fb = 1.0f;
            fogColor[0] = (col >> 16 & 255) / 255.0f;
            fogColor[1] = (col >> 8 & 255) / 255.0f;
            fogColor[2] = (col & 255) / 255.0f;
            fogColor[3] = 1.0f;

            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(1024, 768);
            options.Title = "RubyDung";
            options.WindowState = FULLSCREEN_MODE ? WindowState.Fullscreen : WindowState.Normal;
            // Request a compatibility-profile GL context so fixed-function
            // calls (glBegin/glVertex, matrix stack, display lists) work,
            // matching Silk.NET.OpenGL.Legacy's expectations.
            options.API = new GraphicsAPI(
                ContextAPI.OpenGL,
                ContextProfile.Compatability,
                ContextFlags.Default,
                new APIVersion(2, 1));

            window = Window.Create(options);
            window.Load += OnLoad;
            window.Render += OnRender;
            window.Closing += OnClosing;
            window.Run();
        }

        private void OnLoad()
        {
            GlState.Gl = GL.GetApi(window);
            input = window.CreateInput();
            mouse = input.Mice.Count > 0 ? input.Mice[0] : null;

            if (input.Keyboards.Count > 0)
            {
                Player.Keyboard = input.Keyboards[0];
                input.Keyboards[0].KeyDown += OnKeyDown;
            }

            if (mouse != null)
            {
                mouse.Cursor.CursorMode = CursorMode.Raw;
                mouse.MouseDown += OnMouseDown;
            }

            width = window.Size.X;
            height = window.Size.Y;

            var gl = GlState.Gl;
            gl.Enable(EnableCap.Texture2D);
            gl.ShadeModel(ShadingModel.Smooth);
            gl.ClearColor(0.5f, 0.8f, 1.0f, 0.0f);
            gl.ClearDepth(1.0);
            gl.Enable(EnableCap.DepthTest);
            gl.DepthFunc(DepthFunction.Lequal);
            gl.MatrixMode(MatrixMode.Projection);
            gl.LoadIdentity();
            gl.MatrixMode(MatrixMode.Modelview);

            level = new Level(256, 256, 64);
            levelRenderer = new LevelRenderer(level);
            player = new Player(level);

            for (int i = 0; i < 100; ++i)
            {
                zombies.Add(new Zombie(level, 128.0f, 0.0f, 128.0f));
            }
        }

        private void OnClosing()
        {
            level?.Save();
        }

        private void OnKeyDown(IKeyboard kb, Key key, int code)
        {
            if (key == Key.Enter)
            {
                level.Save();
            }

            if (key == Key.Escape)
            {
                window.Close();
            }
        }

        private void OnMouseDown(IMouse m, MouseButton button)
        {
            if (button == MouseButton.Right && hitResult != null)
            {
                level.SetTile(hitResult.x, hitResult.y, hitResult.z, 0);
            }

            if (button == MouseButton.Left && hitResult != null)
            {
                int x = hitResult.x;
                int y = hitResult.y;
                int z = hitResult.z;

                if (hitResult.f == 0) --y;
                if (hitResult.f == 1) ++y;
                if (hitResult.f == 2) --z;
                if (hitResult.f == 3) ++z;
                if (hitResult.f == 4) --x;
                if (hitResult.f == 5) ++x;

                level.SetTile(x, y, z, 1);
            }
        }

        public void Tick()
        {
            for (int i = 0; i < zombies.Count; ++i)
            {
                zombies[i].Tick();
            }

            player.Tick();
        }

        private void MoveCameraToPlayer(float a)
        {
            var gl = GlState.Gl;
            gl.Translate(0.0f, 0.0f, -0.3f);
            gl.Rotate(player.xRot, 1.0f, 0.0f, 0.0f);
            gl.Rotate(player.yRot, 0.0f, 1.0f, 0.0f);
            float x = player.xo + (player.x - player.xo) * a;
            float y = player.yo + (player.y - player.yo) * a;
            float z = player.zo + (player.z - player.zo) * a;
            gl.Translate(-x, -y, -z);
        }

        // No GLU in Silk.NET, so gluPerspective is replaced with a manual
        // right-handed perspective projection matrix loaded via glLoadMatrix.
        private unsafe void SetupCamera(float a)
        {
            var gl = GlState.Gl;
            gl.MatrixMode(MatrixMode.Projection);
            gl.LoadIdentity();

            float fovYRad = 70.0f * (float)Math.PI / 180.0f;
            float aspect = (float)width / height;
            float zNear = 0.05f;
            float zFar = 1000.0f;

            Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView(fovYRad, aspect, zNear, zFar);
            float* m = stackalloc float[16]
            {
                proj.M11, proj.M12, proj.M13, proj.M14,
                proj.M21, proj.M22, proj.M23, proj.M24,
                proj.M31, proj.M32, proj.M33, proj.M34,
                proj.M41, proj.M42, proj.M43, proj.M44
            };

            // TODO-VERIFY: exact Silk.NET.OpenGL.Legacy method name/signature
            // for glLoadMatrixf - confirm via IDE intellisense on GlState.Gl.
            gl.LoadMatrix(m);

            gl.MatrixMode(MatrixMode.Modelview);
            gl.LoadIdentity();
            MoveCameraToPlayer(a);
        }

        public void Render(float a)
        {
            if (mouse != null)
            {
                var pos = mouse.Position;
                if (!mouseInitialized)
                {
                    lastMousePos = pos;
                    mouseInitialized = true;
                }

                float dx = pos.X - lastMousePos.X;
                float dy = lastMousePos.Y - pos.Y;
                lastMousePos = pos;
                player.Turn(dx, dy);
            }

            hitResult = levelRenderer.Pick(player, 5.0f);

            var gl = GlState.Gl;
            gl.Clear((uint)(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit));
            SetupCamera(a);
            gl.Enable(EnableCap.CullFace);
            gl.Enable(EnableCap.Fog);
            gl.Fog(FogParameter.FogMode, (int)GLEnum.Exp);
            gl.Fog(FogParameter.FogDensity, 0.2f);

            unsafe
            {
                fixed (float* fogPtr = fogColor)
                {
                    // TODO-VERIFY: exact Silk.NET.OpenGL.Legacy overload for
                    // glFogfv (vector fog color) - confirm via IDE intellisense.
                    gl.Fog(FogParameter.FogColor, fogPtr);
                }
            }

            gl.Disable(EnableCap.Fog);
            levelRenderer.Render(player, 0);

            for (int i = 0; i < zombies.Count; ++i)
            {
                zombies[i].Render(a);
            }

            gl.Enable(EnableCap.Fog);
            levelRenderer.Render(player, 1);
            gl.Disable(EnableCap.Texture2D);

            if (hitResult != null)
            {
                levelRenderer.RenderHit(hitResult);
            }

            gl.Disable(EnableCap.Fog);
        }

        private void OnRender(double deltaSeconds)
        {
            timer.AdvanceTime();

            for (int i = 0; i < timer.ticks; ++i)
            {
                Tick();
            }

            Render(timer.a);
        }

        public static void CheckError()
        {
            var gl = GlState.Gl;
            var e = gl.GetError();
            if (e != GLEnum.NoError)
            {
                throw new InvalidOperationException("GL error: " + e);
            }
        }

        public static void Main(string[] args)
        {
            try
            {
                new RubyDung().Init();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to start RubyDung: " + ex);
            }
        }
    }
}