using HarmonyLib;
using RimWorld;
using RimWorld.SketchGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ItemFinder
{
    public class Main : HugsLib.ModBase {
        public override string ModIdentifier => Strings.ID;

        public override void DefsLoaded() {
        }

        public override void MapLoaded(Map map) => Storage.Update(map);
    }
}
