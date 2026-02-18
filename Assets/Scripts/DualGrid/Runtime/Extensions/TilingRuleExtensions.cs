using UnityEngine;
using static UnityEngine.RuleTile;

namespace DualGrid.Runtime.Extensions
{
    public static class TilingRuleExtensions
    {
        /// <summary>
        ///     Calculates the relative neighbor offset from the dataTileOffset and returns the correct index of the neighbor
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="dataTileOffset"></param>
        /// <returns></returns>
        public static int GetNeighborIndex(this TilingRule rule, Vector3Int dataTileOffset)
        {
            Vector3Int neighborOffsetPosition = DualGridUtils.ConvertDataTileOffsetToNeighborOffset(dataTileOffset);

            var neighborIndex = rule.m_NeighborPositions.IndexOf(neighborOffsetPosition);

            return neighborIndex;
        }
    }
}
