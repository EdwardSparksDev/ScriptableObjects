using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ActiveCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    #region Variables & Properties

    #region Local
    CardData cardData;
    RectTransform rectTransform;
    CanvasGroup canvasGroup;
    Canvas canvas;

    bool isUpgraded;
    bool hasBeenPlayed;
    bool discarded;
    int currentSiblingIndex;

    Vector2 defaultScale;
    #endregion

    #region SerializeField
    [Header("Settings")]
    [SerializeField] float draggingScale;

    [Space(25), Header("UI References")]
    [SerializeField] Image image_CardImage;
    [SerializeField] TMP_Text text_CardEnergyCost;
    [SerializeField] TMP_Text text_CardName;
    [SerializeField] TMP_Text text_CardType;
    [SerializeField] TMP_Text text_CardDescription;

    [Space(10), SerializeField] Color upgradedCardTitleColor;

    #endregion

    #region Properties
    public bool HasBeenPlayed { get => hasBeenPlayed; set => hasBeenPlayed = value; }
    public bool Discarded { get => discarded; set => discarded = value; }
    public int CurrentSiblingIndex => currentSiblingIndex;
    public bool IsUpgraded => isUpgraded;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        defaultScale = transform.localScale;

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }


    private void OnEnable()
    {
        EventManager.SaveCardsSiblingIndexes += OnSaveSiblingIndex;
    }


    private void OnDisable()
    {
        EventManager.SaveCardsSiblingIndexes -= OnSaveSiblingIndex;
    }
    #endregion


    #region Methods
    public override string ToString()
    {
        return cardData.CardName;
    }


    public void InitializeCard(CardData newCardData, bool upgrade = false)
    {
        cardData = newCardData;
        isUpgraded = upgrade;

        image_CardImage.sprite = cardData.CardSprite;
        text_CardEnergyCost.text = cardData.CardEnergyCost.defaultAmount.ToString();
        text_CardName.text = cardData.CardName;
        text_CardType.text = cardData.CardType.ToString();
        text_CardDescription.text = cardData.CardDescription;

        canvas = transform.root.GetComponent<Canvas>();
    }


    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        if (!GameManager.CanPlay || hasBeenPlayed) return;

        EventManager.PlayCardDrawnAudio?.Invoke();

        canvasGroup.blocksRaycasts = false;
        transform.SetAsLastSibling();
        transform.localScale = new Vector2(draggingScale, draggingScale);
        rectTransform.position = Input.mousePosition;
        rectTransform.rotation = Quaternion.identity;

        EventManager.RequestHandUpdate?.Invoke(this, false);
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (!GameManager.CanPlay || hasBeenPlayed) return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        if (!GameManager.CanPlay || hasBeenPlayed) return;

        canvasGroup.blocksRaycasts = true;

        if (!HasBeenPlayed && !discarded)
        {
            ResetScale();
            transform.SetSiblingIndex(currentSiblingIndex);

            EventManager.RequestHandUpdate?.Invoke(this, true);
        }
    }


    private void OnSaveSiblingIndex()
    {
        currentSiblingIndex = transform.GetSiblingIndex();
    }


    public void ResetScale()
    {
        transform.localScale = defaultScale;
    }


    public int GetEnergyCost()
    {
        return isUpgraded ? cardData.CardEnergyCost.upgradedAmount : cardData.CardEnergyCost.defaultAmount;
    }


    public int GetBlock(int currentBlock, out bool blockMultiplied)
    {
        blockMultiplied = false;

        switch (cardData.BlockAlterationOperator)
        {
            case E_OperatorType.ADD:
                return isUpgraded ? cardData.BlockIncrease.upgradedAmount : cardData.BlockIncrease.defaultAmount;

            case E_OperatorType.MULTIPLY:
                blockMultiplied = true;
                return (isUpgraded ? cardData.BlockIncrease.upgradedAmount : cardData.BlockIncrease.defaultAmount) * currentBlock - currentBlock;

            default:
                return int.MaxValue;
        }
    }


    public void LoadDamage(in ActiveCharacter player, in ActiveCharacter enemy, out int damageDealt, out int damageReceived, ref int counterDamage)
    {
        damageDealt = 0;
        damageReceived = 0;

        if (cardData.ApplyDamageOverride)
        {
            switch (cardData.DamageOverrideStat)
            {
                case E_StatType.BLOCK:
                    switch (cardData.DamageOverrideSource)
                    {
                        case E_TargetType.SELF:
                            damageDealt = player.Block;
                            break;

                        case E_TargetType.ENEMY:
                            damageDealt = enemy.Block;
                            break;
                    }
                    break;

                case E_StatType.NONE:
                    Debug.LogError("No stat type selected on damage override!");
                    break;
            }
        }
        else
        {
            if (isUpgraded)
                damageDealt = cardData.CardDamage.upgradedAmount;
            else
                damageDealt = cardData.CardDamage.defaultAmount;
        }

        if (cardData.RecoilDamage > 0)
            damageReceived = cardData.RecoilDamage;

        if ((!isUpgraded && cardData.CounterDamage.defaultAmount > 0) || (isUpgraded && cardData.CounterDamage.upgradedAmount > 0))
            counterDamage += isUpgraded ? cardData.CounterDamage.upgradedAmount : cardData.CounterDamage.defaultAmount;
    }


    public int GetBlockClearDelay()
    {
        return cardData.BlockClearDelayOverride;
    }


    public void UpgradeCard()
    {
        if (isUpgraded) return;

        isUpgraded = true;
        text_CardName.color = upgradedCardTitleColor;
        text_CardEnergyCost.text = cardData.CardEnergyCost.upgradedAmount.ToString();
        text_CardDescription.text = cardData.UpgradedCardDescription;
    }


    public bool PerformsUpgrade()
    {
        return cardData.UpgradesAllCards;
    }


    public bool IsExhaustable()
    {
        return cardData.ExhaustCard;
    }


    public int GetCardsToDraw()
    {
        return isUpgraded ? cardData.CardsToDraw.upgradedAmount : cardData.CardsToDraw.defaultAmount;
    }


    public int GetCardsToDiscard()
    {
        return isUpgraded ? cardData.CardsToDiscard.upgradedAmount : cardData.CardsToDiscard.defaultAmount;
    }


    public int GetLatentEnergy()
    {
        return cardData.EnergyIncrease;
    }


    public int GetFlatCostReduction()
    {
        return cardData.ReduceCardsCostToFixedAmount ? cardData.CardsCostReduction : -1;
    }


    public void UpdateEnergyCostDisplay(int newEnergyCost)
    {
        int currentCost = isUpgraded ? cardData.CardEnergyCost.upgradedAmount : cardData.CardEnergyCost.defaultAmount;

        if (newEnergyCost >= 0 && newEnergyCost < currentCost)
            text_CardEnergyCost.text = newEnergyCost.ToString();
        else
            text_CardEnergyCost.text = currentCost.ToString();
    }


    public List<ST_UpgradeStatusEffect> GetStatusEffects()
    {
        return cardData.CardEffects;
    }
    #endregion
}
