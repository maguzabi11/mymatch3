using Zenject;
using NUnit.Framework;
using Match3;

[TestFixture]
public class UntitledUnitTest : ZenjectUnitTestFixture
{
    [SetUp]
    public void CommonInstall()
    {
        SignalBusInstaller.Install(Container);
        Container.DeclareSignal<TileMovementSignal>();
        Container.DeclareSignal<TileDeleteSignal>();        
        Container.Bind<GamePanel>().AsSingle();
        Container.BindFactory<int, Point2D, Tile, Tile.Factory>();
        Container.Inject(this);
    }

    [Inject]
    GamePanel _gamepanel;

    [Test]
    public void 타일생성_매치3_발생하지않게()
    {
        //var gp = Container.Resolve<GamePanel>();
        var gp = _gamepanel;
        gp.CreatePanel(4, 5);
        gp.CreateTilesWithoutMatch3(null, null);
        Assert.That(gp.FindAllMatches() == 0);
        gp.OutputTiles();
    }
}