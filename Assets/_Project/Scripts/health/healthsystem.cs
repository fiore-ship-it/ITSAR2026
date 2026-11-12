using System;
using UnityEngine;

public class healthsystem : MonoBehaviour
{
    [SerializeField] float maxHealth;

    float currentHealth;
    public float CurrentHealth => currentHealth;

    public bool IsDead { get; private set; }
    public Action<float, float> OnHealthChange;  //una action è un evento

    public Action Died;

    private void Awake()
    {
        currentHealth = maxHealth;
    }
    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        currentHealth -= damage;
        Debug.Log($"HP: {currentHealth}/{maxHealth}");
        OnHealthChange?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }

    }

    public void Heal(float heal)
    {
        if (IsDead) return;

        currentHealth = Mathf.Min(currentHealth + heal, maxHealth);
        Debug.Log($"HP: {currentHealth}/{maxHealth}");
        OnHealthChange?.Invoke(currentHealth, maxHealth); // aggiorna la UI anche con le cure
    }

    public void SetHealth(float _health, float _maxHealth)
    {
        currentHealth = _health;
        maxHealth = _maxHealth;
        OnHealthChange?.Invoke(currentHealth, maxHealth);
    }

    void Die()
    {
        IsDead = true;
        Died?.Invoke();
        gameObject.SetActive(false);


    }


}