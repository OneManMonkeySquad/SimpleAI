# SimpleAI
Coroutine-based utility AI for [Unity3d](https://unity3d.com).

![Action](Docs/Action.png)
![ActionSet](Docs/ActionSet.png)
![Parts](Docs/Parts.png)

```cs
// A context contains all the data that's relevant for the AI to run.
public class ActorAIContext : IContext {
    public MonoBehaviour CoroutineTarget => Actor;

    /// The Pawn this AIAgent controls
    public Actor Actor;

    public Actor BestTarget;
    public SmartObjectBase BestSmartObject;

    public bool IsArmed = false;

    // The next 2 methods are required for the inspector to display "considerations" for an action
    public float GetCurrentConsiderationScore(int considerationIdx)
        => considerationIdx switch {
            0 => 1,
            1 => BestTarget != null ? 1 : 0,
            2 => IsArmed ? 1 : 0,
            _ => throw new Exception("case missing")
        };

    public string[] GetConsiderationDescriptions() 
        => new string[] {
            "Constant",
            "Has Target",
            "Is Armed",
        };
}

public class Actor : MonoBehaviour {
    public Intelligence Intelligence;

    ActorAIContext ctx;
    AIAgent<ActorAIContext> ai;

    float nextContextUpdateTime;
    float nextAttackTime;

    void Start() {
        ctx = new ActorAIContext() { Actor = this };
        ai = new AIAgent<ActorAIContext>(Intelligence);
    }

    public override void Update() {
        if (Time.time >= nextContextUpdateTime) {
            nextContextUpdateTime = Time.time + 0.1f;
            
            // Getters, raycasts, Physics.OverlapSphereNonAlloc, etc. to fill the context with valuable information (ctx.SmartObjects for instance)
            ctx.BestTarget = ...;
            ctx.BestSmartObject = ai.SelectSmartObject(ctx, ctx.SmartObjects);
            ctx.IsArmed = ...;
        }

        ai.Tick(ctx);
    }

    void Attack() {
        if(Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + 1;
        Debug.Log($"Attacking {ctx.BestTarget}");
    }
}

[CreateAssetMenu(menuName = "AI/Actor/Attack")]
public class AttackAction : Action<ActorAIContext> {
    public override IEnumerator StartAction(ActorAIContext ctx) {
        Debug.Log("Start attack");

        while (ctx.BestTarget != null) {
            ctx.Actor.Attack();
            yield return null;
        }

        Debug.Log("Done Attacking");
    }

    public override bool CheckProceduralPreconditions(ActorAIContext ctx) {
        return ctx.BestTarget != null;
    }
}
```

## Debugging
![Logger](Docs/Logger.png)

## Environment Query System
![Query](Docs/Query.png)

```cs
public class MoveAndFireAction : Action<ActorAIContext> {
    public QueryRunMode RunMode = QueryRunMode.Best;
    public Query Query;

    public override IEnumerator StartAction(ActorAIContext ctx) {
        // Give the EQS some context
        var queryCtx = new QueryRunContext() {
            Querier = ctx.Actor.transform.position,
            Target = ctx.TargetActorInfo.LastKnownLocation
        };

        Vector3 targetPosition;
        {
            // Run and wait for result
            var moveToPosition = Query.ExecuteAsync(RunMode, queryCtx);
            yield return moveToPosition.AsIEnumerator();

            if (moveToPosition.Result.Score < 0.01f)
                yield break; // No good position found

            targetPosition = moveToPosition.Result.Point;
        }

        // ...
    }
 }
```