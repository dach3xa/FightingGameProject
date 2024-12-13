using System.Collections;
using UnityEngine;

public class TwoHandedFist : Fist
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private GameObject LeftFist;
    [SerializeField] private GameObject RightFist;
    void Start()
    {
        CurrentItemType = ItemType.TwoHandedFist;
        base.Start();
        IsTwoHanded = true;
    }

    private void OnEnable()
    {
        ActionCoolDownTimer = 2f;
    }

    void Update()
    {
        UpdateTimers();
        ResetComboCheck();
        CollisionWithWeaponInAttackStateCheck();

        StopHoldingCheck();
    }
    //---------Drawing collider------------
    override protected Collider2D DrawingCollider()
    {
        float offsetAngle = ColliderOffsetAngle;
        float angle = transform.eulerAngles.z + offsetAngle; // Add an offset angle
        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        RaycastHit2D EnemiesHitLeftHand = Physics2D.CircleCast(LeftFist.transform.position, WidthOfCollider, direction, HeightOfCollider, HolderController.EnemyLayer);
        RaycastHit2D EnemiesHitRightHand = Physics2D.CircleCast(RightFist.transform.position, WidthOfCollider, direction, HeightOfCollider, HolderController.EnemyLayer);

        if (EnemiesHitLeftHand && (PlayingAttackAnimationCheck.Item1 == "SecondaryAttack" || PlayingAttackAnimationCheck.Item1 == "PrimaryAttack2")) return EnemiesHitLeftHand.collider;
        else if (EnemiesHitRightHand && (PlayingAttackAnimationCheck.Item1 == "PrimaryAttack")) return EnemiesHitRightHand.collider;
        else return null;
    }
}
