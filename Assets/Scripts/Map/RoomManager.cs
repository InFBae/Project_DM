using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomManager : MonoBehaviour
{
    [SerializeField] RoomContainer roomContainer;
    [SerializeField] Camera mainCamera;
    public int stage;
    StageInfo stageInfo;

    Vector3 curPoint;

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

        GameObject mapContainer = new GameObject("MapContainer");

        Dictionary<int, RoomBase> roomDic = new Dictionary<int, RoomBase>();
        int height = Constant.maxHeight * stageInfo.rooms.Count * 2 * Constant.blockSize;
        int width = Constant.maxWidth * stageInfo.rooms.Count * 2 * Constant.blockSize;

        Dictionary<Vector2, int> map = new Dictionary<Vector2, int>();

        curPoint.x = width * 0.5f;
        curPoint.y = height * 0.5f;
        mapContainer.transform.position = curPoint;

        mainCamera.transform.position = new Vector3(curPoint.x, curPoint.y, -900);

        Queue<RoomBase> roomQueue = new Queue<RoomBase>();
        for(int i = 0; i < stageInfo.rooms.Count; i++)
        {
            roomDic.Add(i, Instantiate(Resources.Load<GameObject>($"Map/Room/{stageInfo.rooms[i]}"), mapContainer.transform).GetComponent<RoomBase>());                
        }
        roomQueue.Enqueue(roomDic[0]);

        for (int i = 0; i < stageInfo.edges.Count; i++)
        {
            roomDic[stageInfo.edges[i][0]].nextRooms.Add(roomDic[stageInfo.edges[i][1]]);
            roomDic[stageInfo.edges[i][1]].beforeRoom = roomDic[stageInfo.edges[i][0]];
        }

        while( roomQueue.Count > 0)
        {
            RoomBase currentRoom = roomQueue.Dequeue();

            for(int i = 0; i < currentRoom.width; i++)
            {
                for(int j = 0; j < currentRoom.height; j++)
                {
                    map.TryAdd(new Vector2((int)curPoint.x + i, (int)curPoint.y - j) , 1);
                }
            }      

            // 연결된 방 만들기
            for(int i = 0; i < currentRoom.nextRooms.Count; i++)
            {              
                RoomBase nextRoom = currentRoom.nextRooms[i];
                roomQueue.Enqueue(nextRoom);

                // 아직 남아 있는 통로 리스트 만들기
                List<RoomPath> leftPathList = new List<RoomPath>();
                foreach (RoomPath path in currentRoom.paths)
                {
                    if (path.isActiveAndEnabled)
                    {
                        leftPathList.Add(path);
                    }
                }

                bool remake = false;
                // 방에 남아있는 통로 중 랜덤으로 뽑아서 다음 방 옮기기
                while (leftPathList.Count > 0)
                {
                    int random = Random.Range(0, leftPathList.Count);
                    RoomPath currentPath = leftPathList[random];
                    leftPathList.RemoveAt(random);
                    currentPath.gameObject.SetActive(false);

                    curPoint = currentPath.transform.position;
                    RoomPath nextPath = null;
                    int needCorridorCount = 0;

                    foreach(Constant.PathDir dir in Enum.GetValues(typeof(Constant.PathDir)))
                    {
                        int reverseDir = ((int)dir + 2) % 4;
                        if (currentPath.pathDir == dir && nextRoom.dirPaths[reverseDir].Count > 0)
                        {
                            random = Random.Range(0, nextRoom.dirPaths[reverseDir].Count);
                            nextPath = nextRoom.dirPaths[reverseDir][random];
                            remake = false;
                            if ((needCorridorCount = nextRoom.dirPaths[reverseDir][random].needCorridorCount) > 0)
                            {
                                curPoint -= Constant.dir[reverseDir] * needCorridorCount * Constant.blockSize;
                            }
                        }
                    }

                    if (nextPath == null)
                    {
                        remake = true;
                        currentPath.gameObject.SetActive(true);
                        continue;
                    }

                    curPoint -= nextPath.transform.localPosition;


                    // 충돌 체크    

                    for (int w = 0; w < nextRoom.width; w++)
                    {
                        for (int h = 0; h < nextRoom.height; h++)
                        {
                            if (map.ContainsKey(new Vector2(curPoint.x + w, curPoint.y - h))){
                                remake = true;
                                currentPath.gameObject.SetActive(true);
                                break;
                            }
                        }
                        if (remake) { break; }
                    }

                    if (remake)
                    {
                        continue;
                    }
                    
                    remake = false;                    
                    nextPath.gameObject.SetActive(false);
                    nextRoom.transform.position = curPoint;
                    break;
                }  
                if (remake)
                {
                    Debug.Log($"{nextRoom.roomType}Room Make Failed");
                    // Add Corridor?
                }

            }
        }

        Debug.Log("Create Completed!");

    }
}
