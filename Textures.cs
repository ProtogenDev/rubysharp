using System;
using System.Collections.Generic;
using System.IO;
using Silk.NET.OpenGL.Legacy;
using StbImageSharp;

namespace mc_c_
{
    public class Textures
    {
        private static readonly Dictionary<string, uint> idMap = new Dictionary<string, uint>();

        // resourceName is expected to be a path to an image file on disk
        // (the original Java version pulled these from embedded resources
        // via getResourceAsStream - here we just read from the filesystem,
        // e.g. an "Assets" folder copied to the output directory).
        public static unsafe uint LoadTexture(string resourceName, int mode)
        {
            if (idMap.TryGetValue(resourceName, out uint existing))
            {
                return existing;
            }

            var gl = GlState.Gl;
            uint id = gl.GenTexture();
            idMap[resourceName] = id;
            Console.WriteLine(resourceName + " -> " + id);

            gl.BindTexture(TextureTarget.Texture2D, id);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, mode);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, mode);

            byte[] fileBytes = File.ReadAllBytes(resourceName);
            // Java's BufferedImage/ImageIO reads top-down; StbImage by default
            // reads bottom-up like GL expects, so keep default (no vertical flip)
            // to match the original's raw getRGB(...) row order into GL.
            ImageResult image = ImageResult.FromMemory(fileBytes, ColorComponents.RedGreenBlueAlpha);

            fixed (byte* pixelsPtr = image.Data)
            {
                gl.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    (int)InternalFormat.Rgba,
                    (uint)image.Width,
                    (uint)image.Height,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    pixelsPtr);
            }

            gl.GenerateMipmap(TextureTarget.Texture2D);

            return id;
        }
    }
}
