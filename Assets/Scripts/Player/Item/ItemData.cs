using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item Data", menuName = "ScriptableObject/Item Data", order =int.MaxValue)]
public class ItemData : ScriptableObject
{
    [SerializeField]
    private new string name;
    [SerializeField]
    private int data;



}
