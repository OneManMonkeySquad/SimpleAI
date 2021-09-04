using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace SimpleAI {
    public abstract class SmartObjectBase : MonoBehaviour {
        public CheckBase[] Checks;
        public Consideration[] Considerations;

        public float Score(IContext ctx) {
            if (Considerations.Length == 0)
                return 1;

            var totalScore = 1f;

            var modificationFactor = 1f - 1f / Considerations.Length;
            foreach (var consideration in Considerations) {
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
        public abstract void StopAction(IContext ctx);

        void OnValidate() {
            foreach (var check in Checks) {
                Assert.IsNotNull(check);
            }
            foreach (var consideration in Considerations) {
                Assert.IsNotNull(consideration);
            }
        }
    }

    public abstract class SmartObject<T> : SmartObjectBase, IBoundToContextType<T> where T : IContext, new() {
        public virtual bool CheckProceduralPreconditions(T ctx) => true;

        public abstract IEnumerator StartAction(T ctx);
        public virtual void StopAction(T ctx) { }

        public override bool CheckProceduralPreconditions(IContext ctx) => CheckProceduralPreconditions((T)ctx);
        public override IEnumerator StartAction(IContext ctx) => StartAction((T)ctx);
        public override void StopAction(IContext ctx) => StopAction((T)ctx);
    }
}