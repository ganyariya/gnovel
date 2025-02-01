using System.Collections;
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
        /// BaseCharacter に Animator が存在する
        /// 一方 Live2D にはデフォルトで設定されている motionAnimator がありそちらを変わりに利用する
        /// </summary>
        private Animator live2DMotionAnimator;
        private CubismRenderController cubismRenderController;
        private CubismExpressionController cubismExpressionController;

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

        protected override IEnumerator ShowingOrHiding(bool isShow)
        {
            float targetAlpha = isShow ? 1 : 0;

            while (cubismRenderController.Opacity != targetAlpha)
            {
                cubismRenderController.Opacity = Mathf.MoveTowards(cubismRenderController.Opacity, targetAlpha, Time.deltaTime * DEFAULT_TRANSITION_SPEED);
                yield return null;
            }

            revealingCoroutine = null;
            hidingCoroutine = null;
        }
    }
}
