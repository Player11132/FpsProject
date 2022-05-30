using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float range;
    public string interactable_tag;
    public LayerMask raycast_layer;
    public Camera cam;
    private IInteractable target;
    private void Update()
    {
        Detect();
        if(Input.GetKeyDown(KeyCode.E))
            if (target != null)
                target.OnInteract();
        
    }

    private void Detect()
    {
        RaycastHit hit;
        Vector3 rayOrigin = cam.ViewportToWorldPoint(new Vector3(.5f, .5f, 0));
        if (Physics.Raycast(rayOrigin, cam.transform.forward, out hit, range))
        {
            print(hit.transform.gameObject.name);
            if (hit.transform.gameObject.CompareTag(interactable_tag))
            {
                IInteractable detected = hit.transform.gameObject.GetComponent<IInteractable>();
                if (detected != null)
                {
                    if (detected == target)
                        return;
                    else if (target != null)
                    {
                        target.OnEndHover();
                        target = detected;
                        target.OnStartHover();
                        return;
                    }
                    else
                    {
                        target = detected;
                        target.OnStartHover();
                        return;
                    }
                }
                else
                {
                    target.OnEndHover();
                    target = null;
                    return;
                }
            }
        }
        else
        {
            if(target!=null)
                target.OnEndHover();
            target = null;
            return;
        }
    }
}
