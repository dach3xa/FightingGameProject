using UnityEngine;
using System.Collections.Generic;

public interface IAttackablePrimary
{
    public CurrentStateOfAction CurrentState { get; }
    public float BaseAttackValue { get;  }
    public float BaseStaminaReduceValue { get;  }
    public float PrimaryAttackMultiplier { get; }
    public float WidthOfCollider { get;  }
    public float HeightOfCollider { get;  }
    public float ColliderOffsetAngle { get;  }
    public List<GameObject> EnemiesHitWhileInAttackState { get;  }
    public float ActionCoolDownTimer { get;  }
    public float ActionCoolDownAttackPrimary { get;  }

    public bool AttacksClashed(GameObject EnemyWeapon);
    public void AttackPrimary();

    //---animation events--
    public void AttackStateStartPrimary();
    public void AttackStateEnd();
}
