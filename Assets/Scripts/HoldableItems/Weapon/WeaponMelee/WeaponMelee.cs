using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
public enum CurrentStateOfWeapon
{
    None,
    AttackingPrimary,
    AttackingSecondary,
    Blocking
}
public abstract class WeaponMelee : HoldableItem, IBlockable
{
    //Weapon info
    [SerializeField] public CurrentStateOfWeapon currentState { get; set; }//change in future
    [SerializeField] protected float BaseAttackValue = 30f;
    [SerializeField] public float BaseStaminaReduceValue = 20f;
    [SerializeField] public float PrimaryAttackMultiplier = 1.2f;
    [SerializeField] public float SecondaryAttackMultiplier = 0.8f;

    //for creating a dynamic collider
    [SerializeField] protected float WidthOfWeapon = 0.3f;
    [SerializeField] protected float HeightOfWeapon = 2f;
    [SerializeField] protected float WeaponOffsetAngle = 90f;
     
    //not to hit repeatedly
    [SerializeField] protected List<GameObject> EnemiesHitWhileInAttackState = new List<GameObject>();

    //combo Attack counter 
    [SerializeField] public int currentComboAnimationAttackPrimary = 0;
    [SerializeField] protected int currentPlayingComboAnimationAttackPrimary = 0;

    //cooldowns
    [SerializeField] protected float comboCoolDownTimer = 0;
    [SerializeField] protected float comboMaxTime = 0.8f;

    [SerializeField] protected float ActionCoolDownTimer = 0;
    [SerializeField] protected float ActionCoolDownBlock = 1.1f;
    [SerializeField] protected float ActionCoolDownAttackPrimary = 1.3f;
    [SerializeField] protected float ActionCoolDownAttackSecondary = 1.3f;
    protected void Start()
    {
        base.Start();
    }

    protected void UpdateTimers()
    {
        ActionCoolDownTimer += Time.deltaTime;
        comboCoolDownTimer += Time.deltaTime;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        // Draw the starting circle
        Gizmos.DrawWireSphere(transform.position, WidthOfWeapon);

        // Draw the ending circle
        float offsetAngle = WeaponOffsetAngle;
        float angle = transform.eulerAngles.z + offsetAngle; // Add an offset angle
        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        Vector2 endPosition = new Vector2(transform.position.x,transform.position.y) + direction.normalized * HeightOfWeapon;
        Gizmos.DrawWireSphere(endPosition, WidthOfWeapon);

        // Draw lines connecting the two circles
        Vector3 offset = Vector3.right * WidthOfWeapon; // This creates a perpendicular vector
        Vector3 originRight = (Vector3)transform.position + offset;
        Vector3 endRight = (Vector3)endPosition + offset;

        Gizmos.DrawLine(transform.position + (Vector3)direction * WidthOfWeapon, (Vector3)endPosition + (Vector3)direction * WidthOfWeapon);
    }

    protected void CollisionWithWeaponInAttackStateCheck()
    {
        if (currentState == CurrentStateOfWeapon.AttackingPrimary || currentState == CurrentStateOfWeapon.AttackingSecondary)
        {
            var EnemyHitColliders = DrawingCollider();

            if (EnemyHitColliders)
            {
                CheckEnemyHit(EnemyHitColliders);
            }
        }
    }

    virtual protected Collider2D DrawingCollider()
    {
        float offsetAngle = WeaponOffsetAngle;
        float angle = transform.eulerAngles.z + offsetAngle; // Add an offset angle
        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        RaycastHit2D EnemiesHit = Physics2D.CircleCast(transform.position, WidthOfWeapon, direction, HeightOfWeapon, HolderController.EnemyLayer);
        return EnemiesHit.collider;
    }

    protected void CheckEnemyHit(Collider2D collision)
    {
        if (!EnemiesHitWhileInAttackState.Contains(collision.gameObject))
        {
            CharacterStatController enemyStats = collision.gameObject.GetComponent<CharacterStatController>();
            float damage = (currentState == CurrentStateOfWeapon.AttackingPrimary) ? BaseAttackValue * PrimaryAttackMultiplier : BaseAttackValue * SecondaryAttackMultiplier;
            if (!enemyStats.RecieveAttack(damage, gameObject))
            {
                HoldersAnimator.SetTrigger("Blocked");
                SoundEffects["WeaponMeleeHitSound"].pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                SoundEffects["WeaponMeleeHitSound"].Play();
                ActionCoolDownTimer = 0;

                ResetCombo();
            }
            else
            {
                SoundEffects["WeaponMeleeDamageSound"].pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                SoundEffects["WeaponMeleeDamageSound"].Play();
            }
            EnemiesHitWhileInAttackState.Add(collision.gameObject);
        }
    }

