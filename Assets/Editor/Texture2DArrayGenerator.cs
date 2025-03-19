using UnityEngine;
using UnityEditor;
using System.IO;

public class Texture2DArrayGenerator
{
    [MenuItem("Assets/Create/Texture2D Array Asset")]
    public static void CreateTexture2DArrayAsset()
    {
        // Define the folder where your textures are located.
        string folderPath = "Assets/GeneratedTextures";
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });

        if (guids.Length == 0)
        {
            Debug.LogError("No Texture2D assets found in " + folderPath);
            return;
        }

        // Load the first texture to set size.
        Texture2D firstTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guids[0]));
        int width = firstTexture.width;
        int height = firstTexture.height;

        // Set a known texture format for the array.
        TextureFormat arrayFormat = TextureFormat.RGBA32;

        // Create the Texture2DArray.
        Texture2DArray textureArray = new Texture2DArray(width, height, guids.Length, arrayFormat, false);

        // Copy each texture's pixel data into the Texture2DArray.
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            
            // Ensure the texture is marked as readable.
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }
            
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex.width != width || tex.height != height)
            {
                Debug.LogError("Texture " + tex.name + " has different dimensions. All textures must be the same size.");
                return;
            }
            
            // Copy pixel data manually.
            Color[] pixels = tex.GetPixels();
            textureArray.SetPixels(pixels, i);
        }

        textureArray.Apply();

        // Save the Texture2DArray as an asset.
        string assetPath = "Assets/Texture2DArray.asset";
        AssetDatabase.CreateAsset(textureArray, assetPath);
        AssetDatabase.SaveAssets();

        Debug.Log("Texture2DArray asset created at " + assetPath);
    }
}
