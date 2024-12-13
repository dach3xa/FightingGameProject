using UnityEngine;
using System.Collections.Generic;

public abstract class PrimaryAttackable : UsableObject, IAttackablePrimary
{
    public CurrentStateOfAction CurrentState { get; set; }
    public float BaseAttackValue { get; set; } = 14f;
    public float BaseStaminaReduceValue { get; set; } = 20f;
    public float PrimaryAttackMultiplier { get; set; } = 1.2f;
    public float WidthOfCollider { get; set; } = 0.3f;
    public float HeightOfCollider { get; set; } = 0.5f;
    public float ColliderOffsetAngle { get; set; } = 90f;
    public List<GameObject> EnemiesHitWhileInAttackState { get; set; } = new List<GameObject>();
    public float ActionCoolDownTimer { get; set; } = 0;
    public float ActionCoolDownAttackPrimary { get; set; } = 2f;
    public virtual bool IsAttacking { get { return CurrentState == CurrentStateOfAction.AttackingPrimary; } }

    public virtual float Damage { get
        {
            if(CurrentState == CurrentStateOfAction.AttackingPrimary)
            {
                return BaseAttackValue * PrimaryAttackMultiplier;
            }
            return 0;
        }
    }

    public void Start()
    {
        base.Start();
    }

    virtual protected void UpdateTimers()
    {
        ActionCoolDownTimer += Time.deltaTime;
    }

    protected void CollisionWithWeaponInAttackStateCheck()
    {
        if (IsAttacking)
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
        float offsetAngle = ColliderOffsetAngle;
        float angle = transform.eulerAngles.z + offsetAngle; // Add an offset angle
        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        RaycastHit2D EnemiesHit = Physics2D.CircleCast(transform.position, WidthOfCollider, direction, HeightOfCollider, HolderController.EnemyLayer);
        return EnemiesHit.collider;
    }

    protected void CheckEnemyHit(Collider2D collision)
    {
        if (!EnemiesHitWhileInAttackState.Contains(collision.gameObject))
        {
            CharacterStatController enemyStats = collision.gameObject.GetComponent<CharacterStatController>();

            if (!enemyStats.RecieveAttack(Damage, gameObject))
            {
                HoldersAnimator.SetTrigger("Blocked");
                SoundEffects["WeaponMeleeHitSound"].pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                SoundEffects["WeaponMeleeHitSound"].Play();
                ActionCoolDownTimer = 0;

                ResetAttackPrimary();
            }
            else
            {
                SoundEffects["WeaponMeleeDamageSound"].pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                SoundEffects["WeaponMeleeDamageSound"].Play();
            }

            EnemiesHitWhileInAttackState.Add(collision.gameObject);
        }
    }

    abstract protected void ResetAttackPrimary();

    public virtual bool AttacksClashed(GameObject EnemyWeapon)
    {
        if (!(EnemyWeapon.GetComponent<UsableObject>() is TwoHandedFist || EnemyWeapon.GetComponent<UsableObject>() is Legs))
        {
            HoldersAnimator.SetTrigger("Blocked");
            ResetAttackPrimary();
        }
        else
        {
            if (EnemyWeapon.transform.root.gameObject.GetComponent<CharacterStatController>().RecieveAttack(Damage, gameObject))
            {
                SoundEffects["WeaponMeleeDamageSound"].pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                SoundEffects["WeaponMeleeDamageSound"].Play();
            }
        }
        EnemiesHitWhileInAttackState.Add(EnemyWeapon.transform.root.gameObject);
        return true;
    }
    public abstract void AttackPrimary();

    public override void OnHolderDamaged()
    {
        ResetAttackPrimary();

        ActionCoolDownTimer = 0.5f;
    }

    //---animation events--
    public abstract void AttackStateStartPrimary();
    public abstract void AttackStateEnd();

}
