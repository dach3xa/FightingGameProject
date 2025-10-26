public class OneHandedWeaponSharp : WeaponMelee
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override float Sharpness { get; set; } = 0.2f;
    void Start()
    {
        base.Start();
        IsTwoHanded = false;
        Sharpness = 0.3f;
        Sharpness = 0.3f;

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
