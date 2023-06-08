using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCheck : MonoBehaviour
{
    public GameObject Player;
    private Animator an_Player;

    void Start()
    {
        an_Player = Player.GetComponent<Animator>();
        // an_Player.SetLayerWeight(1,0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
            an_Player.SetLayerWeight(1,1);
    }

    // private void OnTriggerExit(Collider other)
    // {
    //     an_Player.SetLayerWeight(1,0);
    // }

    // // Update is called once per frame
    // void Update()
    // {
        
    // }
}
