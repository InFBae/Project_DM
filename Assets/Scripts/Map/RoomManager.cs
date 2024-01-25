using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.tvOS;
using Random = UnityEngine.Random;

public class RoomManager : MonoBehaviour
{
    [SerializeField] RoomContainer roomContainer;
    [SerializeField] Camera mainCamera;
    public int stage;
    StageInfo stageInfo;

    HashSet<Vector2Int> map = new HashSet<Vector2Int>();
    Vector3 curPoint;
    string errorCause;

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
        int height = Constant.maxHeight * Constant.maxConnectRoom * 2;
        int width = Constant.maxWidth * Constant.maxConnectRoom * 2;

        curPoint.x = width * 0.5f;
        curPoint.y = height * 0.5f;
        mapContainer.transform.position = curPoint;

        mainCamera.transform.position = new Vector3(curPoint.x, curPoint.y, -900);

        // 노드 정보 추가
        for(int i = 0; i < stageInfo.rooms.Count; i++)
        {
            roomDic.Add(i, Instantiate(Resources.Load<GameObject>($"Map/Room/{stageInfo.rooms[i]}"), mapContainer.transform).GetComponent<RoomBase>());
            roomDic[i].gameObject.SetActive(false);
            roomDic[i].gameObject.name = $"{roomDic[i].roomType}{i}";
        }

        // 간선 정보 추가
        for (int i = 0; i < stageInfo.edges.Count; i++)
        {
            roomDic[stageInfo.edges[i][0]].nextRooms.Add(roomDic[stageInfo.edges[i][1]]);
            roomDic[stageInfo.edges[i][1]].beforeRoom = roomDic[stageInfo.edges[i][0]];
        }

        Stack<RoomBase> roomStack = new Stack<RoomBase>();
        roomStack.Push(roomDic[0]);

