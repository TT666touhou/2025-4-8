using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteoriteScript : MonoBehaviour
{
    public int damage = 10;
    public float bulletSpeed = 10f;
    public float maxDistance = 10f;
    private Vector3 direction;
    private Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        direction = dir.normalized;
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
