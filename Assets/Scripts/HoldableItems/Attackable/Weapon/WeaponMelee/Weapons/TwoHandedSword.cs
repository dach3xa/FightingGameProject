using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Rendering;
using NUnit.Framework;

public class TwoHandedSword : WeaponMelee
{

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
