using UnityEngine;
using Zenject;
using Match3;

public class CommonInstaller : MonoInstaller
{

    public override void InstallBindings()
    {
        SignalBusInstaller.Install(Container);
        Container.DeclareSignal<TileMovementSignal>();

        // 방법1
        // Container.BindSignal<TileMovementSignal>()
        //     .ToMethod<TileController>(x => x.OnTileMoveSignal).FromResolve();
        
        Container.BindFactory<UnityEngine.Object, TileInput, TileInput.Factory>()
            .FromFactory<TileInputFactory>()
            .WhenInjectedInto<TileBuilder>();

        Container.Bind<TileBuilder>().AsSingle().NonLazy();
       
        // var tilemanager = GameObject.Find("TileManager");
        // Container.Bind<TileController>().FromComponentOn(tilemanager);
    }


}