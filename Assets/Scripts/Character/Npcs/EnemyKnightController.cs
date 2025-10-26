using UnityEngine;

public class EnemyKnightController : NPCCharacterControllerMeleeWeapon
{
    [SerializeField] override protected float CombatStateStartDistence { get; set; } = 3f;
    [SerializeField] override protected float AttackCoolDownRangeMin { get; set; } = 0.8f;
    [SerializeField] override protected float AttackCoolDownRangeMax { get; set; } = 1.3f;
    [SerializeField] override protected float DistenceToEnemyStartBlocking { get; set; } = 3.5f;
    [SerializeField] override protected float BlockChance { get; set; } = 0.7f;
    void Start()
    {
        base.Start();
    }

    void Update()
    {
        HandleAnimations();
    }
    void LateUpdate()
    {
        HandleRotate();
    }

    private void FixedUpdate()
    {
        MoveToTheNextPoint();
    }
}
