using System.IO;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public MapData mapData;
    public UnitData unitData;
}

public class SaveSystem : MonoBehaviour
{
    private static GameData _data = new GameData();
    private static string _path;

    private static string Path => _path ?? (_path = Application.persistentDataPath + "/savefile.json");

    // Save the map data to the file
    public static void SaveMapData(MapData mapData)
    {
        _data.mapData = mapData;
        WriteToFile();
    }

    // Load the map data from the file
    public static MapData? LoadMapData()
    {
        if (File.Exists(Path))
        {
            string json = File.ReadAllText(Path);
            GameData data = JsonUtility.FromJson<GameData>(json);
            return data.mapData;
        }
        return null;
    }

    // Save the unit data to the file
    public static void SaveUnitData(UnitData unitData)
    {
        _data.unitData = unitData;
        WriteToFile();
    }

    // Load the unit data from the file
    public static UnitData? LoadUnitData()
    {
        if (File.Exists(Path))
        {
            string json = File.ReadAllText(Path);
            GameData data = JsonUtility.FromJson<GameData>(json);
            return data.unitData;
        }
        return null;
    }

    // Write the game data to the file
    private static void WriteToFile()
    {
        string json = JsonUtility.ToJson(_data);
        File.WriteAllText(Path, json);
    }
}