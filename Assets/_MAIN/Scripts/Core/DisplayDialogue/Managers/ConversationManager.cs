using System;
using System.Collections;
using System.Collections.Generic;
using Core.Characters;
using Core.CommandDB;
using Core.ScriptParser;
using Extensions;
using UnityEngine;

namespace Core.DisplayDialogue
{
    /// <summary>
    /// RawTestList をもとに DialogueLineData を生成し 非同期で textArchitect を使って画面に出力する
    /// 
    /// MonoBehavior でないことに注意
    /// </summary>
    public class ConversationManager
    {
        public bool IsRunning => process != null;

        private readonly DialogueSystemController dialogueSystem;
        public readonly DisplayTextArchitect textArchitect;
        private Coroutine process;
        private bool userPromptNext = false;

        private TagManager tagManager;
        
        // AutoReader で待機セグメントコマンドを考慮するため
        public bool IsWaitingSegmentSignal { get; private set; } = false;

        public ConversationManager(DialogueSystemController dialogueSystem, DisplayTextArchitect textArchitect)
        {
            dialogueSystem.UserPromptNextEvent += UserPromptNextEventReceived; // イベントを subscribe する
            this.dialogueSystem = dialogueSystem;
            this.textArchitect = textArchitect;
            this.process = null;
            tagManager = new TagManager();
        }

        /// <summary>
        /// DisplaySystemController からイベントが発火されたときの Subscribe 処理
        /// 次の処理に進む
        /// </summary>
        private void UserPromptNextEventReceived()
        {
            userPromptNext = true;
        }

        /// <summary>
        /// DialogueSystemController から呼び出される
        /// 
        /// rawText を split して
        /// - DialogueLineData に変換
        /// - textArchitect を介して会話を画面に出力
        /// する
        /// </summary>
        public Coroutine StartConversation(List<string> conversation)
        {
            StopConversation();

            // Coroutine 自体は MonoBehavior を持つ dialogueSystem に移譲する
            process = dialogueSystem.StartCoroutine(RunningConversation(conversation));

            return process;
        }

        public void StopConversation()
        {
            if (process != null) dialogueSystem.StopCoroutine(process);
            process = null;
        }

        private IEnumerator RunningConversation(List<string> conversation)
        {
            foreach (var rawText in conversation)
            {
                if (string.IsNullOrWhiteSpace(rawText)) continue;

                // 生 string をパースして DialogueLineData に変換する
                DialogueLineData lineData = DialogueParser.Parse(rawText);

                if (lineData.HasDialogue) yield return RunningSingleDialogue(lineData);
                if (lineData.HasCommands) yield return RunningSingleCommands(lineData);

                // Dialogue がある場合のみ待つ
                if (lineData.HasDialogue)
                {
                    // 会話がある場合は それまでに実行されていたコマンドをすべて停止させる
                    // (会話がある場合を `ある区切り点(静止点) にしたいため`)
                    CommandManager.instance.StopAllCommandProcesses();
                    yield return WaitForUserAdvance();
                }
            }
        }

        /// <summary>
        /// ある 1 つの DialogueLineData.dialogueLine をもとに 画面にテキストを出力する
        /// 内部で dialogueLine.Segments を呼び出す
        /// </summary>
        private IEnumerator RunningSingleDialogue(DialogueLineData lineData)
        {
            if (lineData.HasSpeaker) HandleSpeakerLogic(lineData.speakerData);

            // 会話が始まるときにもし DialogueContainer が見えない状態だったら見えるようにする
            // 会話が始まったら必ず会話ボックスを表示したいため
            if (!dialogueSystem.dialogueContainer.isVisible) dialogueSystem.dialogueContainer.Show();

            foreach (var segment in lineData.dialogueData.segments)
            {
                yield return RunningSingleDLDDialogueSegment(segment);
            }

            // 動画だとここで WaitForUserAdvance を呼び出していたが
            // どうやら過去の自分がこの処理を別の場所に移したらしい
            // そのためコメントアウトされている
            // yield return WaitForUserAdvance();
        }

