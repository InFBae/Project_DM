using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item Data", menuName = "ScriptableObject/Item Data", order =int.MaxValue)]
public class ItempData : MonoBehaviour
{
    [SerializeField]
    private new string name;
    private int data;
}
