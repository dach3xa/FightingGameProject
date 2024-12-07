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

    override protected Collider2D DrawingCollider()
    {
        float offsetAngle = WeaponOffsetAngle;
        float angle = transform.eulerAngles.z + offsetAngle; // Add an offset angle
        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        RaycastHit2D EnemiesHitLeftHand = Physics2D.CircleCast(ItemHolderLeftHand.transform.position, WidthOfWeapon, direction, HeightOfWeapon, HolderController.EnemyLayer);
        RaycastHit2D EnemiesHitRightHand = Physics2D.CircleCast(ItemHolderRightHand.transform.position, WidthOfWeapon, direction, HeightOfWeapon, HolderController.EnemyLayer);

        Debug.Log("Enemies Hit left hand: " + EnemiesHitLeftHand);
        Debug.Log("Enemies Hit Right hand: " + EnemiesHitRightHand);
        if (EnemiesHitLeftHand) return EnemiesHitLeftHand.collider;
        else return EnemiesHitRightHand.collider;
    }
}
