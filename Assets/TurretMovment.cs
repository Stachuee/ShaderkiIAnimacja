using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretMovment : MonoBehaviour
{
    [SerializeField]
    Transform turret;

    [SerializeField]
    Transform turretObject;

    [SerializeField, Range(1, 50)]
    private float sensitivity;


    private float rotationX;
    private float rotationY;

    [SerializeField]
    Camera cam;

    [SerializeField]
    LayerMask ignoreLayer;

    private void Update()
    {
        float axisX = Input.GetAxis("Mouse X") * sensitivity;
        float axisY = -Input.GetAxis("Mouse Y") * sensitivity;

        rotationX += axisX;
        rotationY += axisY;

        Vector3 newRotation = new Vector3(rotationY, rotationX, 0);

        turret.localEulerAngles = newRotation;


        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, Mathf.Infinity, ~ignoreLayer))
        {
            turretObject.rotation = Quaternion.LookRotation(hit.point - turretObject.position, turret.up);
        }
        else
        {
            turretObject.localEulerAngles = newRotation;
        }
    }
}
