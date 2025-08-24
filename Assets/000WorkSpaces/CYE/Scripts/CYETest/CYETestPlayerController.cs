using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CYETestPlayerController : MonoBehaviour
{
    private Rigidbody _rigidbody;
    
    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
    void Update()
    {
        _rigidbody.velocity = new Vector3(Input.GetAxis("Vertical"), 0, -Input.GetAxis("Horizontal")) * 3f;
    }
}
