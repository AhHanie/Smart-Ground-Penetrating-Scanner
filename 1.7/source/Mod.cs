using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using HarmonyLib;

namespace Smart_Ground_Penetrating_Scanner
{
    public class Mod: Verse.Mod
    {
        public Mod(ModContentPack content) : base(content)
        {
            LongEventHandler.QueueLongEvent(Init, "SmartGroundScanner.LoadingLabel", doAsynchronously: true, null);
        }

        public void Init()
        {
            new Harmony("sk.smartgroundscanner").PatchAll();
        }
    }
}
