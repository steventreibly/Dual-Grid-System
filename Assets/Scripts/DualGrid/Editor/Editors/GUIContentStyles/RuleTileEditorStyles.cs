using UnityEditor;
using UnityEngine;

namespace DualGrid.Editor
{
    public class RuleTileEditorStyles
    {
        public static readonly GUIContent DefaultSprite = EditorGUIUtility.TrTextContent("Default Sprite", "The default sprite will be used as a last resort when no tiling rules are valid");

        public static readonly GUIContent DefaultGameObject = EditorGUIUtility.TrTextContent("GameObject",
            "Depending on the configuration on the Dual Grid Tilemap component, this GameObject will be used for every tile.");

        public static readonly GUIContent DefaultCollider = EditorGUIUtility.TrTextContent("Collider",
            "The collider type that will be used for this Dual Grid Rule Tile.");

        public static readonly GUIContent OriginalTexture = EditorGUIUtility.TrTextContent("Oringal Texture", "The original Texture2D associated with this Dual Grid Rule Tile. Only textures split into 16 pieces are considered valid.");
        
        public static readonly GUIStyle extendNeighborsDarkStyle = new GUIStyle()
        {
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold,
            fontSize = 10,
            normal = new GUIStyleState()
            {
                textColor = Color.white,
            }
        };
    }
}
    
