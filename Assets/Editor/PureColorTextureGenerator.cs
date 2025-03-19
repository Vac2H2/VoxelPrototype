using UnityEngine;
using UnityEditor;
using System.IO;

public class PureColorTextureGenerator : EditorWindow
{
    private Color textureColor = Color.white;
    private string textureName = "PureColorTexture";

    [MenuItem("Tools/Pure Color Texture Generator")]
    public static void ShowWindow()
    {
        GetWindow<PureColorTextureGenerator>("Pure Color Texture Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Create a 4x4 Pure Color Texture", EditorStyles.boldLabel);
        
        // Display a color picker in the window.
        textureColor = EditorGUILayout.ColorField("Texture Color", textureColor);
        textureName = EditorGUILayout.TextField("Texture Name", textureName);
        
        // Create button to generate the texture.
        if (GUILayout.Button("Create Texture"))
        {
            CreateTexture();
        }
    }

    private void CreateTexture()
    {
        int width = 4;
        int height = 4;
        // Create a new 4x4 texture with RGBA32 format.
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        
        // Create an array of colors and fill it with the chosen color.
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = textureColor;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        
        // Encode the texture to PNG.
        byte[] pngData = texture.EncodeToPNG();
        if (pngData != null)
        {
            // Ensure the folder exists.
            string folderPath = "Assets/GeneratedTextures";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets", "GeneratedTextures");
            }
            // Build the file path.
            string filePath = Path.Combine(folderPath, textureName + ".png");
            File.WriteAllBytes(filePath, pngData);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Texture Created", "Texture saved at: " + filePath, "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Failed to encode texture to PNG.", "OK");
        }
    }
}

