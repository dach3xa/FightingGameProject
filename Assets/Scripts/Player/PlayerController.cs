using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : BaseCharacterController
{
    public Vector2 DirectionToMouse { get; private set; }
    public Vector3 mouseWorldPosition { get; private set; }

    private float HorizontalMoveDirection = 0;

    public RaycastHit2D PlayerRaycastHit { get; private set; }
    void Start()
    {
        base.Start();
    }

    void Update()
    {
        HandleUserInputMovement();

        HandleLook();

        HandleUserInputAttack();
    }
    void FixedUpdate()
    {
        Move();
    }

    void LateUpdate()
    {
        HandleRotate();
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        OnGroundCheck(collision);
    }
    //------------------------------   handling user input for movement
    private void HandleUserInputMovement()
    {
        //getting movement direction 
        HorizontalMoveDirection = Input.GetAxisRaw("Horizontal");

        if (HorizontalMoveDirection != 0) //walking
        {
            Moving = true;
        }
        else if (HorizontalMoveDirection == 0 && Moving)
        {
            Moving = false;
            Running = false;
        }

        //running
        if (Input.GetKeyDown(KeyCode.LeftShift) && Moving == true && characterStatController.Stamina > 0)
        {
            Running = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) || characterStatController.Stamina <= 0)
        {
            Running = false;
        }
        //jumping logic
        if (Input.GetKeyDown(KeyCode.Space) && (MaxJumpCount - JumpCount > 1 || Grounded))
        {
            Jump();
        }

        //changing animation state from moving to jump when in air
        HandleAnimations();

    }

    //------------------------------- handle user input Attack/Block
    private void HandleUserInputAttack()
    {
        //Debug.Log(ItemInHand?.tag);
        if (IsHolding && ItemInHand.CompareTag("Weapon"))
        {
            Debug.Log("Handling User Input!!");
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("AttackingPrimary!!");
                //do the attack
                var currentWeapon = ItemInHand.GetComponent<WeaponMelee>();
                currentWeapon.AttackPrimary();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                //do the Block
                var currentWeapon = ItemInHand.GetComponent<WeaponMelee>();
                currentWeapon.Block();
            }
            else if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                //stab em'
                var currentWeapon = ItemInHand.GetComponent<WeaponMelee>();
                currentWeapon.AttackSecondary();
            }else if (Input.GetKeyDown(KeyCode.Q))
            {
                var currentWeapon = ItemInHand.GetComponent<WeaponMelee>();
                currentWeapon.CancelAttack();
            }
        }
    }
    //------------------------------rotating body parts according to mouse position
    private void HandleRotate()
    {
        mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);//getting mouse position in world
        DirectionToMouse = new Vector2(mouseWorldPosition.x - transform.position.x, mouseWorldPosition.y - transform.position.y).normalized;//getting direction to mouse
        angleToTarget = Mathf.Atan2(DirectionToMouse.y, DirectionToMouse.x) * Mathf.Rad2Deg;//getting the angle of direction in degrees

        //Debug.Log(angleToTarget);
        if (DirectionToMouse.x < 0)  // If mouse is to the left
        {
            RotatePlayerSpritesLeft();
        }
        else  // If mouse is to the right
        {
            RotatePlayerSpritesRight();
        }
    }

    private void RotatePlayerSpritesLeft()
    {
        float angleToMouseCalculation = -(180 - angleToTarget);
        if (angleToMouseCalculation < -90)
        {
            angleToMouseCalculation = angleToMouseCalculation + 360;//dont ask me why this works(it does) 
        }
        //Debug.Log("Rotating");
        transform.localScale = new Vector3(-3, transform.localScale.y, transform.localScale.z);
        Torso.transform.rotation = Quaternion.Euler(new Vector3(Torso.transform.rotation.x, Torso.transform.rotation.y, angleToMouseCalculation / (180 / maxTorsoRotation)));
        Head.transform.rotation = Quaternion.Euler(new Vector3(Head.transform.rotation.x, Head.transform.rotation.y, angleToMouseCalculation / (180 / (maxHeadRotation + maxTorsoRotation))));

        if (IsHolding)
        {
            ArmRight.transform.rotation = Quaternion.Euler(new Vector3(ArmRight.transform.rotation.x, ArmRight.transform.rotation.y, angleToMouseCalculation / (180 / (maxHandRotation + maxTorsoRotation))));
            ArmLeft.transform.rotation = Quaternion.Euler(new Vector3(ArmLeft.transform.rotation.x, ArmLeft.transform.rotation.y, angleToMouseCalculation / (180 / (maxHandRotation + maxTorsoRotation))));
        }//rotating hands
    }

    private void RotatePlayerSpritesRight()
    {
        // Face player sprite to the right
        transform.localScale = new Vector3(3, transform.localScale.y, transform.localScale.z);
        Torso.transform.rotation = Quaternion.Euler(new Vector3(Torso.transform.rotation.x, Torso.transform.rotation.y, angleToTarget / (180 / maxTorsoRotation)));
        Head.transform.rotation = Quaternion.Euler(new Vector3(Head.transform.rotation.x, Head.transform.rotation.y, angleToTarget / (180 / (maxHeadRotation + maxTorsoRotation))));

        if (IsHolding)
        {
            ArmRight.transform.rotation = Quaternion.Euler(new Vector3(ArmRight.transform.rotation.x, ArmRight.transform.rotation.y, angleToTarget / (180 / (maxHandRotation + maxTorsoRotation))));
            ArmLeft.transform.rotation = Quaternion.Euler(new Vector3(ArmLeft.transform.rotation.x, ArmLeft.transform.rotation.y, angleToTarget / (180 / (maxHandRotation + maxTorsoRotation))));
        }//rotating hands
    }


    //------------------------------- handling Look
    private void HandleLook()
    {
        PlayerRaycastHit = Physics2D.Raycast(transform.position, DirectionToMouse, 3f, ~RaycastIgnoreLayer);

        if (PlayerRaycastHit && PlayerRaycastHit.collider.gameObject.CompareTag("Item"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                TakeItem(PlayerRaycastHit.collider.gameObject.name);
                Destroy(PlayerRaycastHit.collider.gameObject);
            }
        }
    }
    //-------------------------------- moving 
    private void Move()
    {
        float Speed = (Running && characterStatController.ReduceStamina(Time.fixedDeltaTime * 10f)) ? baseSpeed * runSpeedModifier : baseSpeed;

        if (Grounded)
        {
            rb.linearVelocity = new Vector2(HorizontalMoveDirection * Speed, rb.linearVelocity.y);
        }
        else
        {
            rb.AddForce(new Vector2(HorizontalMoveDirection * airVelocity, 0), ForceMode2D.Force);
        }
    }
}
