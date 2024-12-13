using UnityEngine;
using System.Collections.Generic;

public class Shield : UsableObject, IBlockable
{
    [SerializeField] public CurrentStateOfAction CurrentState { get; set; }//change in future
    [SerializeField] private Sprite FrontSprite;
    [SerializeField] private Sprite BackSprite;

    //cooldown
    public float ActionCoolDownTimer { get; protected set; } = 0;
    public float ActionCoolDownBlock { get; protected set; } = 1f;
    //stamina
    protected float StaminaReduceValueWhenBlocking = 20f;
    void Start()
    {
        base.Start();
    }

    void Update()
    {
        UpdateTimers();
        ReduceStaminaWhenBlocking();
    }

    protected void UpdateTimers()
    {
        ActionCoolDownTimer += Time.deltaTime;
    }
    protected void ReduceStaminaWhenBlocking()
    {
        if (CurrentState == CurrentStateOfAction.Blocking)
        {
            if(!HolderStatController.ReduceStamina(StaminaReduceValueWhenBlocking * Time.deltaTime))
            {
                BlockEnd();
            }
           
        }
    }
    public override void OnHolderDamaged()
    {
        ActionCoolDownTimer = 0.3f;
        CurrentState = CurrentStateOfAction.None;
    }

    public void BlockStart()
    {
        if (ActionCoolDownTimer >= ActionCoolDownBlock && HolderStatController.Stamina > 0 && CurrentState == CurrentStateOfAction.None)
        {
            HoldersAnimator.SetBool("BlockingShield", true);
            ActionCoolDownTimer = -0.3f;
        }
    }
    public bool BlockImpact(GameObject AttackingWeapon)
    {
        HoldersAnimator.SetTrigger("Blocked");
        if (AttackingWeapon.GetComponent<UsableObject>() is Legs)
        {
            BlockEnd();
            return false;
        }
        else
        {
            return true;
        }
    }

    public void BlockEnd()
    {
        HoldersAnimator.SetBool("BlockingShield", false);
    }

    //--------------------animation events
    public void BlockStateStart()
    {
        CurrentState = CurrentStateOfAction.Blocking;

        GetComponent<SpriteRenderer>().sprite = FrontSprite;
        GetComponent<SpriteRenderer>().sortingOrder = 8;
    }
    public void BlockStateEnd()
    {
        CurrentState = CurrentStateOfAction.None;

        GetComponent<SpriteRenderer>().sprite = BackSprite;
        GetComponent<SpriteRenderer>().sortingOrder = -1;
    }

}
