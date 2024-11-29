using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public abstract class BaseCharacterController : MonoBehaviour
{
    protected GameObject Torso;
    protected GameObject Head;

    public Dictionary<string, AudioSource> SoundEffects;

    protected GameObject ArmLeft;
    protected GameObject ArmRight;

    [SerializeField] protected GameObject ItemHolder;

    [SerializeField] protected GameObject AudioHolder;
    [SerializeField] public GameObject ItemInHand { get; set; }
    [SerializeField] public List<GameObject> Items { get; protected set; }

    [SerializeField] public LayerMask EnemyLayer;

    protected CharacterStatController characterStatController;

    protected Rigidbody2D rb;

    protected Animator animator;

    protected int JumpCount = 0;
    public int MaxJumpCount = 1;

    public float angleToTarget { get; protected set; }

    public float baseSpeed { get; set; } = 5f;
    protected float airVelocity { get; set; } = 300f;
    public float baseJumpForce { get; set; } = 500f;
    public float runSpeedModifier { get; set; } = 1.5f;

    [SerializeField] protected bool Running = false;
    [SerializeField] protected bool Grounded = false;
    [SerializeField] protected bool Moving = false;

    [SerializeField] protected LayerMask RaycastIgnoreLayer;
    [SerializeField] public bool IsHolding { get { return ItemInHand != null; } }

    [SerializeField] protected float maxTorsoRotation = 50f;
    [SerializeField] protected float maxHandRotation = 40f;
    [SerializeField] protected float maxHeadRotation = 20f;
    public bool IsOutOfJumps { get { return JumpCount == MaxJumpCount; } }

    protected void Start()
    {
        Torso = transform.Find("Torso").gameObject;
        Head = transform.Find("Torso/Head").gameObject;

        ArmRight = transform.Find("Torso/ArmRightPivot").gameObject;
        ArmLeft = transform.Find("Torso/ArmLeftPivot").gameObject;

        ItemInHand = null;
        ItemHolder = transform.Find("Torso/ArmRightPivot/ArmRight/ForeArmRight/ItemHolder").gameObject;

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
        var currentWeapon = ItemInHand.GetComponent<WeaponMelee>();
        currentWeapon.AttackStateStartPrimary();
    }

    protected void WeaponAttackStartSecondary()
    {
        var currentWeapon = ItemInHand.GetComponent<WeaponMelee>();
        currentWeapon.AttackStateStartSecondary();
    }

    protected void WeaponAttackEnd()
    {
        var currentWeapon = ItemInHand.GetComponent<WeaponMelee>();
        currentWeapon.AttackStateEnd();
    }

    protected void WeaponBlockStart()
    {
        var currentWeapon = ItemInHand.GetComponent<WeaponMelee>();
        currentWeapon.BlockStateStart();
    }
    protected void WeaponBlockEnd()
    {
        var currentWeapon = ItemInHand.GetComponent<WeaponMelee>();
        currentWeapon.BlockStateEnd();
    }

    protected void ReduceStaminaMeleeWeaponAttack()
    {
        if(IsHolding && ItemInHand.CompareTag("Weapon"))
        {
            var Weapon = ItemInHand.GetComponent<WeaponMelee>();

            float staminaReduceMultiplier = 0;
            if (Weapon.currentState == WeaponMelee.CurrentStateOfWeapon.AttackingPrimary)
                staminaReduceMultiplier = Weapon.PrimaryAttackMultiplier;
            else if (Weapon.currentState == WeaponMelee.CurrentStateOfWeapon.AttackingSecondary)
                staminaReduceMultiplier = Weapon.SecondaryAttackMultiplier;

            var staminaReduce = Weapon.BaseStaminaReduceValue * staminaReduceMultiplier;

            characterStatController.ReduceStamina(staminaReduce);
        }
    }
    //----Item animation events----
    protected void SetItemInHandActive()
    {
        Debug.Log("setting item in hand active!");
        Debug.Log(ItemInHand);
        Debug.Log(ItemInHand.name);
        ItemInHand.SetActive(true);
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
        Debug.Log(itemName);
        ItemInHand = Items.FirstOrDefault(item => item.name == itemName);
        Debug.Log(ItemInHand.name);
        animator.SetBool("IsHolding", IsHolding);
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

}
