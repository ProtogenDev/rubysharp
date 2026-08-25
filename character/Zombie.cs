using System;
using System.Diagnostics;
using Silk.NET.OpenGL.Legacy;
using mc_c_.level;

namespace mc_c_.character
{
    public class Zombie : Entity
    {
        public Cube head;
        public Cube body;
        public Cube arm0;
        public Cube arm1;
        public Cube leg0;
        public Cube leg1;
        public float rot;
        public float timeOffs;
        public float speed;
        public float rotA;

        private static readonly Random rand = new Random();
        private const long NS_PER_SECOND = 1000000000L;

        public Zombie(Level level, float x, float y, float z) : base(level)
        {
            rotA = (float)(rand.NextDouble() + 1.0) * 0.01f;

            this.x = x;
            this.y = y;
            this.z = z;
            timeOffs = (float)rand.NextDouble() * 1239813.0f;
            rot = (float)(rand.NextDouble() * Math.PI * 2.0);
            speed = 1.0f;

            head = new Cube(0, 0);
            head.AddBox(-4.0f, -8.0f, -4.0f, 8, 8, 8);
            body = new Cube(16, 16);
            body.AddBox(-4.0f, 0.0f, -2.0f, 8, 12, 4);
            arm0 = new Cube(40, 16);
            arm0.AddBox(-3.0f, -2.0f, -2.0f, 4, 12, 4);
            arm0.SetPos(-5.0f, 2.0f, 0.0f);
            arm1 = new Cube(40, 16);
            arm1.AddBox(-1.0f, -2.0f, -2.0f, 4, 12, 4);
            arm1.SetPos(5.0f, 2.0f, 0.0f);
            leg0 = new Cube(0, 16);
            leg0.AddBox(-2.0f, 0.0f, -2.0f, 4, 12, 4);
            leg0.SetPos(-2.0f, 12.0f, 0.0f);
            leg1 = new Cube(0, 16);
            leg1.AddBox(-2.0f, 0.0f, -2.0f, 4, 12, 4);
            leg1.SetPos(2.0f, 12.0f, 0.0f);
        }

        public override void Tick()
        {
            xo = x;
            yo = y;
            zo = z;
            float xa;
            float ya;
            rot += rotA;
            rotA = (float)(rotA * 0.99);
            rotA = (float)(rotA + (rand.NextDouble() - rand.NextDouble()) * rand.NextDouble() * rand.NextDouble() * 0.01f);
            xa = (float)Math.Sin(rot);
            ya = (float)Math.Cos(rot);

            if (onGround && rand.NextDouble() < 0.01)
            {
                yd = 0.12f;
            }

            MoveRelative(xa, ya, onGround ? 0.02f : 0.005f);
            yd = (float)(yd - 0.005);
            Move(xd, yd, zd);
            xd *= 0.91f;
            yd *= 0.98f;
            zd *= 0.91f;

            if (y > 100.0f)
            {
                ResetPos();
            }

            if (onGround)
            {
                xd *= 0.8f;
                zd *= 0.8f;
            }
        }

        private static long NanoTime()
        {
            return Stopwatch.GetTimestamp() * NS_PER_SECOND / Stopwatch.Frequency;
        }

        public void Render(float a)
        {
            var gl = GlState.Gl;
            gl.Enable(EnableCap.Texture2D);
            gl.BindTexture(TextureTarget.Texture2D, Textures.LoadTexture("Assets/char.png", (int)GLEnum.Nearest));
            gl.PushMatrix();

            double time = NanoTime() / 1.0e9 * 10.0 * speed + timeOffs;
            float size = 0.058333334f;
            float yy = (float)(-Math.Abs(Math.Sin(time * 0.6662)) * 5.0 - 23.0);

            gl.Translate(xo + (x - xo) * a, yo + (y - yo) * a, zo + (z - zo) * a);
            gl.Scale(1.0f, -1.0f, 1.0f);
            gl.Scale(size, size, size);
            gl.Translate(0.0f, yy, 0.0f);

            const float c = 57.29578f;
            gl.Rotate(rot * c + 180.0f, 0.0f, 1.0f, 0.0f);

            head.yRot = (float)Math.Sin(time * 0.83) * 1.0f;
            head.xRot = (float)Math.Sin(time) * 0.8f;
            arm0.xRot = (float)Math.Sin(time * 0.6662 + Math.PI) * 2.0f;
            arm0.zRot = (float)(Math.Sin(time * 0.2312) + 1.0) * 1.0f;
            arm1.xRot = (float)Math.Sin(time * 0.6662) * 2.0f;
            arm1.zRot = (float)(Math.Sin(time * 0.2812) - 1.0) * 1.0f;
            leg0.xRot = (float)Math.Sin(time * 0.6662) * 1.4f;
            leg1.xRot = (float)Math.Sin(time * 0.6662 + Math.PI) * 1.4f;

            head.Render();
            body.Render();
            arm0.Render();
            arm1.Render();
            leg0.Render();
            leg1.Render();

            gl.PopMatrix();
            gl.Disable(EnableCap.Texture2D);
        }
    }
}
