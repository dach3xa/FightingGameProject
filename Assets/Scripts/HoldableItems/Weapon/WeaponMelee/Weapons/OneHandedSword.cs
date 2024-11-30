using UnityEngine;

public class OneHandedSword : WeaponMelee
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.Start();
        IsTwoHanded = false;

        //attack and stamina
        BaseAttackValue = 20f;
        BaseStaminaReduceValue = 15f;

        //cooldowns
        ActionCoolDownBlock = 1.1f;
        ActionCoolDownAttackPrimary = 1f;
        ActionCoolDownAttackSecondary = 1.15f;

        //define weapon collider
        WidthOfWeapon = 0.25f;
        HeightOfWeapon = 1.5f;
        WeaponOffsetAngle = 90f;
    }

    void Update()
    {
        UpdateTimers();
        ResetComboCheck();
        CollisionWithWeaponInAttackStateCheck();
    }
}
