using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleAI {
    [CreateAssetMenu(menuName = "AI/Intelligence")]
    public class Intelligence : ScriptableObject {
        public ActionSet[] actionSets;

        static List<(float, ActionBase, ActionSet)> s_temp = new List<(float, ActionBase, ActionSet)>();
        public (ActionBase, ActionSet) SelectAction(IContext ctx, float minScore) {
#if UNITY_EDITOR
            var writeDebug = AIDebugger.CurrentDebugTarget == ctx && AIDebugger.Active != null;
            if (writeDebug)
                AIDebugger.LogLine(ctx, "");
#endif

            s_temp.Clear();
            foreach (var actionSet in actionSets) {
                if (actionSet.Checks != null) {
                    var checksFailed = false;
                    foreach (var check in actionSet.Checks) {
                        if (!check.Evaluate(ctx)) {
#if UNITY_EDITOR
                            if (writeDebug)
                                AIDebugger.LogLine(ctx, $"<color=grey><i>[{actionSet.name}]</i> failed <i>{check}</i></color>");
#endif
                            checksFailed = true;
                            break;
                        }
                    }
                    if (checksFailed)
                        continue;
                }


                foreach (var action in actionSet.actions) {
                    var score = action.Score(ctx);
                    score *= actionSet.finalWeight;

                    if (score < minScore) {
#if UNITY_EDITOR
                        if (writeDebug)
                            AIDebugger.LogLine(ctx, $"<i>{action.name}</i> {score:0.00} < {minScore:0.00}");
#endif
                        continue;
                    }
                    if (!action.CheckProceduralPreconditions(ctx)) {
#if UNITY_EDITOR
                        if (writeDebug)
                            AIDebugger.LogLine(ctx, $"<color=grey><i>{action.name}</i> precondition</color>");
#endif
                        continue;
                    }

                    s_temp.Add((score, action, actionSet));
                }
            }

            if (s_temp.Count == 0)
                return (null, null);

            s_temp.Sort((lhs, rhs) => (int)(rhs.Item1 * 100) - (int)(lhs.Item1 * 100));

            var scoreThreshold = s_temp[0].Item1 - 0.1f; // 10% worse than best

            int idx = 0;
            for (; idx < s_temp.Count; ++idx) {
                if (s_temp[idx].Item1 < scoreThreshold)
                    break;
            }

            var finalIdx = idx > 1 ? UnityEngine.Random.Range(0, idx) : 0;

#if UNITY_EDITOR
            if (writeDebug) {
                for (int i = 0; i < idx; ++i) {
                    AIDebugger.LogLine(ctx, $"<i>{s_temp[i].Item2.name}</i> {s_temp[i].Item1:0.00}");
                }
                for (int i = idx; i < s_temp.Count; ++i) {
                    AIDebugger.LogLine(ctx, $"<color=grey><i>{s_temp[i].Item2.name}</i></color> {s_temp[i].Item1:0.00}");
                }
            }
#endif

            return (s_temp[finalIdx].Item2, s_temp[finalIdx].Item3);
        }

        void OnValidate() {
            // Sort ActionSets by estimated weight
            var sortedActionSets = actionSets.OrderByDescending(actionSet => (int)(actionSet.finalWeight));
            if (!Enumerable.SequenceEqual(sortedActionSets, actionSets)) {
                actionSets = sortedActionSets.ToArray();
            }
        }
    }
}