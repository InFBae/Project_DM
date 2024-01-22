using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RoomBase : MonoBehaviour
{
    public Constant.RoomType roomType;

    public int width;
    public int height;

    public RoomPath[] paths;
    public List<List<RoomPath>> dirPaths = new List<List<RoomPath>>();   

    public RoomBase beforeRoom;
    public RoomPath beforePath;
    public List<RoomBase> nextRooms = new List<RoomBase>();

    private void Awake()
    {
        foreach (Constant.PathDir dir in Enum.GetValues(typeof(Constant.PathDir)))
        {
            dirPaths.Add(new List<RoomPath>());
        }       

        foreach(RoomPath path in paths)
        {
            foreach(Constant.PathDir dir in Enum.GetValues(typeof(Constant.PathDir)))
            {
                if (path.pathDir == dir)
                {
                    dirPaths[(int)dir].Add(path);
                }
            }
        }
    }
}
