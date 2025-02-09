using System.Collections;
using System.Collections.Generic;
using Core.CommandDB;
using NUnit.Framework;
using UnityEngine;

namespace Tests.CommandDB.Database
{
    public class TestCommandParameterFetcher
    {
        [Test]
        [TestCase(
            "[bool] -i が取得できる",
            new string[] { "cake", "-i", "true" },
            new string[] { "-i" },
            true,
            true,
            false
        )]
        [TestCase(
            "[bool] -immediate が取得できる && 取得パラメータの先頭が優先される",
            new string[] { "cake", "-immediate", "true", "-i", "false" },
            // -i が優先される
            new string[] { "-i", "-immediate" },
            true,
            false,
            false
        )]
        [TestCase(
            "存在しない場合は結果が false になる& defaultValue が利用される",
            new string[] { "cake", "false" },
            new string[] { "-i" },
            false,
            true,
            true
        )]
        [TestCase(
            "[int] int として取得できる",
            new string[] { "cake", "false", "-i", "10", "20" },
            new string[] { "-i" },
            true,
            10,
            100
        )]
        [TestCase(
            "[float] float として取得できる",
            new string[] { "cake", "false", "-i", "10.40", "20" },
            new string[] { "-i" },
            true,
            10.40f,
            100.0f
        )]
        [TestCase(
            "[string] bool|int|float としてパースできない場合は string になる",
            new string[] { "cake", "false", "-i", "hello, world", "20" },
            new string[] { "-i" },
            true,
            "hello, world",
            ""
        )]
        [TestCase(
            "[string] パラメータをなにも指定しない場合は defaultValue をそのまま帰す",
            new string[] { "cake", "false", "-i", "hello, world", "20" },
            new string[] { },
            false,
            0,
            0
        )]
        [TestCase(
            "[float] マイナスを指定してもちゃんと取得できる",
            new string[] { "-y", "-20" },
            new string[] { "-y" },
            true,
            -20,
            0
        )]
        [TestCase(
            "最後のパラメータが key のみであった場合はスキップする",
            new string[] { "-y", "-20", "-x" },
            new string[] { "-x" },
            false,
            0,
            0
        )]
        public void TestTryGetValue<T>(
            string testCaseName,
            string[] inputParameters,
            string[] targetParameterNames,
            bool expectedTryGetValueResult,
            T expectedValue,
            T defaultValue
        )
        {
            var fetcher = new CommandParameterFetcher(inputParameters);
            var actualResult = fetcher.TryGetValue<T>(targetParameterNames, out T actualValue, defaultValue: defaultValue);
            Assert.That(actualResult, Is.EqualTo(expectedTryGetValueResult), testCaseName + " expectedTryGetValueResult");
            Assert.That(actualValue, Is.EqualTo(expectedValue), testCaseName + " expectedValue");
        }
    }
}

