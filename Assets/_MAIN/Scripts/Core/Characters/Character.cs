using System.Collections;
using System.Collections.Generic;
using Core.DisplayDialogue;
using TMPro;
using UnityEngine;

namespace Core.Characters
{
    public abstract class Character
    {
        public const bool INITIAL_ENABLE = false;
        private const float UN_HIGHLIGHTED_STRENGTH = 0.65f;
        /// <summary>
        /// 今回のゲームエンジンでは　左向きがデフォルト
        /// @todo
        /// 各キャラの ScriptableObject にどちら向きか？の設定を用意すれば各キャラごとにデフォの向きを設定できる
        /// （ただしできればゲーム全体で立ち絵で揃えたほうがいい）
        /// </summary>
        public const bool DEFAULT_ORIENTATION_LEFT = true;

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
        protected string rootCharacterFolder;

        /// <summary>
        /// キャラクターに適用する色を保持している
        /// ただし実際に色を適用するには Image の color を変更する必要がある
        /// </summary>
        public Color color { get; protected set; } = Color.white;
        /// <summary>
        /// color は Character が目指す DDD 的な色のことを指す
        /// 一方 displayColor は実際に画面に表示されている色のことを指す
        /// </summary>
        protected Color displayColor => isHighlighted ? highlightedColor : unHighlightedColor;

        protected Color highlightedColor => color;
        protected Color unHighlightedColor => new Color(color.r * UN_HIGHLIGHTED_STRENGTH, color.g * UN_HIGHLIGHTED_STRENGTH, color.b * UN_HIGHLIGHTED_STRENGTH, color.a);
        public bool isHighlighted { get; protected set; } = true;
        protected bool facingLeft = DEFAULT_ORIENTATION_LEFT;

        public virtual bool isVisible { get; set; }
        public bool isFacingLeft => facingLeft;
        public bool isFacingRight => !facingLeft;

        protected Coroutine revealingCoroutine;
        protected Coroutine hidingCoroutine;
        protected Coroutine movingCoroutine;
        protected Coroutine changingColorCoroutine;
        protected Coroutine highlightingCoroutine;
        protected Coroutine flippingCoroutine;

        public bool isRevealing => revealingCoroutine != null;
        public bool isHiding => hidingCoroutine != null;
        public bool isMoving => movingCoroutine != null;
        public bool isChangingColor => changingColorCoroutine != null;

        private bool isTransitioningHighlight => highlightingCoroutine != null;
        public bool isHighlighting => isHighlighted && isTransitioningHighlight;
        public bool isUnHighlighting => !isHighlighted && isTransitioningHighlight;

        public bool isFlipping => flippingCoroutine != null;

        protected CharacterManager characterManager => CharacterManager.instance;

        public Character(string name, CharacterConfig config, GameObject prefab, string rootCharacterFolder)
        {
            this.name = name;
            this.displayName = name;
            this.rootRectTransform = null;
            this.config = config;
            this.rootCharacterFolder = rootCharacterFolder;

            // prefab があるのであればここで characterLayer に追加してしまう
            // ただこの設計だと Character が直接 Unity と依存してしまい うれしくないね...
            if (prefab != null)
            {
                GameObject prefabInstance = Object.Instantiate(prefab, characterManager.characterLayer);
                prefabInstance.name = characterManager.FormatCharacterPath(characterManager.characterPrefabNameFormat, name);
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

        public virtual void SetColor(Color color)
        {
            this.color = color;
        }

        public Coroutine ExecuteChangingColor(Color color, float speed = 1.0f)
        {
            this.color = color;

            if (isChangingColor) characterManager.StopCoroutine(changingColorCoroutine);

            changingColorCoroutine = characterManager.StartCoroutine(ChangingColor(color, speed));

            return changingColorCoroutine;
        }

        public Coroutine ExecuteHighlighting(float speed = 1f)
        {
            if (isHighlighting) return highlightingCoroutine;
            if (isUnHighlighting) characterManager.StopCoroutine(highlightingCoroutine);

            isHighlighted = true;
            return highlightingCoroutine = characterManager.StartCoroutine(Highlighting(true, speed));
        }
        public Coroutine ExecuteUnHighlighting(float speed = 1f)
        {
            if (isUnHighlighting) return highlightingCoroutine;
            if (isHighlighting) characterManager.StopCoroutine(highlightingCoroutine);

            isHighlighted = false;
            return highlightingCoroutine = characterManager.StartCoroutine(Highlighting(false, speed));
        }
        protected virtual IEnumerator Highlighting(bool highlighted, float speed = 1f)
        {
            yield return null;
        }

        /// <summary>
        /// override method で実装する
        /// </summary>
        protected virtual IEnumerator ChangingColor(Color color, float speed = 1.0f)
        {
            yield return null;
        }

        public Coroutine Flip(float speed = 1f, bool immediate = false)
        {
            if (isFacingLeft) return FlipToRight(speed, immediate);
            else return FlipToLeft(speed, immediate);
        }
        public Coroutine FlipToLeft(float speed = 1f, bool immediate = false)
        {
            if (isFlipping) characterManager.StopCoroutine(flippingCoroutine);
            facingLeft = true;
            return flippingCoroutine = characterManager.StartCoroutine(FlippingToDirection(true, speed, immediate));
        }
        public Coroutine FlipToRight(float speed = 1f, bool immediate = false)
        {
            if (isFlipping) characterManager.StopCoroutine(flippingCoroutine);
            facingLeft = false;
            return flippingCoroutine = characterManager.StartCoroutine(FlippingToDirection(false, speed, immediate));
        }
        public virtual IEnumerator FlippingToDirection(bool facingLeft, float speed = 1f, bool immediate = false)
        {
            yield return null;
        }

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