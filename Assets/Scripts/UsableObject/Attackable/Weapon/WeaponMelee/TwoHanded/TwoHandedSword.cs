public class TwoHandedWeaponSharp : WeaponMelee
{
    public override float Sharpness { get; set; } = 0.45f;
    void Start()
    {
        base.Start();
        IsTwoHanded = true;
    }

    void Update()
    {
        UpdateTimers();
        ResetComboCheck();
        CollisionWithWeaponInAttackStateCheck();
    }
}
