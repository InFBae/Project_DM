using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [SerializeField] RoomContainer roomContainer;
    public int stage;
    StageInfo stageInfo;

    private void Start()
    {
        InitRoom();
    }

    private void LoadStageInfo(int stage)
    {
        string path = Path.Combine(Application.dataPath, $"StageData/stage{stage}.json");
        string jsonData = File.ReadAllText(path);
        stageInfo = JsonConvert.DeserializeObject<StageInfo>(jsonData);
    }

    private void InitRoom()
    {
        LoadStageInfo(stage);
    }
}
