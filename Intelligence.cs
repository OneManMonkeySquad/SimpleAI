using UnityEngine;

namespace SimpleAI {
    [CreateAssetMenu(menuName = "AI/Intelligence")]
    public class Intelligence : ScriptableObject {
        public ActionSet[] actionSets;

        public (ActionBase, ActionSet) SelectAction(IContext ctx, float minScore) {
            var bestScore = minScore;
            ActionBase bestAction = null;
            ActionSet bestActionSet = null;

#if UNITY_EDITOR
            ctx.Listener?.LogLine("");
#endif

            foreach (var actionSet in actionSets) {
                if (actionSet.Checks != null) {
                    var checksFailed = false;
                    foreach (var check in actionSet.Checks) {
                        if (!check.Evaluate(ctx)) {
#if UNITY_EDITOR
                            ctx.Listener?.LogLine($"<color=grey><i>{actionSet.name}</i></color> failed <i>{check}</i>");
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

                    if (score <= bestScore) {
#if UNITY_EDITOR
                        ctx.Listener?.LogLine($"<color=grey><i>{action.name}</i></color> {score:0.00}");
#endif
                        continue;
                    }

                    if (!action.CheckProceduralPreconditions(ctx)) {
#if UNITY_EDITOR
                        ctx.Listener?.LogLine($"<color=grey><i>{action.name}</i></color> precondition");
#endif
                        continue;
                    }

                    bestScore = score;
                    bestAction = action;
                    bestActionSet = actionSet;
#if UNITY_EDITOR
                    ctx.Listener?.LogLine($"<color=green><b><i>{action.name}</i></color> {score:0.00}</b>");
#endif
                }
            }

            return (bestAction, bestActionSet);
        }
    }
}