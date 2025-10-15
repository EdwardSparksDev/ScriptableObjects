using UnityEngine;


[CreateAssetMenu(menuName = "Gameplay/StatusEffect", fileName = "StatusName_StatusEffectData")]
public class StatusEffectData : ScriptableObject
{
    #region Variables & Properties

    #region SerializeField
    [Header("General Info")]
    [SerializeField] string effectName;
    [SerializeField] Sprite effectIcon;
    [Space(10), SerializeField] E_EffectStackingType effectStacking;

    [Space(25), Header("Buffs/Debuffs")]
    [SerializeField] int damageTakenIncreasePercentage;
    [SerializeField] int damageDealtIncreasePercentage;
    [Space(10), SerializeField] int damagePerHitIncrease;
    [SerializeField] int blockPerCardIncrease;
    #endregion

    #region Properties
    public string EffectName => effectName;
    public Sprite EffectIcon => effectIcon;
    public E_EffectStackingType EffectStacking => effectStacking;
    public int DamageTakenIncreasePercentage => damageTakenIncreasePercentage;
    public int DamageDealtIncreasePercentage => damageDealtIncreasePercentage;
    public int DamagePerHitIncrease => damagePerHitIncrease;
    public int BlockPerCardIncrease => blockPerCardIncrease;
    #endregion

    #endregion
}
