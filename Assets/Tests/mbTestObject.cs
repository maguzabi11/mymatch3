using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Match3;

public class mbTestObject : MonoBehaviour
{
    GamePanel gp;

    // Start is called before the first frame update
    void Start()
    {
        gp = new GamePanel(4, 5);
        gp.CreateSpecificTilesforTest();
        gp.OutputTiles();

        int numMatch = gp.FindAllMatches();
        Debug.LogFormat( $"Matches:{numMatch}");

        gp.OutputMatches();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
