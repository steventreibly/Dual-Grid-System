using System.Collections.Generic;
using DualGrid.Tiles;
using DualGrid.Utils;
using UnityEngine;

namespace DualGrid.Editor
{
    public class AutoDualGridRuleTileProvider
    {
        private static readonly Vector3Int UpRightNeighbor = Vector3Int.up + Vector3Int.right;
        private static readonly Vector3Int UpLeftNeighbor = Vector3Int.up + Vector3Int.left;
        private static readonly Vector3Int DownRightNeighbor = Vector3Int.down + Vector3Int.right;
        private static readonly Vector3Int DownLeftNeighbor = Vector3Int.down + Vector3Int.left;

        private readonly struct NeighborPattern
        {
            public Vector3Int Position { get; }
            public int State { get; }

            public NeighborPattern(Vector3Int position, int state)
            {
                Position = position;
                State = state;
            }

            private static List<NeighborPattern> CreatePattern(int upLeft, int upRight, int downLeft, int downRight)
            {
                return new List<NeighborPattern>
                {
                    new NeighborPattern(UpLeftNeighbor, upLeft),
                    new NeighborPattern(UpRightNeighbor, upRight),
                    new NeighborPattern(DownLeftNeighbor, downLeft),
                    new NeighborPattern(DownRightNeighbor, downRight)
                };
            }

            /// <summary>
            ///     Provides all permutations of possible neighbor patterns. It is simpler and more performant to hard code than a permutation algorithm.
            /// </summary>
            private static readonly Dictionary<int, List<NeighborPattern>> IndexedNeighborConfigurations =
                new Dictionary<int, List<NeighborPattern>>()
                {
                    {0, CreatePattern(DualGridNeighbor.NotFilled, DualGridNeighbor.NotFilled, DualGridNeighbor.NotFilled,DualGridNeighbor.NotFilled)},
                    {1, CreatePattern(DualGridNeighbor.NotFilled, DualGridNeighbor.NotFilled, DualGridNeighbor.NotFilled,DualGridNeighbor.Filled)},
                    {2, CreatePattern(DualGridNeighbor.NotFilled, DualGridNeighbor.NotFilled, DualGridNeighbor.Filled, DualGridNeighbor.Filled)},
                    {3, CreatePattern(DualGridNeighbor.NotFilled, DualGridNeighbor.Filled, DualGridNeighbor.Filled, DualGridNeighbor.Filled)},
                    {4, CreatePattern(DualGridNeighbor.Filled, DualGridNeighbor.Filled, DualGridNeighbor.Filled, DualGridNeighbor.Filled)},
                    {5, CreatePattern(DualGridNeighbor.Filled, DualGridNeighbor.NotFilled, DualGridNeighbor.NotFilled, DualGridNeighbor.NotFilled)},
                    {6, CreatePattern(DualGridNeighbor.Filled, DualGridNeighbor.Filled, DualGridNeighbor.NotFilled, DualGridNeighbor.NotFilled)},
                    {7, CreatePattern(DualGridNeighbor.Filled, DualGridNeighbor.Filled, DualGridNeighbor.Filled, DualGridNeighbor.NotFilled)},
                    {8, CreatePattern(DualGridNeighbor.Filled, DualGridNeighbor.NotFilled, DualGridNeighbor.Filled, DualGridNeighbor.NotFilled)},
                    {9, CreatePattern(DualGridNeighbor.Filled, DualGridNeighbor.NotFilled, DualGridNeighbor.NotFilled, DualGridNeighbor.Filled)},
                    {10, CreatePattern(DualGridNeighbor.NotFilled, DualGridNeighbor.Filled, DualGridNeighbor.NotFilled, DualGridNeighbor.Filled)},
                    {11, CreatePattern(DualGridNeighbor.NotFilled, DualGridNeighbor.Filled, DualGridNeighbor.Filled, DualGridNeighbor.NotFilled)},
                    {12, CreatePattern(DualGridNeighbor.NotFilled, DualGridNeighbor.NotFilled, DualGridNeighbor.Filled, DualGridNeighbor.NotFilled)},
                    {13, CreatePattern(DualGridNeighbor.NotFilled, DualGridNeighbor.Filled, DualGridNeighbor.NotFilled, DualGridNeighbor.NotFilled)},
                    {14, CreatePattern(DualGridNeighbor.Filled, DualGridNeighbor.Filled, DualGridNeighbor.NotFilled, DualGridNeighbor.Filled)},
                    {15, CreatePattern(DualGridNeighbor.Filled, DualGridNeighbor.NotFilled, DualGridNeighbor.Filled, DualGridNeighbor.Filled)}
                };
            
            //TODO: Implement logic
            public static void ApplyConfigurationPreset(ref DualGridRuleTile dualGridRuleTile)
            {
               
            }
        }
    }
}
