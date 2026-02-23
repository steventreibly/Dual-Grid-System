using UnityEngine;
using UnityEngine.Tilemaps;

namespace DualGrid
{
    public class DualGridPreviewTile : TileBase
    {
        public bool IsFilled { get; private set; }

        public static DualGridPreviewTile Filled => Create(isFilled: true);
        public static DualGridPreviewTile NotFilled => Create(isFilled: false);

        /// <summary>
        ///     Creates a new Preview Tile instance that is either filled or empty
        /// </summary>
        /// <param name="isFilled"></param>
        /// <returns></returns>
        private static DualGridPreviewTile Create(bool isFilled)
        {
            var dualGridPreviewTile = CreateInstance<DualGridPreviewTile>();
            dualGridPreviewTile.name = $"{(isFilled ? "Filled" : "Empty")} Dual Grid Preview Tile";
            dualGridPreviewTile.IsFilled =  isFilled;
            return dualGridPreviewTile;
        }
    }
}
