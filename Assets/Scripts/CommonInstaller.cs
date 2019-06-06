using UnityEngine;
using Zenject;
using Match3;

public class CommonInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<TileBuilder>().AsSingle().NonLazy();
        //Container.Bind<GamePanel>().AsSingle().WhenInjectedInto<TileController>();
    }
}