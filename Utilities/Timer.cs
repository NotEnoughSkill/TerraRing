using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace TerraRing.Utilities
{
    public static class Timer
    {
        public static void Start(Action action, int delayInFrames)
        {
            ModContent.GetInstance<TimerSystem>().CreateTimer(action, delayInFrames);
        }
    }

    public class TimerSystem : ModSystem
    {
        private class TimerEntry
        {
            public Action Action { get; set; }
            public int RemainingFrames { get; set; }
        }

        private System.Collections.Generic.List<TimerEntry> timers = new();

        public void CreateTimer(Action action, int delayInFrames)
        {
            timers.Add(new TimerEntry { Action = action, RemainingFrames = delayInFrames });
        }

        public override void PostUpdateEverything()
        {
            for (int i = timers.Count - 1; i >= 0; i--)
            {
                timers[i].RemainingFrames--;
                if (timers[i].RemainingFrames <= 0)
                {
                    timers[i].Action();
                    timers.RemoveAt(i);
                }
            }
        }
    }
}