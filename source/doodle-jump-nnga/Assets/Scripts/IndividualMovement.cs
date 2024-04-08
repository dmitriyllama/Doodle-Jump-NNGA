using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndividualMovement : MonoBehaviour
{
    private Rigidbody2D _rb;
    [SerializeField] private float speed = 10f;
    
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Move(float moveInput)
    {
        _rb.velocity = new Vector2(speed * moveInput, _rb.velocity.y);    
    }
}
