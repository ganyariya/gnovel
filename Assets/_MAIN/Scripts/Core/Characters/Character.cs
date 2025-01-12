using System.Collections;
using System.Collections.Generic;
using Core.DisplayDialogue;
using TMPro;
using UnityEngine;

namespace Core.Characters
{
    public abstract class Character
    {
        public DialogueSystemController dialogueSystem => DialogueSystemController.instance;

        public string name;
        public string displayName;

        /// <summary>
        /// Character Prefab が生成されたときの rectTransform
        /// Character-[ganyariya] のように生成された prefab instance の rootRectTransform
        /// </summary>
        public RectTransform rootRectTransform;
        public CharacterConfig config;
        protected Animator animator;

        protected Coroutine revealingCoroutine;
        protected Coroutine hidingCoroutine;
        protected Coroutine movingCoroutine;

        public virtual bool isVisible => false;

        public bool isRevealing => revealingCoroutine != null;
        public bool isHiding => hidingCoroutine != null;
        public bool isMoving => movingCoroutine != null;

        protected CharacterManager characterManager => CharacterManager.instance;

        public Character(string name, CharacterConfig config, GameObject prefab)
        {
            this.name = name;
            this.displayName = name;
            this.rootRectTransform = null;
            this.config = config;

            // prefab があるのであればここで characterLayer に追加してしまう
            // ただこの設計だと Character が直接 Unity と依存してしまい うれしくないね...
            if (prefab != null)
            {
                GameObject prefabInstance = Object.Instantiate(prefab, characterManager.characterLayer);
                prefabInstance.SetActive(true);
                rootRectTransform = prefabInstance.GetComponent<RectTransform>();
                animator = rootRectTransform.GetComponentInChildren<Animator>();
            }
        }

        /// <summary>
        /// DialogueSystemController.Say があれば会話テキストファイルを
        /// - パース
        /// - Unity 上で会話を表示
        /// が行える
        /// 
        /// しかしアウトゲームにおいて会話テキストファイル以外で会話を表示したい場合がある
        /// そのため Character.Say を用意する
        /// </summary>
        public Coroutine Say(string dialogue) => Say(new List<string> { dialogue });

        public Coroutine Say(List<string> dialogues)
        {
            dialogueSystem.DisplaySpeakerName(displayName);
            ApplyTextConfigOnScreen();
            return dialogueSystem.Say(dialogues);
        }

        public virtual Coroutine Show()
        {
            if (isRevealing) return revealingCoroutine;
            if (isHiding) characterManager.StopCoroutine(hidingCoroutine);

            return revealingCoroutine = characterManager.StartCoroutine(ShowingOrHiding(true));
        }
        public virtual Coroutine Hide()
        {
            if (isHiding) return hidingCoroutine;
            if (isRevealing) characterManager.StopCoroutine(revealingCoroutine);

            return hidingCoroutine = characterManager.StartCoroutine(ShowingOrHiding(false));
        }

        public virtual void SetScreenPosition(Vector2 screenPosition)
        {
            if (rootRectTransform == null) return;

            (Vector2 targetMinAnchor, Vector2 targetMaxAnchor) = ConvertClampedScreenCharacterPosition(screenPosition);

            rootRectTransform.anchorMin = targetMinAnchor;
            rootRectTransform.anchorMax = targetMaxAnchor;
        }

        public virtual Coroutine MoveToScreenPosition(Vector2 screenPosition, float speed = 2f, bool smooth = false)
        {
            if (rootRectTransform == null) return null;

            if (isMoving) characterManager.StopCoroutine(movingCoroutine);
            movingCoroutine = characterManager.StartCoroutine(MovingToScreenPosition(screenPosition, speed, smooth));

            return movingCoroutine;
        }

        private IEnumerator MovingToScreenPosition(Vector2 screenPosition, float speed, bool smooth)
        {
            (Vector2 targetMinAnchor, Vector2 targetMaxAnchor) = ConvertClampedScreenCharacterPosition(screenPosition);
            Vector2 anchorRectSize = rootRectTransform.anchorMax - rootRectTransform.anchorMin;

            while (rootRectTransform.anchorMin != targetMinAnchor || rootRectTransform.anchorMax != targetMaxAnchor)
            {
                System.Func<Vector2, Vector2, Vector2> f = (Vector2 current, Vector2 target) =>
                    smooth
                        ? Vector2.Lerp(current, target, speed * Time.deltaTime)
                        : Vector2.MoveTowards(current, target, speed * Time.deltaTime * 0.35f); // そのままだと早すぎるので遅くする

                rootRectTransform.anchorMin = f(rootRectTransform.anchorMin, targetMinAnchor);
                rootRectTransform.anchorMax = rootRectTransform.anchorMin + anchorRectSize;

                if (smooth && Vector2.Distance(rootRectTransform.anchorMin, targetMinAnchor) <= 0.001f)
                {
                    rootRectTransform.anchorMin = targetMinAnchor;
                    rootRectTransform.anchorMax = targetMaxAnchor;
                    break;
                }

                yield return null;
            }

            Debug.Log($"Character {name} has arrived at {screenPosition}, {targetMinAnchor}, {targetMaxAnchor}");
            movingCoroutine = null;
        }

        protected (Vector2, Vector2) ConvertClampedScreenCharacterPosition(Vector2 screenPosition)
        {
            // Parent (画面全体) に対する
            // このキャラクターの表示領域割合を出す
            // (0.3, 0.6) であれば X 方向に画面の 30%, Y 方向に画面の 60% のサイズで表示されている
            Vector2 anchorRectSize = rootRectTransform.anchorMax - rootRectTransform.anchorMin;

            // キャラクターの表示領域を除いた, 画面容量を出す
            // x = 100-30% = 70%, y = 100-60% = 40%
            float maxX = 1f - anchorRectSize.x;
            float maxY = 1f - anchorRectSize.y;

            // 画面における $screenPosition に配置するときに、変換した targetMin, Max を計算する
            // こうすることで position = (0, 0), (1, 1) を指定しても画面から出ないようにできる
            Vector2 targetMinAnchor = new Vector2(maxX * screenPosition.x, maxY * screenPosition.y);
            // anchorRectSize を足すことで、元々の描画領域をそのまま確保し続けられるようにする
            Vector2 targetMaxAnchor = targetMinAnchor + anchorRectSize;

            return (targetMinAnchor, targetMaxAnchor);
        }

        protected virtual IEnumerator ShowingOrHiding(bool isShow) => null;

        public void SetNameColor(Color color) => config.nameColor = color;
        public void SetDialogueColor(Color color) => config.dialogueColor = color;
        public void SetNameFont(TMP_FontAsset font) => config.nameFont = font;
        public void SetDialogueFont(TMP_FontAsset font) => config.dialogueFont = font;

        public void ApplyTextConfigOnScreen() => dialogueSystem.ApplySpeakerConfigToDialogueContainer(config);
        public void ResetConfig() => config = CharacterManager.instance.GetCharacterConfig(name);

        public enum CharacterType
        {
            Text,
            Sprite,
            SpriteSheet,
            Live2D,
            Model3D,
        }
    }
}