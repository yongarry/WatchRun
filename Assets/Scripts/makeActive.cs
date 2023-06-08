using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class makeActive : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Invoke("Active", 2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Active()
    {
        Debug.Log(this.gameObject.transform.childCount);
        if(!this.gameObject.transform.Find("Wood Wall - Vertical - Basic Pack").gameObject.activeSelf)
            this.gameObject.transform.GetChild(1).gameObject.SetActive(true);
        // if(!this.gameObject.transform.GetChild(0).gameObject.activeSelf)
        // {
        //     Debug.Log("fuck");
        //     this.gameObject.transform.GetChild(0).gameObject.SetActive(true);
        // }
    }
}
