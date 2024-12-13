using UnityEngine;

public class OneHandedSword : WeaponMelee
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrentItemType = ItemType.OneHandedWeaponSharp;
        base.Start();
        IsTwoHanded = false;

        //attack and stamina
        BaseAttackValue = 20f;
        BaseStaminaReduceValue = 15f;

        //cooldowns
        ActionCoolDownBlock = 1.1f;
        ActionCoolDownAttackPrimary = 1.25f;
        ActionCoolDownAttackSecondary = 1.25f;
        comboMaxTime = 0.6f;

        //define weapon collider
        WidthOfCollider = 0.25f;
        HeightOfCollider = 1.5f;
        ColliderOffsetAngle = 90f;
    }

    void Update()
    {
        UpdateTimers();
        ResetComboCheck();
        CollisionWithWeaponInAttackStateCheck();
    }
}
