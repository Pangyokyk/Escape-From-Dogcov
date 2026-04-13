using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Game/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("기본 정보")]
    public string weaponName;
    public Sprite icon;

    [Header("스탯")]
    public float damage = 10f;
    public float fireRate = 0.2f;
    public float range = 50f;
    public float reloadTime = 1.5f;

    [Header("탄약")]
    public AmmoType ammoType;
    public int magazineSize = 30;

    [Header("반동")]
    public float recoilAmount = 0.1f;

    [Header("총알")]
    public float bulletSpeed = 50f;

    public enum AmmoType
    {
        Ammo_5_56mm,
        Ammo_7_62mm
    }
}
