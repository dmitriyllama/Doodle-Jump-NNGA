using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;

public class Individual : MonoBehaviour
{

    [SerializeField] private TMP_Text _text_score;
    [SerializeField] private TMP_Text _text_time;
    [SerializeField] private TMP_Text _text_ratio;
    private float _score = 0f;
    private float _time = 0f;

    public float getScore()
    {
        return _score;
    }

    public void resetScore()
    {
        _score = 0;
        _time = 0;
        _text_score.text = "score: " + String.Format("{0:N2}", _score);
        _text_time.text = "time: " + String.Format("{0:N2}", _time);
        _text_ratio.text = "score per time: 0";
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _time += Time.deltaTime;
        if (transform.position.y > _score)
        {
            _score = transform.position.y;
        }
        _text_score.text = "score: " + String.Format("{0:N2}", _score);
        _text_time.text = "time: " + String.Format("{0:N2}", _time);
        _text_ratio.text = "score per time: " + String.Format("{0:N2}", (_score / _time));
    }
}
