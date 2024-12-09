using System.Collections;
using UnityEngine;

public class TwoHandedFist : WeaponMelee
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected float CoolDownStopsHolding = 4f;

    [SerializeField] private GameObject ItemHolderLeftHand;
    [SerializeField] private GameObject ItemHolderRightHand;
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
        WidthOfWeapon = 0.25f;
        HeightOfWeapon = 0.5f;
        WeaponOffsetAngle = 90f;
    }

    private void OnEnable()
    {
        ActionCoolDownTimer = 1.3f;
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
        float offsetAngle = WeaponOffsetAngle;
        float angle = transform.eulerAngles.z + offsetAngle; // Add an offset angle
        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        RaycastHit2D EnemiesHitLeftHand = Physics2D.CircleCast(ItemHolderLeftHand.transform.position, WidthOfWeapon, direction, HeightOfWeapon, HolderController.EnemyLayer);
        RaycastHit2D EnemiesHitRightHand = Physics2D.CircleCast(ItemHolderRightHand.transform.position, WidthOfWeapon, direction, HeightOfWeapon, HolderController.EnemyLayer);

        if (EnemiesHitLeftHand) return EnemiesHitLeftHand.collider;
        else return EnemiesHitRightHand.collider;
    }

    //---------Weapon Clashed-----------------

    public override bool WeaponsClashed(GameObject EnemyWeapon)
    {
        Debug.Log("TwoHandedFist Weaponsclashed called!");
        Debug.Log(EnemyWeapon.GetComponent<HoldableItem>());

        if(EnemyWeapon.GetComponent<HoldableItem>() is TwoHandedFist)
        {
            HoldersAnimator.SetTrigger("Blocked");
            EnemiesHitWhileInAttackState.Add(EnemyWeapon);
            ResetCombo();
            return true;
        }
        else
        {
            HoldersAnimator.SetTrigger("Blocked");
            ResetCombo();
            return false;
        }
    }

    //-------block impact----------------------

    public override bool BlockImpact(GameObject AttackingWeapon)
    {
        if(AttackingWeapon.GetComponent<HoldableItem>() is TwoHandedFist)
        {
            HoldersAnimator.SetTrigger("Blocked");
            return true;
        }
        return false;
    }
}
