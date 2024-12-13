using System.Collections.Generic;
using UnityEngine;

public class Leg : PrimaryAttackable
{
    protected override Dictionary<int, string> AnimationStateNamesAttack { get; set; } = new Dictionary<int, string>
    {
        { Animator.StringToHash("LegKick"), "LegKick" },
    };
    void Start()
    {
        CurrentItemType = ItemType.Legs;
        base.Start();

        ActionCoolDownAttackPrimary = 2f;
    }

    void Update()
    {
        UpdateTimers();
        CollisionWithWeaponInAttackStateCheck();

        StopMovingWhenAttack();
    }

    private void StopMovingWhenAttack()
    {
        //Debug.Log(HolderController.StopMoving);
        //Debug.Log(PlayingAttackAnimationCheck.Item2);
        //Debug.Log(PlayingAttackAnimationCheck.Item1);
        if(HolderController.StopMoving == false && PlayingAttackAnimationCheck.Item2)
        {
            HolderController.StopMoving = true;
        }
        else if(HolderController.StopMoving == true && !PlayingAttackAnimationCheck.Item2)
        {
            HolderController.StopMoving = false;
        }
    }

    override protected void ResetAttackPrimary()
    {
        HoldersAnimator.SetBool("Kick", false);
        CurrentState = CurrentStateOfAction.None;
    }

    public override bool AttacksClashed(GameObject EnemyWeapon)
    {
        if (EnemyWeapon.GetComponent<UsableObject>() is Leg)
        {
            EnemiesHitWhileInAttackState.Add(EnemyWeapon.transform.root.gameObject);
            HoldersAnimator.SetTrigger("Blocked");
            ResetAttackPrimary();
            return true;
        }
        else
        {
            EnemiesHitWhileInAttackState.Add(EnemyWeapon.transform.root.gameObject);
            HoldersAnimator.SetTrigger("Blocked");
            ResetAttackPrimary();
            return false;
        }
    }
    public override void AttackPrimary()
    {
        if (ActionCoolDownTimer > ActionCoolDownAttackPrimary && HolderStatController.Stamina > 20f * 1.2f)
        {
            Debug.Log("Kick!");
            Debug.Log(ActionCoolDownTimer + " : " + ActionCoolDownAttackPrimary);

            HoldersAnimator.SetBool("Kick", true);
            ActionCoolDownTimer = 0;
        }
    }

    //---animation events--
    public override void AttackStateStartPrimary()
    {
        HoldersSortingGroup.sortingOrder = 2;
        CurrentState = CurrentStateOfAction.AttackingPrimary;
        SoundEffects["WeaponMeleeSlashSound"].pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        SoundEffects["WeaponMeleeSlashSound"].Play();
        ActionCoolDownTimer = 0;
    }
    public override void AttackStateEnd()
    {
        CurrentState = CurrentStateOfAction.None;
        EnemiesHitWhileInAttackState.Clear();
        HoldersSortingGroup.sortingOrder = 0;

        ResetAttackPrimary();
    }

}
