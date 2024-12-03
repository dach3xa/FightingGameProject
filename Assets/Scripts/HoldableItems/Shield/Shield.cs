using UnityEngine;

public class Shield : HoldableItem, IBlockable
{
    [SerializeField] public CurrentStateOfWeapon currentState { get; set; }//change in future
    [SerializeField] private Sprite FrontSprite;
    [SerializeField] private Sprite BackSprite;
    //cooldown
    protected float ActionCoolDownTimer = 0;
    protected float ActionCoolDownBlock = 1f;
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
        if (currentState == CurrentStateOfWeapon.Blocking)
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
        currentState = CurrentStateOfWeapon.None;
    }

    public void BlockStart()
    {
        if (ActionCoolDownTimer >= ActionCoolDownBlock && HolderStatController.Stamina > 0 && currentState == CurrentStateOfWeapon.None)
        {
            HoldersAnimator.SetBool("BlockingShield", true);
            ActionCoolDownTimer = -0.3f;
        }
    }
    public void BlockImpact()
    {
        HoldersAnimator.SetTrigger("Blocked");
    }

    public void BlockEnd()
    {
        HoldersAnimator.SetBool("BlockingShield", false);
    }

    //--------------------animation events
    public void BlockStateStart()
    {
        currentState = CurrentStateOfWeapon.Blocking;

        GetComponent<SpriteRenderer>().sprite = FrontSprite;
        GetComponent<SpriteRenderer>().sortingOrder = 8;
    }
    public void BlockStateEnd()
    {
        currentState = CurrentStateOfWeapon.None;

        GetComponent<SpriteRenderer>().sprite = BackSprite;
        GetComponent<SpriteRenderer>().sortingOrder = -1;
    }

}
