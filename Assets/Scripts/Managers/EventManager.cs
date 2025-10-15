using System;
using System.Collections.Generic;
using UnityEngine;


public static class EventManager
{
    #region Variables & Properties

    #region Local
    public static Action<GameObject> LinkCardToHand;
    public static Action<HashSet<ActiveCard>> UpdateHandCardsDisplay;
    public static Action SaveCardsSiblingIndexes;

    public static Action<CharacterData, bool> UpdateCharacterUI;
    public static Action<int> UpdateDrawPileCardsCount;
    public static Action<int> UpdateDiscardPileCardsCount;
    public static Action<int> UpdateExhaustPileCardsCount;
    public static Action<bool> UpdateEndTurnButton;

    public static Action<ActiveCard> StartCardPlayedAnimation;
    public static Action<ActiveCard> StartCardDiscardedAnimation;

    public static Action<ActiveCard, bool> RequestHandUpdate;
    public static Action<ActiveCard> RequestDiscardPileUpdate;
    public static Action<ActiveCard> PlayCard;
    public static Action<int, int> UpdateEnergyDisplay;
    public static Action<int> UpdateBlockDisplay;
    public static Action<ActiveCard> DiscardCardFromHand;
    public static Action<bool, int> UpdateDiscardMessage;
    public static Action<bool, int, int> UpdateHealth;

    public static Action PlayCardDrawnAudio;
    public static Action PlayHitAudio;
    public static Action PlayBlockAudio;
    public static Action PlayNextTurnAudio;

    public static Action EndGame;
    public static Action<bool> ShowGameOverScreen;

    public static Action<ST_ActiveStatusEffect, bool> AddStatusEffectIcon;
    public static Action<bool, ST_ActiveStatusEffect, bool> UpdateStatusEffectIcon;
    #endregion

    #endregion
}
