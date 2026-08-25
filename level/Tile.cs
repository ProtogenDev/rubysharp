namespace mc_c_.level
{
    public class Tile
    {
        public static readonly Tile rock = new Tile(0);
        public static readonly Tile grass = new Tile(1);
        private int tex;

        private Tile(int tex)
        {
            this.tex = tex;
        }

        public void Render(Tesselator t, Level level, int layer, int x, int y, int z)
        {
            float u0 = tex / 16.0f;
            float u1 = u0 + 0.999f / 16.0f;
            float v0 = 0.0f;
            float v1 = v0 + 0.999f / 16.0f;
            const float c1 = 1.0f;
            const float c2 = 0.8f;
            const float c3 = 0.6f;
            float x0 = x + 0.0f;
            float x1 = x + 1.0f;
            float y0 = y + 0.0f;
            float y1 = y + 1.0f;
            float z0 = z + 0.0f;
            float z1 = z + 1.0f;
            float br;

            if (!level.IsSolidTile(x, y - 1, z))
            {
                br = level.GetBrightness(x, y - 1, z) * c1;
                if ((br == c1) ^ (layer == 1))
                {
                    t.Color(br, br, br);
                    t.Tex(u0, v1);
                    t.Vertex(x0, y0, z1);
                    t.Tex(u0, v0);
                    t.Vertex(x0, y0, z0);
                    t.Tex(u1, v0);
                    t.Vertex(x1, y0, z0);
                    t.Tex(u1, v1);
                    t.Vertex(x1, y0, z1);
                }
            }

            if (!level.IsSolidTile(x, y + 1, z))
            {
                br = level.GetBrightness(x, y, z) * c1;
                if ((br == c1) ^ (layer == 1))
                {
                    t.Color(br, br, br);
                    t.Tex(u1, v1);
                    t.Vertex(x1, y1, z1);
                    t.Tex(u1, v0);
                    t.Vertex(x1, y1, z0);
                    t.Tex(u0, v0);
                    t.Vertex(x0, y1, z0);
                    t.Tex(u0, v1);
                    t.Vertex(x0, y1, z1);
                }
            }

            if (!level.IsSolidTile(x, y, z - 1))
            {
                br = level.GetBrightness(x, y, z - 1) * c2;
                if ((br == c2) ^ (layer == 1))
                {
                    t.Color(br, br, br);
                    t.Tex(u1, v0);
                    t.Vertex(x0, y1, z0);
                    t.Tex(u0, v0);
                    t.Vertex(x1, y1, z0);
                    t.Tex(u0, v1);
                    t.Vertex(x1, y0, z0);
                    t.Tex(u1, v1);
                    t.Vertex(x0, y0, z0);
                }
            }

            if (!level.IsSolidTile(x, y, z + 1))
            {
                br = level.GetBrightness(x, y, z + 1) * c2;
                if ((br == c2) ^ (layer == 1))
                {
                    t.Color(br, br, br);
                    t.Tex(u0, v0);
                    t.Vertex(x0, y1, z1);
                    t.Tex(u0, v1);
                    t.Vertex(x0, y0, z1);
                    t.Tex(u1, v1);
                    t.Vertex(x1, y0, z1);
                    t.Tex(u1, v0);
                    t.Vertex(x1, y1, z1);
                }
            }

            if (!level.IsSolidTile(x - 1, y, z))
            {
                br = level.GetBrightness(x - 1, y, z) * c3;
                if ((br == c3) ^ (layer == 1))
                {
                    t.Color(br, br, br);
                    t.Tex(u1, v0);
                    t.Vertex(x0, y1, z1);
                    t.Tex(u0, v0);
                    t.Vertex(x0, y1, z0);
                    t.Tex(u0, v1);
                    t.Vertex(x0, y0, z0);
                    t.Tex(u1, v1);
                    t.Vertex(x0, y0, z1);
                }
            }

            if (!level.IsSolidTile(x + 1, y, z))
            {
                br = level.GetBrightness(x + 1, y, z) * c3;
                if ((br == c3) ^ (layer == 1))
                {
                    t.Color(br, br, br);
                    t.Tex(u0, v1);
                    t.Vertex(x1, y0, z1);
                    t.Tex(u1, v1);
                    t.Vertex(x1, y0, z0);
                    t.Tex(u1, v0);
                    t.Vertex(x1, y1, z0);
                    t.Tex(u0, v0);
                    t.Vertex(x1, y1, z1);
                }
            }
        }

        public void RenderFace(Tesselator t, int x, int y, int z, int face)
        {
            float x0 = x + 0.0f;
            float x1 = x + 1.0f;
            float y0 = y + 0.0f;
            float y1 = y + 1.0f;
            float z0 = z + 0.0f;
            float z1 = z + 1.0f;

            if (face == 0)
            {
                t.Vertex(x0, y0, z1);
                t.Vertex(x0, y0, z0);
                t.Vertex(x1, y0, z0);
                t.Vertex(x1, y0, z1);
            }

            if (face == 1)
            {
                t.Vertex(x1, y1, z1);
                t.Vertex(x1, y1, z0);
                t.Vertex(x0, y1, z0);
                t.Vertex(x0, y1, z1);
            }

            if (face == 2)
            {
                t.Vertex(x0, y1, z0);
                t.Vertex(x1, y1, z0);
                t.Vertex(x1, y0, z0);
                t.Vertex(x0, y0, z0);
            }

            if (face == 3)
            {
                t.Vertex(x0, y1, z1);
                t.Vertex(x0, y0, z1);
                t.Vertex(x1, y0, z1);
                t.Vertex(x1, y1, z1);
            }

            if (face == 4)
            {
                t.Vertex(x0, y1, z1);
                t.Vertex(x0, y1, z0);
                t.Vertex(x0, y0, z0);
                t.Vertex(x0, y0, z1);
            }

            if (face == 5)
            {
                t.Vertex(x1, y0, z1);
                t.Vertex(x1, y0, z0);
                t.Vertex(x1, y1, z0);
                t.Vertex(x1, y1, z1);
            }
        }
    }
}
