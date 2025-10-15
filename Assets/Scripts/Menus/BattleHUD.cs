using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class BattleHUD : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    RectTransform handRT;

    List<GameObject> playerStatusIcons;
    List<GameObject> enemyStatusIcons;
    #endregion

    #region SerializeField
    [Header("Layout Settings")]
    [SerializeField] int handCardsSpacingDelta;
    [SerializeField] float handCardsRotationDelta;
    [SerializeField] float handCardsCurveExponent;
    [SerializeField] float handCardsPositionDelta;

    [Space(10), SerializeField] Color hpDefaultColor;
    [SerializeField] Color hpBlockColor;

    [Space(10), TextArea, SerializeField] string discardMessageText;

    [Space(25), Header("References")]
    [Header("Upper Section")]
    [SerializeField] TMP_Text text_PlayerHpUpper;
    [Space(10), SerializeField] GameObject discardMessage;
    [SerializeField] TMP_Text text_DiscardMessage;
    [Space(10), SerializeField] GameObject gameOverScreen;
    [SerializeField] TMP_Text text_GameOver;

    [Space(25), Header("Lower Section")]
    [SerializeField] Transform cardsHandTransform;
    [SerializeField] TMP_Text text_Energy;
    [Space(10), SerializeField] TMP_Text text_DrawPileCardsCount;
    [SerializeField] TMP_Text text_DiscardPileCardsCount;
    [SerializeField] GameObject exhaustPile;
    [SerializeField] TMP_Text text_ExhaustPileCardsCount;
    [Space(10), SerializeField] Button btn_EndTurn;
    [SerializeField] Sprite endTurnEnabled;
    [SerializeField] Sprite endTurnDisabled;

    [Space(25), Header("Player")]
    [SerializeField] Image image_Player;
    [SerializeField] TMP_Text text_PlayerHp;
    [SerializeField] Image image_PlayerHpFill;
    [SerializeField] TMP_Text text_PlayerBlock;
    [SerializeField] Image image_PlayerBlock;
    [SerializeField] Transform playerStatuses;

    [Space(25), Header("Enemy")]
    [SerializeField] Image image_Enemy;
    [SerializeField] TMP_Text text_EnemyHp;
    [SerializeField] Image image_EnemyHpFill;
    [SerializeField] Transform enemyStatuses;

    [Space(25), Header("Prefabs")]
    [SerializeField] GameObject statusEffectIcon;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        handRT = cardsHandTransform.GetComponent<RectTransform>();

        playerStatusIcons = new List<GameObject>();
        enemyStatusIcons = new List<GameObject>();
    }


    private void OnEnable()
    {
        EventManager.UpdateCharacterUI += OnUpdateCharacterUI;
        EventManager.LinkCardToHand += OnLinkCardToHand;
        EventManager.UpdateHandCardsDisplay += OnUpdateHandCardsDisplay;
        EventManager.UpdateDrawPileCardsCount += OnUpdateDrawPileCounter;
        EventManager.UpdateDiscardPileCardsCount += OnUpdateDiscardPileCounter;
        EventManager.UpdateExhaustPileCardsCount += OnUpdateExhaustPileCounter;
        EventManager.UpdateEndTurnButton += OnUpdateEndTurnButton;
        EventManager.UpdateEnergyDisplay += OnUpdateEnergyDisplay;
        EventManager.UpdateBlockDisplay += OnUpdateBlockDisplay;
        EventManager.UpdateDiscardMessage += OnUpdateDiscardMessage;
        EventManager.UpdateHealth += OnUpdateHealth;
        EventManager.ShowGameOverScreen += OnShowGameOverScreen;
        EventManager.AddStatusEffectIcon += OnAddStatusEffect;
        EventManager.UpdateStatusEffectIcon += OnUpdateStatusEffect;
    }


    private void OnDisable()
    {
        EventManager.UpdateCharacterUI -= OnUpdateCharacterUI;
        EventManager.LinkCardToHand -= OnLinkCardToHand;
        EventManager.UpdateHandCardsDisplay -= OnUpdateHandCardsDisplay;
        EventManager.UpdateDrawPileCardsCount -= OnUpdateDrawPileCounter;
        EventManager.UpdateDiscardPileCardsCount -= OnUpdateDiscardPileCounter;
        EventManager.UpdateExhaustPileCardsCount -= OnUpdateExhaustPileCounter;
        EventManager.UpdateEndTurnButton -= OnUpdateEndTurnButton;
        EventManager.UpdateEnergyDisplay -= OnUpdateEnergyDisplay;
        EventManager.UpdateBlockDisplay -= OnUpdateBlockDisplay;
        EventManager.UpdateDiscardMessage -= OnUpdateDiscardMessage;
        EventManager.UpdateHealth -= OnUpdateHealth;
        EventManager.ShowGameOverScreen -= OnShowGameOverScreen;
        EventManager.AddStatusEffectIcon -= OnAddStatusEffect;
        EventManager.UpdateStatusEffectIcon -= OnUpdateStatusEffect;
    }
    #endregion


    #region Methods
    private void OnUpdateCharacterUI(CharacterData character, bool isPlayer)
    {
        if (isPlayer)
        {
            OnUpdateEnergyDisplay(character.MaxStartingEnergy, character.MaxStartingEnergy);
            OnUpdateBlockDisplay(0);

            image_Player.sprite = character.MainSprite;
            OnUpdateHealth(true, character.Hp, character.Hp);
        }
        else
        {
            image_Enemy.sprite = character.MainSprite;
            OnUpdateHealth(false, character.Hp, character.Hp);
        }
    }


    private void OnLinkCardToHand(GameObject go)
    {
        go.transform.SetParent(cardsHandTransform);
    }


    private void OnUpdateHandCardsDisplay(HashSet<ActiveCard> hand)
    {
        List<ActiveCard> cards = GetSortedHandCards(hand);

        float middlePoint = hand.Count / 2f - .5f;
        float distanceToMiddlePoint;
        float currentRotationDelta;
        int i = 0;
        foreach (ActiveCard card in cards)
        {
            distanceToMiddlePoint = middlePoint - i;
            currentRotationDelta = distanceToMiddlePoint * handCardsRotationDelta;

            RectTransform cardRT = card.GetComponent<RectTransform>();
            cardRT.rotation = Quaternion.Euler(0f, 0f, currentRotationDelta);
            cardRT.position = handRT.position + handCardsSpacingDelta * distanceToMiddlePoint * Vector3.left +
                Mathf.Pow(Mathf.Abs(distanceToMiddlePoint), handCardsCurveExponent) * handCardsPositionDelta * Vector3.up;
            i++;
        }
    }


    private List<ActiveCard> GetSortedHandCards(HashSet<ActiveCard> cards)
    {
        List<ActiveCard> sortedCards = new List<ActiveCard>();

        foreach (ActiveCard card in cards)
            sortedCards.Add(card);

        sortedCards.Sort((x, y) => x.CurrentSiblingIndex > y.CurrentSiblingIndex ? 1 : -1);
        return sortedCards;
    }


    private void OnUpdateDrawPileCounter(int count)
    {
        text_DrawPileCardsCount.text = count.ToString();
    }


    private void OnUpdateDiscardPileCounter(int count)
    {
        text_DiscardPileCardsCount.text = count.ToString();
    }


    private void OnUpdateExhaustPileCounter(int count)
    {
        if (!exhaustPile.activeSelf)
            exhaustPile.SetActive(true);

        text_ExhaustPileCardsCount.text = count.ToString();
    }


    private void OnUpdateEndTurnButton(bool enable)
    {
        btn_EndTurn.enabled = enable;

        if (enable)
            btn_EndTurn.GetComponent<Image>().sprite = endTurnEnabled;
        else
            btn_EndTurn.GetComponent<Image>().sprite = endTurnDisabled;
    }


    private void OnUpdateEnergyDisplay(int currentEnergy, int maxEnergy)
    {
        text_Energy.text = $"{currentEnergy}/{maxEnergy}";
    }


    private void OnUpdateBlockDisplay(int currentBlock)
    {
        if (currentBlock <= 0 && image_PlayerBlock.gameObject.activeSelf)
        {
            image_PlayerBlock.gameObject.SetActive(false);
            image_PlayerHpFill.color = hpDefaultColor;
        }
        else if (currentBlock > 0 && !image_PlayerBlock.gameObject.activeSelf)
        {
            image_PlayerBlock.gameObject.SetActive(true);
            image_PlayerHpFill.color = hpBlockColor;
        }

        text_PlayerBlock.text = currentBlock.ToString();
    }


    private void OnUpdateHealth(bool isPlayer, int hp, int maxHp)
    {
        if (isPlayer)
        {
            text_PlayerHpUpper.text = $"{hp}/{maxHp}";
            text_PlayerHp.text = text_PlayerHpUpper.text;
            image_PlayerHpFill.transform.localScale = new Vector2((float)hp / maxHp, image_PlayerHpFill.transform.localScale.y);
        }
        else
        {
            text_EnemyHp.text = $"{hp}/{maxHp}";
            image_EnemyHpFill.transform.localScale = new Vector2((float)hp / maxHp, image_EnemyHpFill.transform.localScale.y);
        }
    }


    private void OnUpdateDiscardMessage(bool enable, int count)
    {
        if (discardMessage.activeSelf != enable)
            discardMessage.SetActive(enable);

        text_DiscardMessage.text = discardMessageText.Replace("?", count.ToString());
    }


    private void OnShowGameOverScreen(bool isEnemy)
    {
        gameOverScreen.SetActive(true);
        text_GameOver.text = text_GameOver.text.Replace("?", isEnemy ? "ENEMY" : "PLAYER");
    }


    private void OnAddStatusEffect(ST_ActiveStatusEffect statusEffect, bool isPlayer)
    {
        GameObject statusIcon = Instantiate(statusEffectIcon, isPlayer ? playerStatuses : enemyStatuses);
        statusIcon.GetComponent<Image>().sprite = statusEffect.statusEffect.EffectIcon;

        switch (statusEffect.statusEffect.EffectStacking)
        {
            case E_EffectStackingType.DURATION:
                statusIcon.GetComponentInChildren<TMP_Text>().text = statusEffect.duration.ToString();
                break;

            case E_EffectStackingType.INTENSITY:
                statusIcon.GetComponentInChildren<TMP_Text>().text = statusEffect.stacks.ToString();
                break;

            default:
                Debug.LogError("No effect stacking has been defined!");
                break;
        }

        if (isPlayer)
            playerStatusIcons.Add(statusIcon);
        else
            enemyStatusIcons.Add(statusIcon);
    }


    private void OnUpdateStatusEffect(bool remove, ST_ActiveStatusEffect statusEffect, bool isPlayer)
    {
        int statusIconIndex = -1;

        for (int i = 0; i < (isPlayer ? playerStatusIcons.Count : enemyStatusIcons.Count); i++)
        {
            if (isPlayer)
            {
                if (playerStatusIcons[i].GetComponent<Image>().sprite == statusEffect.statusEffect.EffectIcon)
                    statusIconIndex = i;
            }
            else
            {
                if (enemyStatusIcons[i].GetComponent<Image>().sprite == statusEffect.statusEffect.EffectIcon)
                    statusIconIndex = i;
            }
        }

        if (statusIconIndex < 0) return;
        GameObject target = isPlayer ? playerStatusIcons[statusIconIndex] : enemyStatusIcons[statusIconIndex];

        if (remove)
        {

            if (isPlayer)
                playerStatusIcons.RemoveAt(statusIconIndex);
            else
                enemyStatusIcons.RemoveAt(statusIconIndex);

            Destroy(target);
        }
        else
        {
            switch (statusEffect.statusEffect.EffectStacking)
            {
                case E_EffectStackingType.DURATION:
                    target.GetComponentInChildren<TMP_Text>().text = statusEffect.duration.ToString();
                    break;

                case E_EffectStackingType.INTENSITY:
                    target.GetComponentInChildren<TMP_Text>().text = statusEffect.stacks.ToString();
                    break;

                default:
                    Debug.LogError("No effect stacking has been defined!");
                    break;
            }
        }
    }
    #endregion
}
