using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    public Transform Target;
    public float orbitspeed;
    Vector3 offset;
    void Start()
    {
        offset = transform.position - Target.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Target.position + offset;
        transform.RotateAround(Target.position, Vector3.up, orbitspeed * Time.deltaTime);

        offset = transform.position - Target.position;

    }
}
