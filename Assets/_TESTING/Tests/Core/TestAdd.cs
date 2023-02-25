using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestAdd
{
    [Test]
    public void TestAddSimplePasses()
    {
        var a = 10;
        var b = 10;
        Assert.That(a == b);
    }
}
