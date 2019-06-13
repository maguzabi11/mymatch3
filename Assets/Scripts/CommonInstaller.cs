using UnityEngine;
using Zenject;
using Match3;

public class CommonInstaller : MonoInstaller
{

    public override void InstallBindings()
    {
        SignalBusInstaller.Install(Container);
        Container.DeclareSignal<TileMovementSignal>();
        
        Container.BindFactory<UnityEngine.Object, TileInput, TileInput.Factory>()
            .FromFactory<TileInputFactory>();
            // .WhenInjectedInto<TileBuilder>()
            // .NonLazy();

        Container.Bind<TileBuilder>().AsSingle().NonLazy();
       
        // TileController는 게임 오브젝트의 컴포넌트다.
        // var tilemanager = GameObject.Find("TileManager");
        // Container.Bind<TileController>().FromComponentOn(tilemanager);
    }


}