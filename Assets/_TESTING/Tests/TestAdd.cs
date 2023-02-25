using System.Collections;
using System.Collections.Generic;
using Core.DisplayDialogue;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestAdd
{
    // A Test behaves as an ordinary method
    [Test]
    public void TestAddSimplePasses()
    {
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator TestAddWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
