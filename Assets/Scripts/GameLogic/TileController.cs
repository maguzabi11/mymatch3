using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Match3;

public class TileController : MonoBehaviour
{
    [Inject]
    TileBuilder tilebuilder;

    [Inject]
    SignalBus _signalBus;

    [Inject]
    GamePanel gp;

    [SerializeField]
    GameObject tileRoot;

    [Inject]
    public void InitInject(SignalBus signalBus, GamePanel gamepanel)
    {
        _signalBus = signalBus;
        _signalBus.Subscribe<TileMovementSignal>(OnTileMoveSignal);
        gp = gamepanel;
    }

    void Awake()
    {
        tileRoot = gameObject;
        tilebuilder.tileRoot = tileRoot;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitTiles();
    }

    public void InitTiles()
    {
        gp.CreatePanel(4, 5);
        gp.CreateTilesWithoutMatch3(); // 반복된 루프 호출을 피하기 위함.
        // 다양하게 생성할 수 있도록... 외부 스크립트를 활용 고려.
        gp.OutputTiles();
        gp.SetTilePosition();
    }

    public void OnTileMoveSignal(TileMovementSignal signal)
    {
        ReqMoveTile( signal.tile, signal.movement);
    }

    void OnDestroy()
    {
        _signalBus.Unsubscribe<TileMovementSignal>(OnTileMoveSignal);
    }

    // 작업 중
    // 이름 변경 후보: TrySwapAdjacentTile
    public void ReqMoveTile(Tile tile, TileMovement move)
    {
        if( gp.TrySwapTile(tile, move) == SwapTileResult.NoTile )
            TileInput.blockInput = false;
    }

}