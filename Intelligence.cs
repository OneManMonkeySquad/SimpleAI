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
            ctx.Listener?.Log("\n");
#endif

            foreach (var actionSet in actionSets) {
                if (actionSet.checks != null) {
                    var checksFailed = false;
                    foreach (var check in actionSet.checks) {
                        if (!check.Foo(ctx)) {
#if UNITY_EDITOR
                            ctx.Listener?.Log($"<i>{actionSet.name}</i> <color=grey>failed {check}</color>\n");
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
                        ctx.Listener?.Log($"<i>{action.name}</i> <color=grey>{score:0.00}</color>\n");
#endif
                        continue;
                    }

                    if (!action.CheckProceduralPreconditions(ctx)) {
#if UNITY_EDITOR
                        ctx.Listener?.Log($"<i>{action.name}</i> <color=grey>precondition</color>\n");
#endif
                        continue;
                    }

                    bestScore = score;
                    bestAction = action;
                    bestActionSet = actionSet;
#if UNITY_EDITOR
                    ctx.Listener?.Log($"<i>{action.name}</i> <color=green>{score:0.00}</color>\n");
#endif
                }
            }

            return (bestAction, bestActionSet);
        }
    }
}