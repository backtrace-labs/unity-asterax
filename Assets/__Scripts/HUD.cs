using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class HUD : MonoBehaviour
{
    private Text textbox;
    void Start()
    {
        textbox = GetComponent<Text>();
    }

    void Update()
    {
        if (PlayerShip.S == null) {
           textbox.text = "You died, fool!";
           return;
        }

        textbox.text = "Health: " + PlayerShip.S.health + "\n" 
                    + "Bullets: " + PlayerShip.S.bullets + "\n"
                    + "Score: " + AsteraX.score;
    }
}
