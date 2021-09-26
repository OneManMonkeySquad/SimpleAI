using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace SimpleAI {
    public class AIAgent<T> where T : class, IContext {
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

        readonly float evaluationTickRate;
        float nextReevaluationTime;

        public AIAgent(Intelligence intelligence, float evaluationTickRate = 1) {
            Intelligence = intelligence;
            this.evaluationTickRate = evaluationTickRate;
        }

        /// Call this every frame. Might switch to a new action.
        public void Tick(T ctx) {
            if (CurrentAction == null) {
                if (Time.time < nextReevaluationTime)
                    return;

                nextReevaluationTime = Time.time + evaluationTickRate;

                var nextActionPair = Intelligence.SelectAction(ctx, 0);
                if (nextActionPair.Item1 == null) {
                    if (Intelligence.DefaultAction == null)
                        return;

                    nextActionPair = (Intelligence.DefaultAction, null);
                }

                SwitchToAction(ctx, nextActionPair);
            } else if (Time.time >= nextReevaluationTime) {
                ForceEvaluation(ctx);
            }
        }

        /// <summary>
        /// Force the agent to reevaluate possible actions now (ignoring the tick rate).
        /// This can be used in case the context contains significant new information
        /// and needs to be evaluated right now (new enemy, got shot, heard something, ...).
        /// </summary>
        public void ForceEvaluation(T ctx) {
            if (CurrentAction == null || ctx.CoroutineTarget == null)
                return; // Next Action will be chosen next Tick anyway

            nextReevaluationTime = Time.time + evaluationTickRate;

            var setWeight = currentActionSet?.finalWeight ?? 1;
            var scoreThreshold = CurrentAction.Score(ctx) * setWeight * 1.3f; // 30% higher than our current score
            var betterActionPair = Intelligence.SelectAction(ctx, scoreThreshold);
            if (betterActionPair.Item1 == null || betterActionPair.Item1 == CurrentAction) {
#if UNITY_EDITOR
                if (AIDebugger.CurrentDebugTarget == ctx && AIDebugger.Active != null)
                    AIDebugger.LogLine(ctx, $"<b><i>{CurrentAction.name}</i></b>");
#endif
                return;
            }
            SwitchToAction(ctx, betterActionPair);
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

        void SwitchToAction(T ctx, (ActionBase, ActionSet) actionPair) {
#if UNITY_EDITOR
            if (AIDebugger.CurrentDebugTarget == ctx && AIDebugger.Active != null)
                AIDebugger.LogLine(ctx, $"<b>{CurrentAction?.name} -> {actionPair.Item1.name}</b>");
#endif

            Reset(ctx);

            if (actionPair.Item1 == null)
                return;

            ctx.CoroutineTarget.StartCoroutine(CoroutineWrapper(ctx, actionPair.Item1.StartAction(ctx)));

            CurrentAction = actionPair.Item1;
            currentActionSet = actionPair.Item2;
        }

        IEnumerator CoroutineWrapper(T ctx, IEnumerator func) {
            Assert.IsNull(currentActionCoroutine);

            currentActionCoroutine = ctx.CoroutineTarget.StartCoroutine(func);
            yield return currentActionCoroutine;

            CurrentAction.StopAction(ctx);
            CurrentAction = null;
        }
    }
}