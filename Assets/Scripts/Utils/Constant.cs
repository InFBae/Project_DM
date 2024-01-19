using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constant
{
    public enum RoomType { Entrance, Exit, Normal, Shop, Teleport, Treasure};
    public enum PathDir { Left, Top, Right, Bottom };

    public static int[] dx = { 0, 0, -1, 1 };
    public static int[] dy = { -1, 1, 0, 0 };

    public static Vector3[] dir = { new Vector3(-1, 0, 0), new Vector3( 0, -1, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0) };

    public static int maxWidth = 88;
    public static int maxHeight = 92;
    public static int maxConnectRoom = 14;

    public static int blockSize = 4;
}
