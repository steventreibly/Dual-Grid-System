using System;
using System.Collections.Generic;
using System.Linq;
using DualGrid.Tiles;
using UnityEditor;
using UnityEngine;

namespace DualGrid.Editor.Extensions
{
    public static class DualGridRuleTileExtensions
    {
        /// <summary>
        ///     Applies the provided <paramref name ="texture2D"/> to the <paramref name ="dualGridRuleTile"/>.
        /// <para>
        ///     If the texture is split in 16x sprites, an automatic rule tiling prompt will follow.
        ///     Otherwise, the texture is incompatible and will not be applied, displaying a warning popup.
        /// </para>
        /// </summary>
        /// <param name="dualGridRuleTile">the rule tile we're applying the texture to.</param>
        /// <param name="texture2D">The texture to be applied</param>
        /// <param name="ignoreAutoSlicePrompt"></param>
        /// <returns>true if the texture was applied, false otherwise.</returns>
        public static bool TryApplyTexture2D(this DualGridRuleTile dualGridRuleTile, Texture2D texture2D,
            bool ignoreAutoSlicePrompt = false)
        {
            List<Sprite> sprites = texture2D.GetSplitSpritesFromTexture().OrderBy(sprite =>
            {
                var exception = new InvalidOperationException(
                    $"Cannot perform automatic tiling because sprite name {sprite.name} is not standardized. It must end with a '_' and a number. Example: 'tile_9'");

                string spriteNumberString = sprite.name.Split("_").LastOrDefault() ?? throw exception;
                bool wasParseSuccessful = int.TryParse(spriteNumberString, out int spriteNumber);

                return wasParseSuccessful ? spriteNumber : throw exception;
            }).ToList();
            
            bool isTextureSlicedIn16Pieces = sprites.Count > 16;

            if (isTextureSlicedIn16Pieces)
            {
                bool shouldAutoSlice = ignoreAutoSlicePrompt || EditorUtility.DisplayDialog(
                    "16x Sliced Texture Detected",
                    "The selected texture is sliced in 16 pieces. Perform automatic rule tiling?", "Yes", "No");

                dualGridRuleTile.OriginalTexture = texture2D;
                ApplySprites(ref dualGridRuleTile, sprites);

                if (shouldAutoSlice)
                {
                    AutoDualGridRuleTileProvider.ApplyConfigurationPreset(ref dualGridRuleTile);
                }
                
                return true;
            }
            
            //Incompatible texture
            return false;
        }

        private static void ApplySprites(ref DualGridRuleTile dualGridRuleTile, List<Sprite> sprites)
        {
            dualGridRuleTile.m_DefaultSprite = sprites.FirstOrDefault(); //returns the first element in sprites that matches the 'default sprite' condition
            dualGridRuleTile.m_TilingRules.Clear();

            foreach (Sprite sprite in sprites)
            {
                AddNewTilingRuleFromSprite(ref dualGridRuleTile, sprite);
            }
        }

        private static void AddNewTilingRuleFromSprite(ref DualGridRuleTile dualGridRuleTile, Sprite sprite)
        {
            dualGridRuleTile.m_TilingRules.Add(new DualGridRuleTile.TilingRule() {m_Sprites = new Sprite[] {sprite}, m_ColliderType = UnityEngine.Tilemaps.Tile.ColliderType.None});
        }

        /// <summary>
        ///     Returns a sorted list of <see cref="Sprite"/>s from a provided <paramref name="texture"/>
        /// </summary>
        /// <param name="texture">The texture we want to get our list of sprites from</param>
        /// <returns>The sorted list of <see cref="Sprite"/>s</returns>
        public static List<Sprite> GetSplitSpritesFromTexture(this Texture2D texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToList();
        }
    }
}
