using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DestroyOnLowPosition : MonoBehaviour
{
    [SerializeField] private SceneController _scene_controller;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
 /*   void Update()
    {
        if (transform.position.y < _scene_controller.getLowestPlatformPositionY())
        {
            Debug.Log("You lose!");
            _scene_controller.Reset();
            transform.position = new Vector3(0, 0, 0);
        }
    }
*/    
}
