using Silk.NET.OpenGL.Legacy;

namespace mc_c_.character
{
    public class Polygon
    {
        public Vertex[] vertices;
        public int vertexCount;

        public Polygon(Vertex[] vertices)
        {
            vertexCount = 0;
            this.vertices = vertices;
            vertexCount = vertices.Length;
        }

        public Polygon(Vertex[] vertices, int u0, int v0, int u1, int v1)
            : this(vertices)
        {
            vertices[0] = vertices[0].Remap(u1, v0);
            vertices[1] = vertices[1].Remap(u0, v0);
            vertices[2] = vertices[2].Remap(u0, v1);
            vertices[3] = vertices[3].Remap(u1, v1);
        }

        public void Render()
        {
            var gl = GlState.Gl;
            gl.Color3(1.0f, 1.0f, 1.0f);

            for (int i = 3; i >= 0; --i)
            {
                Vertex v = vertices[i];
                gl.TexCoord2(v.u / 64.0f, v.v / 32.0f);
                gl.Vertex3(v.pos.x, v.pos.y, v.pos.z);
            }
        }
    }
}
