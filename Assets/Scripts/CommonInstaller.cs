using UnityEngine;
using Zenject;
using Match3;

public class CommonInstaller : MonoInstaller
{

    public override void InstallBindings()
    {
        SignalBusInstaller.Install(Container);
        Container.DeclareSignal<TileMovementSignal>();
        Container.DeclareSignal<TileDeleteSignal>();
        Container.DeclareSignal<FillTileSignal>();
        Container.DeclareSignal<TileDropSignal>();        

        // 방법1
        // Container.BindSignal<TileMovementSignal>()
        //     .ToMethod<TileController>(x => x.OnTileMoveSignal).FromResolve();
        
        Container.BindFactory<UnityEngine.Object, TileInput, TileInput.Factory>()
            .FromFactory<TileInputFactory>()
            .WhenInjectedInto<TileBuilder>();

        Container.Bind<TileBuilder>().AsSingle().NonLazy();
        Container.Bind<GamePanel>().AsSingle();
        Container.BindFactory<int, Point2D, Tile, Tile.Factory>();
    }


}