using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using UnityEngine;

public static class SaveSystem
{
    public static async Task SaveFileAsync(string fileName, string text)
    {
        string path = GetSavePath(fileName);
        byte[] compressedData = await Task.Run(() => Compress(text));
        File.WriteAllBytes(path, compressedData);
        Debug.Log($"File Saved:{path}");
    }

    public static async Task<string> LoadFileAsync(string fileName)
    {
        string path = GetSavePath(fileName);
        if (!File.Exists(path)) return "";
        byte[] compressedData = File.ReadAllBytes(path);
        string json = await Task.Run(() => Decompress(compressedData));
        return json;
    }

    public static void SaveFile(string fileName, string text)
    {
        string path = GetSavePath(fileName);
        byte[] compressedData = Compress(text);
        File.WriteAllBytes(path, compressedData);
        Debug.Log($"File Saved:{path}");
    }

    public static string LoadGame(string fileName)
    {
        string path = GetSavePath(fileName);
        if (!File.Exists(path)) return ""; // Return default if no save found
        byte[] compressedData = File.ReadAllBytes(path);
        string json = Decompress(compressedData);
        return json;
    }

    private static byte[] Compress(string data)
    {
        using MemoryStream output = new MemoryStream();
        using GZipStream gzip = new GZipStream(output, CompressionMode.Compress);
        using StreamWriter writer = new StreamWriter(gzip);
        writer.Write(data);
        writer.Close();
        return output.ToArray();
    }

    private static string Decompress(byte[] compressedData)
    {
        using MemoryStream input = new MemoryStream(compressedData);
        using GZipStream gzip = new GZipStream(input, CompressionMode.Decompress);
        using StreamReader reader = new StreamReader(gzip);
        return reader.ReadToEnd();
    }
    public static string GetSavePath(string fileName)
    {
        string folder = Application.persistentDataPath + "/Saves";
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        return Path.Combine(folder, fileName + ".gz");
    }

    public static void DeleteFile(string fileName)
    {
        string path = GetSavePath(fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Save file deleted: {path}");
        }
        else
        {
            Debug.LogWarning($"No save file found to delete at: {path}");
        }
    }

}