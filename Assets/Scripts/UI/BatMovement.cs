using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class BatMovement : MonoBehaviour
{
    AudioSource audio;
    public float timer = 0;
    public float resetInterval = 25f;
    // Start is called before the first frame update
    public Vector3 startPosition = new Vector3(0, 0, 0);
    void Start()
    {
        transform.position = startPosition;
        audio = GetComponent<AudioSource>();
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= resetInterval)
        {
            audio.Play();
            timer = 0;
            transform.position = startPosition;
        }
        else
        {
            transform.position += new Vector3(-5f, 3f, 0);
        }
        
    }
}
