using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SimpleController : NetworkBehaviour
{
    [SerializeField] private float speed = 10.0f;

    private void Update()
    {
        if (isLocalPlayer && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
        {
            transform.position += Time.deltaTime * speed *
                                  (Vector3.right * Input.GetAxis("Horizontal") +
                                   Vector3.up * Input.GetAxis("Vertical"));
        }
    }
}