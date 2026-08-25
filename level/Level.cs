using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using mc_c_.phys;

namespace mc_c_.level
{
    public class Level
    {
        public readonly int width;
        public readonly int height;
        public readonly int depth;
        private byte[] blocks;
        private int[] lightDepths;
        private List<LevelListener> levelListeners = new List<LevelListener>();

        public Level(int w, int h, int d)
        {
            width = w;
            height = h;
            depth = d;
            blocks = new byte[w * h * d];
            lightDepths = new int[w * h];

            for (int x = 0; x < w; ++x)
            {
                for (int y = 0; y < d; ++y)
                {
                    for (int z = 0; z < h; ++z)
                    {
                        int i = (y * height + z) * width + x;
                        blocks[i] = (byte)(y <= d * 2 / 3 ? 1 : 0);
                    }
                }
            }

            CalcLightDepths(0, 0, w, h);
            Load();
        }

        public void Load()
        {
            try
            {
                using (var fs = new FileStream("level.dat", FileMode.Open, FileAccess.Read))
                using (var gz = new GZipStream(fs, CompressionMode.Decompress))
                {
                    int offset = 0;
                    int remaining = blocks.Length;
                    while (remaining > 0)
                    {
                        int read = gz.Read(blocks, offset, remaining);
                        if (read <= 0) break;
                        offset += read;
                        remaining -= read;
                    }
                }

                CalcLightDepths(0, 0, width, height);

                for (int i = 0; i < levelListeners.Count; ++i)
                {
                    levelListeners[i].AllChanged();
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }

        public void Save()
        {
            try
            {
                using (var fs = new FileStream("level.dat", FileMode.Create, FileAccess.Write))
                using (var gz = new GZipStream(fs, CompressionMode.Compress))
                {
                    gz.Write(blocks, 0, blocks.Length);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }

        public void CalcLightDepths(int x0, int y0, int x1, int y1)
        {
            for (int x = x0; x < x0 + x1; ++x)
            {
                for (int z = y0; z < y0 + y1; ++z)
                {
                    int oldDepth = lightDepths[x + z * width];

                    int y;
                    for (y = depth - 1; y > 0 && !IsLightBlocker(x, y, z); --y)
                    {
                    }

                    lightDepths[x + z * width] = y;

                    if (oldDepth != y)
                    {
                        int yl0 = oldDepth < y ? oldDepth : y;
                        int yl1 = oldDepth > y ? oldDepth : y;

                        for (int i = 0; i < levelListeners.Count; ++i)
                        {
                            levelListeners[i].LightColumnChanged(x, z, yl0, yl1);
                        }
                    }
                }
            }
        }

        public void AddListener(LevelListener levelListener)
        {
            levelListeners.Add(levelListener);
        }

        public void RemoveListener(LevelListener levelListener)
        {
            levelListeners.Remove(levelListener);
        }

        public bool IsTile(int x, int y, int z)
        {
            return x >= 0 && y >= 0 && z >= 0 && x < width && y < depth && z < height
                && blocks[(y * height + z) * width + x] == 1;
        }

        public bool IsSolidTile(int x, int y, int z)
        {
            return IsTile(x, y, z);
        }

        public bool IsLightBlocker(int x, int y, int z)
        {
            return IsSolidTile(x, y, z);
        }

        public List<AABB> GetCubes(AABB aABB)
        {
            List<AABB> aABBs = new List<AABB>();
            int x0 = (int)aABB.x0;
            int x1 = (int)(aABB.x1 + 1.0f);
            int y0 = (int)aABB.y0;
            int y1 = (int)(aABB.y1 + 1.0f);
            int z0 = (int)aABB.z0;
            int z1 = (int)(aABB.z1 + 1.0f);

            if (x0 < 0) x0 = 0;
            if (y0 < 0) y0 = 0;
            if (z0 < 0) z0 = 0;
            if (x1 > width) x1 = width;
            if (y1 > depth) y1 = depth;
            if (z1 > height) z1 = height;

            for (int x = x0; x < x1; ++x)
            {
                for (int y = y0; y < y1; ++y)
                {
                    for (int z = z0; z < z1; ++z)
                    {
                        if (IsSolidTile(x, y, z))
                        {
                            aABBs.Add(new AABB(x, y, z, x + 1, y + 1, z + 1));
                        }
                    }
                }
            }

            return aABBs;
        }

        public float GetBrightness(int x, int y, int z)
        {
            const float dark = 0.8f;
            const float light = 1.0f;
            return x >= 0 && y >= 0 && z >= 0 && x < width && y < depth && z < height
                ? (y < lightDepths[x + z * width] ? dark : light)
                : light;
        }

        public void SetTile(int x, int y, int z, int type)
        {
            if (x >= 0 && y >= 0 && z >= 0 && x < width && y < depth && z < height)
            {
                blocks[(y * height + z) * width + x] = (byte)type;
                CalcLightDepths(x, z, 1, 1);

                for (int i = 0; i < levelListeners.Count; ++i)
                {
                    levelListeners[i].TileChanged(x, y, z);
                }
            }
        }
    }
}
