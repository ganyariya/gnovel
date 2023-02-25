using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.DisplayDialogue;

namespace Testing
{
    public class TestingTextArchitect : MonoBehaviour
    {
        DialogueSystem dialogueSystem;
        DisplayTextArchitect displayTextArchitect;
        public BuildMethod buildMethod = BuildMethod.fade;

        readonly string[] lines = new string[] {
             "『ダンガンロンパ 希望の学園と絶望の高校生』はハイスピード推理アクションゲームである。「超高校級」と呼ばれる類稀なる才能を持つ生徒たちが「コロシアイ」生活に巻き込まれ、事件後に開廷される「学級裁判」にて殺人犯を特定する。作品名の通り相手の言葉の矛盾などを「論破」していくことに焦点が当てられており、各登場人物・キャラクターのセリフが重要な意味を持つ。",
             "自らを希望ヶ峰学園の学園長と名乗るクマのぬいぐるみ。本作のトリックスター。右半身は白地に点目の可愛らしい顔だが、左半身は黒地で裂けるような赤い目を持ち、鋭い牙を有する。本人は愛らしいマスコットキャラクターを自負しているが、他の登場人物たちはそうは思っていない。",
             " 本作の主人公。身長160cm。一人称は「僕」。男性に対しては「名字＋君」、女性に対しては「名字＋さん」で呼ぶ。やや引っ込み思案だが、人よりすこし前向きで諦めの悪い性格。学生ブレザーの下にパーカーを着込んでいる。他人物に比べると特筆すべき特徴は少なくトレードマークの「くせっ毛」ぐらい"
        };
        // string[] lines = new string[] {
        //     "Hello, World!",
        //     "kyougi purograminngu",
        //     "subarashiki hibi",
        //     "yapparine",
        //     "soudayone",
        //     "nihonnzennkoku",
        //     "syouganai",
        //     "kanasii",
        // };

        void Start()
        {
            dialogueSystem = DialogueSystem.instance;
            displayTextArchitect = new(dialogueSystem.dialogueContainer.dialogueText, null)
            {
                CurrentBuildMethod = buildMethod
            };
        }

        void Update()
        {
            if (buildMethod != displayTextArchitect.CurrentBuildMethod)
            {
                displayTextArchitect.CurrentBuildMethod = buildMethod;
                displayTextArchitect.Stop();
            }

            if (Input.GetKeyDown(KeyCode.S)) displayTextArchitect.Stop();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (displayTextArchitect.IsBuilding)
                {
                    if (!displayTextArchitect.HurryUp) displayTextArchitect.HurryUp = true;
                    else displayTextArchitect.ForceComplete();
                }
                else
                {
                    displayTextArchitect.Build(lines[Random.Range(0, lines.Length)]);
                }
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                displayTextArchitect.Append(lines[Random.Range(0, lines.Length)]);
            }
        }
    }
}
