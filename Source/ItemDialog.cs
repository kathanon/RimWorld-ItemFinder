using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ItemFinder {
    public class ItemDialog : Window {
        public const float Width        = 180f;
        public const float Height       = 500f;
        public const float MarginSize   =  10f;
        public const float Gap          =   4f;
        public const float ScrollMargin =  18f;
        public const float ItemHeight   =  26f;
        public const float IconSize     =  24f;

        private const int AlphaTipID = 648516612;

        public static readonly ItemDialog Instance = new ItemDialog();

        private readonly List<ThingDef> filtered = new List<ThingDef>();
        private readonly QuickSearchWidget search = new QuickSearchWidget();
        private Vector2 scrollPos;
        private bool visible;
        private bool alphaSort = false;

        public ItemDialog() {
            closeOnAccept = false;
            closeOnCancel = false;
            preventCameraMotion = false;
            draggable = true;
            resizeable = true;
            focusWhenOpened = true;
            shadowAlpha = .7f;
            soundAppear = null;
            soundClose = null;
            windowRect = new Rect(0f, 0f, Width, Height);
        }

        public static bool Show {
            get => Instance.visible;
            set {
                if (Instance.visible != value) {
                    if (value) {
                        Find.WindowStack.Add(Instance);
                    } else {
                        Instance.Close();
                    }
                }
                Instance.visible = value;
            }
        }

        protected override void SetInitialSizeAndPosition() { }

        protected override float Margin => MarginSize;

        private List<ThingDef> Items => Storage.NonResources(Find.CurrentMap, alphaSort);

        private void UpdateFiltered() {
            filtered.Clear();
            filtered.AddRange(Items.Where(def => search.filter.Matches(def)));
        }

        public override void DoWindowContents(Rect rect) {
            DoSearchAndSort(rect);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            var list = search.filter.Active ? filtered : Items;
            rect.yMin += QuickSearchWidget.WidgetHeight + Gap;
            Rect view = rect.AtZero();
            view.width -= ScrollMargin;
            view.height = (list.Count - 1) * ItemHeight + IconSize;
            int first = (int) (scrollPos.y / ItemHeight);
            int last = Math.Min(
                first + (int) (rect.height / ItemHeight) + 1,
                list.Count - 1);

            Widgets.BeginScrollView(rect, ref scrollPos, view);
            Rect row = view.TopPartPixels(IconSize);
            row.y = first * ItemHeight;
            foreach (var thing in list) {
                Widgets.ThingIcon(row.LeftPartPixels(row.height), thing);
                Rect labelRect = row.RightPartPixels(row.width - row.height - Gap);
                Widgets.Label(labelRect, thing.LabelCap);
                Storage.HighLightOn(row, thing);
                row.y += ItemHeight;
            }
            Widgets.EndScrollView();
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DoSearchAndSort(Rect rect) {
            Rect searchRect = rect.TopPartPixels(QuickSearchWidget.WidgetHeight);
            Rect alphaRect = searchRect.RightPartPixels(QuickSearchWidget.WidgetHeight);
            searchRect.width -= Gap + QuickSearchWidget.WidgetHeight;
            search.OnGUI(searchRect, UpdateFiltered);
            if (alphaSort) {
                Widgets.DrawBoxSolid(alphaRect, Widgets.MenuSectionBGFillColor);
            }

            if (Widgets.ButtonImage(alphaRect, Resources.AlphaSort)) {
                alphaSort = !alphaSort;
            }

            if (Mouse.IsOver(alphaRect)) {
                TooltipHandler.TipRegion(alphaRect, () => "Sort alphabetically", AlphaTipID);
            }
        }
    }
}
