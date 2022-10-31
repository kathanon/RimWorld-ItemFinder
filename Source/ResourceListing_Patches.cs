using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ItemFinder {
    [HarmonyPatch]
    public static class ResourceListing_Patches {
        // Normal readout
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ResourceReadout), nameof(ResourceReadout.DrawResourceSimple))]
        public static void DrawResourceSimple(Rect rect, ThingDef thingDef) => 
            Storage.HighLightOn(rect, thingDef);


        // Tree readout
        private static ThingDef thing = null;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Listing_ResourceReadout), "DoThingDef")]
        public static void DoThingDef_Pre(ThingDef thingDef) => thing = thingDef;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Listing_ResourceReadout), "DoThingDef")]
        public static void DoThingDef_Post() => thing = null;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mouse), nameof(Mouse.IsOver))]
        public static void Mouse_IsOver(bool __result) {
            if (thing != null) {
                if (__result) Storage.HighLight(thing);
                thing = null;
            }
        }
    }
}
