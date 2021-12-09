using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public float delta = 0;
    public float speed = 1;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = speed * delta;
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = speed * delta;
    }
}