        private void HandleSpeakerLogic(DLD_SpeakerData speakerData)
        {
            var character =
                CharacterManager.instance.GetCharacter(speakerData.name, speakerData.needCharacterInstanceCreation);

            if (speakerData.isAppearanceCharacter && !character.isVisible && !character.isRevealing)
            {
                character.Show();
            }

            // UI にキャラ名を表示する
            // (why) rawText を事前に tagManager.Inject してしまうと、 CharacterConfig `<mainChara>` ができなくなってしまう
            // そのためわざわざ speakerName と dialogueSegment それぞれ直前に tagManager.Inject している
            var uiSpeakerName = tagManager.Inject(speakerData.DisplayName);
            dialogueSystem.DisplaySpeakerName(uiSpeakerName);
            // UI のキャラ名にフォントとフォントカラー設定を反映する
            dialogueSystem.ApplySpeakerConfigToDialogueContainer(speakerData.name);

            if (speakerData.isCastingPosition)
            {
                character?.MoveToScreenPosition(speakerData.castPosition);
            }

            if (speakerData.isCastingExpressions)
            {
                foreach (var (layer, expression) in speakerData.CastExpressions)
                {
                    character.CastingExpression(layer, expression);
                }
            }
        }

        private IEnumerator RunningSingleDLDDialogueSegment(DLD_DialogueSegment segment)
        {
            yield return WaitForDialogueSegmentTriggered(segment);
            yield return DisplayingSingleSegmentDialogueText(segment.dialogue, segment.IsAppendText);
        }

        private IEnumerator WaitForDialogueSegmentTriggered(DLD_DialogueSegment segment)
        {
            switch (segment.startSignal)
            {
                case StartSignal.C:
                case StartSignal.A:
                    yield return WaitForUserAdvance();
                    break;
                case StartSignal.WA:
                case StartSignal.WC:
                    IsWaitingSegmentSignal = true;
                    yield return new WaitForSeconds(segment.signalDelay);
                    IsWaitingSegmentSignal = false;
                    break;
            }
        }

        /// <summary>
        /// 画面に非同期に 1 行 (セグメントごと) の生stringテキストを表示する
        /// 文字送り表示中に userPrompt されたら加速させて一気に表示する
        /// </summary>
        private IEnumerator DisplayingSingleSegmentDialogueText(string dialogueText, bool append = true)
        {
            dialogueText = tagManager.Inject(dialogueText);

            // TMProGUI が dialogue の表示を開始する（非同期で文字が画面に出力され始める）
            if (append) textArchitect.AppendDisplay(dialogueText);
            else textArchitect.Display(dialogueText);

            // 会話文字すべての表示が終わったら、この IEnumerator は終了する
            // かわりに RunningConversation の WaitForUserAdvance() で会話文送りクリックを待っている
            // よって、ここの yield return は「すべての文字が描画されるまで」を待機している
            while (textArchitect.IsDisplaying)
            {
                // テキスト１文字ずつ描画している最中において、ユーザがクリックして強制表示にしたら
                // - テキストをすべて表示する
                // - testArchitect.IsDisplaying が false になる (表示コルーチンが null になるので)
                if (userPromptNext)
                {
                    if (!textArchitect.HurryUp) textArchitect.HurryUp = true;
                    else textArchitect.ForceComplete();
                    userPromptNext = false;
                }

                yield return null;
            }
        }

        private IEnumerator RunningSingleCommands(DialogueLineData lineData)
        {
            Debug.Log("RunningSingleCommands: " + lineData.commandData);

            List<Command> commands = lineData.commandData.commands;
            foreach (var command in commands)
            {
                bool shouldWait = command.waitForCompletion || command.IsForceWaitCoroutine();
                if (!shouldWait)
                {
                    CommandManager.instance.ExecuteCommand(command.name, command.arguments);
                    continue;
                }

                CoroutineWrapper wrapper = CommandManager.instance.ExecuteCommand(command.name, command.arguments);
                if (wrapper == null) continue;

                // CommandManager.RunningCommandProcess の KillTargetCommandProcess で終了すると
                // wrapper.IsDone が false → true に変更されてループが勝手に終了する
                // そのため `コマンドコルーチンが終了したら**自動で**次の行が始まる` が正しい挙動
                while (!wrapper.IsDone)
                {
                    // ユーザ入力もしくはコマンド実行が完了するまではループで待機する
                    // ユーザ入力されると userPromptNext = true になる
                    if (!userPromptNext)
                    {
                        yield return null;
                        continue;
                    }

                    CommandManager.instance.StopCurrentCommandProcess(); // IsDone = true にする
                    userPromptNext = false;
                }
            }
        }

        /// <summary>
        /// ユーザがクリックするまで待機する
        /// </summary>
        private IEnumerator WaitForUserAdvance()
        {
            dialogueSystem.Prompt.Show();

            while (!userPromptNext) yield return null;

            dialogueSystem.Prompt.Hide();
            userPromptNext = false;
        }
    }
}