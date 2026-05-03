#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class PrefabIconGenerator
{
    public static Sprite GenerateIcon(GameObject prefab)
    {
        GameObject instance = Object.Instantiate(prefab);

        Camera cam = new GameObject("IconCam").AddComponent<Camera>();
        cam.backgroundColor = Color.clear;
        cam.clearFlags = CameraClearFlags.SolidColor;

        RenderTexture rt = new RenderTexture(256, 256, 16);
        cam.targetTexture = rt;

        cam.transform.position = instance.transform.position + new Vector3(0, 1, -3);
        cam.transform.LookAt(instance.transform);

        cam.Render();

        RenderTexture previousRT = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(256, 256, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
        tex.Apply();

        RenderTexture.active = previousRT;

        string folderPath = "Assets/ModuleIcons";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filePath = $"{folderPath}/{prefab.name}.png";

        byte[] png = tex.EncodeToPNG();
        File.WriteAllBytes(filePath, png);

        AssetDatabase.ImportAsset(filePath);

        TextureImporter importer = TextureImporter.GetAtPath(filePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        Object.DestroyImmediate(instance);
        Object.DestroyImmediate(cam.gameObject);
        Object.DestroyImmediate(tex);
        rt.Release();
        Object.DestroyImmediate(rt);

        Debug.Log($"Saved and imported icon as Sprite: {filePath}");
        return AssetDatabase.LoadAssetAtPath<Sprite>(filePath);
    }
}
#endif