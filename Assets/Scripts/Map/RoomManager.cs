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

    HashSet<Vector2> map = new HashSet<Vector2>();
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
        int height = Constant.maxHeight * Constant.maxConnectRoom * 2;
        int width = Constant.maxWidth * Constant.maxConnectRoom * 2;

        curPoint.x = width * 0.5f;
        curPoint.y = height * 0.5f;
        mapContainer.transform.position = curPoint;

        mainCamera.transform.position = new Vector3(curPoint.x, curPoint.y, -900);

        // ��� ���� �߰�
        for(int i = 0; i < stageInfo.rooms.Count; i++)
        {
            roomDic.Add(i, Instantiate(Resources.Load<GameObject>($"Map/Room/{stageInfo.rooms[i]}"), mapContainer.transform).GetComponent<RoomBase>());                
        }

        // ���� ���� �߰�
        for (int i = 0; i < stageInfo.edges.Count; i++)
        {
            roomDic[stageInfo.edges[i][0]].nextRooms.Add(roomDic[stageInfo.edges[i][1]]);
            roomDic[stageInfo.edges[i][1]].beforeRoom = roomDic[stageInfo.edges[i][0]];
        }

        Stack<RoomBase> roomStack = new Stack<RoomBase>();
        roomStack.Push(roomDic[0]);
        int count = 0;
        while ( roomStack.Count > 0 && count++ < 1000)
        {
            RoomBase currentRoom = roomStack.Pop();

            for(int i = 0; i < currentRoom.width; i++)
            {
                for(int j = 0; j < currentRoom.height; j++)
                {
                    map.Add(new Vector2((int)curPoint.x + i, (int)curPoint.y - j));
                }
            }
            #region ���� �� �����
            // ����� �� �����
            for (int i = 0; i < currentRoom.nextRooms.Count; i++)
            {              
                RoomBase nextRoom = currentRoom.nextRooms[i];
                roomStack.Push(nextRoom);

                // ���� ���� �ִ� ��� ����Ʈ �����
                List<RoomPath> leftPathList = new List<RoomPath>();
                foreach (RoomPath path in currentRoom.paths)
                {
                    if (!path.isUsing && !path.backTracked)
                    {
                        leftPathList.Add(path);
                    }
                }

                bool backTrack = false;

                // �濡 �����ִ� �� �� �������� �̾Ƽ� ���� �� �ű��
                while (leftPathList.Count > 0)
                {
                    int random = Random.Range(0, leftPathList.Count);
                    RoomPath currentPath = leftPathList[random];
                    leftPathList.RemoveAt(random);                   

                    curPoint = currentPath.transform.position;
                    RoomPath nextPath = null;
                    int needCorridorCount = 0;

                    // ���� ��ο� ����Ǵ� �� Ȯ��
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

                            // �浹Ȯ�� �� �Ұ����ϴٸ� ��� ����
                            if (CheckCollision(nextRoom.width, nextRoom.height))
                            {
                                nextPath = null;
                            }
                            break;
                        }
                    }

                    // ����Ǵ� ���� ���ٸ� �ٽ� ���� �� �߿� �������� ã��
                    if (nextPath == null)
                    {                      
                        if (leftPathList.Count == 0)
                        {
                            backTrack = true;
                        }
                        continue;
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
                
                // ���� ��� ��ΰ� �Ұ������� ��
                if (backTrack)
                {
                    Debug.Log($"{nextRoom.roomType}Room Make Failed");
                    currentRoom.beforePath.backTracked = true;
                    foreach (RoomPath path in currentRoom.paths)
                    {
                        path.backTracked = false;
                    }
                    roomStack.Pop();
                    roomStack.Push(currentRoom.beforeRoom);
                    return;
                    break;
                }

            }
            #endregion
        }

        Debug.Log("Create Completed!");

    }

    private bool CheckCollision(int width, int height)
    {
        for (int w = 5; w < width - 4; w++)
        {
            for (int h = 5; h < height - 4; h++)
            {
                if (map.Contains(new Vector2((int)curPoint.x + w, (int)curPoint.y - h)))
                {
                    return true;
                }
            }
        }
        return false;
    }

}
