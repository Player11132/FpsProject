using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GunManager gun;
    [Header("UI elements")]
    [Header("Gun Info")]
    public Image GunIcon;
    public Text Gun_Name;
    [Header("Ammo")]
    public Text ammo_Counter;
    public int Ammo_Medium_Thres;
    public int Ammo_Low_Thres;
    public Color Ammo_Medium;
    public Color Ammo_Low;
    private Color Ammo_def;
    [Header("Special Counter")]
    public Image[] special_Counter;
    public Text Special_Name;
    [Header("Health")]
    public Text Health;
    public int Health_Medium_Thres;
    public int Health_Low_Thres;
    public Color Health_Medium;
    public Color Health_Low;
    private Color Health_def;

    private void Start()
    {
        Health_def = Health.color;
        Ammo_def = ammo_Counter.color;
    }
    void Update()
    {
        if (gun != null)
        {
            //update ammo count
            int curAmmo = gun.weaponammo[gun.curweaponIndex];
            ammo_Counter.text = curAmmo.ToString() + "/" + gun.curweapon.MaxAmmo.ToString();

            //set gun name
            if(gun.curweapon.gun_Icon!=null)
                GunIcon.sprite = gun.curweapon.gun_Icon;
            Gun_Name.text = gun.curweapon.Name;

            //Set color
            if (curAmmo < gun.curweapon.MaxAmmo / Ammo_Low_Thres)
                ammo_Counter.color = Ammo_Low;
            else if (curAmmo < gun.curweapon.MaxAmmo / Ammo_Medium_Thres)
                ammo_Counter.color = Ammo_Medium;
            else
                ammo_Counter.color = Ammo_def;

        }
    }
}
