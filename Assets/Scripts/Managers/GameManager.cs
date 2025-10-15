using System.Collections;
using System.Collections.Generic;
using TypeExtensions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    ActiveCharacter player;
    ActiveCharacter enemy;

    Queue<ActiveCard> drawPile = new Queue<ActiveCard>();
    HashSet<ActiveCard> handCards = new HashSet<ActiveCard>();
    Stack<ActiveCard> discardPile = new Stack<ActiveCard>();
    List<ActiveCard> shuffler = new List<ActiveCard>();
    int exhaustPileCounter;

    WaitForSeconds cardDrawDelay;
    WaitForSeconds newTurnDelay;

    static bool canPlay;
    static int cardsToDiscard;

    bool upgradeCards;
    int blockClearTimer;
    int counterDamage;
    int additionalEnergy;
    int flatEnergyCost = -1;
    #endregion 

    #region SerializeField
    [Header("Match Settings")]
    [SerializeField] CardsDeckData deckData;
    [SerializeField] CharacterData playerData;
    [SerializeField] CharacterData enemyData;

    [Space(25), Header("Game Settings")]
    [SerializeField] int cardsDrawnPerTurn;
    [SerializeField] float drawDelay;
    [SerializeField] float newTurnDelayTime;

    [Space(25), Header("References")]
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform handTransform;
    #endregion

    #region Properties
    public static bool CanPlay => canPlay;
    public static int CardsToDiscard { get => cardsToDiscard; set => cardsToDiscard = value; }
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        cardDrawDelay = new WaitForSeconds(drawDelay);
        newTurnDelay = new WaitForSeconds(newTurnDelayTime);

        player = new ActiveCharacter(playerData);
        enemy = new ActiveCharacter(enemyData);
        EventManager.UpdateCharacterUI?.Invoke(playerData, true);
        EventManager.UpdateCharacterUI?.Invoke(enemyData, false);

        StartMatch();
    }


    private void OnEnable()
    {
        EventManager.RequestHandUpdate += OnRequestHandUpdate;
        EventManager.RequestDiscardPileUpdate += OnRequestDiscardPileUpdate;
        EventManager.PlayCard += OnPlayCard;
        EventManager.DiscardCardFromHand += OnDiscardCardFromHand;
        EventManager.EndGame += OnGameOver;
    }


    private void OnDisable()
    {
        EventManager.RequestHandUpdate -= OnRequestHandUpdate;
        EventManager.RequestDiscardPileUpdate -= OnRequestDiscardPileUpdate;
        EventManager.PlayCard -= OnPlayCard;
        EventManager.DiscardCardFromHand -= OnDiscardCardFromHand;
        EventManager.EndGame -= OnGameOver;
    }
    #endregion


    #region Methods
    private void StartMatch()
    {
        EventManager.UpdateEndTurnButton?.Invoke(false);
        SpawnAllCards();
        shuffler.Shuffle();
        FillDrawPile();
        StartCoroutine(DrawCards(cardsDrawnPerTurn));
    }


    private void SpawnAllCards()
    {
        for (int i = 0; i < deckData.Cards.Count; i++)
        {
            GameObject obj = Instantiate(cardPrefab);
            EventManager.LinkCardToHand?.Invoke(obj);

            ActiveCard ac = obj.GetComponent<ActiveCard>();
            ac.InitializeCard(deckData.Cards[i]);

            shuffler.Add(ac);
            ac.gameObject.SetActive(false);
        }
    }


    private void FillDrawPile()
    {
        for (int i = 0; i < shuffler.Count; i++)
            drawPile.Enqueue(shuffler[i]);
        shuffler.Clear();

        EventManager.UpdateDrawPileCardsCount?.Invoke(drawPile.Count);
        EventManager.UpdateDiscardPileCardsCount?.Invoke(discardPile.Count);
    }


    private IEnumerator DrawCards(int cardsCount)
    {
        for (int i = 0; i < cardsCount; i++)
        {
            if (drawPile.Count <= 0)
                RecycleDiscardPile();
            yield return cardDrawDelay;
            DrawCard();
        }

        yield return cardDrawDelay;

        canPlay = true;
        if (cardsToDiscard <= 0)
            EventManager.UpdateEndTurnButton?.Invoke(true);
    }


    private void DrawCard()
    {
        EventManager.PlayCardDrawnAudio?.Invoke();

        ActiveCard ac = drawPile.Dequeue();
        handCards.Add(ac);
        ac.gameObject.SetActive(true);
        ac.transform.SetAsLastSibling();
        ac.ResetScale();

        if (upgradeCards)
            ac.UpgradeCard();

        ac.UpdateEnergyCostDisplay(flatEnergyCost);

        EventManager.SaveCardsSiblingIndexes?.Invoke();
        EventManager.UpdateHandCardsDisplay?.Invoke(handCards);
        EventManager.UpdateDrawPileCardsCount?.Invoke(drawPile.Count);
    }


    private void OnRequestHandUpdate(ActiveCard card, bool add)
    {
        if (add)
            handCards.Add(card);
        else
            handCards.Remove(card);

        EventManager.UpdateHandCardsDisplay?.Invoke(handCards);
    }


    private void OnPlayCard(ActiveCard card)
    {
        if ((card.GetEnergyCost() <= player.Energy) || (flatEnergyCost >= 0 && (player.Energy >= flatEnergyCost)))
        {
            if (flatEnergyCost >= 0 && card.GetEnergyCost() > flatEnergyCost)
                player.ApplyEnergy(-flatEnergyCost);
            else
                player.ApplyEnergy(-card.GetEnergyCost());
            ApplyCardEffects(card);

            card.HasBeenPlayed = true;
            card.ResetScale();

            EventManager.SaveCardsSiblingIndexes?.Invoke();
            EventManager.StartCardPlayedAnimation?.Invoke(card);
        }
    }


    private void OnDiscardCardFromHand(ActiveCard card)
    {
        card.Discarded = true;
        EventManager.StartCardDiscardedAnimation?.Invoke(card);
        EventManager.SaveCardsSiblingIndexes?.Invoke();
        EventManager.UpdateDiscardMessage?.Invoke(cardsToDiscard > 0, cardsToDiscard);
    }


    private void ApplyCardEffects(ActiveCard card)
    {
        TryUpgradingCards(card);
        TryApplyingBlock(card);
        TryApplyingDamage(card);
        TryAlteringEnergy(card);
        TryApplyingEffects(card);
        TryDrawingAndDiscardingCards(card);
    }


    private void TryApplyingEffects(ActiveCard card)
    {
        if (card.GetStatusEffects() == null) return;

        List<ST_UpgradeStatusEffect> effects = card.GetStatusEffects();
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i].target == E_TargetType.SELF)
                player.ApplyStatusEffects(effects[i], card.IsUpgraded, true);
            else
                enemy.ApplyStatusEffects(effects[i], card.IsUpgraded, false);
        }
    }


    private void TryAlteringEnergy(ActiveCard card)
    {
        additionalEnergy += card.GetLatentEnergy();
        if (card.GetFlatCostReduction() >= 0)
            flatEnergyCost = card.GetFlatCostReduction();

        if (flatEnergyCost >= 0)
        {
            foreach (ActiveCard heldCard in handCards)
                heldCard.UpdateEnergyCostDisplay(flatEnergyCost);
        }
    }


    private void TryApplyingDamage(ActiveCard card)
    {
        int damageDealt;
        int recoilDamage;

        card.LoadDamage(player, enemy, out damageDealt, out recoilDamage, ref counterDamage);
        if (player.GetStrength() > 0)
            enemy.ApplyDamage(false, damageDealt + player.GetStrength());
        else
            enemy.ApplyDamage(false, damageDealt);
        player.ApplyDamage(true, recoilDamage, true);
    }


    private void TryDrawingAndDiscardingCards(ActiveCard card)
    {
        if (card.GetCardsToDraw() > 0)
        {
            canPlay = false;
            EventManager.UpdateEndTurnButton?.Invoke(false);
            StartCoroutine(DrawCards(card.GetCardsToDraw()));
        }

        if (card.GetCardsToDiscard() > 0)
        {
            cardsToDiscard = card.GetCardsToDiscard();
            EventManager.UpdateEndTurnButton?.Invoke(false);
            EventManager.UpdateDiscardMessage?.Invoke(true, cardsToDiscard);
        }
    }


    private void TryUpgradingCards(ActiveCard card)
    {
        if (card.PerformsUpgrade())
        {
            upgradeCards = true;

            foreach (ActiveCard heldCard in handCards)
                heldCard.UpgradeCard();
        }
    }


    private void TryApplyingBlock(ActiveCard card)
    {
        bool blockMultiplied;
        player.ApplyBlock(card.GetBlock(player.Block, out blockMultiplied), blockMultiplied);
        blockClearTimer += card.GetBlockClearDelay();
    }


    private void OnRequestDiscardPileUpdate(ActiveCard card)
    {
        if (card.IsExhaustable() && card.HasBeenPlayed)
        {
            Destroy(card.gameObject);
            EventManager.UpdateExhaustPileCardsCount?.Invoke(++exhaustPileCounter);
        }
        else
        {
            discardPile.Push(card);
            EventManager.UpdateDiscardPileCardsCount?.Invoke(discardPile.Count);
        }

        if (cardsToDiscard <= 0)
        {
            EventManager.UpdateDiscardMessage?.Invoke(false, cardsToDiscard);
            EventManager.UpdateEndTurnButton?.Invoke(true);
        }
    }


    public void EndTurn()
    {
        StartCoroutine(EndTurnCR());
    }


    private IEnumerator EndTurnCR()
    {
        canPlay = false;
        EventManager.UpdateEndTurnButton?.Invoke(false);

        EventManager.PlayNextTurnAudio?.Invoke();

        DiscardCards(handCards);

        yield return newTurnDelay;
        player.ApplyDamage(true, enemy.GetDamage());
        enemy.ApplyDamage(false, counterDamage);
        yield return newTurnDelay;

        ResetEnergy();
        ResetBlock();
        counterDamage = 0;
        player.TickActiveStatusEffects(true);
        enemy.TickActiveStatusEffects(false);

        StartCoroutine(DrawCards(cardsDrawnPerTurn));
    }


    private void OnGameOver()
    {
        StartCoroutine(GameOverCR());
    }


    private IEnumerator GameOverCR()
    {
        canPlay = false;
        EventManager.UpdateEndTurnButton?.Invoke(false);
        EventManager.ShowGameOverScreen?.Invoke(player.Hp <= 0);
        yield return new WaitForSeconds(4);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    private void ResetEnergy()
    {
        player.ResetEnergy(additionalEnergy);
        additionalEnergy = 0;
        flatEnergyCost = -1;
    }


    private void ResetBlock()
    {
        if (--blockClearTimer < 0)
        {
            blockClearTimer = 0;
            player.ResetBlock();
        }
    }


    private void DiscardCards(HashSet<ActiveCard> cards)
    {
        foreach (ActiveCard card in cards)
        {
            card.ResetScale();
            EventManager.StartCardDiscardedAnimation?.Invoke(card);
        }

        cards.Clear();
    }


    private void RecycleDiscardPile()
    {
        ActiveCard card;
        int cardsToRecover = discardPile.Count;

        for (int i = 0; i < cardsToRecover; i++)
        {
            card = discardPile.Pop();
            card.HasBeenPlayed = false;
            card.Discarded = false;
            card.GetComponent<CanvasGroup>().blocksRaycasts = true;

            shuffler.Add(card);
        }

        shuffler.Shuffle();
        FillDrawPile();
    }


    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

        Application.Quit();
    }
    #endregion
}
