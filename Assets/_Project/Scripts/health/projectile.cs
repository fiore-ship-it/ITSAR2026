using UnityEngine;

public class projectile : MonoBehaviour
{
    [SerializeField] float damage = 5f;
    [SerializeField] float speed = 15f;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.TryGetComponent(out healthsystem _health))

        {
            _health.TakeDamage(damage);
        }
        Destroy(this.gameObject); 
    }
}
