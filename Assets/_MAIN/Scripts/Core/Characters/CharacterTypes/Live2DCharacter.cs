using System.Collections;
using System.Collections.Generic;
using Live2D.Cubism.Framework.Expression;
using Live2D.Cubism.Rendering;
using UnityEngine;

namespace Core.Characters
{
    public class Live2DCharacter : Character
    {
        private const int UNDEFINED_EXPRESSION = -1;

        /// <summary>
        /// BaseCharacter に Animator が存在する
        /// 一方 Live2D にはデフォルトで設定されている motionAnimator がありそちらを変わりに利用する
        /// </summary>
        private Animator live2DMotionAnimator;

        private CubismRenderController cubismRenderController;
        private CubismExpressionController cubismExpressionController;

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
    }
}
