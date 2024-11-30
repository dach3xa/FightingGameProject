using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class HoldableItem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] protected GameObject Holder;
    [SerializeField] protected CharacterStatController HolderStatController;
    [SerializeField] protected BaseCharacterController HolderController;
    [SerializeField] protected SortingGroup HoldersSortingGroup;
    [SerializeField] protected Animator HoldersAnimator;
    [SerializeField] protected Dictionary<string, AudioSource> HoldersSoundEffects;
    [SerializeField] public bool IsTwoHanded;
    protected void Start()
    {
        Holder = transform.root.gameObject;
        HolderStatController = Holder.GetComponent<CharacterStatController>();
        HolderController = Holder.GetComponent<BaseCharacterController>();
        HoldersSortingGroup = Holder.GetComponent<SortingGroup>();
        HoldersAnimator = Holder.GetComponent<Animator>();
        HoldersSoundEffects = HolderController.SoundEffects;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
