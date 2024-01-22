using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomPath : MonoBehaviour
{
    public Constant.PathDir pathDir;
    public int needCorridorCount;
    public bool backTracked = false;
    public bool isUsing = false;
}
