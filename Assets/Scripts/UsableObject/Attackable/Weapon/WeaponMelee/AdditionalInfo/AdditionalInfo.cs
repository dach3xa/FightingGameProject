

using UnityEngine;

namespace Assets.Scripts.UsableObject.Attackable.Weapon.WeaponMelee.AdditionalInfo
{
    public class AdditionalInfo
    {
        public AdditionalInfo()
        {
        }

        public AdditionalInfo(GameObject attackerWeapon)
        {
            AttackerWeapon = attackerWeapon;
        }

        public AdditionalInfo(GameObject attackerWeapon, bool? wasLastAttackDirect)
        {
            AttackerWeapon = attackerWeapon;
            WasLastAttackDirect = wasLastAttackDirect;
        }

        public GameObject? AttackerWeapon { get; set; }
        public bool? WasLastAttackDirect { get; set; }
    }
}
