using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Gameplay/Card", fileName = "CardName_CardData")]
public class CardData : ScriptableObject
{
    #region Variables & Properties

    #region SerializeField
    [Header("General Info")]
    [Space(5), SerializeField] string cardName;
    [Space(10), TextArea, SerializeField] string cardDescription;
    [TextArea, SerializeField] string upgradedCardDescription;

    [Space(10), SerializeField] E_CardRarity cardRarity;
    [SerializeField] E_CardType cardType;
    [Space(10), SerializeField] Sprite cardSprite;

    [Space(25), Header("Energy")]
    [Space(5), SerializeField] ST_UpgradeAmount cardEnergyCost;
    [Space(10), SerializeField] int energyIncrease;
    [Space(10), SerializeField] bool reduceCardsCostToFixedAmount;
    [SerializeField] int cardsCostReduction;

    [Space(25), Header("Damage")]
    [Space(5), SerializeField] ST_UpgradeAmount cardDamage;
    [Space(10), SerializeField] bool applyDamageOverride;
    [SerializeField] E_StatType damageOverrideStat;
    [SerializeField] E_TargetType damageOverrideSource;
    [Space(10), SerializeField] int recoilDamage;
    [Space(10), SerializeField] ST_UpgradeAmount counterDamage;

    [Space(25), Header("Block")]
    [Space(5), SerializeField] ST_UpgradeAmount blockIncrease;
    [SerializeField] E_OperatorType blockAlterationOperator;
    [Space(10), SerializeField] int blockClearDelayOverride;

    [Space(25), Header("Cards")]
    [Space(5), SerializeField] ST_UpgradeAmount cardsToDraw;
    [Space(10), SerializeField] ST_UpgradeAmount cardsToDiscard;

    [Space(25), SerializeField] bool upgradesAllCards;
    [Space(10), SerializeField] bool exhaustCard;

    [Space(25), Header("Effects")]
    [Space(5), SerializeField] List<ST_UpgradeStatusEffect> cardEffects;
    #endregion

    #region Properties
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public string UpgradedCardDescription => upgradedCardDescription;
    public E_CardRarity CardRarity => cardRarity;
    public E_CardType CardType => cardType;
    public Sprite CardSprite => cardSprite;
    public ST_UpgradeAmount CardEnergyCost => cardEnergyCost;
    public int EnergyIncrease => energyIncrease;
    public bool ReduceCardsCostToFixedAmount => reduceCardsCostToFixedAmount;
    public int CardsCostReduction => cardsCostReduction;
    public ST_UpgradeAmount CardDamage => cardDamage;
    public bool ApplyDamageOverride => applyDamageOverride;
    public E_StatType DamageOverrideStat => damageOverrideStat;
    public E_TargetType DamageOverrideSource => damageOverrideSource;
    public int RecoilDamage => recoilDamage;
    public ST_UpgradeAmount CounterDamage => counterDamage;
    public ST_UpgradeAmount BlockIncrease => blockIncrease;
    public E_OperatorType BlockAlterationOperator => blockAlterationOperator;
    public int BlockClearDelayOverride => blockClearDelayOverride;
    public ST_UpgradeAmount CardsToDraw => cardsToDraw;
    public ST_UpgradeAmount CardsToDiscard => cardsToDiscard;
    public bool UpgradesAllCards => upgradesAllCards;
    public bool ExhaustCard => exhaustCard;
    public List<ST_UpgradeStatusEffect> CardEffects => cardEffects;
    #endregion

    #endregion
}
