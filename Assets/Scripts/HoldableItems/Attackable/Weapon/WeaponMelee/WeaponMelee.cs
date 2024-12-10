using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
public enum CurrentStateOfAction
{
    None,
    AttackingPrimary,
    AttackingSecondary,
    Blocking
}
public abstract class WeaponMelee : PrimaryAttackable, IBlockable
{
    //Weapon info
    public virtual float SecondaryAttackMultiplier { get; protected set; } = 0.8f;

    //combo Attack counter 
    public int currentComboAnimationAttackPrimary { get; protected set; } = 0;
    public int currentPlayingComboAnimationAttackPrimary { get; protected set; }  = 0;

    //cooldowns
    public float comboCoolDownTimer { get; protected set; } = 0;
    public float comboMaxTime { get; protected set; } = 0.8f;

    public float ActionCoolDownBlock { get; protected set; } = 1.1f;
    public float ActionCoolDownAttackSecondary { get; protected set; } = 1.3f;

    public override bool IsAttacking { get { return CurrentState == CurrentStateOfAction.AttackingPrimary || CurrentState == CurrentStateOfAction.AttackingSecondary; } }

    public override float Damage
    {
        get
        {
            if (CurrentState == CurrentStateOfAction.AttackingPrimary)
            {
                return BaseAttackValue * PrimaryAttackMultiplier;
            }
            else if(CurrentState == CurrentStateOfAction.AttackingSecondary)
            {
                return BaseAttackValue * SecondaryAttackMultiplier;
            }

            return 0;
        }
    }
    protected void Start()
    {
        base.Start();

        BaseAttackValue = 30f;
        BaseStaminaReduceValue = 20f;
        PrimaryAttackMultiplier = 1.2f;
        SecondaryAttackMultiplier = 0.8f;

        WidthOfCollider = 0.3f;
        HeightOfCollider = 2f;
        ColliderOffsetAngle = 90f;

        comboCoolDownTimer = 0;
        comboMaxTime = 0.8f;

        ActionCoolDownTimer = 0;
        ActionCoolDownBlock = 1.1f;
        ActionCoolDownAttackPrimary = 1.3f;
        ActionCoolDownAttackSecondary = 1.3f;
    }

    protected override void UpdateTimers()
    {
        ActionCoolDownTimer += Time.deltaTime;
        comboCoolDownTimer += Time.deltaTime;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        // Draw the starting circle
        Gizmos.DrawWireSphere(transform.position, WidthOfCollider);

        // Draw the ending circle
        float offsetAngle = ColliderOffsetAngle;
        float angle = transform.eulerAngles.z + offsetAngle; // Add an offset angle
        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        Vector2 endPosition = new Vector2(transform.position.x, transform.position.y) + direction.normalized * HeightOfCollider;
        Gizmos.DrawWireSphere(endPosition, WidthOfCollider);

        // Draw lines connecting the two circles
        Vector3 offset = Vector3.right * WidthOfCollider; // This creates a perpendicular vector
        Vector3 originRight = (Vector3)transform.position + offset;
        Vector3 endRight = (Vector3)endPosition + offset;

        Gizmos.DrawLine(transform.position + (Vector3)direction * WidthOfCollider, (Vector3)endPosition + (Vector3)direction * WidthOfCollider);
    }

    public override bool AttacksClashed(GameObject EnemyWeapon)
    {
        if(!(EnemyWeapon.GetComponent<UsableObject>() is TwoHandedFist || EnemyWeapon.GetComponent<UsableObject>() is Leg))
        {
            HoldersAnimator.SetTrigger("Blocked");
            Debug.Log("Melee Weapon Weapons clashed called!");
            ResetAttackPrimary();
        }
        else
        {
            float damage = (CurrentState == CurrentStateOfAction.AttackingPrimary) ? BaseAttackValue * PrimaryAttackMultiplier : BaseAttackValue * SecondaryAttackMultiplier;
            if (EnemyWeapon.transform.root.gameObject.GetComponent<CharacterStatController>().RecieveAttack(damage, gameObject))
            {
                SoundEffects["WeaponMeleeDamageSound"].pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                SoundEffects["WeaponMeleeDamageSound"].Play();
            }
        }
        EnemiesHitWhileInAttackState.Add(EnemyWeapon.transform.root.gameObject);
        return true;
    }
    protected void ResetComboCheck()
    {
        AnimatorStateInfo stateInfo = HoldersAnimator.GetCurrentAnimatorStateInfo(HolderController.CurrentAnimatorHoldingLayerRight);
        if ((currentComboAnimationAttackPrimary > 0 && comboCoolDownTimer >= comboMaxTime))
        {
            Debug.Log("Resetting combo!");
            Debug.Log("comboCoolDownTimer : " + comboCoolDownTimer);
            ResetAttackPrimary();
        }
    }

