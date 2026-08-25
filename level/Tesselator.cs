using Silk.NET.OpenGL.Legacy;

namespace mc_c_.level
{
    public class Tesselator
    {
        private const int MAX_VERTICES = 100000;

        private float[] vertexBuffer = new float[300000];
        private float[] texCoordBuffer = new float[200000];
        private float[] colorBuffer = new float[300000];
        private int vertices = 0;
        private float u;
        private float v;
        private float r;
        private float g;
        private float b;
        private bool hasColor = false;
        private bool hasTexture = false;

        public unsafe void Flush()
        {
            var gl = GlState.Gl;

            fixed (float* vPtr = vertexBuffer)
            fixed (float* tPtr = texCoordBuffer)
            fixed (float* cPtr = colorBuffer)
            {
                gl.VertexPointer(3, VertexPointerType.Float, 0, vPtr);

                if (hasTexture)
                {
                    gl.TexCoordPointer(2, TexCoordPointerType.Float, 0, tPtr);
                }

                if (hasColor)
                {
                    gl.ColorPointer(3, ColorPointerType.Float, 0, cPtr);
                }

                gl.EnableClientState(EnableCap.VertexArray);
                if (hasTexture) gl.EnableClientState(EnableCap.TextureCoordArray);
                if (hasColor) gl.EnableClientState(EnableCap.ColorArray);

                gl.DrawArrays(PrimitiveType.Quads, 0, (uint)vertices);

                gl.DisableClientState(EnableCap.VertexArray);
                if (hasTexture) gl.DisableClientState(EnableCap.TextureCoordArray);
                if (hasColor) gl.DisableClientState(EnableCap.ColorArray);
            }

            Clear();
        }

        private void Clear()
        {
            vertices = 0;
        }

        public void Init()
        {
            Clear();
            hasColor = false;
            hasTexture = false;
        }

        public void Tex(float u, float v)
        {
            hasTexture = true;
            this.u = u;
            this.v = v;
        }

        public void Color(float r, float g, float b)
        {
            hasColor = true;
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public void Vertex(float x, float y, float z)
        {
            vertexBuffer[vertices * 3 + 0] = x;
            vertexBuffer[vertices * 3 + 1] = y;
            vertexBuffer[vertices * 3 + 2] = z;

            if (hasTexture)
            {
                texCoordBuffer[vertices * 2 + 0] = u;
                texCoordBuffer[vertices * 2 + 1] = v;
            }

            if (hasColor)
            {
                colorBuffer[vertices * 3 + 0] = r;
                colorBuffer[vertices * 3 + 1] = g;
                colorBuffer[vertices * 3 + 2] = b;
            }

            ++vertices;
            if (vertices == MAX_VERTICES)
            {
                Flush();
            }
        }
    }
}
