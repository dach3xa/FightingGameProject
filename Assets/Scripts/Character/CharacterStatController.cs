using Assets.Scripts.UsableObject.Attackable.Weapon.WeaponMelee.AdditionalInfo;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CharacterStatController : MonoBehaviour
{
    protected enum CurrentStatState
    {
        normal,
        Damaged,
        Exhausted,
        Dead
    }

    CurrentStatState statState = CurrentStatState.normal;

    [SerializeField] public float Health;
    [SerializeField] protected float baseHealth = 100f;
    [SerializeField] public float Stamina;
    [SerializeField] protected float baseStamina = 200f;
    [SerializeField] public float Mana;
    [SerializeField] protected float baseMana = 0f;
    [SerializeField] protected float DamageDirectionHorizontal;


    [SerializeField] protected float ManaRegenSpeed;
    [SerializeField] protected float HealthRegenSpeed;
    [SerializeField] protected float StaminaRegenSpeed;

    [SerializeField] BaseCharacterController CharacterControllerScript;
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rigidbody;
    [SerializeField] BoxCollider2D collider;

    [SerializeField] protected List<GameObject> BodyParts;
    [SerializeField] private Dictionary<string, float> Limbs;

    protected void Start()
    {
        CharacterControllerScript = GetComponent<BaseCharacterController>();

        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();
        Limbs = new Dictionary<string, float> { { "Head", 0.6f }, { "ArmRight", 1.2f }, { "ArmLeft", 1.2f } };

        Health = baseHealth;
        Stamina = baseStamina;
        Mana = baseMana;

        StaminaRegenSpeed = 10f;
        HealthRegenSpeed = 1f;
        ManaRegenSpeed = 0f;
    }

    //---------------------Stat Updates and Validation
    protected void UpdateStats()
    {
        UpdateStamina();

        UpdateMana();

        UpdateHealth();

        CheckCurrentStatState();
    }

    //---- current stat state check
    protected void CheckCurrentStatState()
    {
        if(statState == CurrentStatState.Exhausted && Stamina > 0)
        {
            Stamina = 0;
        }
        else if(statState == CurrentStatState.Damaged && CharacterControllerScript.enabled == true)
        {
            CharacterControllerScript.enabled = false;
        }
        else if(statState == CurrentStatState.normal && CharacterControllerScript.enabled == false)
        {
            CharacterControllerScript.enabled = true;
        }
    }
    //---------------------- current stat state controller

    protected void CurrentStatStateController(CurrentStatState NewStatState, AdditionalInfo AdditionalInfo = null)
    {
        switch (NewStatState) 
        {
            case CurrentStatState.Exhausted:
                this.statState = CurrentStatState.Exhausted;
                break;
            case CurrentStatState.Damaged:
                ChangeCurrentStatStateToDamaged(AdditionalInfo);
                break;
            case CurrentStatState.normal:
                ChangeCurrentStatStateToNormal();
                break;
            case CurrentStatState.Dead:
                ChangeCurrentStatStateToDead(AdditionalInfo);
                break;
        }
    }
    protected void ChangeCurrentStatStateToDamaged(AdditionalInfo AdditionalInfo)
    {
        ResetAllAnimationParameters();

        CharacterControllerScript.ItemInHand?.GetComponent<UsableObject>().OnHolderDamaged();
        CharacterControllerScript.ItemInHandLeft?.GetComponent<UsableObject>().OnHolderDamaged();

        CharacterControllerScript.enabled = false;

        if (CharacterControllerScript.ItemInHand) 
        {
            if(CharacterControllerScript.ItemInHand.GetComponent<UsableObject>() is WeaponMelee weaponInHand)
                weaponInHand.AttackStateEnd();

            CharacterControllerScript.ItemInHand.GetComponent<UsableObject>().enabled = false;
        }

        if (CharacterControllerScript.ItemInHandLeft) 
        {
            if (CharacterControllerScript.ItemInHandLeft.GetComponent<UsableObject>() is WeaponMelee weaponInHand)
                weaponInHand.AttackStateEnd();

            CharacterControllerScript.ItemInHandLeft.GetComponent<UsableObject>().enabled = false;
        }

        HandleDamagedAnimation(AdditionalInfo);
        this.statState = CurrentStatState.Damaged;
    }

    //-------handle damage animation-------------------------------------
   protected void HandleDamagedAnimation(AdditionalInfo AdditionalInfo)
   {
        GameObject AttackerWeapon = AdditionalInfo.AttackerWeapon;
        BaseCharacterController attackerController = AttackerWeapon.transform.root.GetComponent<BaseCharacterController>();
        if (Mathf.Sign(gameObject.transform.localScale.x) == Mathf.Sign(-AttackerWeapon.transform.lossyScale.x))
        {
            HandleDamageAnimationFront(attackerController, AttackerWeapon);
        }
        else
        {
            HandleDamageAnimationBack(attackerController, AttackerWeapon);
        }

        animator.SetTrigger("Damaged");
   }

    protected void HandleDamageAnimationFront(BaseCharacterController attackerController, GameObject AttackerWeapon)
    {
        var currentAttackingAnimation = AttackerWeapon.GetComponent<UsableObject>().PlayingAttackAnimationCheck.Item1;

        if (AttackerWeapon.GetComponent<UsableObject>() is TwoHandedFist)
        {
            animator.Play("Damaged", 0, 0f);
            return;
        }

        if (currentAttackingAnimation == "PrimaryAttack" || currentAttackingAnimation == "SecondaryAttack" || currentAttackingAnimation == "LegKick")
        {
            animator.Play("DamagedBottom", 0, 0f);
        }
        else
        {
            animator.Play("Damaged", 0, 0f);
        }
    }

    protected void HandleDamageAnimationBack(BaseCharacterController attackerController, GameObject AttackerWeapon)
    {
        var currentAttackingAnimation = AttackerWeapon.GetComponent<UsableObject>().PlayingAttackAnimationCheck.Item1;

        if (AttackerWeapon.GetComponent<UsableObject>() is TwoHandedFist)
        {
            animator.Play("DamagedBottom", 0, 0f);
            return;
        }

        if (currentAttackingAnimation == "SecondaryAttack" || currentAttackingAnimation == "PrimaryAttack2" || currentAttackingAnimation == "LegKick")
        {
            animator.Play("Damaged", 0, 0f);
        }
        else
        {
            animator.Play("DamagedBottom", 0, 0f);
        }
    }

    protected void ResetAllAnimationParameters()
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    animator.SetFloat(parameter.name, 0f);
                    break;
                case AnimatorControllerParameterType.Int:
                    animator.SetInteger(parameter.name, 0);
                    break;
                case AnimatorControllerParameterType.Bool:

                    if (parameter.name != "IsHolding")
                    {
                        animator.SetBool(parameter.name, false);
                    }

                    break;
                case AnimatorControllerParameterType.Trigger:
                    animator.ResetTrigger(parameter.name);
                    break;
            }
        }
    }

    protected void ChangeCurrentStatStateToNormal()
    {
        CharacterControllerScript.enabled = true;
        if (CharacterControllerScript.ItemInHand) CharacterControllerScript.ItemInHand.GetComponent<UsableObject>().enabled = true;
        if (CharacterControllerScript.ItemInHandLeft) CharacterControllerScript.ItemInHandLeft.GetComponent<UsableObject>().enabled = true;

        animator.ResetTrigger("Damaged");
        this.statState = CurrentStatState.normal;
    }

    protected void ChangeCurrentStatStateToDead(AdditionalInfo AdditionalInfo)
    {
        CharacterControllerScript.enabled = false;
        animator.enabled = false;
        rigidbody.bodyType = RigidbodyType2D.Static;
        collider.isTrigger = true;
        this.statState = CurrentStatState.Dead;
        gameObject.layer = 0;
        this.enabled = false;


        TurnOnRigidbodyAndJointsForBodyParts();
        DropPrimaryItems();
        ApplyForceToBodyParts();

        if (ShouldChopOffRandomPartCheck(AdditionalInfo))
        {
            ChopOffRandomPart();
        }
    }

    protected bool ShouldChopOffRandomPartCheck(AdditionalInfo AdditionalInfo)
    {
        GameObject AttackerWeapon = AdditionalInfo.AttackerWeapon;
        bool WasLastAttackDirect = AdditionalInfo.WasLastAttackDirect.Value;

        var attackerWeaponMeleeScript = AttackerWeapon.GetComponent<WeaponMelee>();
        if (attackerWeaponMeleeScript == null)
            return false;

        var randomNum = Random.Range(0f, 1f);
        Debug.Log(randomNum);

        return attackerWeaponMeleeScript.PlayingAttackAnimationCheck.Item2
                && randomNum < attackerWeaponMeleeScript.Sharpness
                && attackerWeaponMeleeScript.PlayingAttackAnimationCheck.Item1.ToLower().Contains("primaryattack")
                && WasLastAttackDirect;
    }

    protected void TurnOnRigidbodyAndJointsForBodyParts()
    {
        foreach(var BodyPart in BodyParts)
        {
            //Debug.Log("Enabling body part : " + BodyPart.name);
            BodyPart.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            BodyPart.GetComponent<CapsuleCollider2D>().enabled = true;

            if(BodyPart.name != "Torso")
            {
                BodyPart.GetComponent<HingeJoint2D>().enabled = true;
            }

            if(BodyPart.name == "ForeArmLeft")
            {
                BodyPart.GetComponent<SpriteRenderer>().sortingOrder = 0;
            }
        }
    }

    protected void ApplyForceToBodyParts()
    {
        Rigidbody2D TorsoRb = BodyParts.FirstOrDefault(bodypart => bodypart.name == "Torso").GetComponent<Rigidbody2D>();
        Rigidbody2D RightCalfRb = BodyParts.FirstOrDefault(bodypart => bodypart.name == "RightCalf").GetComponent<Rigidbody2D>();
        Rigidbody2D LeftCalfRb = BodyParts.FirstOrDefault(bodypart => bodypart.name == "LeftCalf").GetComponent<Rigidbody2D>();

        TorsoRb.AddForce(new Vector2(DamageDirectionHorizontal, 0).normalized * 500f, ForceMode2D.Impulse);//get direction from hit
        RightCalfRb.AddForce(new Vector2(-DamageDirectionHorizontal, 0).normalized * 0.4f, ForceMode2D.Force);
        LeftCalfRb.AddForce(new Vector2(-DamageDirectionHorizontal, 0).normalized * 0.4f, ForceMode2D.Force);
    }

    protected void DropPrimaryItems()
    {
        if(CharacterControllerScript.ItemInHand) CharacterControllerScript.DropItem(CharacterControllerScript.ItemInHand);
        if(CharacterControllerScript.ItemInHandLeft) CharacterControllerScript.DropItem(CharacterControllerScript.ItemInHandLeft);
    }

    protected void ChopOffRandomPart()
    {
        var limbName = Limbs.Keys.ToList()[Random.Range(0, Limbs.Keys.Count)];
        var randomLimb = BodyParts.FirstOrDefault(bodyPart => bodyPart.name == limbName);
        randomLimb.transform.parent = null;
        randomLimb.GetComponent<Rigidbody2D>().AddForce(new Vector2(DamageDirectionHorizontal, 0).normalized * 2500f * Limbs[limbName], ForceMode2D.Force);
        randomLimb.GetComponent<HingeJoint2D>().enabled = false;

        if(limbName.ToLower() == "head")
        {
            randomLimb.GetComponent<SpriteRenderer>().sortingOrder = 8;
            var headArmor = randomLimb.transform.Find("HeadArmor");

            if (headArmor != null)
                headArmor.GetComponent<SpriteRenderer>().sortingOrder = 9;
        }
    }

    //--------------------------- updating stats
    protected void UpdateStamina()
    {
        if (Stamina < baseStamina)
        {
            Stamina += StaminaRegenSpeed * Time.deltaTime;
            Mathf.Clamp(Stamina, 0, baseStamina);
        }
    }

    protected void UpdateMana()
    {
        if (Mana < baseMana)
        {
            Mana += ManaRegenSpeed * Time.deltaTime;
            Mathf.Clamp(Mana, 0, baseMana);
        }
    }

    protected void UpdateHealth()
    {
        if (Health < baseHealth && Health > 0)
        {
            Health += HealthRegenSpeed * Time.deltaTime;
            Mathf.Clamp(Health, 0, baseHealth);
        }
        else if (Health <= 0)
        {
            Health = 0;
        }
    }

    //-----------------------Receveing Stat Updates from Outside

    public bool RecieveAttack(float damage, GameObject AttackerWeapon)
    {
        //Debug.Log("Recieved the attack from " + AttackerWeapon.name);

        WeaponMelee ItemInHand = CharacterControllerScript.ItemInHand?.GetComponent<WeaponMelee>();
        Shield ItemInHandLeft = CharacterControllerScript.ItemInHandLeft?.GetComponent<Shield>();

        bool IsAttackingOrBlockingRightHand = ItemInHand && (ItemInHand.CurrentState == CurrentStateOfAction.Blocking || ItemInHand.CurrentState == CurrentStateOfAction.AttackingSecondary || ItemInHand.CurrentState == CurrentStateOfAction.AttackingPrimary);
        bool IsBlockingLeftHand = ItemInHandLeft && (ItemInHandLeft.CurrentState == CurrentStateOfAction.Blocking);

        Vector2 toAttacker = AttackerWeapon.transform.position - transform.position;
        bool amFacingAttacker =
            Mathf.Sign(toAttacker.x) == Mathf.Sign(transform.localScale.x);

        if (CharacterControllerScript.IsHolding && (IsAttackingOrBlockingRightHand || IsBlockingLeftHand) && amFacingAttacker)
        {
            if(CheckAttackingOrBlockingCurrentWeapon(IsAttackingOrBlockingRightHand, IsBlockingLeftHand, ItemInHand, ItemInHandLeft, AttackerWeapon))
            {
                TakeDamage(damage, AttackerWeapon);
                return true;
            }

            TakeDamage(damage / 10, AttackerWeapon, IsDirectDamage: false);

            ReduceStamina(damage / 2);

            return false;
        }
        else
        {
            //Debug.Log("Recieved damage from " + AttackerWeapon.name);
            TakeDamage(damage, AttackerWeapon);
            return true;
        }
    }

    protected bool CheckAttackingOrBlockingCurrentWeapon(bool IsAttackingOrBlockingRightHand, bool IsBlockingLeftHand, WeaponMelee ItemInHand, Shield ItemInHandLeft, GameObject AttackerWeapon)
    {
        if (IsBlockingLeftHand)//shield
        {
            if (ItemInHandLeft.BlockImpact(AttackerWeapon) == false)
            {
                return true;
            }
        }
        else if (IsAttackingOrBlockingRightHand)
        {
            //Debug.Log("Blocking or attacking right hand!");
            if (ItemInHand.CurrentState == CurrentStateOfAction.Blocking)
            {
                if (ItemInHand.BlockImpact(AttackerWeapon) == false)
                {
                    return true;
                }
            }
            else if (ItemInHand.AttacksClashed(AttackerWeapon) == false)
            {
                return true;
            }
        }
        return false;
    }

    public void TakeDamage(float damage, GameObject AttackerWeapon, bool IsDirectDamage = true)
    {
        if (IsDirectDamage)
        {
            CurrentStatStateController(CurrentStatState.Damaged, new AdditionalInfo(AttackerWeapon));
        }
        DamageDirectionHorizontal = Mathf.Sign(AttackerWeapon?.transform.lossyScale.x ?? 0f);

        Health -= damage;
        if(Health < 0)
        {
            CurrentStatStateController(CurrentStatState.Dead, new AdditionalInfo(AttackerWeapon, IsDirectDamage));
        }
        Mathf.Clamp(Health, 0, baseHealth);
    }

    public bool ReduceStamina(float StaminaReduce)
    {
        if (Stamina + 2 > StaminaReduce)
        {
            Stamina -= StaminaReduce;
            Mathf.Clamp(Stamina, 0, baseStamina);
            if(Stamina == 0)
            {
                CurrentStatStateController(CurrentStatState.Exhausted);
            }
            return true;
        }
        return false;
    }

    //---------------Animation Events------------

    protected void StaminaExhoustionStateEnd()
    {
        if (statState == CurrentStatState.Exhausted)
        {
            CurrentStatStateController(CurrentStatState.normal);
        }
    }

    protected void DamagedStateEnd()
    {
        if (statState == CurrentStatState.Damaged)
        {
            CurrentStatStateController(CurrentStatState.normal);
        }
    }
}
