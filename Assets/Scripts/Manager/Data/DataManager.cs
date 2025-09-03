using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
namespace D.Editor
{

#if UNITY_EDITOR

    using UnityEditor;

    [CustomEditor(typeof(DataManager))]
    public class DataManagerEditor : Editor
    {
        public Dictionary<string, string> prefInfos;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            DataManager dataManager = target as DataManager;
            if (dataManager.data == null)
            {
                if (GUILayout.Button("Load Data"))
                {
                    dataManager.LoadData();
                }
                return;
            }
            prefInfos = dataManager.data.otherPrefs;
            EditorGUILayout.LabelField("Gold", Mathf.RoundToInt((float)dataManager.data.gold).ToString());
            dataManager.data.gold = EditorGUILayout.IntField("Set Gold", Mathf.RoundToInt((float)dataManager.data.gold));

            EditorGUILayout.LabelField("Diamond", Mathf.RoundToInt((float)dataManager.data.diamond).ToString());
            dataManager.data.diamond = EditorGUILayout.IntField("Set Diamond", Mathf.RoundToInt((float)dataManager.data.diamond));

            EditorGUILayout.LabelField("Level", dataManager.data.currentLevel.ToString());
            dataManager.data.currentLevel = EditorGUILayout.IntField("Set Level", dataManager.data.currentLevel);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("OtherPrefs", EditorStyles.boldLabel);
            foreach (var kvp in prefInfos)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(kvp.Key, GUILayout.Width(200));
                EditorGUILayout.LabelField(kvp.Value);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Save Data"))
            {
                dataManager.SaveData();
                Debug.Log("Saved Data");
            }
            if (GUILayout.Button("Delete Data"))
            {
                SaveSystem.DeleteFile(DataManager.DATAPATH);
                Debug.Log("Deleted Data File");
                dataManager.data = null; // reset d? li?u hi?n th?
            }
        }


        private bool foldOut;

        private void ShowPrefs()
        {
            int index = 0;
            foldOut = EditorGUILayout.Foldout(foldOut, "ListPrefs");
            if (foldOut)
            {
                EditorGUI.indentLevel++;
                var newList = new Dictionary<string, string>(prefInfos);
                foreach (var item in newList)
                {
                    EditorGUILayout.LabelField("Element " + index);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(item.Key);
                    string newValue = "";
                    if (int.TryParse(item.Value, out int newNum))
                    {
                        newValue = EditorGUILayout.IntField(newNum).ToString();
                    }
                    else
                    {
                        newValue = EditorGUILayout.TextField(item.Value);
                    }
                    if (newValue != item.Value)
                    {
                        prefInfos[item.Key] = newValue;
                        Debug.Log("change " + item.Key + ":" + newValue);
                    }
                    EditorGUILayout.EndHorizontal();
                    index++;
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }

        }
    }

#endif

}
public class DataManager : Singleton<DataManager>
{
    public static readonly string DATAPATH = "player_data";
    public PlayerData data;

    public void LoadData()
    {
        string json = SaveSystem.LoadGame(DATAPATH);
        if (!string.IsNullOrEmpty(json))
        {
            data = JsonConvert.DeserializeObject<PlayerData>(json);
        }
        else
        {
            data = new PlayerData();
        }
    }

    public void SaveData()
    {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        SaveSystem.SaveFile(DATAPATH, json);
        Debug.Log("Save Done:\n" + json);
    }

}

[System.Serializable]
public class PlayerData
{
    public double gold;
    public double diamond;
    public double GoldReceivedAfterLevel ;
    public double DiamondReceivedAfterLevel ;
    public int currentLevel;
    public Dictionary<string, string> otherPrefs; 

    public PlayerData()
    {
        diamond = 1;
        gold = 0;
        currentLevel = 1;
        GoldReceivedAfterLevel = 10;
        DiamondReceivedAfterLevel = 1;
        otherPrefs = new Dictionary<string, string>();
    }
}