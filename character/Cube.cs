using Silk.NET.OpenGL.Legacy;

namespace mc_c_.character
{
    public class Cube
    {
        private Vertex[] vertices;
        private Polygon[] polygons;
        private int xTexOffs;
        private int yTexOffs;
        public float x;
        public float y;
        public float z;
        public float xRot;
        public float yRot;
        public float zRot;

        public Cube(int xTexOffs, int yTexOffs)
        {
            this.xTexOffs = xTexOffs;
            this.yTexOffs = yTexOffs;
        }

        public void SetTexOffs(int xTexOffs, int yTexOffs)
        {
            this.xTexOffs = xTexOffs;
            this.yTexOffs = yTexOffs;
        }

        public void AddBox(float x0, float y0, float z0, int w, int h, int d)
        {
            vertices = new Vertex[8];
            polygons = new Polygon[6];
            float x1 = x0 + w;
            float y1 = y0 + h;
            float z1 = z0 + d;
            Vertex u0 = new Vertex(x0, y0, z0, 0.0f, 0.0f);
            Vertex u1 = new Vertex(x1, y0, z0, 0.0f, 8.0f);
            Vertex u2 = new Vertex(x1, y1, z0, 8.0f, 8.0f);
            Vertex u3 = new Vertex(x0, y1, z0, 8.0f, 0.0f);
            Vertex l0 = new Vertex(x0, y0, z1, 0.0f, 0.0f);
            Vertex l1 = new Vertex(x1, y0, z1, 0.0f, 8.0f);
            Vertex l2 = new Vertex(x1, y1, z1, 8.0f, 8.0f);
            Vertex l3 = new Vertex(x0, y1, z1, 8.0f, 0.0f);
            vertices[0] = u0;
            vertices[1] = u1;
            vertices[2] = u2;
            vertices[3] = u3;
            vertices[4] = l0;
            vertices[5] = l1;
            vertices[6] = l2;
            vertices[7] = l3;
            polygons[0] = new Polygon(new[] { l1, u1, u2, l2 }, xTexOffs + d + w, yTexOffs + d, xTexOffs + d + w + d, yTexOffs + d + h);
            polygons[1] = new Polygon(new[] { u0, l0, l3, u3 }, xTexOffs + 0, yTexOffs + d, xTexOffs + d, yTexOffs + d + h);
            polygons[2] = new Polygon(new[] { l1, l0, u0, u1 }, xTexOffs + d, yTexOffs + 0, xTexOffs + d + w, yTexOffs + d);
            polygons[3] = new Polygon(new[] { u2, u3, l3, l2 }, xTexOffs + d + w, yTexOffs + 0, xTexOffs + d + w + w, yTexOffs + d);
            polygons[4] = new Polygon(new[] { u1, u0, u3, u2 }, xTexOffs + d, yTexOffs + d, xTexOffs + d + w, yTexOffs + d + h);
            polygons[5] = new Polygon(new[] { l0, l1, l2, l3 }, xTexOffs + d + w + d, yTexOffs + d, xTexOffs + d + w + d + w, yTexOffs + d + h);
        }

        public void SetPos(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public unsafe void Render()
        {
            var gl = GlState.Gl;
            const float c = 57.29578f;
            gl.PushMatrix();
            gl.Translate(x, y, z);
            gl.Rotate(zRot * c, 0.0f, 0.0f, 1.0f);
            gl.Rotate(yRot * c, 0.0f, 1.0f, 0.0f);
            gl.Rotate(xRot * c, 1.0f, 0.0f, 0.0f);
            gl.Begin(PrimitiveType.Quads);

            if (polygons != null)
            {
                for (int i = 0; i < polygons.Length; ++i)
                {
                    polygons[i].Render();
                }
            }

            gl.End();
            gl.PopMatrix();
        }
    }
}
