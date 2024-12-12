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
    //---------taking an item--------------------

    protected override void TakeItem(string ItemName)
    {
        base.TakeItem(ItemName);

        MaxDistenceToEnemyStop = (float)ItemInHand?.GetComponent<WeaponMelee>().HeightOfCollider * (1 / 2);
        MinDistenceToEnemyStop = (float)ItemInHand?.GetComponent<WeaponMelee>().HeightOfCollider * (1 / 3);

        float WeaponDistence = (float)ItemInHand?.GetComponent<WeaponMelee>().HeightOfCollider * 2;
        DistenceToEnemyStartAttacking = (WeaponDistence > 1.5f) ? (float)ItemInHand?.GetComponent<WeaponMelee>().HeightOfCollider : 1.5f;
        Debug.Log(DistenceToEnemyStartAttacking);
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

            ActionPattern(ref AttackCoolDownTimer);
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

    protected override void ActionPattern(ref float AttackCoolDownTimer)
    {
        var currentWeapon = ItemInHand?.GetComponent<WeaponMelee>();
        var EnemyController = EnemyFocused.GetComponent<BaseCharacterController>();

        var EnemyWeapon = EnemyController.ItemInHand?.GetComponent<WeaponMelee>();
        var EnemyShield = EnemyController.ItemInHandLeft?.GetComponent<Shield>();

        BlockPattern(EnemyWeapon, EnemyController);

        AttackPattern(EnemyShield, currentWeapon, ref AttackCoolDownTimer);
    }

    protected void BlockPattern(WeaponMelee EnemyWeapon, BaseCharacterController EnemyController)
    {
        if (EnemyWeapon != null)
        {
            if (DistenceToEnemy < DistenceToEnemyStartBlocking && characterStatController.Stamina > 0 && EnemyWeapon.PlayingAttackAnimationCheck.Item2)
            {
                float RandomFloat = UnityEngine.Random.Range(0, 1.0f);

                if (BlockChance >= RandomFloat)
                {
                    StartBlocking();
                }
            }
        }
    }

    private void AttackPattern( Shield EnemyShield, WeaponMelee currentWeapon, ref float AttackCoolDownTimer)
    {
        float AttackCoolDown = UnityEngine.Random.Range(AttackCoolDownRangeMin, AttackCoolDownRangeMax);
        float AttackTypeChance = UnityEngine.Random.Range(0, 1f);

        if (DistenceToEnemy < DistenceToEnemyStartAttacking)
        {

            if ((EnemyShield && EnemyShield.CurrentState == CurrentStateOfAction.Blocking))
            {
                Kick();
                AttackCoolDownTimer = 1f;
            }
            else if (currentWeapon && characterStatController.Stamina > 60f && AttackCoolDownTimer > AttackCoolDown && (AttackTypeChance > 0.3f || currentWeapon.currentComboAnimationAttackPrimary > 0))
            {

                Attack("Primary");
                if (currentWeapon.currentComboAnimationAttackPrimary >= 3)
                {
                    AttackCoolDownTimer = 0;
                }
            }
            else if (currentWeapon && characterStatController.Stamina > 30f && AttackCoolDownTimer > AttackCoolDown && AttackTypeChance <= 0.3f)
            {

                Attack("Secondary");
                AttackCoolDownTimer = 0;
            }

        }
    }
}
