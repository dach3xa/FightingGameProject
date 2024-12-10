using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class NPCCharacterControllerMeleeWeapon : NPCCharacterController
{
    abstract protected float CombatStateStartDistence { get; set; }
    protected float MaxDistenceToEnemyStop { get; set; }
    protected float MinDistenceToEnemyStop { get; set; }
    abstract protected float AttackCoolDownRangeMin { get; set; }
    abstract protected float AttackCoolDownRangeMax { get; set; }
    abstract protected float DistenceToEnemyStartBlocking { get; set; }
    protected float DistenceToEnemyStartAttacking { get; set; }
    abstract protected float BlockChance { get; set; }
    protected void Start()
    {
        base.Start();
    }

    // ----- checking for enemy during combat 
    protected override void CheckForEnemyCombat()
    {
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, 1f, DirectionToEnemy, CombatStateStartDistence, EnemyLayer.value & ~RaycastIgnoreLayer.value);
        DistenceToEnemy = Vector2.Distance(transform.position, EnemyFocused.transform.position);

        if (hit == false || hit.collider.gameObject != EnemyFocused)
        {
            StateManager(CurrentEnemyState.SawEnemy);
        }
    }


    //-------- Enemy Saw State Behvaiour -----
    protected override IEnumerator BehaviourControllerSawEnemy()
    {
        while (currentState == CurrentEnemyState.SawEnemy)
        {
            EnemySawBehaviour();
            yield return new WaitForSeconds(0.1f);
        }
    }
    protected override void EnemySawBehaviour()
    {
        if (!IsHolding)
        {
            GameObject[] MeleeWeapons = Items.Where(Item => Item.GetComponent<WeaponMelee>() != null).ToArray();
            TakeItem(MeleeWeapons[UnityEngine.Random.Range(0, MeleeWeapons.Length)].name);
            MaxDistenceToEnemyStop = (float)ItemInHand?.GetComponent<WeaponMelee>().HeightOfCollider * (1/2);
            MinDistenceToEnemyStop = (float)ItemInHand?.GetComponent<WeaponMelee>().HeightOfCollider * (1/4);
            DistenceToEnemyStartAttacking = (float)ItemInHand?.GetComponent<WeaponMelee>().HeightOfCollider;
        }
        ChangeMovePositionEnemySaw();
    }
    protected override void ChangeMovePositionEnemySaw()
    {
        MovePosition = new Vector2(EnemyFocused.transform.position.x, EnemyFocused.transform.position.y + 0.6f);
        Running = true;
    }

    //-------- Combat State Behvaiour -----
    protected override IEnumerator BehaviourControllerCombat()
    {
        float AttackCoolDownTimer = 0;
        while (currentState == CurrentEnemyState.Combat)
        {
            CombatBehaviour(ref AttackCoolDownTimer);
            yield return new WaitForSeconds(0.1f);
            AttackCoolDownTimer += 0.1f;
        }
    }
    protected override void CombatBehaviour(ref float AttackCoolDownTimer)
    {
        if (this.enabled)
        {
            ChangeMovePositionCombat();

            AttackPattern(ref AttackCoolDownTimer);
        }
    }
    protected override void ChangeMovePositionCombat()
    {
        if (DistenceToEnemy > MaxDistenceToEnemyStop)
        {
            MovePosition = new Vector2(EnemyFocused.transform.position.x, EnemyFocused.transform.position.y + 0.6f);
        }
        else if (DistenceToEnemy < MinDistenceToEnemyStop)
        {
            MovePosition = new Vector2(transform.position.x - transform.localScale.x, transform.position.y);
        }
        else
        {
            MovePosition = transform.position;
        }
    }

    protected override void AttackPattern(ref float AttackCoolDownTimer)
    {
        var currentWeapon = ItemInHand?.GetComponent<WeaponMelee>();
        var EnemyController = EnemyFocused.GetComponent<BaseCharacterController>();

        float AttackCoolDown = UnityEngine.Random.Range(AttackCoolDownRangeMin, AttackCoolDownRangeMax);
        var EnemyWeapon = EnemyFocused.GetComponent<BaseCharacterController>().ItemInHand?.GetComponent<WeaponMelee>();

        if (EnemyWeapon != null)
        {
            if (DistenceToEnemy < DistenceToEnemyStartBlocking && characterStatController.Stamina > 0 && EnemyController.IsAttackingCheck().Item2 )
            {
                block(currentWeapon);
            }
        }

        float AttackTypeChance = UnityEngine.Random.Range(0, 1f);
        if (DistenceToEnemy < DistenceToEnemyStartAttacking)
        {
            if (characterStatController.Stamina > 60f && AttackCoolDownTimer > AttackCoolDown && (AttackTypeChance > 0.3f || currentWeapon.currentComboAnimationAttackPrimary > 0))
            {
                currentWeapon.AttackPrimary();
                if (currentWeapon.currentComboAnimationAttackPrimary >= 3)
                {
                    AttackCoolDownTimer = 0;
                }
            }
            else if (characterStatController.Stamina > 30f && AttackCoolDownTimer > AttackCoolDown && AttackTypeChance <= 0.3f)
            {
                currentWeapon.AttackSecondary();
                AttackCoolDownTimer = 0;
            }

        }
    }

    private void block(WeaponMelee currentWeapon)
    {
        float RandomFloat = UnityEngine.Random.Range(0, 1.0f);

        if (BlockChance >= RandomFloat)
        {
            currentWeapon?.BlockStart();
        }
    }
}
