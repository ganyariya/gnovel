using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Characters
{
    public class Live2DCharacter : Character
    {
        /// <summary>
        /// BaseCharacter に Animator が存在する
        /// 一方 Live2D にはデフォルトで設定されている motionAnimator がありそちらを変わりに利用する
        /// </summary>
        private Animator live2DMotionAnimator;

        public Live2DCharacter(string name, CharacterConfig config, GameObject prefab, string rootCharacterFolder) : base(name, config, prefab, rootCharacterFolder)
        {
            live2DMotionAnimator = animator.transform.GetChild(0).GetComponentInChildren<Animator>();
        }

        public void playMotion(string animationName)
        {
            live2DMotionAnimator.Play(animationName);
        }
    }
}
