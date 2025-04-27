using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{
    public static void SaveData<T>(T saveData)
    {
        string path = Application.persistentDataPath + FileNamesContainer.GetFileName(typeof(T));

        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        var formatter = new BinaryFormatter();

        formatter.Serialize(stream, saveData);
    }

    public static T LoadData<T>() where T : SaveData
    {
        string path = Application.persistentDataPath + FileNamesContainer.GetFileName(typeof(T));

        bool fileExist = File.Exists(path);

        if (fileExist == false)
        {
            return null;
        }

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        BinaryFormatter formatter = new BinaryFormatter();

        T deserialized = (T)formatter.Deserialize(stream);

        stream.Close();

        return deserialized;
    }
}