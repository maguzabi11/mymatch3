using Zenject;
using NUnit.Framework;
using UnityEngine;


[TestFixture]
public class HelperTest : ZenjectUnitTestFixture
{
    [Test]
    public void RunTest1()
    {
        var path = Helper.GetStreamingAssetPath("mapinit.txt");
        Debug.Log(path);
        Assert.AreNotEqual(path.Length, 0);
    }
}