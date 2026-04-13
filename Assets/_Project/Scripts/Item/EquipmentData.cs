using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment", menuName = "Game/Equipment Data")]
public class EquipmentData : ScriptableObject
{
    [Header("기본 정보")]
    public string equipmentName;
    public Sprite icon;
    public string description;

    [Header(" 장비 타입")]
    public EquipmentType equipmentType;

    [Header("스탯")]
    public float armorValue;
    public float movementPenalty;

    public enum EquipmentType
    {
        Helmet,
        Armor
    }



}
