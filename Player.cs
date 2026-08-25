using Silk.NET.Input;
using mc_c_.level;

namespace mc_c_
{
    public class Player : Entity
    {
        // Set once at startup by the app's input setup code - polled here
        // each tick the same way the original polled org.lwjgl.input.Keyboard.
        public static IKeyboard Keyboard;

        public Player(Level level) : base(level)
        {
            heightOffset = 1.62f;
        }

        public override void Tick()
        {
            xo = x;
            yo = y;
            zo = z;
            float xa = 0.0f;
            float ya = 0.0f;

            if (Keyboard.IsKeyPressed(Key.R))
            {
                ResetPos();
            }

            if (Keyboard.IsKeyPressed(Key.Up) || Keyboard.IsKeyPressed(Key.W))
            {
                --ya;
            }

            if (Keyboard.IsKeyPressed(Key.Down) || Keyboard.IsKeyPressed(Key.S))
            {
                ++ya;
            }

            if (Keyboard.IsKeyPressed(Key.Left) || Keyboard.IsKeyPressed(Key.A))
            {
                --xa;
            }

            if (Keyboard.IsKeyPressed(Key.Right) || Keyboard.IsKeyPressed(Key.D))
            {
                ++xa;
            }

            if ((Keyboard.IsKeyPressed(Key.Space) || Keyboard.IsKeyPressed(Key.SuperLeft)) && onGround)
            {
                yd = 0.12f;
            }

            MoveRelative(xa, ya, onGround ? 0.02f : 0.005f);
            yd = (float)(yd - 0.005);
            Move(xd, yd, zd);
            xd *= 0.91f;
            yd *= 0.98f;
            zd *= 0.91f;

            if (onGround)
            {
                xd *= 0.8f;
                zd *= 0.8f;
            }
        }
    }
}
