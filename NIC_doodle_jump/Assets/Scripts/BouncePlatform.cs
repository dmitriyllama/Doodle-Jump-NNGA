using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncePlatform : MonoBehaviour
{
    
    [SerializeField] private float bounce_force = 600f; //controls height of bounce
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (Mathf.Abs(other.gameObject.GetComponent<Rigidbody2D>().velocity.y) <= 0.01)
        {
            other.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.up * bounce_force);
        }
    }
}
