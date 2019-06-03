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
        public void TestCreateTiles()
        {
            bool b = gp.CreateTiles();
            Assert.That(b == true);
            gp.OutputTiles();
        }

        [Test]
        public void TestMatchTile()
        {
            gp.CreateSpecificTilesforTest();
            
            Assert.IsTrue(gp.IsMatch3Tile(0, 2));
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
