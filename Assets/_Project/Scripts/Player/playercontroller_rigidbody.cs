using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class playercontroller_rigidbody : MonoBehaviour
{
    private Rigidbody rb; // questa (e anche le altre private sotto) è una dichiarazione di una variabile, momentaneamente vuota

    private Vector2 input;

    private float force = 5f;
    private GameControls controls;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); // QUESTA ROBA NON SI FA MAI DENTRO UPDATE e FIXED UPDATE !!!!
        controls = new GameControls();

    }

    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();
    private void OnDestroy() => controls.Dispose();



    private void Update()
    {
        input = controls.Player.Move.ReadValue<Vector2>();
        Debug.Log(input);

    }


    private void FixedUpdate()

    {
        Vector3 input3 = new Vector3(input.x, 0f, input.y);
        Vector3 velocity = input3 * force;

        //rb.AddForce(input3 * force);        questo comando serve quando abbiamo use gravity invece che kinetic

        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);

    }

   


}
