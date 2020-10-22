using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastDestination : MonoBehaviour
{
    private Ray ray;
    private RaycastHit hitInfo;

    [SerializeField]
    private Transform raycastOrigin;

    // Update is called once per frame
    void Update()
    {
      ray.origin = raycastOrigin.position;
      ray.direction = raycastOrigin.forward;

      if (Physics.Raycast(ray, out hitInfo))
        transform.position = hitInfo.point;
      else
        transform.position = ray.origin + ray.direction * 1000.0f;
    }
}
