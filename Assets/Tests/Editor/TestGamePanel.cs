using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Match3;

namespace Tests
{
    // fixture 자동?
    public class TestGamePanel
    {
        GamePanel gp;

        [SetUp]
        public void Setup()
        {
            // Use the Assert class to test conditions
            gp = new GamePanel(4, 5);
        }

        [Test]
        public void TestGamePanelSize()
        {
            Assert.AreEqual( gp.tiles.Length, 20 );
        }

 
        [Test]
        public void 타일생성_매치3_발생하지않게()
        {
            gp.CreateTilesWithoutMatch3(null, null);
            Assert.That(gp.FindAllMatches() == 0);
            gp.OutputTiles();
        }

        [Test]
        public void 매치발생확인()
        {
            gp.CreateSpecificTilesforTest();
            
            Assert.IsTrue(gp.IsMatch3Tile(0, 2));
        }

        [Test]
        public void TestMatchTiles()
        {
            gp.CreateSpecificTilesforTest();
            Assert.That(gp.FindAllMatches(), Is.EqualTo(2));
            gp.OutputTiles();
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
        public void 빈자리에타일이동()
        {
            gp.CreateSpecificTilesforTest();
            gp.FindAllMatches();
            gp.DeleteMatchTiles(); // 일련의 과정 중 하나
            gp.FillTilesToEmptyPlace();
            Assert.That(gp.tiles[3,0].Type, Is.EqualTo(3));
            gp.OutputTiles();
        }

        [Test]
        public void 타일_스왑후에_매치발생확인()
        {
            gp.CreateSpecificTilesforTest();
            Assert.That(gp.TrySwapTile(gp.tiles[1,1], TileMovement.Right), Is.True);
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
