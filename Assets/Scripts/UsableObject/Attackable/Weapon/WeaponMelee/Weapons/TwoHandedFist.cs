using System.Collections;
using UnityEngine;

public class TwoHandedFist : WeaponMelee
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected float CoolDownStopsHolding = 4f;

    [SerializeField] private GameObject LeftFist;
    [SerializeField] private GameObject RightFist;
    void Start()
    {
        base.Start();
        IsTwoHanded = true;

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

            ActionCoolDownTimer = 0f;

            StartCoroutine(DisableFistsCoroutine());
        }
    }

    IEnumerator DisableFistsCoroutine()
    {
        yield return new WaitForSeconds(0.45f);
        if(ActionCoolDownTimer > 0.43f)
        {
            HolderController.DisablePreviousItem(gameObject);
        }
    }
    //---------Drawing collider------------
    override protected Collider2D DrawingCollider()
    {
        float offsetAngle = ColliderOffsetAngle;
        float angle = transform.eulerAngles.z + offsetAngle; // Add an offset angle
        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        RaycastHit2D EnemiesHitLeftHand = Physics2D.CircleCast(LeftFist.transform.position, WidthOfCollider, direction, HeightOfCollider, HolderController.EnemyLayer);
        RaycastHit2D EnemiesHitRightHand = Physics2D.CircleCast(RightFist.transform.position, WidthOfCollider, direction, HeightOfCollider, HolderController.EnemyLayer);

        if (EnemiesHitLeftHand && (HolderController.IsAttackingCheck().Item1 == "SecondaryAttack" || HolderController.IsAttackingCheck().Item1 == "PrimaryAttack2")) return EnemiesHitLeftHand.collider;
        else if (EnemiesHitRightHand && (HolderController.IsAttackingCheck().Item1 == "PrimaryAttack")) return EnemiesHitRightHand.collider;
        else return null;
    }

    //---------Weapon Clashed-----------------

    public override bool AttacksClashed(GameObject EnemyWeapon)
    {
        Debug.Log("TwoHandedFist Weaponsclashed called!");
        Debug.Log(EnemyWeapon.GetComponent<UsableObject>());

        if(EnemyWeapon.GetComponent<UsableObject>() is TwoHandedFist)
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
        if(AttackingWeapon.GetComponent<UsableObject>() is TwoHandedFist || AttackingWeapon.GetComponent<UsableObject>() is Leg)
        {
            HoldersAnimator.SetTrigger("Blocked");
            return true;
        }
        return false;
    }
}
