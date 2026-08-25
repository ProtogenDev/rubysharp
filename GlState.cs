using Silk.NET.OpenGL.Legacy;

namespace mc_c_
{
    // Holds the single active Silk.NET GL binding for the app.
    //
    // This project is built entirely around fixed-function / immediate-mode
    // OpenGL (glBegin/glVertex, the matrix stack, display lists) which is
    // exactly what the original Java "RubyDung" / classic-Minecraft source
    // used. Modern core-profile OpenGL removed all of that, so we deliberately
    // use Silk.NET.OpenGL.Legacy (which targets a compatibility-profile
    // context) instead of Silk.NET.OpenGL (core). Everything that used to
    // call the static Java "GL11.xxx()" calls now calls GlState.Gl.xxx().
    public static class GlState
    {
        public static GL Gl;
    }
}
