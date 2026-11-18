using UnityEngine;
using UnityEngine.Assemblies;

public abstract class Weapon : MonoBehaviour //mettere abstract perchè il sistema lo richiede

{

    [SerializeField] protected float cooldown = 1f;
    [SerializeField] protected float range = 1f;

    [SerializeField] protected float damage = 1f;

    protected AutoTargeter targeter;
    private float timer;
    protected virtual bool NeedsTarget => true;
    protected abstract void Fire();

    protected virtual void Awake()
    { }
    protected virtual void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;  // se il timer superiore a zero ritorna
        if (NeedsTarget && targeter.CurrentTarget == null) ;  //se ha bisogno di un target  e NON ha un current target ritorna
        timer = cooldown; 
        Fire();
    }

}