        int count = 0;
        while (roomStack.Count > 0 && count++ < 1000)
        {
            RoomBase currentRoom = roomStack.Pop();
            AddMap(currentRoom.width, currentRoom.height, currentRoom.transform.position);
            currentRoom.gameObject.SetActive(true);

            #region 다음 방 만들기
            // 연결된 방 만들기
            for (int i = 0; i < currentRoom.nextRooms.Count; i++)
            {              
                RoomBase nextRoom = currentRoom.nextRooms[i];
                roomStack.Push(nextRoom);

                // 아직 남아 있는 통로 리스트 만들기
                List<RoomPath> leftPathList = new List<RoomPath>();
                foreach (RoomPath path in currentRoom.paths)
                {
                    if (!path.isUsing && !path.backTracked)
                    {
                        leftPathList.Add(path);
                    }
                }

                bool backTrack = false;

                if (leftPathList.Count == 0)
                {
                    errorCause = $"{currentRoom.name}의 남은 경로 없음";
                    backTrack = true ;
                }

                // 방에 남아있는 길 중 랜덤으로 뽑아서 다음 방 옮기기
                while (leftPathList.Count > 0)
                {
                    int random = Random.Range(0, leftPathList.Count);
                    RoomPath currentPath = leftPathList[random];
                    leftPathList.RemoveAt(random);                   

                    curPoint = currentPath.transform.position;
                    RoomPath nextPath = null;
                    int needCorridorCount = 0;

                    // 뽑은 경로에 연결되는 길 확인
                    foreach(Constant.PathDir dir in Enum.GetValues(typeof(Constant.PathDir)))
                    {
                        int reverseDir = ((int)dir + 2) % 4;
                        if (currentPath.pathDir == dir && nextRoom.dirPaths[reverseDir].Count > 0)
                        {
                            random = Random.Range(0, nextRoom.dirPaths[reverseDir].Count);
                            nextPath = nextRoom.dirPaths[reverseDir][random];
                            
                            if ((needCorridorCount = nextRoom.dirPaths[reverseDir][random].needCorridorCount) > 0)
                            {
                                curPoint -= Constant.dir[reverseDir] * needCorridorCount * Constant.blockSize;
                            }
                            curPoint -= nextPath.transform.localPosition;

                            // 충돌확인 후 불가능하다면 경로 제거
                            if (CheckCollision(nextRoom.width, nextRoom.height, curPoint))
                            {
                                errorCause = $"{nextRoom.name} 충돌";
                                nextPath = null;
                            }
                            break;
                        }
                    }

                    // 연결되는 길이 없다면 다시 남은 길 중에 랜덤으로 찾기
                    if (nextPath == null)
                    {
                        backTrack = true;
                        continue;
                    }                    

                    foreach(Constant.PathDir dir in Enum.GetValues(typeof(Constant.PathDir)))
                    {
                        if (dir == nextPath.pathDir)
                        {
                            for (int c = 1; c <= needCorridorCount; c++)
                            {
                                GameObject corridor = Instantiate(Resources.Load<GameObject>("Map/HorizontalCorridor"), nextRoom.transform);
                                nextRoom.corridors.Add(corridor);
                                corridor.transform.localPosition = nextPath.transform.localPosition + (Constant.dir[(int)nextPath.pathDir] * c * Constant.blockSize);       
                                corridor.transform.localScale = nextPath.transform.localScale;
                            }
                        }
                    }

                    backTrack = false;
                    currentPath.isUsing = true;
                    currentPath.gameObject.SetActive(false);
                    nextPath.isUsing = true;
                    nextPath.gameObject.SetActive(false);
                    nextRoom.transform.position = curPoint;
                    nextRoom.beforePath = currentPath;
                    break;
                }  
                
                // 만약 모든 경로가 불가능했을 때
                if (backTrack)
                {
                    Debug.Log(errorCause);
                    Debug.Log($"{nextRoom.gameObject.name} Make Failed");
                    Debug.Log($"BackTrack to {currentRoom.beforeRoom.name}");

                    currentRoom.beforePath.backTracked = true;
                    currentRoom.beforePath.isUsing = false;
                    currentRoom.beforePath.gameObject.SetActive(true);

                    BackTrackMap(currentRoom);
                  
                    roomStack.Pop();
                    roomStack.Push(currentRoom.beforeRoom);                    
                    break;
                }

            }
            #endregion
        }

        Debug.Log("Create Completed!");

    }

    private void BackTrackMap(RoomBase currentRoom)
    {
        if (currentRoom.nextRooms.Count > 0)
        {
            foreach (RoomBase nextRoom in currentRoom.nextRooms)
            {
                BackTrackMap(nextRoom);
            }
        }
        foreach (RoomPath path in currentRoom.paths)
        {
            path.gameObject.SetActive(true);
            path.isUsing = false;
            path.backTracked = false;
        }
        foreach(GameObject corridor in currentRoom.corridors)
        {           
            Destroy(corridor);
        }
        currentRoom.corridors.Clear();

        currentRoom.gameObject.SetActive(false);       

        RemoveMap(currentRoom.width, currentRoom.height, currentRoom.transform.position);
    }


    private void AddMap(int width, int height, Vector3 startPoint)
    {
        for (int w = 0; w < width; w++)
        {
            for(int  h = 0; h < height; h++)
            {
                map.Add(new Vector2Int((int)startPoint.x + w, (int)startPoint.y + h));
            }
        }
    }

    private void RemoveMap(int width, int height, Vector3 startPoint)
    {
        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                map.Remove(new Vector2Int((int)startPoint.x + w, (int)startPoint.y + h));             
            }
        }
    }

    private bool CheckCollision(int width, int height, Vector3 startPoint)
    {
        for (int w = 1; w < width - 1; w++)
        {
            for (int h = 1; h < height - 1; h++)
            {
                if (map.Contains(new Vector2Int((int)startPoint.x + w, (int)startPoint.y + h)))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
