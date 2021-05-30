# SimpleAI
Coroutine-based utility AI for [Unity3d](https://unity3d.com).

![Action](Docs/Action.png)
![ActionSet](Docs/ActionSet.png)
![Parts](Docs/Parts.png)

```cs
public class ActorAIContext : IContext {
    public Actor Actor;
#if UNITY_EDITOR
    public IAIListener Listener { get; set; }
#endif
    public MonoBehaviour CoroutineTarget => Actor;
    public Blackboard Blackboard = new Blackboard();

    public Actor BestTarget;
    public SmartObjectBase BestSmartObject;

    public bool IsArmed = false;
    public float Threat;

    public float GetCurrentConsiderationScore(int considerationIdx) =>
        considerationIdx switch {
            0 => 1,
            1 => BestTarget != null ? 1 : 0,
            2 => IsArmed ? 1 : 0,
            _ => throw new ArgumentException("", "considerationIdx")
        };

    public string[] GetConsiderationDescriptions() => new string[] {
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