    public virtual bool WeaponsClashed(GameObject EnemyWeapon)
    {
        HoldersAnimator.SetTrigger("Blocked");
        EnemiesHitWhileInAttackState.Add(EnemyWeapon.transform.root.gameObject);

        ResetCombo();
        return true;
    }

    public override void OnHolderDamaged()
    {
        ResetCombo();

        ActionCoolDownTimer = 0.5f;
    }

    protected void ResetComboCheck()
    {
        AnimatorStateInfo stateInfo = HoldersAnimator.GetCurrentAnimatorStateInfo(HolderController.CurrentAnimatorHoldingLayerRight);
        if ((currentComboAnimationAttackPrimary > 0 && comboCoolDownTimer >= comboMaxTime))
        {
            Debug.Log("Resetting combo!");
            Debug.Log("comboCoolDownTimer : " + comboCoolDownTimer);
            ResetCombo();
        }
    }

    protected void ResetCombo()
    {
        currentPlayingComboAnimationAttackPrimary = 0;
        currentComboAnimationAttackPrimary = 0;
        currentState = CurrentStateOfWeapon.None;
        HoldersAnimator.SetInteger("WeaponMeleePrimaryAttackComboCount", currentComboAnimationAttackPrimary);
    }

    //------------Actions----------------------------
    public void AttackPrimary()
    {
        if ((comboCoolDownTimer <= comboMaxTime && currentComboAnimationAttackPrimary < 3 && currentComboAnimationAttackPrimary > 0|| ActionCoolDownTimer > ActionCoolDownAttackPrimary) && HolderStatController.Stamina > 20f * 1.2f)
        {
            Debug.Log("Attacking primary!");
            HoldersAnimator.SetInteger("WeaponMeleePrimaryAttackComboCount", ++currentComboAnimationAttackPrimary);
            ActionCoolDownTimer = 0;
            comboCoolDownTimer = 0;
        }
    }

    public void AttackSecondary()
    {
        if (ActionCoolDownTimer >= ActionCoolDownAttackSecondary && HolderStatController.Stamina > 20f * 0.8f && currentState == CurrentStateOfWeapon.None)
        {
            HoldersAnimator.SetTrigger("WeaponMeleeSecondaryAttack");
        }
    }

    public void CancelAttack()
    {
        Debug.Log("Trying to Cancel the event!");
        AnimatorStateInfo HolderAnimatorStateInfo = HoldersAnimator.GetCurrentAnimatorStateInfo(HolderController.CurrentAnimatorHoldingLayerRight);
        if (currentState == CurrentStateOfWeapon.None && currentComboAnimationAttackPrimary < 2 && (HolderAnimatorStateInfo.IsName("PrimaryAttack") || HolderAnimatorStateInfo.IsName("SecondaryAttack")))
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
        if (ActionCoolDownTimer >= ActionCoolDownBlock && HolderStatController.Stamina > 0 && currentState == CurrentStateOfWeapon.None)
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

    public void AttackStateStartPrimary()
    {
        HoldersSortingGroup.sortingOrder = 1;
        currentState = CurrentStateOfWeapon.AttackingPrimary;
        SoundEffects["WeaponMeleeSlashSound"].pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        SoundEffects["WeaponMeleeSlashSound"].Play();

        if(currentPlayingComboAnimationAttackPrimary < currentComboAnimationAttackPrimary)
        {
            currentPlayingComboAnimationAttackPrimary++;
        }

        ActionCoolDownTimer = 0;
        comboCoolDownTimer = 0;
    }

    public void AttackStateStartSecondary()
    {
        HoldersSortingGroup.sortingOrder = 1;
        currentState = CurrentStateOfWeapon.AttackingSecondary;
        SoundEffects["WeaponMeleeThrustSound"].pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        SoundEffects["WeaponMeleeThrustSound"].Play();
        ActionCoolDownTimer = 0;
    }

    public void AttackStateEnd()
    {
        currentState = CurrentStateOfWeapon.None;
        EnemiesHitWhileInAttackState.Clear();
        HoldersSortingGroup.sortingOrder = 0;

        if(currentPlayingComboAnimationAttackPrimary == 3)
        {
            currentComboAnimationAttackPrimary = 0;
            currentPlayingComboAnimationAttackPrimary = 0;

            HoldersAnimator.SetInteger("WeaponMeleePrimaryAttackComboCount", currentComboAnimationAttackPrimary);
            Debug.Log("Current playing combo animation is 3 resetting!");
        }
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
