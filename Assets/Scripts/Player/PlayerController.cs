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
        mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        DirectionToMouse = new Vector2(mouseWorldPosition.x - transform.position.x, mouseWorldPosition.y - transform.position.y).normalized;
        angleToTarget = Mathf.Atan2(DirectionToMouse.y, DirectionToMouse.x) * Mathf.Rad2Deg;

        if (DirectionToMouse.x < 0) 
        {
            RotatePlayerSpritesLeft();
        }
        else  
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

        transform.localScale = new Vector3(-3, transform.localScale.y, transform.localScale.z);

        RotatePlayerSpriteBodyAndHead(angleToMouseCalculation);
        RotatePlayerSpriteHands(angleToMouseCalculation);
    }

    private void RotatePlayerSpritesRight()
    {
        transform.localScale = new Vector3(3, transform.localScale.y, transform.localScale.z);

        RotatePlayerSpriteBodyAndHead(angleToTarget);
        RotatePlayerSpriteHands(angleToTarget);
    }
    private void RotatePlayerSpriteBodyAndHead(float angleToMouse)
    {
        Torso.transform.rotation = Quaternion.Euler(new Vector3(Torso.transform.rotation.x, Torso.transform.rotation.y, angleToMouse / (180 / maxTorsoRotation)));
        Head.transform.rotation = Quaternion.Euler(new Vector3(Head.transform.rotation.x, Head.transform.rotation.y, angleToMouse / (180 / (maxHeadRotation + maxTorsoRotation))));
    }

    private void RotatePlayerSpriteHands(float angleToMouse)
    {
        if (IsHolding)
        {
            if (ItemInHand != null)
            {
                ArmRight.transform.rotation = Quaternion.Euler(new Vector3(ArmRight.transform.rotation.x, ArmRight.transform.rotation.y, angleToMouse / (180 / (maxHandRotation + maxTorsoRotation))));
            }
            if ((ItemInHand && ItemInHand.GetComponent<HoldableItem>().IsTwoHanded) || ItemInHandLeft != null)
            {
                ArmLeft.transform.rotation = Quaternion.Euler(new Vector3(ArmLeft.transform.rotation.x, ArmLeft.transform.rotation.y, angleToMouse / (180 / (maxHandRotation + maxTorsoRotation))));
            }
        }
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
