using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class UsableObject : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] protected GameObject Holder;
    [SerializeField] protected CharacterStatController HolderStatController;
    [SerializeField] protected BaseCharacterController HolderController;
    [SerializeField] protected SortingGroup HoldersSortingGroup;
    [SerializeField] protected Animator HoldersAnimator;
    [SerializeField] protected GameObject AudioHolder;
    [SerializeField] protected Dictionary<string, AudioSource> SoundEffects;
    [SerializeField] public bool IsTwoHanded;
    [SerializeField] public virtual int AnimationLayer { get; set; }
    protected virtual Dictionary<int, string> AnimationStateNamesAttack { get; set; }
    public (string,bool) PlayingAttackAnimationCheck
    { 
        get 
        {
            AnimatorStateInfo stateInfoWeapon = HoldersAnimator.GetCurrentAnimatorStateInfo(AnimationLayer);

            return AnimationStateNamesAttack.ContainsKey(stateInfoWeapon.shortNameHash)
                ? (AnimationStateNamesAttack[stateInfoWeapon.shortNameHash], true)
                : ("None", false);
        }
    }

    protected void Start()
    {
        InitializeVariables();
    }

    protected void Awake()
    {
        InitializeVariables();
    }

    protected void InitializeVariables()
    {
        Holder = transform.root.gameObject;
        HolderStatController = Holder.GetComponent<CharacterStatController>();
        HolderController = Holder.GetComponent<BaseCharacterController>();
        HoldersSortingGroup = Holder.GetComponent<SortingGroup>();
        HoldersAnimator = Holder.GetComponent<Animator>();

        AnimationLayer = HoldersAnimator.GetLayerIndex(gameObject.name);//probably better to make it a tag than name
        AudioHolder = transform.Find("AudioSfx").gameObject;
        SoundEffects = new Dictionary<string, AudioSource>();

        IntializeSoundEffects();
    }

    protected void IntializeSoundEffects()
    {
        foreach (Transform Audio in AudioHolder.transform)
        {
            SoundEffects.Add(Audio.name, Audio.GetComponent<AudioSource>());
        }
    }

    public abstract void OnHolderDamaged();
    
}
