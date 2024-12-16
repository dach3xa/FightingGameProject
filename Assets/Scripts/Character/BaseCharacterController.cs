using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;

public abstract class BaseCharacterController : MonoBehaviour
{
    protected GameObject TorsoPivot;
    protected GameObject HeadPivot;
    protected GameObject ArmLeftPivot;
    protected GameObject ArmRightPivot;

    public Dictionary<string, AudioSource> SoundEffects;

    [SerializeField] protected GameObject ItemHolder;
    [SerializeField] protected GameObject ItemHolderLeft;
    [SerializeField] protected GameObject AudioHolder;

    [SerializeField] public GameObject Leg { get; protected set; }
    [SerializeField] public Legs LegScript { get; protected set; }

    [SerializeField] public GameObject ItemInHand { get; protected set; }
    [SerializeField] public UsableObject ItemInHandScript { get; protected set; }

    [SerializeField] public GameObject ItemInHandLeft { get; protected set; }
    [SerializeField] public UsableObject ItemInHandLeftScript { get; protected set; }

    [SerializeField] public List<GameObject> Items { get; protected set; }
    [SerializeField] public LayerMask EnemyLayer;

    protected CharacterStatController characterStatController;

    protected Rigidbody2D rb;

    protected Animator animator;

    [SerializeField]protected int JumpCount = 0;
    [SerializeField]protected int MaxJumpCount = 1;

    public float angleToTarget { get; protected set; }
    public float baseSpeed { get; set; } = 5f;
    protected float airVelocity { get; set; } = 300f;
    public float baseJumpForce { get; set; } = 500f;
    public float runSpeedModifier { get; set; } = 1.5f;

    [SerializeField] protected bool Running = false;
    [SerializeField] protected bool Grounded = false;
    [SerializeField] protected bool Moving = false;

    [SerializeField] protected LayerMask RaycastIgnoreLayer;
    [SerializeField] public bool IsHolding { get { return (ItemInHand != null || ItemInHandLeft != null); } }

    [SerializeField] protected float maxTorsoRotation = 50f;
    [SerializeField] protected float maxHandRotation = 40f;
    [SerializeField] protected float maxHeadRotation = 20f;

    public bool StopMoving { get; set; } = false;
    public bool IsOutOfJumps { get { return JumpCount == MaxJumpCount; } }

    protected void Start()
    {
        TorsoPivot = transform.Find("TorsoPivot").gameObject;
        HeadPivot = transform.Find("TorsoPivot/Torso/HeadPivot").gameObject;

        ArmRightPivot = transform.Find("TorsoPivot/Torso/ArmRightPivot").gameObject;
        ArmLeftPivot = transform.Find("TorsoPivot/Torso/ArmLeftPivot").gameObject;

        Leg = transform.Find("GluteRight/RightCalf/Leg").gameObject;
        LegScript = Leg.GetComponent<Legs>();

        ItemInHand = null;
        ItemInHandScript = null;
        ItemInHandLeft = null;
        ItemInHandLeftScript = null;

        ItemHolderLeft = transform.Find("TorsoPivot/Torso/ArmLeftPivot/ArmLeft/ForeArmLeft/ItemHolder").gameObject;
        ItemHolder = transform.Find("TorsoPivot/Torso/ArmRightPivot/ArmRight/ForeArmRight/ItemHolder").gameObject;

        Items = new List<GameObject>();
        InitializeItemsList();

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        characterStatController = GetComponent<CharacterStatController>();

        SoundEffects = new Dictionary<string, AudioSource>();
        IntializeSoundEffects();
    }

    //--------------------Initializing stuff------------------------------
    protected void InitializeItemsList()
    {
        foreach (Transform item in ItemHolder.transform)
        {
            Items.Add(item.gameObject);
        }

        foreach (Transform item in ItemHolderLeft.transform)
        {
            Items.Add(item.gameObject);
        }
    }

