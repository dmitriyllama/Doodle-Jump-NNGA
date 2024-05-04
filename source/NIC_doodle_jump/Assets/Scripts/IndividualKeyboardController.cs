using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndividualKeyboardController : MonoBehaviour
{
    private IndividualMovement _im;
    // Start is called before the first frame update
    void Start()
    {
        _im = GetComponent<IndividualMovement>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _im.Move(Input.GetAxis("Horizontal"));
    }
}
