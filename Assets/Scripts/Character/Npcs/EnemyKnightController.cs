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
    [SerializeField]override protected float CombatStateStartDistence { get; set; } = 3f;
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
