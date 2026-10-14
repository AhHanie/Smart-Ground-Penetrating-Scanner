using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Smart_Ground_Penetrating_Scanner
{
    [HarmonyPatch(typeof(CompDeepScanner), "DoFind")]
    public static class CompDeepScanner_DoFind_Patch
    {
        [HarmonyPostfix]
        [HarmonyInfix(typeof(CellFinderLoose), nameof(CellFinderLoose.TryFindRandomNotEdgeCellWith))]
        public static bool CenterPostfix(bool __result, int minEdgeDistance, Predicate<IntVec3> validator, Map map, ref IntVec3 result)
        {
            if (!__result || !IsMountainCell(result, map))
            {
                return __result;
            }

            if (CellFinderLoose.TryFindRandomNotEdgeCellWith(
                minEdgeDistance,
                cell => validator(cell) && !IsMountainCell(cell, map),
                map,
                out IntVec3 openCell))
            {
                result = openCell;
            }

            return __result;
        }

        [HarmonyPostfix]
        [HarmonyInfix(typeof(GridShapeMaker), nameof(GridShapeMaker.IrregularLump))]
        public static List<IntVec3> LumpPostfix(List<IntVec3> __result, IntVec3 center, Map map)
        {
            if (IsMountainCell(center, map))
            {
                return __result;
            }

            List<IntVec3> openCells = __result.FindAll(cell => !IsMountainCell(cell, map));
            return openCells.Count > 0 ? openCells : __result;
        }

        private static bool IsMountainCell(IntVec3 cell, Map map)
        {
            return map.roofGrid.RoofAt(cell) == RoofDefOf.RoofRockThick;
        }
    }
}
