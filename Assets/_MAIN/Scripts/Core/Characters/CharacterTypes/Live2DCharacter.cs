using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Live2D.Cubism.Framework.Expression;
using Live2D.Cubism.Rendering;
using UnityEngine;

namespace Core.Characters
{
    public class Live2DCharacter : Character
    {
        private const int UNDEFINED_EXPRESSION = -1;
        private const float DEFAULT_TRANSITION_SPEED = 3;
        /// <summary>
        /// Live2D は各画像（renderer） がそれぞれ異なる depth で設定されている
        /// そのため live2DCharacterA, B の depth を 10, 15 のように設定すると
        /// 画像の depth が一緒になってキャラ表示が崩れてしまう
        /// (live2DCharacterA: 10,11,..,35, B: 15,16,...,74 のようになる)
        /// そのため確実にキャラ同士で被らない 250 を depth offset として設定する
        /// </summary>
        public const int LIVE2D_CHARACTER_DEPTH_OFFSET = 250;

        /// <summary>
        /// BaseCharacter に Animator が存在する
        /// 一方 Live2D にはデフォルトで設定されている motionAnimator がありそちらを変わりに利用する
        /// </summary>
        private Animator live2DMotionAnimator;
        private CubismRenderController cubismRenderController;
        private CubismExpressionController cubismExpressionController;

        private List<CubismRenderController> oldCubismRenderControllers = new List<CubismRenderController>();

        public override bool isVisible
        {
            get => isRevealing || cubismRenderController.Opacity == 1;
            set => cubismRenderController.Opacity = value ? 1 : 0;
        }

        public Live2DCharacter(string name, CharacterConfig config, GameObject prefab, string rootCharacterFolder) : base(name, config, prefab, rootCharacterFolder)
        {
            live2DMotionAnimator = animator.transform.GetChild(0).GetComponentInChildren<Animator>();
            cubismRenderController = live2DMotionAnimator.GetComponent<CubismRenderController>();
            cubismExpressionController = live2DMotionAnimator.GetComponent<CubismExpressionController>();
        }

        public void PlayMotion(string animationName)
        {
            live2DMotionAnimator.Play(animationName);
        }

        public void SetExpression(int expressionIndex)
        {
            cubismExpressionController.CurrentExpressionIndex = expressionIndex;
        }

        public void SetExpression(string expressionName)
        {
            SetExpression(GetExpressionIndexByName(expressionName));
        }

        private int GetExpressionIndexByName(string expressionName)
        {
            expressionName = expressionName.ToLower();
            for (int i = 0; i < cubismExpressionController.ExpressionsList.CubismExpressionObjects.Length; i++)
            {
                CubismExpressionData data = cubismExpressionController.ExpressionsList.CubismExpressionObjects[i];

                // xxx.exp3 の xxx の部分を取得
                if (data.name.Split('.')[0].ToLower() == expressionName)
                {
                    return i;
                }
            }

            return UNDEFINED_EXPRESSION;
        }

        protected override IEnumerator ShowingOrHiding(bool isShow, float speedMultiplier = 1f)
        {
            float targetAlpha = isShow ? 1 : 0;

            while (cubismRenderController.Opacity != targetAlpha)
            {
                cubismRenderController.Opacity = Mathf.MoveTowards(cubismRenderController.Opacity, targetAlpha, Time.deltaTime * DEFAULT_TRANSITION_SPEED * speedMultiplier);
                yield return null;
            }

            revealingCoroutine = null;
            hidingCoroutine = null;
        }

        public override void SetColor(Color color)
        {
            base.SetColor(color);

            foreach (var renderer in cubismRenderController.Renderers)
            {
                renderer.Color = displayColor;
            }
        }

        protected override IEnumerator ChangingColor(Color color, float speed = 1)
        {
            yield return ChangingCubismRendererColor(cubismRenderController.Renderers, color, speed);

            changingColorCoroutine = null;
        }

        protected override IEnumerator Highlighting(bool highlighted, float speed = 1)
        {
            Color targetColor = displayColor;
            yield return ChangingColor(targetColor, speed);

            highlightingCoroutine = null;
        }

        private IEnumerator ChangingCubismRendererColor(CubismRenderer[] cubismRenderers, Color color, float speed)
        {
            Color oldColor = cubismRenderers[0].Color;

            float colorPercent = 0;
            while (colorPercent < 1.0f)
            {
                colorPercent += DEFAULT_TRANSITION_SPEED * speed * Time.deltaTime;
                var targetColor = Color.Lerp(oldColor, color, colorPercent);
                foreach (var renderer in cubismRenderers) renderer.Color = targetColor;
                yield return null;
            }

            yield return null;
        }

        public override IEnumerator FlippingToDirection(bool facingLeft, float speed = 1, bool immediate = false)
        {
            GameObject newLive2DCharacter = CopyAndRegisterLive2DCharacter();

            // Live2DController に紐づく gameObject の xScale は 500 などサイズ情報も持っている
            // そのため - をつけてそのまま反転する (isLeft ? 1 : -1 のようにするとサイズがなくなって縦長の線になる) 
            // SpriteCharacter は RectTransform のため 1:-1 でよかった
            float currentXScale = newLive2DCharacter.transform.localScale.x;
            newLive2DCharacter.transform.localScale = new Vector3(
                facingLeft ? currentXScale : -currentXScale,
                newLive2DCharacter.transform.localScale.y,
                newLive2DCharacter.transform.localScale.z
            );

            float transitionSpeed = DEFAULT_TRANSITION_SPEED * speed * Time.deltaTime;

            // 新しい Live2DCharacter の透明度を 0 から 1 に
            // 古い Live2DCharacter の透明度を 1 から 0 に変えていく
            cubismRenderController.Opacity = 0;
            while (cubismRenderController.Opacity < 1 || oldCubismRenderControllers.Any(r => r.Opacity > 0))
            {
                cubismRenderController.Opacity = Mathf.MoveTowards(cubismRenderController.Opacity, 1, transitionSpeed);
                foreach (var oldCubismRenderController in oldCubismRenderControllers)
                {
                    oldCubismRenderController.Opacity = Mathf.MoveTowards(oldCubismRenderController.Opacity, 0, transitionSpeed);
                }

                yield return null;
            }

            foreach (var oldCubismRenderController in oldCubismRenderControllers)
            {
                Object.Destroy(oldCubismRenderController.gameObject);
            }

            oldCubismRenderControllers.Clear();

            flippingCoroutine = null;
        }

        /// <summary>
        /// CubismRenderController をコピーして古いリストに保存する（画像 transition 用）
        /// 
        /// その後 cubismRenderController に紐づく gameObject を複製して parent につける
        /// 
        /// ### 注意
        /// 副作用を持つことに注意する
        /// </summary>
        private GameObject CopyAndRegisterLive2DCharacter()
        {
            oldCubismRenderControllers.Add(cubismRenderController);

            // cubismRenderController に紐づく gameObject を複製して parent につける
            GameObject newLive2DCharacter = Object.Instantiate(cubismRenderController.gameObject, cubismRenderController.transform.parent);
            newLive2DCharacter.name = name;

            cubismRenderController = newLive2DCharacter.GetComponent<CubismRenderController>();
            cubismExpressionController = newLive2DCharacter.GetComponent<CubismExpressionController>();
            live2DMotionAnimator = newLive2DCharacter.GetComponent<Animator>();

            return newLive2DCharacter;
        }

        public override void CallbackOnSort(int sortedIndex)
        {
            cubismRenderController.SortingOrder = sortedIndex * LIVE2D_CHARACTER_DEPTH_OFFSET;
        }
    }
}