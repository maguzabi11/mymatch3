using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Match3;
using Zenject;

public class mbTestObject : MonoBehaviour
{
    [Inject]
    TileBuilder tileBuilder;

    GamePanel gp;

    // Start is called before the first frame update
    void Start()
    {
        gp = new GamePanel(4, 5);
        gp.CreateTiles();
        gp.OutputTiles();

        int numMatch = gp.FindAllMatches();
        Debug.LogFormat( $"Matches:{numMatch}");

        gp.OutputMatches();

        //

       // GameObject objTile = tileBuilder.CreateTileResource(1);
        //Debug.Assert(objTile);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
