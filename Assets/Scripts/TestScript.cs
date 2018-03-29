using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour {
    public GameObject text;
    bool status;


    private void Start()
    {
        status = text.activeSelf;    
    }


    public void Toggle ()
    {
        status = !status;
        text.SetActive(status);
    }
}
