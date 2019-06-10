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
        InitTiles();
    }

    public void InitTiles()
    {
        gp = new GamePanel(4, 5);
        gp.CreateTilesWithoutMatch3(tilebuilder, tileRoot); // 반복된 루프 호출을 피하기 위함.
        gp.SetTilePosition();
    }


}
