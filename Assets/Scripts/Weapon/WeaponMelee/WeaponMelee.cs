using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class WeaponMelee : MonoBehaviour
{
    public enum CurrentStateOfWeapon
    {
        None,
        AttackingPrimary,
        AttackingSecondary,
        Blocking
    }
    //info about Holder of this Weapon
    [SerializeField] protected GameObject Holder;
    [SerializeField] protected CharacterStatController HolderStatController;
    [SerializeField] protected BaseCharacterController HolderController;
    [SerializeField] protected SortingGroup HoldersSortingGroup;
    [SerializeField] protected Animator HoldersAnimator;
    [SerializeField] protected Dictionary<string, AudioSource> HoldersSoundEffects;

    //Weapon info
    [SerializeField] public CurrentStateOfWeapon currentState { get; set; }//change in future
    [SerializeField] protected float BaseAttackValue = 30f;
    [SerializeField] public float BaseStaminaReduceValue = 20f;
    [SerializeField] public float PrimaryAttackMultiplier = 1.2f;
    [SerializeField] public float SecondaryAttackMultiplier = 0.8f;

    //for creating a dynamic collider
    [SerializeField] protected float WidthOfWeapon = 0.3f;
    [SerializeField] protected float HieghtOfWeapon = 2f;
    [SerializeField] protected float WeaponOffsetAngle = 90f;
     
    //not to hit repeatedly
    [SerializeField] protected List<GameObject> EnemiesHitWhileInAttackState = new List<GameObject>();

    //combo Attack counter 
    public int currentComboAnimationAttackPrimary = 0;

    //cooldowns
    protected float comboCoolDownTimer = 0;
    protected float comboMaxTime = 0.6f;
    protected float ActionCoolDownTimer = 0;
    protected float ActionCoolDownBlock = 1.2f;
    protected float ActionCoolDownAttackPrimary = 1.2f;
    protected float ActionCoolDownAttackSecondary = 1.4f;
    protected void Start()
    {
        Holder = transform.root.gameObject;
        HolderStatController = Holder.GetComponent<CharacterStatController>();
        HolderController = Holder.GetComponent<BaseCharacterController>();
        HoldersSortingGroup = Holder.GetComponent<SortingGroup>();
        HoldersAnimator = Holder.GetComponent<Animator>();
        HoldersSoundEffects = HolderController.SoundEffects;
    }

    protected void UpdateTimers()
    {
        ActionCoolDownTimer += Time.deltaTime;
        comboCoolDownTimer += Time.deltaTime;
    }

    protected void CollisionWithWeaponInAttackStateCheck()
    {
        if (currentState == CurrentStateOfWeapon.AttackingPrimary || currentState == CurrentStateOfWeapon.AttackingSecondary)
        {
            float offsetAngle = WeaponOffsetAngle;
            float angle = transform.eulerAngles.z + offsetAngle; // Add an offset angle
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            RaycastHit2D EnemiesHit = Physics2D.CircleCast(transform.position, WidthOfWeapon, direction, HieghtOfWeapon, HolderController.EnemyLayer);
            if (EnemiesHit)
            {
                CheckEnemyHit(EnemiesHit.collider);
            }
        }
    }

    protected void CheckEnemyHit(Collider2D collision)
    {
        if (!EnemiesHitWhileInAttackState.Contains(collision.gameObject))
        {
            CharacterStatController enemyStats = collision.gameObject.GetComponent<CharacterStatController>();
            float damage = (currentState == CurrentStateOfWeapon.AttackingPrimary) ? BaseAttackValue * PrimaryAttackMultiplier : BaseAttackValue * SecondaryAttackMultiplier;
            if (!enemyStats.TakeDamage(damage, Holder.transform.localScale.x / 3))
            {
                HoldersAnimator.SetTrigger("Blocked");
                HoldersSoundEffects["WeaponMeleeHitSound"].pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                HoldersSoundEffects["WeaponMeleeHitSound"].Play();
                ActionCoolDownTimer = 0;
            }
            else
            {
                HoldersSoundEffects["WeaponMeleeDamageSound"].pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                HoldersSoundEffects["WeaponMeleeDamageSound"].Play();
            }
            EnemiesHitWhileInAttackState.Add(collision.gameObject);
        }
    }

    public void OnHolderDamaged()
    {
        Debug.Log("Holder was damaged!");
        currentComboAnimationAttackPrimary = 0;
        ActionCoolDownTimer = -0.4f;
        comboCoolDownTimer = 0;
    }

    protected void ResetComboCheck()
    {
        if (currentComboAnimationAttackPrimary > 0 && comboCoolDownTimer > comboMaxTime)
        {
            currentComboAnimationAttackPrimary = 0;
            currentState = CurrentStateOfWeapon.None;
        }
        HoldersAnimator.SetInteger("WeaponMeleePrimaryAttackComboCount", currentComboAnimationAttackPrimary);
    }

    //------------Actions----------------------------
    public void AttackPrimary()
    {
        if ((comboCoolDownTimer <= comboMaxTime && currentComboAnimationAttackPrimary < 3 || ActionCoolDownTimer > ActionCoolDownAttackPrimary) && HolderStatController.Stamina > 20f * 1.2f)
        {
            HoldersAnimator.SetInteger("WeaponMeleePrimaryAttackComboCount", ++currentComboAnimationAttackPrimary);

            ActionCoolDownTimer = 0;
            comboCoolDownTimer = 0;
        }
    }
    public void AttackSecondary()
    {
        if (ActionCoolDownTimer > ActionCoolDownAttackSecondary && HolderStatController.Stamina > 20f * 0.8f && currentState == CurrentStateOfWeapon.None)
        {
            HoldersAnimator.SetTrigger("WeaponMeleeSecondaryAttack");

            ActionCoolDownTimer = 0;
        }
    }

    public void CancelAttack()
    {
        if (currentState == CurrentStateOfWeapon.None && (currentComboAnimationAttackPrimary != 0 || ActionCoolDownTimer <= 0.5f))
        {
            HoldersAnimator.SetTrigger("StopAttack");
            StartCoroutine(ResetTriggerCoroutine("StopAttack"));
            currentComboAnimationAttackPrimary = 0;
            ActionCoolDownTimer = 0.9f;
        }
    }

    protected IEnumerator ResetTriggerCoroutine(string triggerName)
    {
        yield return new WaitForSeconds(0.2f);
        HoldersAnimator.ResetTrigger(triggerName);
    }

    public void Block()
    {
        if (ActionCoolDownTimer > ActionCoolDownBlock && HolderStatController.Stamina > 0 && currentState == CurrentStateOfWeapon.None)
        {
            HoldersAnimator.SetTrigger("Block");
            ActionCoolDownTimer = -0.3f;
        }
    }

    //-------------------Animation Events-----------------

    public void AttackStateStartPrimary()
    {
        HoldersSortingGroup.sortingOrder = 1;
        currentState = CurrentStateOfWeapon.AttackingPrimary;
        HoldersSoundEffects["WeaponMeleeSlashSound"].pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        HoldersSoundEffects["WeaponMeleeSlashSound"].Play();
    }

    public void AttackStateStartSecondary()
    {
        HoldersSortingGroup.sortingOrder = 1;
        currentState = CurrentStateOfWeapon.AttackingSecondary;
        HoldersSoundEffects["WeaponMeleeThrustSound"].pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        HoldersSoundEffects["WeaponMeleeThrustSound"].Play();
    }

    public void AttackStateEnd()
    {
        currentState = CurrentStateOfWeapon.None;
        EnemiesHitWhileInAttackState.Clear();
        HoldersSortingGroup.sortingOrder = 0;
    }

    public void BlockStateStart()
    {
        currentState = CurrentStateOfWeapon.Blocking;
    }

    public void BlockStateEnd()
    {
        currentState = CurrentStateOfWeapon.None;
    }

}
