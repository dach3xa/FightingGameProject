using UnityEngine;

public class TwoHandedFist : WeaponMelee
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected float CoolDownStopsHolding = 4f;
    void Start()
    {
        base.Start();
        IsTwoHanded = true;

        //attack and stamina
        BaseAttackValue = 8f;
        BaseStaminaReduceValue = 12f;

        //cooldowns
        ActionCoolDownBlock = 1.1f;
        ActionCoolDownAttackPrimary = 1.25f;
        ActionCoolDownAttackSecondary = 1.25f;
        comboMaxTime = 0.6f;

        //define weapon collider
        WidthOfWeapon = 0.25f;
        HeightOfWeapon = 0.5f;
        WeaponOffsetAngle = 90f;
    }

    void Update()
    {
        UpdateTimers();
        ResetComboCheck();
        CollisionWithWeaponInAttackStateCheck();

        StopHoldingCheck();
    }

    private void StopHoldingCheck()
    {
        if (ActionCoolDownTimer > CoolDownStopsHolding)
        {
            HoldersAnimator.Play("HoldingEnd", HolderController.CurrentAnimatorHoldingLayerRight, 0f);
            HolderController.DisablePreviousItem(gameObject);
        }
    }
}
