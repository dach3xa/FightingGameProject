using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
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

    protected void Start()
    {
        CharacterControllerScript = GetComponent<BaseCharacterController>();
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();

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

    protected void CurrentStatStateController(CurrentStatState NewStatState)
    {
        switch (NewStatState) 
        {
            case CurrentStatState.Exhausted:
                this.statState = NewStatState;
                break;
            case CurrentStatState.Damaged:
                ChangeCurrentStatStateToDamaged();
                break;
            case CurrentStatState.normal:
                ChangeCurrentStatStateToNormal();
                break;
            case CurrentStatState.Dead:
                ChangeCurrentStatStateToDead();
                break;
        }
    }
    protected void ChangeCurrentStatStateToDamaged()
    {
        ResetAllAnimationParameters();

        CharacterControllerScript.ItemInHand?.GetComponent<HoldableItem>().OnHolderDamaged();
        CharacterControllerScript.ItemInHandLeft?.GetComponent<HoldableItem>().OnHolderDamaged();

        if(CharacterControllerScript.ItemInHand) CharacterControllerScript.ItemInHand.GetComponent<HoldableItem>().enabled = false;
        if(CharacterControllerScript.ItemInHandLeft) CharacterControllerScript.ItemInHandLeft.GetComponent<HoldableItem>().enabled = false;

        animator.Play("Damaged", 0, 0f);
        animator.SetTrigger("Damaged");
        CharacterControllerScript.enabled = false;
        this.statState = CurrentStatState.Damaged;
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
            }
        }
    }

    protected void ChangeCurrentStatStateToNormal()
    {
        CharacterControllerScript.enabled = true;
        if (CharacterControllerScript.ItemInHand) CharacterControllerScript.ItemInHand.GetComponent<HoldableItem>().enabled = true;
        if (CharacterControllerScript.ItemInHandLeft) CharacterControllerScript.ItemInHandLeft.GetComponent<HoldableItem>().enabled = true;
        this.statState = CurrentStatState.normal;
    }

    protected void ChangeCurrentStatStateToDead()
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
    }

    protected void TurnOnRigidbodyAndJointsForBodyParts()
    {
        foreach(var BodyPart in BodyParts)
        {
            Debug.Log("Enabling body part : " + BodyPart.name);
            BodyPart.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            BodyPart.GetComponent<CapsuleCollider2D>().enabled = true;

            if(BodyPart.name != "Torso")
            {
                BodyPart.GetComponent<HingeJoint2D>().enabled = true;
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
        if (CharacterControllerScript.ItemInHandLeft) CharacterControllerScript.DropItem(CharacterControllerScript.ItemInHandLeft);
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
            CurrentStatStateController(CurrentStatState.Dead);
        }
    }

    //-----------------------Receveing Stat Updates from Outside

    public bool TakeDamage(float damage, float damageDirectionHorizontal)
    {
        var ItemInHand = CharacterControllerScript.ItemInHand?.GetComponent<WeaponMelee>();
        var ItemInHandLeft = CharacterControllerScript.ItemInHandLeft?.GetComponent<Shield>();
        bool IsAttackingOrBlocking = 
            (ItemInHand && (ItemInHand.currentState == CurrentStateOfWeapon.Blocking || ItemInHand.currentState == CurrentStateOfWeapon.AttackingSecondary || ItemInHand.currentState == CurrentStateOfWeapon.AttackingPrimary)) 
            || (ItemInHandLeft && (ItemInHandLeft.currentState == CurrentStateOfWeapon.Blocking));

        if (CharacterControllerScript.IsHolding && IsAttackingOrBlocking && (gameObject.transform.localScale.x/3) == -damageDirectionHorizontal)
        {
            Health -= damage / 10;
            Stamina -= damage / 2;
            Mathf.Clamp(Health, 0, baseHealth);
            Mathf.Clamp(Stamina, 0, baseStamina);

            return false;
        }
        else
        {
            CurrentStatStateController(CurrentStatState.Damaged);
            DamageDirectionHorizontal = damageDirectionHorizontal;
            Health -= damage;
            Mathf.Clamp(Health, 0, baseHealth);

            return true;
        }
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
