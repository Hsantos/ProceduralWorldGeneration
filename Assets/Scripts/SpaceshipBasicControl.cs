using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceshipBasicControl : MonoBehaviour
{
    public float speed = 12f;
    public float rotation = 180f;
    
    private Rigidbody rb;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        rb.AddRelativeForce(transform.forward * speed, ForceMode.Force);
    }

    private void Update()
    {
        Move();
        Turn();
    }
    
    private void Move() {
        Vector3 movement = transform.forward * Input.GetAxis("Vertical") * speed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);
    }

    private void Turn() {
        float giro = Input.GetAxis("Horizontal") * rotation * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, giro, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }
}