    protected override void ResetAttackPrimary()
    {
        currentPlayingComboAnimationAttackPrimary = 0;
        currentComboAnimationAttackPrimary = 0;
        CurrentState = CurrentStateOfAction.None;
        HoldersAnimator.SetInteger("WeaponMeleePrimaryAttackComboCount", currentComboAnimationAttackPrimary);
    }

    //------------Actions----------------------------
    public override void AttackPrimary()
    {
        if ((comboCoolDownTimer <= comboMaxTime && currentComboAnimationAttackPrimary < 3 && currentComboAnimationAttackPrimary > 0 || ActionCoolDownTimer > ActionCoolDownAttackPrimary) && HolderStatController.Stamina > 20f * 1.2f)
        {
            Debug.Log("Attacking primary!");
            HoldersAnimator.SetInteger("WeaponMeleePrimaryAttackComboCount", ++currentComboAnimationAttackPrimary);
            ActionCoolDownTimer = 0;
            comboCoolDownTimer = 0;
        }
    }

    public void AttackSecondary()
    {
        if (ActionCoolDownTimer >= ActionCoolDownAttackSecondary && HolderStatController.Stamina > 20f * 0.8f && CurrentState == CurrentStateOfAction.None)
        {
            Debug.Log(ActionCoolDownTimer);
            ActionCoolDownTimer = 0;
            HoldersAnimator.SetTrigger("WeaponMeleeSecondaryAttack");
        }
    }

    public void CancelAttack()
    {
        Debug.Log("Trying to Cancel the event!");
        AnimatorStateInfo HolderAnimatorStateInfo = HoldersAnimator.GetCurrentAnimatorStateInfo(HolderController.CurrentAnimatorHoldingLayerRight);
        if (CurrentState == CurrentStateOfAction.None && currentComboAnimationAttackPrimary < 2 && (HolderAnimatorStateInfo.IsName("PrimaryAttack") || HolderAnimatorStateInfo.IsName("SecondaryAttack")))
        {
            HoldersAnimator.SetBool("StopAttack", true);
            StartCoroutine(ResetBool("StopAttack"));

            currentComboAnimationAttackPrimary = 0;
            currentPlayingComboAnimationAttackPrimary = 0;
            ActionCoolDownTimer = 0.8f;
        }
    }

    protected IEnumerator ResetBool(string triggerName)
    {
        yield return new WaitForSeconds(0.4f);
        HoldersAnimator.ResetTrigger(triggerName);
    }

    public void BlockStart()
    {
        if (ActionCoolDownTimer >= ActionCoolDownBlock && HolderStatController.Stamina > 0 && CurrentState == CurrentStateOfAction.None)
        {
            HoldersAnimator.SetTrigger("Block");
            ActionCoolDownTimer = -0.3f;
        }
    }

    public virtual bool BlockImpact(GameObject AttackingWeapon)
    {
        HoldersAnimator.SetTrigger("Blocked");
        return true;
    }

    public void BlockEnd()
    {
        return;//only shields use this mechanic
    }

    //-------------------Animation Events-----------------

    public override void AttackStateStartPrimary()
    {
        HoldersSortingGroup.sortingOrder = 2;
        CurrentState = CurrentStateOfAction.AttackingPrimary;
        SoundEffects["WeaponMeleeSlashSound"].pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        SoundEffects["WeaponMeleeSlashSound"].Play();

        if (currentPlayingComboAnimationAttackPrimary < currentComboAnimationAttackPrimary)
        {
            currentPlayingComboAnimationAttackPrimary++;
        }

        ActionCoolDownTimer = 0;
        comboCoolDownTimer = 0;
    }

    public void AttackStateStartSecondary()
    {
        HoldersSortingGroup.sortingOrder = 2;
        CurrentState = CurrentStateOfAction.AttackingSecondary;
        SoundEffects["WeaponMeleeThrustSound"].pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        SoundEffects["WeaponMeleeThrustSound"].Play();
        ActionCoolDownTimer = 0;
    }

    public override void AttackStateEnd()
    {
        CurrentState = CurrentStateOfAction.None;
        EnemiesHitWhileInAttackState.Clear();
        HoldersSortingGroup.sortingOrder = 0;

        if (currentPlayingComboAnimationAttackPrimary == 3 || currentPlayingComboAnimationAttackPrimary == currentComboAnimationAttackPrimary)
        {
            ResetAttackPrimary();
        }
    }

    public void BlockStateStart()
    {
        CurrentState = CurrentStateOfAction.Blocking;
    }

    public void BlockStateEnd()
    {
        CurrentState = CurrentStateOfAction.None;
    }

}
