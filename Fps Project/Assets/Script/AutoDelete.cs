using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDelete : MonoBehaviour
{
    [Header("Auto deletes objects to improove performance")]
    public float time_to_destroy;
    void Start()
    {
        StartCoroutine(countdown());
    }

    private IEnumerator countdown()
    {
        yield return new WaitForSeconds(time_to_destroy);
        Destroy(gameObject);
    }

}
