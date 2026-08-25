using System.Diagnostics;

namespace mc_c_
{
    public class Timer
    {
        private const long NS_PER_SECOND = 1000000000L;
        private const long MAX_NS_PER_UPDATE = 1000000000L;
        private const int MAX_TICKS_PER_UPDATE = 100;
        private float ticksPerSecond;
        private long lastTime;
        public int ticks;
        public float a;
        public float timeScale = 1.0f;
        public float fps = 0.0f;
        public float passedTime = 0.0f;

        // Java's System.nanoTime() is an arbitrary monotonic clock - the
        // .NET equivalent is Stopwatch's tick count converted to nanoseconds.
        private static long NanoTime()
        {
            return Stopwatch.GetTimestamp() * NS_PER_SECOND / Stopwatch.Frequency;
        }

        public Timer(float ticksPerSecond)
        {
            this.ticksPerSecond = ticksPerSecond;
            lastTime = NanoTime();
        }

        public void AdvanceTime()
        {
            long now = NanoTime();
            long passedNs = now - lastTime;
            lastTime = now;

            if (passedNs < 0L) passedNs = 0L;
            if (passedNs > NS_PER_SECOND) passedNs = NS_PER_SECOND;

            fps = (float)(NS_PER_SECOND / passedNs);
            passedTime += passedNs * timeScale * ticksPerSecond / 1.0e9f;
            ticks = (int)passedTime;

            if (ticks > MAX_TICKS_PER_UPDATE) ticks = MAX_TICKS_PER_UPDATE;

            passedTime -= ticks;
            a = passedTime;
        }
    }
}
