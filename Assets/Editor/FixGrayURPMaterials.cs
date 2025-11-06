using UnityEditor;
using UnityEngine;

public class FixGrayURPMaterials : EditorWindow
{
    [MenuItem("Tools/Fix Gray URP Materials")]
    public static void FixMaterials()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material");
        int fixedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat != null && mat.shader != null && mat.shader.name == "Universal Render Pipeline/Lit")
            {
                if (mat.GetTexture("_BaseMap") == null)
                {
                    Texture oldTex = mat.GetTexture("_MainTex");
                    if (oldTex != null)
                    {
                        mat.SetTexture("_BaseMap", oldTex);
                        fixedCount++;
                    }
                }
            }
        }

        Debug.Log($"✅ Fixed {fixedCount} URP materials (restored BaseMap textures)");
        AssetDatabase.SaveAssets();
    }
}
