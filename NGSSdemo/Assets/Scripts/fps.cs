using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class fps : MonoBehaviour
{
    public Text fpsText;
    private float deltaTime = 0.01f;
    // Start is called before the first frame update
    void Start()
    {
        fpsText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        fpsFonk();
    }

    public void fpsFonk()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.01f;
        float fps = 1.0f / deltaTime;
        fpsText.text =  "Fps: " + Mathf.Ceil(fps).ToString();
    }
}
