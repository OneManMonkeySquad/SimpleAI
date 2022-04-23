using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace SimpleAI {
    public class AIAgent<T> where T : class, IContext {
        public ActionBase CurrentAction { get; private set; }
        public Intelligence Intelligence { get; private set; }

        ActionSet _currentActionSet;
        Coroutine _currentActionCoroutine;

        readonly float _evaluationTickRate;
        float _nextReevaluationTime;

        public AIAgent(Intelligence intelligence, float evaluationTickRate = 1) {
            Intelligence = intelligence;
            _evaluationTickRate = evaluationTickRate;
        }

        /// <summary>
        /// Call this every frame. Might switch to a new action.
        /// </summary>
        public void Tick(T ctx) {
            // Note: We don't need to update the current action here because it's a coroutine
            if (CurrentAction == null) {
                if (Time.time < _nextReevaluationTime)
                    return;

                _nextReevaluationTime = Time.time + _evaluationTickRate;

                var nextActionPair = Intelligence.SelectAction(ctx, 0);
                if (nextActionPair.Item1 == null)
                    return;

                SwitchToAction(ctx, nextActionPair);
            } else if (Time.time >= _nextReevaluationTime) {
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

            _nextReevaluationTime = Time.time + _evaluationTickRate;

            var actionSetWeight = _currentActionSet?.finalWeight ?? 1;
            var scoreThreshold = CurrentAction.Score(ctx) * actionSetWeight * 1.3f; // 30% higher than our current score
            var betterActionPair = Intelligence.SelectAction(ctx, scoreThreshold);
            if (betterActionPair.Item1 == null || betterActionPair.Item1 == CurrentAction) {
#if UNITY_EDITOR
                if (AIDebugger.CurrentDebugTarget == ctx && AIDebugger.Active != null) {
                    AIDebugger.LogLine(ctx, $"<b>Keep <i>{CurrentAction.name}</i></b>");
                }
#endif
                return;
            }
            SwitchToAction(ctx, betterActionPair);
        }

        /// Stop any running Action. This is usually done before destroying the controlled GameObject.
        public void Stop(T ctx) {
            if (_currentActionCoroutine != null) {
                ctx.CoroutineTarget.StopCoroutine(_currentActionCoroutine);
                _currentActionCoroutine = null;
            }
            if (CurrentAction != null) {
                CurrentAction.StopAction(ctx);
                CurrentAction = null;
            }
        }

        void SwitchToAction(T ctx, (ActionBase, ActionSet) actionPair) {
            Assert.IsNotNull(actionPair.Item1);

#if UNITY_EDITOR
            if (AIDebugger.CurrentDebugTarget == ctx && AIDebugger.Active != null) {
                if (CurrentAction != null) {
                    AIDebugger.LogLine(ctx, $"> <b>{CurrentAction.name} -> {actionPair.Item1.name}</b>");
                } else {
                    AIDebugger.LogLine(ctx, $"> <b>Start {actionPair.Item1.name}</b>");
                }
            }
#endif

            Stop(ctx);

            ctx.CoroutineTarget.StartCoroutine(CoroutineWrapper(ctx, actionPair.Item1.StartAction(ctx)));

            CurrentAction = actionPair.Item1;
            _currentActionSet = actionPair.Item2;
        }

        IEnumerator CoroutineWrapper(T ctx, IEnumerator func) {
            Assert.IsNull(_currentActionCoroutine);

            _currentActionCoroutine = ctx.CoroutineTarget.StartCoroutine(func);
            yield return _currentActionCoroutine;

            CurrentAction.StopAction(ctx);
            CurrentAction = null;
        }
    }
}