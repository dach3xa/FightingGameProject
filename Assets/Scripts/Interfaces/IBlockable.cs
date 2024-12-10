using UnityEngine;

public interface IBlockable
{
    public CurrentStateOfAction CurrentState { get; }
    public float ActionCoolDownTimer { get;  }
    public float ActionCoolDownBlock { get; }

    public void BlockStart();
    public bool BlockImpact(GameObject AttackingWeapon);
    public void BlockEnd();
    //--------------------animation events
    public void BlockStateStart();
    public void BlockStateEnd();
}
