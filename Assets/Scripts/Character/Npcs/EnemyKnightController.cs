using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using System.Drawing;
using static UnityEngine.Random;
using System;
using System.Threading;
using NUnit.Framework.Interfaces;
using System.Threading.Tasks;

public class EnemyKnightController : NPCCharacterControllerMeleeWeapon
{
    override protected float CombatStateStartDistence { get; set; } = 3f;
    override protected float MaxDistenceToEnemyStop { get; set; } = 2f;//too far
    override protected float MinDistenceToEnemyStop { get; set; } = 1f;//too close
    override protected float AttackCoolDownRangeMin { get; set; } = 1f;
    override protected float AttackCoolDownRangeMax { get; set; } = 1f;
    override protected float DistenceToEnemyStartBlocking { get; set; } = 3.5f;
    override protected float DistenceToEnemyStartAttacking { get; set; } = 2.8f;
    override protected float BlockChance { get; set; } = 0.7f;
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
