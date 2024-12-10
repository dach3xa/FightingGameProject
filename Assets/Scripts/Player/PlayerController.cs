using System;
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

        HandleUserInputAction();
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
    private void HandleUserInputAction()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Attack("Primary");
        }
        if (Input.GetMouseButtonDown(1))
        {
            StartBlocking();
        }
        else if (Input.GetMouseButtonUp(1))
        {

            StopBlocking();
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            //stab em'
            Attack("Secondary");
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            CancelAttack();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Kick();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (!IsHolding) return;

            if (ItemInHand) DropItem(ItemInHand);
            else if (ItemInHandLeft) DropItem(ItemInHandLeft);
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
        TorsoPivot.transform.rotation = Quaternion.Euler(new Vector3(TorsoPivot.transform.rotation.x, TorsoPivot.transform.rotation.y, angleToMouse / (180 / maxTorsoRotation)));
        HeadPivot.transform.rotation = Quaternion.Euler(new Vector3(HeadPivot.transform.rotation.x, HeadPivot.transform.rotation.y, angleToMouse / (180 / (maxHeadRotation + maxTorsoRotation))));
    }

    private void RotatePlayerSpriteHands(float angleToMouse)
    {
        if (IsHolding)
        {
          
            if (ItemInHand && ItemInHand.GetComponent<SpriteRenderer>().enabled)
            {
                ArmRightPivot.transform.rotation = Quaternion.Euler(new Vector3(ArmRightPivot.transform.rotation.x, ArmRightPivot.transform.rotation.y, angleToMouse / (180 / (maxHandRotation + maxTorsoRotation))));
            }
            else if(ArmRightPivot.transform.rotation.z != -3.7f)
            {
                BringDownToNormal(ArmRightPivot);
            }

            if ((ItemInHand && ItemInHand.GetComponent<UsableObject>().IsTwoHanded && ItemInHand.GetComponent<SpriteRenderer>().enabled) || (ItemInHandLeft && ItemInHandLeft.GetComponent<SpriteRenderer>().enabled))
            {
                ArmLeftPivot.transform.rotation = Quaternion.Euler(new Vector3(ArmLeftPivot.transform.rotation.x, ArmLeftPivot.transform.rotation.y, angleToMouse / (180 / (maxHandRotation + maxTorsoRotation))));
            }
            else if (ArmLeftPivot.transform.rotation.z != -3.7f)
            {
                BringDownToNormal(ArmLeftPivot);
            }
        }
        else if(ArmRightPivot.transform.rotation.z != -3.7 || ArmLeftPivot.transform.rotation.z != -3.7f)
        {
            BringDownToNormal(ArmRightPivot);
            BringDownToNormal(ArmLeftPivot);
        }
    }

    private void BringDownToNormal(GameObject ArmPivot)
    {

        if(ArmPivot.transform.localEulerAngles.z > 1f && ArmPivot.transform.localEulerAngles.z < 180)
        {
            ArmPivot.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, ArmPivot.transform.localEulerAngles.z - 120 * Time.deltaTime));
        }
        else if (ArmPivot.transform.localEulerAngles.z < 359f && ArmPivot.transform.localEulerAngles.z > 180f)
        {
            ArmPivot.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, ArmPivot.transform.localEulerAngles.z + 120 * Time.deltaTime));
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
