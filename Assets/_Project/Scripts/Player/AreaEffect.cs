using UnityEngine;

/// <summary>
/// Area con collider trigger che applica un effetto al giocatore mentre ci sta dentro:
/// danno periodico oppure cura periodica, più effetti opzionali di movimento
/// (scivolamento, rallentamento, accelerazione).
/// Gli effetti di movimento restano attivi per la loro durata (max 3 secondi) anche dopo
/// l'uscita dall'area, e la durata si rinnova finché il giocatore resta dentro.
/// </summary>
[RequireComponent(typeof(Collider))]
public class AreaEffect : MonoBehaviour
{
    public enum AreaType { Damage, Heal, None }

    [Header("Tipo di area")]
    [SerializeField] private AreaType areaType = AreaType.Damage;
    [SerializeField, Tooltip("Layer che attivano l'area (di default: Player)")] private LayerMask targetLayers;

    [Header("Danno / Cura periodica")]
    [SerializeField, Min(0f)] private float amountPerTick = 5f;
    [SerializeField, Min(0.05f), Tooltip("Secondi tra un tick e il successivo")] private float tickInterval = 1f;
    [SerializeField, Tooltip("Se attivo, il primo tick avviene appena il giocatore entra")] private bool tickImmediately = true;

    [Header("Scivolamento (opzionale)")]
    [SerializeField] private bool enableSlide;
    [SerializeField, Range(0.1f, PlayerController.MaxEffectDuration)] private float slideDuration = 3f;
    [SerializeField, Min(0.1f), Tooltip("Più è basso, più il giocatore scivola (ghiaccio)")] private float slideAcceleration = 3f;

    [Header("Rallentamento (opzionale)")]
    [SerializeField] private bool enableSlow;
    [SerializeField, Range(0.1f, PlayerController.MaxEffectDuration)] private float slowDuration = 3f;
    [SerializeField, Range(0.1f, 1f), Tooltip("Moltiplicatore di velocità (0.5 = metà)")] private float slowMultiplier = 0.5f;

    [Header("Accelerazione (opzionale)")]
    [SerializeField] private bool enableSpeedBoost;
    [SerializeField, Range(0.1f, PlayerController.MaxEffectDuration)] private float boostDuration = 3f;
    [SerializeField, Range(1f, 3f), Tooltip("Moltiplicatore di velocità (1.5 = +50%)")] private float boostMultiplier = 1.5f;

    [Header("Danno da permanenza prolungata (opzionale)")]
    [SerializeField, Tooltip("Attivalo solo sulle piattaforme dove vuoi questa penalità")] private bool enableOvertimeDamage;
    [SerializeField, Min(0f), Tooltip("Secondi di permanenza continua prima che inizi il danno")] private float overtimeThreshold = 10f;
    [SerializeField, Min(0f), Tooltip("Punti vita persi al secondo dopo la soglia")] private float overtimeDamagePerSecond = 20f;
    [SerializeField, Min(0.05f), Tooltip("Ogni quanti secondi viene applicato il danno (il totale al secondo non cambia)")] private float overtimeTickInterval = 0.5f;

    private healthsystem targetHealth;
    private PlayerController targetPlayer;
    private float nextTickTime;
    private float insideSince;
    private float nextOvertimeTick;

    // Si chiama quando aggiungi il componente: imposta valori sensati di default.
    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
        targetLayers = LayerMask.GetMask("Player");
    }

    private void OnValidate()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"{name}: il Collider di AreaEffect deve avere 'Is Trigger' attivo.", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsTarget(other)) return;

        CacheTarget(other);
        nextTickTime = Time.time + (tickImmediately ? 0f : tickInterval);
        StartOvertimeTimer();
        Process();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsTarget(other)) return;

        // Sicurezza: se Stay arriva senza Enter (es. dopo un hot reload) recupera i riferimenti.
        if (targetHealth == null && targetPlayer == null)
        {
            CacheTarget(other);
            nextTickTime = Time.time;
            StartOvertimeTimer();
        }

        Process();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsTarget(other)) return;

        // Gli effetti di movimento non vengono tolti qui: scadono da soli dopo la loro durata.
        targetHealth = null;
        targetPlayer = null;
    }

    // Il primo tick di danno scatta esattamente allo scadere della soglia.
    private void StartOvertimeTimer()
    {
        insideSince = Time.time;
        nextOvertimeTick = insideSince + overtimeThreshold;
    }

    private bool IsTarget(Collider other)
    {
        return (targetLayers.value & (1 << other.gameObject.layer)) != 0;
    }

    private void CacheTarget(Collider other)
    {
        targetHealth = other.GetComponentInParent<healthsystem>();
        targetPlayer = other.GetComponentInParent<PlayerController>();
    }

    private void Process()
    {
        // Danno o cura a intervalli regolari.
        if (areaType != AreaType.None && targetHealth != null && Time.time >= nextTickTime)
        {
            if (areaType == AreaType.Damage)
            {
                targetHealth.TakeDamage(amountPerTick);
            }
            else
            {
                targetHealth.Heal(amountPerTick);
            }

            nextTickTime = Time.time + tickInterval;
        }

        // Permanenza troppo lunga: dopo la soglia il giocatore perde vita di continuo, fino a morire.
        if (enableOvertimeDamage && targetHealth != null && Time.time >= nextOvertimeTick)
        {
            targetHealth.TakeDamage(overtimeDamagePerSecond * overtimeTickInterval);
            nextOvertimeTick = Time.time + overtimeTickInterval;
        }

        // Effetti di movimento: rinnovati a ogni chiamata finché il giocatore è dentro.
        if (targetPlayer != null)
        {
            if (enableSlide) targetPlayer.ApplySlide(slideAcceleration, slideDuration);
            if (enableSlow) targetPlayer.ApplySlow(slowMultiplier, slowDuration);
            if (enableSpeedBoost) targetPlayer.ApplySpeedBoost(boostMultiplier, boostDuration);
        }
    }
}