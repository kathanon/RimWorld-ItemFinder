using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace ItemFinder {
    [HarmonyPatch]
    public static class Storage {
        private static readonly Dictionary<Map,Dictionary<ThingDef,LookTargets>> targets = 
            new Dictionary<Map, Dictionary<ThingDef, LookTargets>>();
        private static readonly Dictionary<Map,Lists> nonResources = 
            new Dictionary<Map, Lists>();
        private static readonly List<ThingDef> empty = new List<ThingDef>();

        public static void Update(Map map) {
            if (!targets.ContainsKey(map)) {
                targets.Add(map, new Dictionary<ThingDef, LookTargets>());
            }
            var targetDict = targets[map];
            if (!nonResources.ContainsKey(map)) {
                nonResources.Add(map, new Lists());
            }
            var nrList = nonResources[map];
            var nrSet = new HashSet<ThingDef>();

            foreach (var target in targetDict.Values) {
                target.targets.Clear();
            }
            nrList.Clear();
            var groups = map.haulDestinationManager.AllGroupsListForReading;
            foreach (var group in groups) {
                foreach (var thing in group.HeldThings) {
                    Thing inner = thing.GetInnerIfMinified();
                    if (!inner.IsNotFresh()) {
                        if (!targetDict.ContainsKey(inner.def)) {
                            targetDict.Add(inner.def, new LookTargets());
                        }
                        targetDict[inner.def].targets.Add(thing);
                        if (!inner.def.CountAsResource && nrSet.Add(inner.def)) {
                            nrList.Add(inner.def);
                        }
                    }
                }
            }
            nrList.Sort();
        }

        public static List<ThingDef> NonResources(Map map, bool alpha) => 
            nonResources.TryGetValue(map, out var val) ? val.List(alpha) : empty;

        public static void HighLightOn(Rect rect, ThingDef thing) {
            if (Mouse.IsOver(rect)) HighLight(thing);
        }

        public static void HighLight(ThingDef thing) {
            if (targets.TryGetValue(Find.CurrentMap, out var dict) && 
                dict.TryGetValue(thing, out var target) && target.Any) {
                target.Highlight(true, false, true);
            }
        }

        private class Lists {
            public readonly List<ThingDef> Standard = new List<ThingDef>();
            public readonly List<ThingDef> Alpha    = new List<ThingDef>();

            private readonly List<ThingDef> temp = new List<ThingDef>();
            private readonly HashSet<ThingDef> set = new HashSet<ThingDef>();

            public List<ThingDef> List(bool alpha) => alpha ? Alpha : Standard;

            public void Clear() {
                Standard.Clear();
                Alpha.Clear();
            }

            public void Sort() {
                SortStandard();
                Alpha.SortBy(def => def.label);
            }

            public void Add(ThingDef thing) {
                Standard.Add(thing);
                Alpha.Add(thing);
            }

            private void SortStandard() {
                set.Clear();
                temp.Clear();
                set.AddRange(Standard);
                Standard.Clear();
                foreach (var node in ThingCategoryNodeDatabase.RootNode.ChildCategoryNodesAndThis) {
                    foreach (var thing in node.catDef.SortedChildThingDefs) {
                        if (set.Remove(thing)) {
                            Standard.Add(thing);
                        }
                    }
                }
                temp.AddRange(set);
                temp.SortBy(def => def.label);
                Standard.AddRange(temp);
                set.Clear();
                temp.Clear();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ResourceCounter), nameof(ResourceCounter.UpdateResourceCounts))]
        public static void UpdateResourceCounts(Map ___map) => Update(___map);
    }
}
