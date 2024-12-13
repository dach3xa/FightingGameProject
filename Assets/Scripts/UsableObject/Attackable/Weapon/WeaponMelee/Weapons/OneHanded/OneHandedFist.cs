using System.Collections;
using UnityEngine;
public class OneHandedFist : WeaponMelee
{
    protected float CoolDownStopsHolding = 4f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.Start();
        IsTwoHanded = false;
        //attack and stamina
        BaseAttackValue = 8f;
        BaseStaminaReduceValue = 12f;

        //cooldowns
        ActionCoolDownBlock = 1.1f;
        ActionCoolDownAttackPrimary = 1.3f;
        ActionCoolDownAttackSecondary = 1.3f;
        comboMaxTime = 0.8f;

        //define weapon collider
        WidthOfCollider = 0.3f;
        HeightOfCollider = 0.5f;
        ColliderOffsetAngle = 90f;

        ActionCoolDownTimer = 2f;
    }
    private void OnEnable()
    {
        ActionCoolDownTimer = 2f;
    }
    // Update is called once per frame
    void Update()
    {
        Debug.Log(ActionCoolDownTimer);

        UpdateTimers();
        ResetComboCheck();
        CollisionWithWeaponInAttackStateCheck();

        StopHoldingCheck();
    }

    protected void StopHoldingCheck()
    {
        if (ActionCoolDownTimer > CoolDownStopsHolding)
        {
            HoldersAnimator.Play("HoldingEnd", AnimationLayer, 0f);

            ActionCoolDownTimer = 0f;

            StartCoroutine(DisableFistsCoroutine());
        }
    }

    IEnumerator DisableFistsCoroutine()
    {
        yield return new WaitForSeconds(0.45f);
        if (ActionCoolDownTimer > 0.43f)
        {
            HolderController.DisablePreviousItem(gameObject);

        }
    }

    //---------Weapon Clashed-----------------
    public override bool AttacksClashed(GameObject EnemyWeapon)
    {
        Debug.Log("TwoHandedFist Weaponsclashed called!");
        Debug.Log(EnemyWeapon.GetComponent<UsableObject>());

        if (EnemyWeapon.GetComponent<UsableObject>() is OneHandedFist)
        {
            HoldersAnimator.SetTrigger("Blocked");
            EnemiesHitWhileInAttackState.Add(EnemyWeapon);
            ResetAttackPrimary();
            return true;
        }
        else
        {
            HoldersAnimator.SetTrigger("Blocked");
            ResetAttackPrimary();
            return false;
        }
    }

    //-------block impact----------------------

    public override bool BlockImpact(GameObject AttackingWeapon)
    {
        if (AttackingWeapon.GetComponent<UsableObject>() is OneHandedFist || AttackingWeapon.GetComponent<UsableObject>() is Legs)
        {
            HoldersAnimator.Play("BlockImpact", AnimationLayer);
            return true;
        }
        return false;
    }
}
