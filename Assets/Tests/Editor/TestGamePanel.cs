using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using Zenject;
using NUnit.Framework;
using Match3;

namespace Tests
{
    [TestFixture]
    // fixture 자동?
    public class TestGamePanel : ZenjectUnitTestFixture
    {
        [Inject]
        GamePanel gp;

        [SetUp]
        public void CommonInstall()
        {
            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<TileMovementSignal>().OptionalSubscriber();
            Container.DeclareSignal<TileDeleteSignal>().OptionalSubscriber();  
            Container.DeclareSignal<FillTileSignal>().OptionalSubscriber();
            Container.DeclareSignal<TileDropSignal>();

            Container.BindFactory<UnityEngine.Object, TileInput, TileInput.Factory>()
                .FromFactory<TileInputFactory>()
                .WhenInjectedInto<TileBuilder>();
                
            Container.Bind<TileBuilder>().AsSingle().NonLazy(); 
            Container.Bind<ScoreManager>().AsSingle().NonLazy();
            Container.Bind<GamePanel>().AsSingle();
            Container.Bind<MatchingChecker>().AsSingle();
            Container.BindFactory<int, Point2D, Tile, Tile.Factory>();
            Container.Inject(this);

            Container.Resolve<GamePanel>().CreatePanel(4,5);
        }

        [Test]
        public void TestGamePanelSize()
        {
            Assert.AreEqual( gp.tiles.Length, 20 );
        }

        [Test]
        public void 교환할_수_있는가()
        {
            gp.CreateSpecificTilesforTest();
            Assert.IsFalse(gp.IsSwapable(0, 2));
            Assert.IsTrue(gp.IsSwapable(1, 0));
        }

        [Test]
        public void TestMatchTiles()
        {
            gp.CreateSpecificTilesforTest();
            gp.OutputTiles();            
            Assert.That(gp.FindAllMatches(), Is.EqualTo(1));
            gp.OutputMatches();
        }

        [Test]
        public void DeleteMatchTiles()
        {
            gp.CreateSpecificTilesforTest();
            gp.FindAllMatches();
            gp.DeleteMatchTiles(); // 일련의 과정 중 하나
            gp.OutputTiles();
        }

        [Test]
        public void 타일생성_매치3_발생하지않게()
        {
            gp.CreatePanel(4, 5);
            gp.CreateTilesWithoutMatch();
            Assert.That(gp.FindAllMatches() == 0);
            gp.OutputTiles();
        }

        [Test]
        public void 빈자리에타일이동()
        {
            gp.CreateSpecificTilesforTest();
            gp.FindAllMatches();
            gp.DeleteMatchTiles(); // 일련의 과정 중 하나

            gp.FillTilesToEmptyPlace();
            gp.OutputTiles();
            Assert.That(gp.tiles[3,0].Type, Is.EqualTo(3));
        }

        [Test]
        public void 타일_스왑후에_매치발생확인()
        {
            gp.CreateSpecificTilesforTest();
            Assert.That(gp.TrySwapTile(gp.tiles[1,1], TileMovement.Right), Is.EqualTo(SwapTileResult.Success));
        }

        [Test]
        public void 타일_스왑후_매치확인2()
        {
            gp.CreateSpecificTilesforTest();
            Assert.That(gp.TrySwapTile(gp.tiles[0,2], TileMovement.Down), Is.EqualTo(SwapTileResult.Success));
        }

        [Test]
        public void 타일_스왑후_매치확인3()
        {
            gp.CreateSpecificTilesforTest();
            Assert.That(gp.TrySwapTile(gp.tiles[1,4], TileMovement.Down), Is.EqualTo(SwapTileResult.Success));
            gp.DeleteMatchTiles();
            gp.OutputTiles();
        }

        [Test]
        public void 매치된_타일_중복확인()
        {
            gp.CreateforMatchTileTestCase();
            gp.FindAllMatches();
            gp.DeleteMatchTiles();
            //gp.OutputTiles();
        }

        [Test]
        public void 매치_가능_검사()
        {
            gp.CreateSpecificTilesforTest();
            int num = gp.NumMatchable(); // 각 매치에 대해 확인해야함.
            Debug.LogFormat($"matchable count: {num}");

            Assert.That(num, Is.EqualTo(15));
        }

        [Test]
        public void 매치가능이없으면_매치가능한타일생성()
        {
            gp.CreateNotMatchableCaseAfterDelete();
            gp.FindAllMatches();
            gp.DeleteMatchTiles(); // 일련의 과정 중 하나
            gp.FillTilesToEmptyPlace();

            gp.OutputTiles();
               // Assert.That(false);
        }

        [Test]
        public void 매치_Cross1()
        {
            gp.CreateforMatchTileTestCase(); // '+'형
            //gp.CreateMatchWithCross(); // 'ㄱ'형
            gp.FindAllMatches(); // 움직여서 검사?

            Assert.That(gp.IsExistRemover(RemoveType.Bomb));
        }

        [Test]
        public void 매치_가로줄제거()
        {
            gp.CreateMatch4();
            gp.FindAllMatches();
            Assert.That(gp.IsExistRemover(RemoveType.HorizonRemover));
        }

        [Test]
        public void 매치_세로줄제거()
        {
            gp.CreateMatch4();
            gp.FindAllMatches();
            Assert.That(gp.IsExistRemover(RemoveType.VerticalRemover));
        }

        [Test]
        public void 매치_2by2()
        {
            gp.CreateMatchSqure22();
            gp.IsMatch2by2(0, 3);
            Assert.That(gp.IsExistRemover(RemoveType.Butterfly));            
        }

       [Test]
        public void 매치_2by2And3withFindAll()
        {
            gp.CreateMatchSqure22_and_3();
            gp.FindAllMatches();
            Assert.That(gp.IsExistRemover(RemoveType.Butterfly));            
            gp.DeleteMatchTiles();
        }        

        //// A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        //// `yield return null;` to skip a frame.
        //[UnityTest]
        //public IEnumerator TestGamePanelWithEnumeratorPasses()
        //{
        //    // Use the Assert class to test conditions.
        //    // Use yield to skip a frame.
        //    yield return null;
        //}
    }
}
