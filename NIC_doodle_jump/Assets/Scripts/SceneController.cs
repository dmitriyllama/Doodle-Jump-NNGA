using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SceneController : MonoBehaviour
{

    [SerializeField] private GameObject _regular_platform;
    [SerializeField] private GameObject _extrabounce_platform;
    [SerializeField] [Range(0, 10)] private float _number_of_platforms;
    [SerializeField] [Range(0, 1)] private float _extrabounce_platform_chance;
    [SerializeField] [Range(0, float.MaxValue)] private float _range_of_platform_spawn;
    private float _priviest_highest_platform = 9;
    private float _priviest_lowest_platform = -5f;
    
    // Start is called before the first frame update
    void Start()
    {
        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float getLowestPlatformPositionY()
    {
        return _priviest_lowest_platform;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Platform"))
        {
            float _new_height = _priviest_highest_platform + 2f + Random.Range(0f, 5.1f);
            float _new_x = Random.Range(-_range_of_platform_spawn, _range_of_platform_spawn);
            GameObject _new_platform;
            if (Random.Range(0f, 1f) > _extrabounce_platform_chance)
            {
                _new_platform = Instantiate(_regular_platform, new Vector2(_new_x,
                    _new_height), Quaternion.identity);    
            }
            else
            {
                _new_platform = Instantiate(_extrabounce_platform, new Vector2(_new_x,
                    _new_height), Quaternion.identity); 
            }
            
            _new_platform.gameObject.transform.localScale = new Vector3(Random.Range(1f, 10f),1, 1);
            _priviest_highest_platform = _new_height;

            _priviest_lowest_platform = other.gameObject.transform.position.y;
        }
        Destroy(other.gameObject);
    }

    private void CreateInitialPlatforms()
    {
        float platform_position_x = 0;
        float platform_position_y = -4.5f;

        for (int i = 0; i < _number_of_platforms; i++)
        {
            if (i == 0) _priviest_lowest_platform = platform_position_y;
            if (i == 4) _priviest_highest_platform = platform_position_y;
            
            GameObject _new_platform;
            if (Random.Range(0f, 1f) > _extrabounce_platform_chance)
            {
                _new_platform = Instantiate(_regular_platform, new Vector2(platform_position_x,
                    platform_position_y), Quaternion.identity);    
            }
            else
            {
                _new_platform = Instantiate(_extrabounce_platform, new Vector2(platform_position_x,
                    platform_position_y), Quaternion.identity); 
            }
            _new_platform.gameObject.transform.localScale = new Vector3(Random.Range(1f, 10f),1, 1);
            
            platform_position_y = platform_position_y + 2f + Random.Range(0f, 5.1f);
            platform_position_x = Random.Range(-_range_of_platform_spawn, _range_of_platform_spawn);
        }
    }

    private void DestroyAllPlatforms()
    {
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Platform");
        foreach (GameObject platform in platforms)
        {
            Destroy(platform);
        }
    }

    public void Reset()
    {
        DestroyAllPlatforms();
        CreateInitialPlatforms();
    }
}
