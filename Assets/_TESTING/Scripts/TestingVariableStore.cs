using System.Collections;
using System.Collections.Generic;
using Core.LogicalLine;
using UnityEngine;

namespace Testing
{
    public class TestingVariableStore : MonoBehaviour
    {
        public void Start()
        {
            VariableStore store = new VariableStore();
            store.CreateDatabase("db1");
            store.CreateDatabase("db2");
            store.PrintAllDatabases();

            store.CreateVariable("db1.var1", 10);
            store.CreateVariable("var1", 30);
            store.PrintAllVariables();

            store.TrySetValue("db1.not_found", 43);
            store.TrySetValue("db1.var1", 15);
            store.PrintAllVariables();

            store.TryGetVariable("db1.var1", out int x);
            Debug.Log($"x is {x}");
        }
    }
}