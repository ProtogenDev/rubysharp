#define DEBUG_RAYCAST
using System;
using System.Numerics;
using Silk.NET.OpenGL.Legacy;
using mc_c_.phys;

namespace mc_c_.level
{
    public class LevelRenderer : LevelListener
    {
        private const int CHUNK_SIZE = 16;
        private Level level;
        private Chunk[] chunks;
        private int xChunks;
        private int yChunks;
        private int zChunks;
        private Tesselator t = new Tesselator();

        public LevelRenderer(Level level)
        {
            this.level = level;
            level.AddListener(this);
            xChunks = level.width / 16;
            yChunks = level.depth / 16;
            zChunks = level.height / 16;
            chunks = new Chunk[xChunks * yChunks * zChunks];

            for (int x = 0; x < xChunks; ++x)
            {
                for (int y = 0; y < yChunks; ++y)
                {
                    for (int z = 0; z < zChunks; ++z)
                    {
                        int x0 = x * 16;
                        int y0 = y * 16;
                        int z0 = z * 16;
                        int x1 = (x + 1) * 16;
                        int y1 = (y + 1) * 16;
                        int z1 = (z + 1) * 16;

                        if (x1 > level.width) x1 = level.width;
                        if (y1 > level.depth) y1 = level.depth;
                        if (z1 > level.height) z1 = level.height;

                        chunks[(x + y * xChunks) * zChunks + z] = new Chunk(level, x0, y0, z0, x1, y1, z1);
                    }
                }
            }
        }

        public void Render(Player player, int layer)
        {
            Chunk.rebuiltThisFrame = 0;
            Frustum frustum = Frustum.GetFrustum();

            for (int i = 0; i < chunks.Length; ++i)
            {
                if (frustum.CubeInFrustum(chunks[i].aabb))
                {
                    chunks[i].Render(layer);
                }
            }
        }

        // Original Java used the (now-removed) OpenGL selection-buffer
        // picking API (glInitNames/glPushName/GL_SELECT render mode) to
        // figure out what block the player is looking at. That's gone in
        // modern GL and isn't meaningfully exposed by Silk.NET, so this is
        // replaced with a straightforward voxel DDA raycast from the
        // player's eye along their look direction - the standard modern
        // equivalent, and it's more reliable than selection-buffer picking
        // ever was.
        public HitResult Pick(Player player, float reach)
        {
            double px = player.x;
            double py = player.y;
            double pz = player.z;

            double yawRad = player.yRot * Math.PI / 180.0;
            double pitchRad = player.xRot * Math.PI / 180.0;

            // Built directly from OpenGL's glRotate spec (right-hand rule,
            // counterclockwise about the axis) applied in the same order as
            // MoveCameraToPlayer: Rx(xRot) then Ry(yRot) on top of the
            // camera-space forward vector (0,0,-1), matching how the actual
            // fixed-function pipeline composes it. Computed via Matrix4x4
            // instead of hand trig to remove any derivation error.
            Matrix4x4 rx = Matrix4x4.CreateRotationX((float)-pitchRad);
            Matrix4x4 ry = Matrix4x4.CreateRotationY((float)-yawRad);
            Vector3 forward = Vector3.Transform(new Vector3(0, 0, -1), rx * ry);

            double dx = forward.X;
            double dy = forward.Y;
            double dz = forward.Z;

//#if DEBUG_RAYCAST
//            Console.WriteLine($"yRot={player.yRot:F1} xRot={player.xRot:F1} dir=({dx:F2},{dy:F2},{dz:F2})");
//#endif

            int x = (int)Math.Floor(px);
            int y = (int)Math.Floor(py);
            int z = (int)Math.Floor(pz);

            int stepX = dx > 0 ? 1 : -1;
            int stepY = dy > 0 ? 1 : -1;
            int stepZ = dz > 0 ? 1 : -1;

            double tDeltaX = dx != 0 ? Math.Abs(1.0 / dx) : double.PositiveInfinity;
            double tDeltaY = dy != 0 ? Math.Abs(1.0 / dy) : double.PositiveInfinity;
            double tDeltaZ = dz != 0 ? Math.Abs(1.0 / dz) : double.PositiveInfinity;

            double tMaxX = dx != 0 ? ((dx > 0 ? (x + 1 - px) : (px - x)) / Math.Abs(dx)) : double.PositiveInfinity;
            double tMaxY = dy != 0 ? ((dy > 0 ? (y + 1 - py) : (py - y)) / Math.Abs(dy)) : double.PositiveInfinity;
            double tMaxZ = dz != 0 ? ((dz > 0 ? (z + 1 - pz) : (pz - z)) / Math.Abs(dz)) : double.PositiveInfinity;

            int lastFace = -1;
            double traveled = 0.0;

            while (traveled <= reach)
            {
                if (level.IsSolidTile(x, y, z) && lastFace >= 0)
                {
                    return new HitResult(x, y, z, 0, lastFace);
                }

                if (tMaxX < tMaxY && tMaxX < tMaxZ)
                {
                    x += stepX;
                    traveled = tMaxX;
                    tMaxX += tDeltaX;
                    lastFace = stepX > 0 ? 4 : 5;
                }
                else if (tMaxY < tMaxZ)
                {
                    y += stepY;
                    traveled = tMaxY;
                    tMaxY += tDeltaY;
                    lastFace = stepY > 0 ? 0 : 1;
                }
                else
                {
                    z += stepZ;
                    traveled = tMaxZ;
                    tMaxZ += tDeltaZ;
                    lastFace = stepZ > 0 ? 2 : 3;
                }
            }

            return null;
        }

        public void RenderHit(HitResult h)
        {
            var gl = GlState.Gl;
            gl.Enable(EnableCap.Blend);
            gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
            float alpha = (float)Math.Sin(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 100.0) * 0.2f + 0.4f;
            gl.Color4(1.0f, 1.0f, 1.0f, alpha);
            t.Init();
            Tile.rock.RenderFace(t, h.x, h.y, h.z, h.f);
            t.Flush();
            gl.Disable(EnableCap.Blend);
        }

        public void SetDirty(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            x0 /= 16;
            x1 /= 16;
            y0 /= 16;
            y1 /= 16;
            z0 /= 16;
            z1 /= 16;

            if (x0 < 0) x0 = 0;
            if (y0 < 0) y0 = 0;
            if (z0 < 0) z0 = 0;
            if (x1 >= xChunks) x1 = xChunks - 1;
            if (y1 >= yChunks) y1 = yChunks - 1;
            if (z1 >= zChunks) z1 = zChunks - 1;

            for (int x = x0; x <= x1; ++x)
            {
                for (int y = y0; y <= y1; ++y)
                {
                    for (int z = z0; z <= z1; ++z)
                    {
                        chunks[(x + y * xChunks) * zChunks + z].SetDirty();
                    }
                }
            }
        }

        public void TileChanged(int x, int y, int z)
        {
            SetDirty(x - 1, y - 1, z - 1, x + 1, y + 1, z + 1);
        }

        public void LightColumnChanged(int x, int z, int y0, int y1)
        {
            SetDirty(x - 1, y0 - 1, z - 1, x + 1, y1 + 1, z + 1);
        }

        public void AllChanged()
        {
            SetDirty(0, 0, 0, level.width, level.depth, level.height);
        }
    }
}