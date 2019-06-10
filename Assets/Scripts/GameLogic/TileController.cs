using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Match3;

public class TileController : MonoBehaviour
{
    [Inject]
    TileBuilder tilebuilder;

    GamePanel gp;

    [SerializeField]
    GameObject tileRoot;

    void Awake()
    {
        tileRoot = gameObject;
    }

    // Start is called before the first frame update
    void Start()
    {
        gp = new GamePanel(4, 5);
        gp.CreateTilesWithoutMatch3(tilebuilder, tileRoot); // 반복된 루프 호출을 피하기 위함.
        InitTilePosition();
    }

    public void InitTilePosition()
    {
        // 타일 배치
        /*
        - 홀수(3이상): i-iCenter;
        - 짝수 개일 때 위치: i - iCenter + 0.5
         */        
        int iHoriCenter = gp.NumCol / 2;
        int iVertCenter = gp.NumRow / 2;
        float xOffset = (gp.NumCol%2 == 0) ? 0.5f : 0f;
        float yOffset = (gp.NumRow%2 == 0) ? 0.5f : 0f;
        for( int i=0; i<gp.NumRow; i++)
        {
            for( int j=0; j<gp.NumCol; j++)
            {
                float y = i-iVertCenter + yOffset;
                float x = j-iHoriCenter + xOffset;
                gp.tiles[i,j].SetPosition(x, y);
            }
        }
    }


}
