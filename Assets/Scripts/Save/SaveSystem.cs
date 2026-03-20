using System;
using System.IO;
using UnityEngine;

namespace Game.Save
{
    [Serializable]
    public class SaveData
    {
        public int currentLevel = 1;
        public int gold = 0;
        public float music = 1f;
        public float sfx = 1f;
    }

    public interface ISaveService
    {
        SaveData Data { get; }
        void Load();
        void Save();
        void ResetAll();
    }

    public sealed class SaveService : ISaveService
    {
        public SaveData Data { get; private set; } = new SaveData();
        private readonly string _path;

        public SaveService()
        {
            _path = Path.Combine(Application.persistentDataPath, "save.json");
        }

        public void Load()
        {
            try
            {
                if (!File.Exists(_path))
                {
                    Data = new SaveData();
                    Save();
                    return;
                }

                var json = File.ReadAllText(_path);
                Data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
            }
            catch
            {
                Data = new SaveData();
                Save();
            }
        }

        public void Save()
        {
            try
            {
                var json = JsonUtility.ToJson(Data, prettyPrint: false);
                File.WriteAllText(_path, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Save failed: {e}");
            }
        }

        public void ResetAll()
        {
            Data = new SaveData();
            Save();
        }
    }
}
