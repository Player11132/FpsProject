using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Recoil made using Gilbert tutorial on recoil
public class Recoil : MonoBehaviour
{
    public float RecoilX;
    public float RecoilY;
    public float RecoilZ;

    public float Snapiness;
    public float ReturnSpeed;

    private Vector3 curentrotation;
    public Vector3 targetrotation;


    private void Update()
    {
        targetrotation = Vector3.Lerp(targetrotation, Vector3.zero, ReturnSpeed * Time.deltaTime);
        curentrotation = Vector3.Slerp(curentrotation, targetrotation, Snapiness * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(curentrotation);
    }

    public void RecoilFire()
    {
        targetrotation += new Vector3(RecoilX, Random.Range(-RecoilY, RecoilY), Random.Range(-RecoilZ, RecoilZ));
    }
}
