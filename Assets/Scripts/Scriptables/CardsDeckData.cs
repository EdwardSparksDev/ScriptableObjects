using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Gameplay/Deck", fileName = "DeckName_DeckData")]
public class CardsDeckData : ScriptableObject
{
    #region Variables & Properties

    #region SerializeField
    [Header("Contents")]
    [SerializeField] string deckName;

    [Space(10), SerializeField] List<CardData> cards;
    #endregion

    #region Properties
    public string DeckName => deckName;
    public List<CardData> Cards => cards;
    #endregion

    #endregion
}
