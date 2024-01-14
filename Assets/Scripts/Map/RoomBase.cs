using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBase : MonoBehaviour
{
    public Constant.RoomType roomType;

    public int width;
    public int height;

    public Transform[] horizontalPaths;
    public Transform[] verticalPaths;

    public RoomBase leftRoom, rightRoom, topRoom, bottomRoom;
}
