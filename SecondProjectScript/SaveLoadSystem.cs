using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveLoadSystem
{
    public static void SaveSystem(PlayerStatus player, DeckInfo[] playerDeck)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/gamedata";
        FileStream stream = new FileStream(path, FileMode.Create);

        SaveData data = new SaveData(player, playerDeck);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static SaveData LoadSystem()
    {
        string path = Application.persistentDataPath + "/gamedata";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            SaveData data = formatter.Deserialize(stream) as SaveData;
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in" + path);
            return null;
        }
    }
}
