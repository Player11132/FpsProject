using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunManager : MonoBehaviour
{
    [Header("Weapons")]
    public Weapon Weap1;
    public Weapon Weap2;
    public Weapon Weap3;
    [Header("Impact")]
    [Tooltip("Layer should have same index as the impact")]
    [SerializeField]private List<string> layers;
    [SerializeField]private List<GameObject> impacts;
    private Dictionary<string, GameObject> impactvfx = new Dictionary<string, GameObject>();
    [Header("Other Stuff")]
    [SerializeField]private Recoil recoil;
    //[SerializeField]private Recoil ViewmodelRecoil;
    [SerializeField] private Transform weaponContainer;
    [SerializeField]private Camera cam;

    //[HideInInspector]
    public float speed;
    public bool Paused;

    public Weapon curweapon;
    public int curweaponIndex;
    private GameObject viewmodel;

    private float lastfired;
    private bool reloading = false;
    private Vector3 weapontargetpos;

    private bool inIdle;
    private bool aiming;

    public int[] weaponammo = new int[3];


    private Quaternion def;
    private void Start()
    {
        //Initialize weapons
        curweapon = Weap1;
        curweaponIndex = 0;
        ChangeWeapon();
        if (Weap1 != null)
            weaponammo[0] = Weap1.MaxAmmo;
        if(Weap2!=null)
            weaponammo[1] = Weap2.MaxAmmo;
        if (Weap3!=null)
            weaponammo[2] = Weap3.MaxAmmo;

        //Initialize Dictionary
        impactvfx.Clear();

        for (int i = 0; i < Mathf.Min(layers.Capacity, impacts.Capacity); i++)
        {
            impactvfx.Add(layers[i], impacts[i]);
        }
    }

    private void Update()
    {
        //Shooting
        if(Input.GetMouseButton(0) && !reloading && weaponammo[curweaponIndex] > 0)
        {
            if (curweapon.weaponType == Weapon.WeaponType.Auto && Time.time >lastfired+curweapon.Firerate )
            {
                Shoot();
                lastfired = Time.time;
            }
        }

        if(Input.GetMouseButtonDown(0)&&weaponammo[curweaponIndex]>0&&!reloading)
        {
            if(curweapon.weaponType == Weapon.WeaponType.SemiAuto && Time.time>lastfired+curweapon.Firerate)
            {
                Shoot();
                lastfired = Time.time;
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && !reloading&&weaponammo[curweaponIndex]!=curweapon.MaxAmmo)
            StartCoroutine(Reload());
        
        //Changing Weapon
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            curweapon = Weap1;
            curweaponIndex = 0;
            ChangeWeapon();
        }  
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            curweapon = Weap2;
            curweaponIndex = 1;
            ChangeWeapon();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            curweapon = Weap3;
            curweaponIndex = 2;
            ChangeWeapon();
        }

        //Aim
        if(Input.GetMouseButtonDown(1))
        {
            aiming = true;
            weapontargetpos = curweapon.AimPosition;   
        }
        else if(Input.GetMouseButtonUp(1))
        {
            aiming = false;
            weapontargetpos = curweapon.optimaposition;
        }

        if (speed==0)
        {
            inIdle = true;
        }
        else
            inIdle = false;

        Sway();
        Bob();

        //Move gun
        weaponContainer.localPosition = Vector3.Lerp(weaponContainer.localPosition, weapontargetpos, curweapon.aimspeed);
    }

    private IEnumerator Reload()
    {
        reloading = true;
        viewmodel.GetComponent<Animator>().Play("Reload");
        yield return new WaitForSeconds(curweapon.reloadtime);
        reloading = false;
        weaponammo[curweaponIndex] = curweapon.MaxAmmo;
        Debug.Log("Reloaded");
    }

    private void Shoot()
    {
        recoil.RecoilFire();
        //ViewmodelRecoil.RecoilFire();
        viewmodel.GetComponent<Animator>().Play("Shoot");
        Vector3 rayOrigin = cam.ViewportToWorldPoint(new Vector3(.5f, .5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin,cam.transform.forward,out hit,curweapon.range))
        {
            Debug.Log(hit.transform.gameObject.name);

            //impact vfx
            
            if(impactvfx.ContainsKey(hit.transform.gameObject.tag))
            {
                if (impactvfx[hit.transform.gameObject.tag] != null)
                {
                    GameObject impact = Instantiate(impactvfx[hit.transform.gameObject.tag]);
                    impact.transform.position = hit.point;
                    impact.transform.rotation = Quaternion.Euler(hit.normal);
                }
                else
                    Debug.LogWarning("No impact");
            }
        }

        weaponammo[curweaponIndex]--;

        //flash
        if (curweapon.Flash != null)
        {
            GameObject flash = Instantiate(curweapon.Flash, viewmodel.transform);
            flash.transform.localPosition = curweapon.localBarellpos;
        }
    }

    private float horizontal, vertical, timer, waveSlice;

    private void ChangeWeapon()
    {
        if(viewmodel!=null)
           Destroy(viewmodel);
        reloading = false;
        StopAllCoroutines();

        timer = 0;
        waveSlice = 0;

        viewmodel = Instantiate(curweapon.model, weaponContainer);
        viewmodel.transform.rotation = Quaternion.Euler(Vector3.zero);

        recoil.RecoilX = curweapon.recoilX;
        recoil.RecoilY = curweapon.recoilY;
        recoil.RecoilZ = curweapon.recoilZ;

        recoil.Snapiness = curweapon.snappines;
        recoil.ReturnSpeed = curweapon.returnspeed;

        //ViewmodelRecoil = viewmodel.GetComponent<Recoil>();

        //ViewmodelRecoil.RecoilX = curweapon.GrecoilX;
        //ViewmodelRecoil.RecoilY = curweapon.GrecoilY;
        //ViewmodelRecoil.RecoilZ = curweapon.GrecoilZ;

        //ViewmodelRecoil.Snapiness = curweapon.Gsnappines;
        //ViewmodelRecoil.ReturnSpeed = curweapon.Greturnspeed;

        viewmodel.transform.localPosition = Vector3.zero;
        weapontargetpos = curweapon.optimaposition;
        def = viewmodel.transform.localRotation;
    }



    private Vector3 midPoint=Vector3.zero;

    private void Bob()
    {
        horizontal = Input.GetAxis("Horizontal");

        Vector3 localPosition = viewmodel.transform.localPosition;

        if (Mathf.Abs(horizontal) == 0 && Mathf.Abs(vertical) == 0)
        {
            timer = 0.0f;
        }
        else
        {
            waveSlice = Mathf.Sin(timer);
            timer = timer + curweapon.bobSpeed * speed/1000;
            if (timer > Mathf.PI * 2)
            {
                timer = timer - (Mathf.PI * 2);
            }
        }
        if (waveSlice != 0)
        {
            float translateChange = waveSlice * curweapon.bobDistance;
            float totalAxes = Mathf.Abs(horizontal);
            totalAxes = Mathf.Clamp(totalAxes, 0.0f, 1.0f);
            translateChange = totalAxes * translateChange;
            localPosition.y = midPoint.y + translateChange;
        }
        else
        {
            localPosition.y = midPoint.y;
            localPosition.x = midPoint.x;
        }

        viewmodel.transform.localPosition = localPosition;
    }

    private void Sway()
    {
        float factorX = (Input.GetAxis("Mouse Y")) * curweapon.multiplier;
        float factorY = -(Input.GetAxis("Mouse X")) * curweapon.multiplier;
        float factorZ = -Input.GetAxis("Horizontal") * curweapon.multiplier;

        Quaternion Final = Quaternion.Euler(def.x + factorX, def.y + factorY, def.z + factorZ);
        viewmodel.transform.localRotation = Quaternion.Slerp(viewmodel.transform.localRotation, Final, (Time.deltaTime * curweapon.smoothnes));
    }
}
