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

    public Dictionary<int, string> AttackStateNames = new Dictionary<int, string>
    {
        { Animator.StringToHash("PrimaryAttack"), "PrimaryAttack" },
        { Animator.StringToHash("PrimaryAttack2"), "PrimaryAttack2" },
        { Animator.StringToHash("SecondaryAttack"), "SecondaryAttack" },
        { Animator.StringToHash("LegKick"), "LegKick" }
    };

    public Dictionary<string, AudioSource> SoundEffects;

    [SerializeField] protected GameObject ItemHolder;

    [SerializeField] protected GameObject ItemHolderLeft;

    [SerializeField] protected GameObject Leg;

    [SerializeField] protected GameObject AudioHolder;
    [SerializeField] public GameObject ItemInHand { get; protected set; }
    [SerializeField] public GameObject ItemInHandLeft { get; protected set; }
    [SerializeField] public List<GameObject> Items { get; protected set; }
    [SerializeField] public LayerMask EnemyLayer;
    [SerializeField] public int CurrentAnimatorHoldingLayerRight { get; protected set; }
    [SerializeField] public int CurrentAnimatorHoldingLayerLeft { get; protected set; }

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
    public bool IsOutOfJumps { get { return JumpCount == MaxJumpCount; } }

    protected void Start()
    {
        TorsoPivot = transform.Find("TorsoPivot").gameObject;
        HeadPivot = transform.Find("TorsoPivot/Torso/HeadPivot").gameObject;

        ArmRightPivot = transform.Find("TorsoPivot/Torso/ArmRightPivot").gameObject;
        ArmLeftPivot = transform.Find("TorsoPivot/Torso/ArmLeftPivot").gameObject;

        ItemInHand = null;
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
    //--------------IsAttackingCheck -------------------------

    public (string, UsableObject, bool) IsAttackingCheck()
    {
        AnimatorStateInfo stateInfoWeapon = animator.GetCurrentAnimatorStateInfo(CurrentAnimatorHoldingLayerRight);
        AnimatorStateInfo stateInfoLegs = animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("LegsLayer"));

        if (ItemInHand && (AttackStateNames.ContainsKey(stateInfoWeapon.shortNameHash)))
        {
            return (AttackStateNames[stateInfoWeapon.shortNameHash], ItemInHand.GetComponent<WeaponMelee>(), true);
        }
        else if (AttackStateNames.ContainsKey(stateInfoLegs.shortNameHash))
        {
            return (AttackStateNames[stateInfoLegs.shortNameHash], Leg.GetComponent<Leg>(), true);
        }
        else
        {
            return ("not Attacking", null, false);
        }
        
    }

    //--------------animation events -------------------------

    //----weapon animation events----
    protected void WeaponAttackStartPrimary()
    {
        IAttackablePrimary currentAttackingObject = (IsAttackingCheck().Item1 == "LegKick")? Leg.GetComponent<Leg>() : ItemInHand.GetComponent<WeaponMelee>();
        currentAttackingObject.AttackStateStartPrimary();
    }

    protected void WeaponAttackStartSecondary()
    {
        var currentAttackingObject = ItemInHand.GetComponent<WeaponMelee>();
        currentAttackingObject.AttackStateStartSecondary();
    }

    protected void WeaponAttackEnd()
    {
        IAttackablePrimary currentAttackingObject = (ItemInHand && ItemInHand.GetComponent<WeaponMelee>().IsAttacking) ? ItemInHand.GetComponent<WeaponMelee>() : Leg.GetComponent<Leg>();
        currentAttackingObject.AttackStateEnd();
    }

    protected void WeaponBlockStart()
    {
        IBlockable currentWeapon = (ItemInHandLeft && ItemInHandLeft.GetComponent<UsableObject>() is Shield) ? ItemInHandLeft.GetComponent<Shield>() : ItemInHand.GetComponent<WeaponMelee>();
        currentWeapon.BlockStateStart();
    }
    protected void WeaponBlockEnd()
    {
        IBlockable currentWeapon = (ItemInHandLeft && ItemInHandLeft.GetComponent<UsableObject>() is Shield) ? ItemInHandLeft.GetComponent<Shield>() : ItemInHand.GetComponent<WeaponMelee>();
        currentWeapon.BlockStateEnd();
    }

    protected void ReduceStaminaMeleeWeaponAttack()
    {
        IAttackablePrimary currentAttackingObject = (ItemInHand && ItemInHand.GetComponent<WeaponMelee>().IsAttacking) ? ItemInHand.GetComponent<WeaponMelee>() : Leg.GetComponent<Leg>();

        float staminaReduceMultiplier = 0;

        if (currentAttackingObject.CurrentState == CurrentStateOfAction.AttackingPrimary)
            staminaReduceMultiplier = currentAttackingObject.PrimaryAttackMultiplier;
        else if (currentAttackingObject.CurrentState == CurrentStateOfAction.AttackingSecondary)
            staminaReduceMultiplier = currentAttackingObject.PrimaryAttackMultiplier;//change in the future this part

        var staminaReduce = currentAttackingObject.BaseStaminaReduceValue * staminaReduceMultiplier;

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
    protected void TakeItem(string itemName)
    {
        var ItemToTake = Items.FirstOrDefault(item => item.name == itemName);
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

        CurrentAnimatorHoldingLayerLeft = animator.GetLayerIndex(ItemInHandLeft.name);
        animator.SetLayerWeight(CurrentAnimatorHoldingLayerLeft, 1);

        animator.Play("NotHolding", CurrentAnimatorHoldingLayerLeft, 0f);
        animator.SetBool("IsHolding", true);
    }

    protected void TakeTwoHandedWeapon(GameObject TwoHandedWeapon)
    {
        if (ItemInHand != null || ItemInHandLeft != null)
        {
            DisablePreviousItem(ItemInHand);
            DisablePreviousItem(ItemInHandLeft);
        }

        ItemInHand = TwoHandedWeapon;

        CurrentAnimatorHoldingLayerRight = animator.GetLayerIndex(ItemInHand.name);
        animator.SetLayerWeight(CurrentAnimatorHoldingLayerRight, 1);

        animator.Play("NotHolding", CurrentAnimatorHoldingLayerRight, 0f);
        animator.SetBool("IsHolding", true);
    }

    protected void TakeOneHandedWeapon(GameObject OneHandedWeapon)
    {
        if (ItemInHand != null)
        {
            DisablePreviousItem(ItemInHand);
        }
        ItemInHand = OneHandedWeapon;

        CurrentAnimatorHoldingLayerRight = animator.GetLayerIndex(ItemInHand.name);
        animator.SetLayerWeight(CurrentAnimatorHoldingLayerRight, 1);

        animator.Play("NotHolding", CurrentAnimatorHoldingLayerRight, 0f);
        animator.SetBool("IsHolding", true);
    }

    public void DisablePreviousItem(GameObject itemInHand)
    {
        if (itemInHand)
        {

            itemInHand.GetComponent<UsableObject>().enabled = false;
            itemInHand.GetComponent<SpriteRenderer>().enabled = false;
            itemInHand.GetComponent<BoxCollider2D>().enabled = false;

            if (itemInHand == ItemInHand)
            {
                ItemInHand = null;
            }
            else if(itemInHand == ItemInHandLeft)
            {
                ItemInHandLeft = null;
            }
            animator.SetLayerWeight(animator.GetLayerIndex(itemInHand.name), 0);
        }
    }
    //-----------------combat Actions------------------------

    protected void Attack(string PrimaryOrSecondary)
    {

        if (ItemInHandLeft && ItemInHandLeft.GetComponent<UsableObject>() is Shield && ItemInHandLeft.GetComponent<Shield>().CurrentState == CurrentStateOfAction.Blocking)
        {
            return;
        }

        if (!ItemInHand)
        {
            if (!ItemInHandLeft)
            {
                TakeItem("TwoHandedFist");
            }

            return;//one handed fist here
        }

        var currentWeapon = ItemInHand.GetComponent<WeaponMelee>();

        if (PrimaryOrSecondary == "Primary")
        {
            currentWeapon.AttackPrimary();
        }
        else if (PrimaryOrSecondary == "Secondary")
        {
            currentWeapon.AttackSecondary();
        }
    }

    protected void StartBlocking()
    {
        if (IsAttackingCheck().Item2)
        {
            return;
        }

        if (!ItemInHand)
        {
            if (!ItemInHandLeft)
            {
                TakeItem("TwoHandedFist");
            }
        }

        IBlockable currentItem = (ItemInHandLeft && ItemInHandLeft.GetComponent<UsableObject>() is Shield) ? ItemInHandLeft.GetComponent<Shield>() : ItemInHand.GetComponent<WeaponMelee>();
        currentItem.BlockStart();
    }

    protected void StopBlocking()
    {
        if (!IsHolding)
        {
            return;
        }

        IBlockable currentItem = (ItemInHandLeft && ItemInHandLeft.GetComponent<UsableObject>() is Shield) ? ItemInHandLeft.GetComponent<Shield>() : ItemInHand.GetComponent<WeaponMelee>();
        currentItem.BlockEnd();
    }

    protected void CancelAttack()
    {
        Debug.Log("Calling the cancel event!");
        var currentWeapon = ItemInHand?.GetComponent<WeaponMelee>();

        currentWeapon?.CancelAttack();
    }

    protected void Kick()
    {
        if (ItemInHand && ItemInHand.GetComponent<UsableObject>() is IAttackablePrimary)
        {
            if((ItemInHand.GetComponent<IAttackablePrimary>().ActionCoolDownTimer <= ItemInHand.GetComponent<IAttackablePrimary>().ActionCoolDownAttackPrimary))
            {
                return;
            }
        }

        var LegScript = Leg.GetComponent<Leg>();
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
}
