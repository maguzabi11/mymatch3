using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Match3;

public class TileInputFactory : IFactory<UnityEngine.Object, TileInput>
{
    readonly DiContainer _container;

    public TileInputFactory(DiContainer container)
    {
        _container = container;
    }

    public TileInput Create(UnityEngine.Object prefab)
    {
        return _container.InstantiatePrefabForComponent<TileInput>(prefab);
    }
}
