using System.Collections.Generic;
using UnityEngine;

namespace SimpleAI {
    [CreateAssetMenu(menuName = "AI/Intelligence")]
    public class Intelligence : ScriptableObject {
        public ActionSet[] actionSets;
        public ActionBase DefaultAction;

        static List<(float, ActionBase, ActionSet)> temp = new List<(float, ActionBase, ActionSet)>();
        public (ActionBase, ActionSet) SelectAction(IContext ctx, float minScore) {
#if UNITY_EDITOR
            var writeDebug = AIDebugger.CurrentDebugTarget == ctx && AIDebugger.Active != null;
            if (writeDebug)
                AIDebugger.LogLine(ctx, "");
#endif

            temp.Clear();
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

                    temp.Add((score, action, actionSet));
                }
            }

            if (temp.Count == 0)
                return (null, null);

            temp.Sort((lhs, rhs) => (int)(rhs.Item1 * 100) - (int)(lhs.Item1 * 100));

            var scoreThreshold = temp[0].Item1 - 0.1f; // 10% worse than best

            int idx = 0;
            for (; idx < temp.Count; ++idx) {
                if (temp[idx].Item1 < scoreThreshold)
                    break;
            }

            var finalIdx = idx > 1 ? Random.Range(0, idx) : 0;

#if UNITY_EDITOR
            if (writeDebug) {
                for (int i = 0; i < idx; ++i) {
                    AIDebugger.LogLine(ctx, $"<i>{temp[i].Item2.name}</i> {temp[i].Item1:0.00}");
                }
                for (int i = idx; i < temp.Count; ++i) {
                    AIDebugger.LogLine(ctx, $"<color=grey><i>{temp[i].Item2.name}</i></color> {temp[i].Item1:0.00}");
                }
            }
#endif

            return (temp[finalIdx].Item2, temp[finalIdx].Item3);
        }
    }
}