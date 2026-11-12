using UnityEngine;

public class AutoTargeter : MonoBehaviour
{

    [SerializeField] float range;
    [SerializeField] float maxDistance;
    [SerializeField] LayerMask targetMask;
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] float eyeHeight = 1.5f;

    void FixedUpdate()   
    {
        FindNearest();
    }
    private healthsystem FindNearest()
    {
        Vector3 eye = transform.position + Vector3.up * eyeHeight;

        
        Collider[]  //questo è un array (un tipo di lista)
            hits = Physics.OverlapSphere(transform.position, range, targetMask);

        healthsystem currentElegibleEnemy = null; 

        foreach (Collider hit in hits)
        {
            healthsystem health = hit.GetComponentInParent<healthsystem>();
            if (!health)
                         continue;

            Vector3 point = health.transform.position + Vector3.up * eyeHeight; 

            if (Physics.Linecast(eye, point, obstacleMask))
                                                            continue;

          
            if(currentElegibleEnemy==null)              
            {
                currentElegibleEnemy = health;
            }
            else
            {
                float currentEnemyDistance = Vector3.Distance(currentElegibleEnemy.transform.position, transform.position);
                float newEnemyDistance = Vector3.Distance(health.transform.position, transform.position);

                if (newEnemyDistance < currentEnemyDistance)
                    currentElegibleEnemy = health;
            }
        }
        Debug.Log(currentElegibleEnemy ? currentElegibleEnemy.gameObject.name : "nothing" + " found a target");
        return currentElegibleEnemy;
    }
    



}
