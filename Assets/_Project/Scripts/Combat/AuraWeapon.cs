using UnityEngine;

public class AuraWeapon : Weapon  // primo passo, togliere MonoBehavior e mettere Weapon
{
    protected override bool NeedsTarget => false;
    protected override void Fire()
    {
        if(targeter != null)
        {
            foreach(Health enemy in targeter.FindAllTargetsInRange(range)) 
            {
                enemy.TakeDamage(damage);
            }
        }
    }
    
    void Start()
    {
        
    }

}
