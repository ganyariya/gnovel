using System;
using Extensions;
using UnityEngine.Events;

namespace Core.CommandDB
{
    /// <summary>
    /// `Command` (キャラを動かす・表示するなど) を実行するときの Process を管理する Container 
    /// </summary>
    public class CommandProcess
    {
        public Guid ID { get; private set; }
        public string commandName { get; private set; }
        /// <summary>
        /// キャラを動かす・表示するなどの 実際に実行したいコマンド
        /// </summary>
        public Delegate command { get; private set; }
        /// <summary>
        /// `command` を MonoBehavior が Coroutine として実行する
        /// その Coroutine の Wrapper
        /// 
        /// Coroutine 生成の都合上 あとから coroutineWrapper を設定する必要がある
        /// </summary>
        public CoroutineWrapper coroutineWrapper { get; private set; }
        public string[] args { get; private set; }
        public UnityEvent onTerminateAction { get; private set; }

        public CommandProcess(
            Guid ID,
            string commandName,
            Delegate command,
            CoroutineWrapper runningCoroutineWrapper,
            string[] args,
            UnityEvent onTerminateAction
        )
        {
            this.ID = ID;
            this.commandName = commandName;
            this.command = command;
            this.coroutineWrapper = runningCoroutineWrapper;
            this.args = args;
            this.onTerminateAction = onTerminateAction;
        }

        public static CommandProcess Create(
            string commandName,
            Delegate command,
            string[] args
        )
        {
            return new CommandProcess(
                Guid.NewGuid(),
                commandName,
                command,
                null,
                args,
                null
            );
        }

        /// <summary>
        /// coroutine 生成の都合上 あとから set になってしまう
        /// </summary>
        public void SetCoroutineWrapper(CoroutineWrapper coroutineWrapper)
        {
            this.coroutineWrapper = coroutineWrapper;
        }

        /// <summary>
        /// Coroutine 停止要請が与えられたときに
        /// 本当にコルーチンを停止してよいか判定する
        /// </summary>
        public bool ShouldStopCoroutine()
        {
            return
                coroutineWrapper != null
                // まだ完了していないのであれば 停止作業を行ってよい
                && !coroutineWrapper.IsDone;
        }

        public void StopCoroutine()
        {
            coroutineWrapper.Stop();
        }

        public void ExecuteTerminationEvent()
        {
            onTerminateAction?.Invoke();
        }
    }
}

