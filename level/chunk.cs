using Silk.NET.OpenGL.Legacy;
using mc_c_.phys;

namespace mc_c_.level
{
    public class Chunk
    {
        public AABB aabb;
        public readonly Level level;
        public readonly int x0;
        public readonly int y0;
        public readonly int z0;
        public readonly int x1;
        public readonly int y1;
        public readonly int z1;
        private bool dirty = true;
        private uint lists;
        private static Tesselator t = new Tesselator();
        public static int rebuiltThisFrame = 0;
        public static int updates = 0;

        public Chunk(Level level, int x0, int y0, int z0, int x1, int y1, int z1)
        {
            this.level = level;
            this.x0 = x0;
            this.y0 = y0;
            this.z0 = z0;
            this.x1 = x1;
            this.y1 = y1;
            this.z1 = z1;
            aabb = new AABB(x0, y0, z0, x1, y1, z1);

            // TODO-VERIFY: exact Silk.NET.OpenGL.Legacy method name/signature
            // for glGenLists - confirm via IDE intellisense on GlState.Gl.
            lists = GlState.Gl.GenLists(2);
        }

        private void Rebuild(int layer)
        {
            if (rebuiltThisFrame != 2)
            {
                var gl = GlState.Gl;

                dirty = false;
                ++updates;
                ++rebuiltThisFrame;
                uint id = Textures.LoadTexture("Assets/terrain.png", (int)GLEnum.Nearest);

                // TODO-VERIFY: exact Silk.NET.OpenGL.Legacy method names/signatures
                // for glNewList / GL_COMPILE / glEndList - confirm via IDE intellisense.
                gl.NewList(lists + (uint)layer, ListMode.Compile);
                gl.Enable(EnableCap.Texture2D);
                gl.BindTexture(TextureTarget.Texture2D, id);
                t.Init();
                int tiles = 0;

                for (int x = x0; x < x1; ++x)
                {
                    for (int y = y0; y < y1; ++y)
                    {
                        for (int z = z0; z < z1; ++z)
                        {
                            if (level.IsTile(x, y, z))
                            {
                                bool tex = y != level.depth * 2 / 3;
                                ++tiles;
                                if (!tex)
                                {
                                    Tile.rock.Render(t, level, layer, x, y, z);
                                }
                                else
                                {
                                    Tile.grass.Render(t, level, layer, x, y, z);
                                }
                            }
                        }
                    }
                }

                t.Flush();
                gl.Disable(EnableCap.Texture2D);
                gl.EndList();
            }
        }

        public void Render(int layer)
        {
            if (dirty)
            {
                Rebuild(0);
                Rebuild(1);
            }

            // TODO-VERIFY: exact Silk.NET.OpenGL.Legacy method name for glCallList.
            GlState.Gl.CallList(lists + (uint)layer);
        }

        public void SetDirty()
        {
            dirty = true;
        }
    }
}
