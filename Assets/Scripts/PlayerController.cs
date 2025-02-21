using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    //[SerializeField] private InputActionAsset playerInputActionASSET; // this is a class with no SPECIFIC action maps or input actions defined. So this could be used as a placeholder?
    private Vector2 moveDirection;
    private PlayerInput playerInput; // this is a PARTIAL class (defintion below), of an InputActionAsset class, this one is structured with action maps and input actions.
    // A partial class in C# allows you to split the definition of a class across multiple files.
    // This is useful for organizing large classes, working with auto-generated code, or collaborating on different parts of a class without merging conflicts.

    private InputActionMap playerActionMap;
    private InputAction moveInputAction;
    private InputAction grabInputAction;

    private Rigidbody2D playerRB;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float grabRange = 1f;

    [SerializeField] private LayerMask playerLayer;

    private void Awake()
    {
        playerInput = new PlayerInput(); // this is a type of InputActionAsset but references the created asset
        playerActionMap = playerInput.Player;         // both this...
        moveInputAction = playerInput.Player.Move;    // and this... are broader variables so they stay as just "InputActionMap" and "InputAction" because they are defined in both classes
                                                      // if we used the InputActionAsset '.Player' and the input action '.Player.Move' dont exist in that class

        grabInputAction = playerInput.Player.Fire;
        grabInputAction.performed += OnGrabPerformed; //subscribe the method to  ".performed" 

        playerRB = this.GetComponent<Rigidbody2D>();
    }

    void Start()
    {
    }

    private void OnEnable()
    {
        playerInput.Player.Enable(); // you have to enable the object created from the PlayerInput script
        //grabInputAction.Enable(); //do i have to enable this if I have already enabled all inputs? ANS: Only if its in a different action map
    }

    private void OnDisable()
    {
        playerInput.Player.Disable();
    }

    private void OnDestroy()
    {
        grabInputAction.performed -= OnGrabPerformed; //Unsubscribing to prevent memory leak
    }

    private Collider2D? grabbedObject;
    private void OnGrabPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Player Grabbing");
        grabbedObject= Physics2D.OverlapCircle(transform.position, grabRange, ~playerLayer);// this will have to be changed to OverlapCircleAll to get every thing that enters the collider
        if (grabbedObject != null)
        {
            Debug.Log(grabbedObject);
            grabbedObject.transform.SetParent(transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        moveDirection = moveInputAction.ReadValue<Vector2>().normalized;
        //Debug.Log(moveDirection);
        
    }

    private void FixedUpdate()
    {
        //transform.position += (Vector3)moveDirection.normalized*Time.deltaTime*5f; // this is for instant movement by moving the position directly (not good supposedly)
        playerRB.velocity = moveDirection * speed * Time.fixedDeltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, grabRange);
    }
}
