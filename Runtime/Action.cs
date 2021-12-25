using System;
using System.Collections;
using UnityEngine;

namespace SimpleAI {
    [Serializable]
    public class Consideration {
        public int Idx;
        public AnimationCurve Curve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) });
    }

    public abstract class ActionBase : ScriptableObject {
        public Consideration[] considerations;

        /// [0,1] - highter is better
        public float Score(IContext ctx) {
            if (considerations == null || considerations.Length == 0)
                return 1;

            var totalScore = 1f;

            var modificationFactor = 1f - 1f / considerations.Length;
            foreach (var consideration in considerations) {
                var score = ctx.GetCurrentConsiderationScore(consideration.Idx);
                score = consideration.Curve.Evaluate(score);

                var makeUpValue = (1f - score) * modificationFactor;
                score += (makeUpValue * score);

                totalScore *= score;
                if (totalScore < 0.01f)
                    break;
            }

            return totalScore;
        }

        public abstract bool CheckProceduralPreconditions(IContext ctx);

        public abstract IEnumerator StartAction(IContext ctx);
        public virtual void StopAction(IContext ctx) { }

        public virtual string ToString(IContext ctx) => name;
    }

    public abstract class Action<T> : ActionBase, IBoundToContextType<T> where T : IContext {
        public virtual bool CheckProceduralPreconditions(T ctx) => true;

        public abstract IEnumerator StartAction(T ctx);
        public virtual void StopAction(T ctx) { }

        public virtual string ToString(T ctx) => name;



        public override bool CheckProceduralPreconditions(IContext ctx) => CheckProceduralPreconditions((T)ctx);

        public override IEnumerator StartAction(IContext ctx) => StartAction((T)ctx);
        public override void StopAction(IContext ctx) => StopAction((T)ctx);

        public override string ToString(IContext ctx) => ToString((T)ctx);
    }
}