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

    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float orbitRadius = 2f;
    private float currentRotationSpeed = 0f;
    private float angle = 0f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float maxRotationSpeed = 200f;

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
    private Vector2 relativePositionOfGrabbed;
    private void OnGrabPerformed(InputAction.CallbackContext context)
    {
        //how do I know if its being held down?
        Debug.Log("Player Grabbing");
        grabbedObject= Physics2D.OverlapCircle(transform.position, grabRange, ~playerLayer);// this will have to be changed to OverlapCircleAll to get every thing that enters the collider
        if (grabbedObject != null)
        {
            //set the position to the radius in the direction of the grabbed object, then start circling left or right
            Debug.Log(grabbedObject);
            relativePositionOfGrabbed = transform.InverseTransformPoint(grabbedObject.transform.position);
            Debug.Log(relativePositionOfGrabbed);
            grabbedObject.transform.SetParent(transform);
            grabbedObject.GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private void RotatingAroundPlayerNOPHYSICS()
    {
        //... will eventually need a function to get the vector component that aligns with the spin, and return the speed(magnitude) of that vector.. how to do this while keeping the sign of the vector

        //sets new rotation speed by adding acceleration, limits rotation speed to under max.
        currentRotationSpeed = Mathf.Min(currentRotationSpeed + acceleration * Time.deltaTime, maxRotationSpeed);
            // changing the acceleration here to a negative would be for other direction?

        //angle starts at 0 here (to the right of the player) so if you want the angle of grab it will have to take that into account, +pi/2 for directly up
        angle += currentRotationSpeed * Time.deltaTime;

        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * orbitRadius;
            //changing the cos and sin changes the direction of the swing
            // x goes from 1 to 0 to -1 to 0 (starting on the right of the player)
            // y goes from 0 to 1 to 0 to -1 (starting on the same height as the player)
            // together they form a circular path
            // x = r*cos(angle) y=r*sin(angle)

        if (grabbedObject == null) return;
        grabbedObject.transform.position = transform.position + offset;
            //this manipulates the position of the object rather than the velocity, but we will have the velocity in the currentRotationSpeed.

        //TO ADD
        // - have to make a way for the object to rotate around the player correctly, rotating to face towards the player as it rotates
        // - angle at time of grab kept when starting spin
        // - HOLD mechanic, left for rotating left, right click for rotating right
        // - ...and subsequently, let go mechanic, keeping velocity and direction at that point
        // - Speed kept when grabbed
        // - ...Speed in the right direction kept when grabbed
    }

    private void RotatingAroundPlayerPhysically()
    {
        //i think it would be better for the game feel to have it really fling out so I dont know if that would be better with physics or not.
        //if I was going to do otherwise id be faking the effects of physically weighted objects
    }


    // Update is called once per frame
    void Update()
    {
        moveDirection = moveInputAction.ReadValue<Vector2>().normalized;
        //Debug.Log(moveDirection);
        RotatingAroundPlayerNOPHYSICS();
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
