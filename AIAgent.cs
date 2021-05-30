using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace SimpleAI {
    public class AIAgent<T> where T : IContext {
        public ActionBase CurrentAction {
            get;
            internal set;
        }

        public Intelligence Intelligence {
            get;
            internal set;
        }

        ActionSet currentActionSet;
        Coroutine currentActionCoroutine;
        bool hasCurrentActionEnded;

        readonly float evaluationTickRate;
        float nextReevaluationTime;

        public AIAgent(Intelligence intelligence, float evaluationTickRate = 1) {
            Intelligence = intelligence;
            this.evaluationTickRate = evaluationTickRate;
        }

        /// Call this every frame. Might switch to a new action.
        public void Tick(T ctx) {
            if (hasCurrentActionEnded) {
                CurrentAction = null;
            }

            if (CurrentAction == null) {
                if (Time.time < nextReevaluationTime)
                    return;

                nextReevaluationTime = Time.time + evaluationTickRate;

                var nextActionPair = Intelligence.SelectAction(ctx, 0);
                if (nextActionPair.Item1 == null)
                    return;

                SwitchToAction(nextActionPair, ctx);
            } else if (Time.time >= nextReevaluationTime) {
                ForceEvaluation(ctx);
            }
        }

        /// Force the agent to reevaluate possible actions now (ignoring the tick rate).
        /// This can be used in case the context contains significant new information
        /// and needs to be evaluated right now (new enemy, got shot, heard something, ...).
        public void ForceEvaluation(T ctx) {
            if (CurrentAction == null || ctx.CoroutineTarget == null)
                return; // Next Action will be chosen next Tick anyway

            nextReevaluationTime = Time.time + evaluationTickRate;

            var scoreThreshold = CurrentAction.Score(ctx) * currentActionSet.finalWeight * 1.3f; // 30% higher than our current score
            var betterActionPair = Intelligence.SelectAction(ctx, scoreThreshold);
            if (betterActionPair.Item1 == null || betterActionPair.Item1 == CurrentAction) {
#if UNITY_EDITOR
                ctx.Listener?.Log($"Keep current action <i>{CurrentAction.name}</i>\n");
#endif
                return;
            }
            SwitchToAction(betterActionPair, ctx);
        }

        /// Stop any running Action.
        public void Reset(T ctx) {
            if (currentActionCoroutine != null) {
                ctx.CoroutineTarget.StopCoroutine(currentActionCoroutine);
                currentActionCoroutine = null;
            }
            if (CurrentAction != null) {
                CurrentAction.StopAction(ctx);
                CurrentAction = null;
            }
        }

        /// Select the best (and possible) SmartObject from a list of SmartObjects.
        public SmartObjectBase SelectSmartObject(T ctx, List<SmartObjectBase> smartObjects) {
            SmartObjectBase bestSmartObject = null;
            float bestScore = 0;

            foreach (var smartObject in smartObjects) {
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

        void SwitchToAction((ActionBase, ActionSet) actionPair, T ctx) {
#if UNITY_EDITOR
            ctx.Listener?.Log($"<b>{CurrentAction?.name} -> {actionPair.Item1.name}</b>\n");
#endif

            Reset(ctx);

            if (actionPair.Item1 == null)
                return;

            ctx.CoroutineTarget.StartCoroutine(CoroutineWrapper(ctx.CoroutineTarget, actionPair.Item1.StartAction(ctx)));

            hasCurrentActionEnded = false;
            CurrentAction = actionPair.Item1;
            currentActionSet = actionPair.Item2;
        }

        IEnumerator CoroutineWrapper(MonoBehaviour actor, IEnumerator func) {
            Assert.IsNull(currentActionCoroutine);

            currentActionCoroutine = actor.StartCoroutine(func);
            yield return currentActionCoroutine;
            hasCurrentActionEnded = true;
        }
    }
}