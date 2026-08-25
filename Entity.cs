using System;
using System.Collections.Generic;
using mc_c_.level;
using mc_c_.phys;

namespace mc_c_
{
    public class Entity
    {
        private Level level;
        public float xo;
        public float yo;
        public float zo;
        public float x;
        public float y;
        public float z;
        public float xd;
        public float yd;
        public float zd;
        public float yRot;
        public float xRot;
        public AABB bb;
        public bool onGround = false;
        protected float heightOffset = 0.0f;

        private static readonly Random rand = new Random();

        public Entity(Level level)
        {
            this.level = level;
            ResetPos();
        }

        protected virtual void ResetPos()
        {
            float x = (float)rand.NextDouble() * level.width;
            float y = level.depth + 10;
            float z = (float)rand.NextDouble() * level.height;
            SetPos(x, y, z);
        }

        private void SetPos(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            float w = 0.3f;
            float h = 0.9f;
            bb = new AABB(x - w, y - h, z - w, x + w, y + h, z + w);
        }

        public void Turn(float xo, float yo)
        {
            yRot = (float)(yRot + xo * 0.15);
            xRot = (float)(xRot - yo * 0.15);

            if (xRot < -90.0f) xRot = -90.0f;
            if (xRot > 90.0f) xRot = 90.0f;
        }

        public virtual void Tick()
        {
            xo = x;
            yo = y;
            zo = z;
        }

        public virtual void Move(float xa, float ya, float za)
        {
            float xaOrg = xa;
            float yaOrg = ya;
            float zaOrg = za;
            List<AABB> aABBs = level.GetCubes(bb.Expand(xa, ya, za));

            int i;
            for (i = 0; i < aABBs.Count; ++i)
            {
                ya = aABBs[i].ClipYCollide(bb, ya);
            }

            bb.Move(0.0f, ya, 0.0f);

            for (i = 0; i < aABBs.Count; ++i)
            {
                xa = aABBs[i].ClipXCollide(bb, xa);
            }

            bb.Move(xa, 0.0f, 0.0f);

            for (i = 0; i < aABBs.Count; ++i)
            {
                za = aABBs[i].ClipZCollide(bb, za);
            }

            bb.Move(0.0f, 0.0f, za);
            onGround = yaOrg != ya && yaOrg < 0.0f;

            if (xaOrg != xa) xd = 0.0f;
            if (yaOrg != ya) yd = 0.0f;
            if (zaOrg != za) zd = 0.0f;

            x = (bb.x0 + bb.x1) / 2.0f;
            y = bb.y0 + heightOffset;
            z = (bb.z0 + bb.z1) / 2.0f;
        }

        public void MoveRelative(float xa, float za, float speed)
        {
            float dist = xa * xa + za * za;
            if (dist >= 0.01f)
            {
                dist = speed / (float)Math.Sqrt(dist);
                xa *= dist;
                za *= dist;
                float sin = (float)Math.Sin(yRot * Math.PI / 180.0);
                float cos = (float)Math.Cos(yRot * Math.PI / 180.0);
                xd += xa * cos - za * sin;
                zd += za * cos + xa * sin;
            }
        }
    }
}
