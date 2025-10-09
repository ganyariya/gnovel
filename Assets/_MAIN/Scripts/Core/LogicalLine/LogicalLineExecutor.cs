using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.DisplayDialogue;
using Core.ScriptParser;
using UnityEngine;

namespace Core.LogicalLine
{
    public class LogicalLineExecutor
    {
        private DialogueSystemController DialogueSystemController => DialogueSystemController.instance;
        private readonly List<ILogicalLine> _logicalLines = new List<ILogicalLine>();

        public LogicalLineExecutor()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            var types = assembly
                .GetTypes()
                .Where(t => typeof(ILogicalLine).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToArray();

            foreach (var type in types)
            {
                var line = (ILogicalLine)Activator.CreateInstance(type);
                _logicalLines.Add(line);
            }
        }

        /// <summary>
        /// lineData が `LogicalLine` かチェックし、そうであれば処理を実行する
        ///
        /// TryExecute を呼び出した側で coroutine を yield して待機してもらう
        /// </summary>
        public bool TryExecute(DialogueLineData lineData, DialogueSystemController dialogueSystemController, out Coroutine coroutine)
        {
            foreach (var logicalLine in _logicalLines.Where(logicalLine => logicalLine.Match(lineData)))
            {
                coroutine = DialogueSystemController.StartCoroutine(logicalLine.Execute(lineData, dialogueSystemController));
                return true;
            }

            coroutine = null;
            return false;
        }
    }
}