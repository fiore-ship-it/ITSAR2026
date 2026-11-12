using UnityEngine;

/// <summary>
/// Movimento del giocatore con CharacterController e nuovo Input System.
/// Il movimento è relativo alla camera: "su" significa sempre "verso l'alto dello schermo".
/// Supporta effetti temporanei (scivolamento, rallentamento, accelerazione) applicati da AreaEffect.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    /// <summary>Durata massima di qualsiasi effetto temporaneo, in secondi.</summary>
    public const float MaxEffectDuration = 3f;

    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float sprintMultiplier = 1.6f;
    [SerializeField, Tooltip("Gradi al secondo")] private float rotationSpeed = 720f;
    [SerializeField] private float gravity = -20f;

    [Header("Riferimenti")]
    [SerializeField, Tooltip("Se vuoto usa la Main Camera")] private Transform cameraTransform;

    private CharacterController controller;
    private GameControls controls;
    private float verticalVelocity;

    // Velocità orizzontale attuale (serve per lo scivolamento).
    private Vector3 horizontalVelocity;

    // Effetti temporanei: ognuno resta attivo finché Time.time < "Until".
    private float slideUntil;
    private float slideAcceleration;
    private float slowUntil;
    private float slowMultiplier = 1f;
    private float boostUntil;
    private float boostMultiplier = 1f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        controls = new GameControls();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    // Le action vanno abilitate e disabilitate insieme al componente.
    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();
    private void OnDestroy() => controls.Dispose();

    // ---------- API effetti temporanei ----------

    /// <summary>Scivolamento: la velocità cambia gradualmente (accel bassa = più scivoloso).</summary>
    public void ApplySlide(float acceleration, float duration)
    {
        slideAcceleration = Mathf.Max(0.1f, acceleration);
        slideUntil = Time.time + Mathf.Clamp(duration, 0f, MaxEffectDuration);
    }

    /// <summary>Rallentamento: moltiplicatore di velocità tra 0 e 1.</summary>
    public void ApplySlow(float multiplier, float duration)
    {
        slowMultiplier = Mathf.Clamp(multiplier, 0.05f, 1f);
        slowUntil = Time.time + Mathf.Clamp(duration, 0f, MaxEffectDuration);
    }

    /// <summary>Accelerazione: moltiplicatore di velocità maggiore di 1.</summary>
    public void ApplySpeedBoost(float multiplier, float duration)
    {
        boostMultiplier = Mathf.Max(1f, multiplier);
        boostUntil = Time.time + Mathf.Clamp(duration, 0f, MaxEffectDuration);
    }

    // ---------- Movimento ----------

    private void Update()
    {
        Vector2 input = controls.Player.Move.ReadValue<Vector2>();
        Vector3 direction = CameraRelative(input);

        float speed = moveSpeed;
        if (controls.Player.Sprint.IsPressed())
        {
            speed *= sprintMultiplier;
        }
        if (Time.time < slowUntil) speed *= slowMultiplier;
        if (Time.time < boostUntil) speed *= boostMultiplier;

        // Con lo scivolamento la velocità si avvicina a quella desiderata un po' alla volta.
        Vector3 targetVelocity = direction * speed;
        if (Time.time < slideUntil)
        {
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, slideAcceleration * Time.deltaTime);
        }
        else
        {
            horizontalVelocity = targetVelocity;
        }

        // Gravità applicata a mano: il CharacterController non usa la fisica.
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>Converte l'input 2D in una direzione sul piano XZ, orientata come la camera.</summary>
    private Vector3 CameraRelative(Vector2 input)
    {
        if (cameraTransform == null)
        {
            return new Vector3(input.x, 0f, input.y);
        }

        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0f;
        right.Normalize();

        return Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);
    }
}