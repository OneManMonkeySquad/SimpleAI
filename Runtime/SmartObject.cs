using System.Collections;
using System.Collections.Generic;
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

        /// Select the best (and possible) SmartObject from a list of SmartObjects.
        public static SmartObjectBase SelectSmartObject(IContext ctx, IEnumerable<SmartObjectBase> smartObjects) {
            Assert.IsNotNull(ctx);
            Assert.IsNotNull(smartObjects);
            if (ctx == null || smartObjects == null)
                return null;

            SmartObjectBase bestSmartObject = null;
            float bestScore = 0;

            foreach (var smartObject in smartObjects) {
                if (smartObject == null) {
                    Debug.LogWarning("SmartObject null");
                    continue;
                }

                if (smartObject.Checks != null) {
                    var checksFailed = false;
                    foreach (var check in smartObject.Checks) {
                        if (check == null)
                            continue;

                        if (!check.Evaluate(ctx)) {
                            checksFailed = true;
                            break;
                        }
                    }
                    if (checksFailed)
                        continue;
                }

                var score = smartObject.Score(ctx);
                if (score < 0.01f)
                    continue;

                if (score > bestScore && smartObject.CheckProceduralPreconditions(ctx)) {
                    bestScore = score;
                    bestSmartObject = smartObject;
                }
            }

            return bestSmartObject;
        }

#if UNITY_EDITOR
        void OnValidate() {
            if (Checks != null) {
                foreach (var check in Checks) {
                    if (check == null) {
                        Debug.LogError("null", this);
                    }
                }
            }
            if (Considerations != null) {
                foreach (var consideration in Considerations) {
                    if (consideration == null) {
                        Debug.LogError("null", this);
                    }
                }
            }
        }
#endif
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