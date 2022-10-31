using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ItemFinder {
    [StaticConstructorOnStartup]
    public static class Resources {
        public static readonly Texture2D Icon = ContentFinder<Texture2D>.Get("Icon");
        public static readonly Texture2D AlphaSort = ContentFinder<Texture2D>.Get("AlphaSort");
    }
}