    protected void IntializeSoundEffects()
    {
        foreach (Transform Audio in AudioHolder.transform)
        {
            SoundEffects.Add(Audio.name, Audio.GetComponent<AudioSource>());
        }
    }

    //-------------------collision stuff----------------------------------

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        OnGroundCheck(collision);
    }

    protected void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Grounded = false;
        }
    }

    public void OnGroundCheck(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal == Vector2.up)
                {
                    if(Grounded == false)
                    {
                        Grounded = true;
                        SoundEffects["JumpLandSound"].pitch = UnityEngine.Random.Range(0.7f, 1.2f);
                        SoundEffects["JumpLandSound"].Play();
                        JumpCount = 0;
                        animator.SetBool("Jump", false);
                        return;
                    }
                    else
                    {
                        return;
                    }
                }

            }
        }

        Grounded = false;
    }

    //--------------animation events -------------------------

    //----weapon animation events----
    protected void WeaponAttackStartPrimary()
    {
        var currentAttackingObject = (IAttackablePrimary)ItemInHandScript;

        currentAttackingObject.AttackStateStartPrimary();
    }

    protected void WeaponAttackStartSecondary()
    {
        var currentAttackingObject = (WeaponMelee)ItemInHandScript;

        currentAttackingObject.AttackStateStartSecondary();
    }

    protected void WeaponAttackEnd()
    {
        var currentAttackingObject = (WeaponMelee)ItemInHandScript;

        currentAttackingObject.AttackStateEnd();
    }

    protected void LegAttackStart()
    {
        LegScript.AttackStateStartPrimary();
    }

    protected void LegAttackEnd()
    {
        LegScript.AttackStateEnd();
    }

    protected void ReduceStaminaLegAttack()
    {
        characterStatController.ReduceStamina(LegScript.BaseStaminaReduceValue);
    }

    protected void WeaponBlockStart()
    {
        Debug.Log("block Start");
        IBlockable currentWeapon = (ItemInHandLeftScript is Shield)
                    ? (IBlockable)ItemInHandLeftScript
                    : (IBlockable)ItemInHandScript;

        currentWeapon.BlockStateStart();
    }

    protected void WeaponBlockEnd()
    {
        Debug.Log("block End");
        IBlockable currentWeapon = (ItemInHandLeftScript is Shield)
            ? (IBlockable)ItemInHandLeftScript
            : (IBlockable)ItemInHandScript;

        currentWeapon.BlockStateEnd();
    }

    protected void ReduceStaminaMeleeWeaponAttack()
    {
        var AttackingWeapon = (WeaponMelee)ItemInHandScript;
        float staminaReduce = 0;

        if (AttackingWeapon.CurrentState == CurrentStateOfAction.AttackingPrimary)
            staminaReduce = AttackingWeapon.BaseStaminaReduceValue * AttackingWeapon.PrimaryAttackMultiplier;
        else if (AttackingWeapon.CurrentState == CurrentStateOfAction.AttackingSecondary)
            staminaReduce = AttackingWeapon.BaseStaminaReduceValue * AttackingWeapon.SecondaryAttackMultiplier;

        characterStatController.ReduceStamina(staminaReduce);
    }
    //----Item animation events----
    protected void SetItemInHandActive()
    {
        ItemInHand.GetComponent<SpriteRenderer>().enabled = true;
        ItemInHand.GetComponent<UsableObject>().enabled = true;
        ItemInHand.GetComponent<BoxCollider2D>().enabled = true;    
    }

    protected void SetItemInHandLeftActive()
    {
        Debug.Log("Enabling item in hand left :" + ItemInHandLeft);
        ItemInHandLeft.GetComponent<SpriteRenderer>().enabled = true;
        ItemInHandLeft.GetComponent<UsableObject>().enabled = true;
        ItemInHandLeft.GetComponent<BoxCollider2D>().enabled = true;
    }
    //----Movement animation events----
    protected void PlayStepSoundEffect()
    {
        SoundEffects["SteppingSound"].pitch = UnityEngine.Random.Range(0.7f, 1.2f);
        SoundEffects["SteppingSound"].Play();
    }
    //-----------------movement stuff---------------------------------
    public void Jump()
    {
        if (characterStatController.ReduceStamina(15f))
        {
            SoundEffects["JumpStartSound"].Play();
            float jumpForce = (Moving) ? baseJumpForce * 0.8f : baseJumpForce;
            animator.SetBool("Jump", true);
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            JumpCount++;
        }
    }

    //---------------------Taking Item------------------------------
    protected virtual void TakeItem(string itemName)
    {
        Debug.Log(itemName);
        var ItemToTake = Items.FirstOrDefault(item => item.name == itemName);
        Debug.Log(ItemToTake);
        Debug.Log(ItemToTake?.GetComponent<UsableObject>());
        if (ItemToTake.GetComponent<UsableObject>() is Shield)
        {
            TakeShield(ItemToTake);
        }
        else if(ItemToTake.GetComponent<UsableObject>().IsTwoHanded)
        {
            TakeTwoHandedWeapon(ItemToTake);
        }
        else if (!ItemToTake.GetComponent<UsableObject>().IsTwoHanded)
        {
            TakeOneHandedWeapon(ItemToTake);
        }
    }

    protected void TakeShield(GameObject Shield)
    {
        if (ItemInHand != null && ItemInHand.GetComponent<UsableObject>().IsTwoHanded)
        {
            DisablePreviousItem(ItemInHand);
        }

        if (ItemInHandLeft != null)
        {
            DisablePreviousItem(ItemInHandLeft);
        }

        ItemInHandLeft = Shield;
        ItemInHandLeftScript = ItemInHandLeft.GetComponent<UsableObject>();

        animator.SetLayerWeight(ItemInHandLeftScript.AnimationLayer, 1);

        animator.Play("HoldingStart", ItemInHandLeftScript.AnimationLayer, 0f);
        animator.SetBool("IsHolding", true);
    }

    protected void TakeTwoHandedWeapon(GameObject TwoHandedWeapon)
    {
        Debug.Log("Taking two handed weapon!");
        if (ItemInHand != null || ItemInHandLeft != null)
        {
            DisablePreviousItem(ItemInHand);
            DisablePreviousItem(ItemInHandLeft);
        }

        ItemInHand = TwoHandedWeapon;
        ItemInHandScript = ItemInHand.GetComponent<UsableObject>();

        animator.SetLayerWeight(ItemInHandScript.AnimationLayer, 1);
        //Debug.Log(ItemInHandScript.AnimationLayer);
        animator.Play("HoldingStart", ItemInHandScript.AnimationLayer, 0f);
        animator.SetBool("IsHolding", true);
    }

    protected void TakeOneHandedWeapon(GameObject OneHandedWeapon)
    {
        if (ItemInHand != null)
        {
            DisablePreviousItem(ItemInHand);
        }
        ItemInHand = OneHandedWeapon;
        ItemInHandScript = ItemInHand.GetComponent<UsableObject>();

        animator.SetLayerWeight(ItemInHandScript.AnimationLayer, 1);

        Debug.Log(ItemInHandScript.AnimationLayer);
        animator.Play("HoldingStart", ItemInHandScript.AnimationLayer, 0f);
        animator.SetBool("IsHolding", true);
    }

    public void DisablePreviousItem(GameObject itemInHand)
    {
        if (itemInHand)
        {
            UsableObject itemInHandScript = itemInHand.GetComponent<UsableObject>();

            itemInHandScript.enabled = false;
            itemInHand.GetComponent<SpriteRenderer>().enabled = false;
            itemInHand.GetComponent<BoxCollider2D>().enabled = false;
 

            if (itemInHand == ItemInHand)
            {
                ItemInHand = null;
                ItemInHandScript = null;    
            }
            else if(itemInHand == ItemInHandLeft)
            {
                ItemInHandLeft = null;
                ItemInHandLeftScript = null;
            }
            Debug.Log(itemInHandScript.GetType().ToString());
            animator.SetLayerWeight(animator.GetLayerIndex(itemInHandScript.GetType().ToString()), 0);
        }
    }
    //-----------------combat Actions------------------------

    protected void Attack(string PrimaryOrSecondary)
    {
        var Shield = ItemInHandLeftScript as Shield;
        if (Shield && Shield.CurrentState == CurrentStateOfAction.Blocking)
        {
            return;
        }

        if (!ItemInHand)
        {
            if (!ItemInHandLeft)
            {
                TakeItem("TwoHandedFist");
            }
            else
            {
                TakeItem("OneHandedFist");
            }
        }

        var currentWeapon = (WeaponMelee)ItemInHandScript;

        if (PrimaryOrSecondary == "Primary")
        {
            //Debug.Log("Attacking Primary!");
            currentWeapon.AttackPrimary();
        }
        else if (PrimaryOrSecondary == "Secondary")
        {
            currentWeapon.AttackSecondary();
        }
    }

    protected void StartBlocking()
    {
        if (ItemInHandScript && ItemInHandScript.PlayingAttackAnimationCheck.Item2)
        {
            Debug.Log(ItemInHandScript.PlayingAttackAnimationCheck.Item1);
            return;
        }

        if (!ItemInHand)
        {
            if (!ItemInHandLeft)
            {
                TakeItem("TwoHandedFist");
            }
        }

        IBlockable currentItem = (ItemInHandLeftScript is Shield) 
            ? (Shield)ItemInHandLeftScript 
            : (WeaponMelee)ItemInHandScript;

        currentItem.BlockStart();
    }

    protected void StopBlocking()
    {
        if (!IsHolding)
        {
            return;
        }

        IBlockable currentItem = (ItemInHandLeftScript is Shield) 
            ? (Shield)ItemInHandLeftScript 
            : (WeaponMelee)ItemInHandScript;

        currentItem.BlockEnd();
    }

    protected void CancelAttack()
    {
        Debug.Log("Calling the cancel event!");
        var currentWeapon = (WeaponMelee)ItemInHandScript;

        currentWeapon?.CancelAttack();
    }

    protected void Kick()
    {
        Debug.Log("kicking!");
        var Shield = ItemInHandLeftScript as Shield;
        if ((Shield && Shield.CurrentState == CurrentStateOfAction.Blocking) || (ItemInHandScript && ItemInHandScript.PlayingAttackAnimationCheck.Item2))
        {
            return;
        }

        var Attackable = ItemInHandScript as IAttackablePrimary;
        if (Attackable != null)
        {
            if((Attackable.ActionCoolDownTimer <= Attackable.ActionCoolDownAttackPrimary))
            {
                Debug.Log("the timer for weapon didnt end!");
                return;
            }
        }

        LegScript.AttackPrimary();
    }

    //-----------------animation controller--------------------

    protected void HandleAnimations()
    {
        if (Grounded)
        {
            animator.SetBool("Moving", Moving);
            animator.SetBool("Running", Running);

            if(animator.GetBool("Jump") == true)
            {
                animator.SetBool("Jump", false);
            }
        }
        else
        {
            animator.SetBool("Jump", true);
        }

        if(animator.GetBool("IsHolding") != IsHolding)
        {
            animator.SetBool("IsHolding", IsHolding);
        }
    }


    void OnDisable()
    {
        rb.linearVelocity = Vector3.zero;
    }

    //-----------Dropping--

    public void DropItem(GameObject item)
    {
        if (!item) return;

        DisablePreviousItem(item);

    }
    protected abstract void Move(float SpeedValue);
}
