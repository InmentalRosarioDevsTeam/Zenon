using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAnim : MonoBehaviour
{
    private void Update()
    {
        transform.Rotate(Vector3.forward * 200f * Time.deltaTime);
    }
}
