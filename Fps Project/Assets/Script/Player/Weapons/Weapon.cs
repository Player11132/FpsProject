using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon", order = 1)]
public class Weapon :  ScriptableObject
{
    [Header("Name")]
    public string Name;
    [Header("Basic Settings")]
    public int damage;
    public int range;
    public int MaxAmmo;
    public float reloadtime;
    public float Firerate;
    public float Mass;
    public enum WeaponType {Auto,SemiAuto}
    public WeaponType weaponType;

    [Header("Recoil Settings")]
    public float recoilX;
    public float recoilY;
    public float recoilZ;

    public float snappines;
    public float returnspeed;

    [Header("Aiming")]
    public float aimspeed;
    public Vector3 AimPosition;

    [Header("Visuals")]
    public GameObject model;
    public Vector3 optimaposition;
    public Vector3 localBarellpos;
    
    public GameObject Flash;

    [Header("Bobing")]
    public float bobSpeed = 1f;
    public float bobDistance = 1f;

    [Header("Sway")]
    public float smoothnes;
    public float multiplier;
    public float max_sway;

    [Header("Sounds")]
    public AudioClip ShootClip;
    public AudioClip ReloadClip;

    [Header("UI")]
    public Sprite gun_Icon;


}
