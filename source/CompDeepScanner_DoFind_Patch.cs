using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Smart_Ground_Penetrating_Scanner
{
    [HarmonyPatch(typeof(CompDeepScanner), "DoFind")]
    public static class CompDeepScanner_DoFind_Patch
    {
        private const int MaxMountainAvoidanceRetries = 15;

        [HarmonyPrefix]
        public static bool Prefix(CompDeepScanner __instance, Pawn worker)
        {
            Map map = __instance.parent.Map;

            for (int attempt = 0; attempt < MaxMountainAvoidanceRetries; attempt++)
            {
                IntVec3 center;
                bool hasCenter = CellFinderLoose.TryFindRandomNotEdgeCellWith(
                    10,
                    cell => CanScatterAt(cell, map) && !IsMountainCell(cell, map),
                    map,
                    out center
                );

                if (!hasCenter)
                {
                    break;
                }

                ThingDef thingDef = ChooseLumpThingDef();
                int numCells = Mathf.CeilToInt(thingDef.deepLumpSizeRange.RandomInRange);
                bool placedAny = false;

                foreach (IntVec3 cell in GridShapeMaker.IrregularLump(center, map, numCells))
                {
                    if (CanScatterAt(cell, map) && !cell.InNoBuildEdgeArea(map) && !IsMountainCell(cell, map))
                    {
                        map.deepResourceGrid.SetAt(cell, thingDef, thingDef.deepCountPerCell);
                        placedAny = true;
                    }
                }

                if (!placedAny)
                {
                    continue;
                }

                SendScannerLetter(worker, map, center, thingDef);
                return false;
            }

            return true;
        }

        private static bool CanScatterAt(IntVec3 pos, Map map)
        {
            int index = CellIndicesUtility.CellToIndex(pos, map.Size.x);
            TerrainDef terrainDef = map.terrainGrid.BaseTerrainAt(pos);

            if ((terrainDef != null && terrainDef.IsWater && terrainDef.passability == Traversability.Impassable)
                || !pos.GetAffordances(map).Contains(ThingDefOf.DeepDrill.terrainAffordanceNeeded))
            {
                return false;
            }

            return !map.deepResourceGrid.GetCellBool(index);
        }

        private static bool IsMountainCell(IntVec3 cell, Map map)
        {
            return map.roofGrid.RoofAt(cell) == RoofDefOf.RoofRockThick;
        }

        private static ThingDef ChooseLumpThingDef()
        {
            return DefDatabase<ThingDef>.AllDefs.RandomElementByWeight(def => def.deepCommonality);
        }

        private static void SendScannerLetter(Pawn worker, Map map, IntVec3 center, ThingDef thingDef)
        {
            string key = "LetterDeepScannerFoundLump".CanTranslate()
                ? "LetterDeepScannerFoundLump"
                : (!"DeepScannerFoundLump".CanTranslate() ? "LetterDeepScannerFoundLump" : "DeepScannerFoundLump");

            Find.LetterStack.ReceiveLetter(
                "LetterLabelDeepScannerFoundLump".Translate() + ": " + thingDef.LabelCap,
                key.Translate(thingDef.label, worker.Named("FINDER")),
                LetterDefOf.PositiveEvent,
                new LookTargets(center, map)
            );
        }
    }
}
