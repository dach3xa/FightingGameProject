using UnityEngine;

public interface IBlockable
{
    public void BlockStart();
    public void BlockImpact();
    public void BlockEnd();
    //--------------------animation events
    public void BlockStateStart();
    public void BlockStateEnd();
}
