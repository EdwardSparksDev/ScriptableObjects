using UnityEngine;
using UnityEngine.EventSystems;


public class DiscardArea : MonoBehaviour, IDropHandler
{
    #region Variables & Properties

    #region Local
    ActiveCard lastDroppedCard;
    #endregion

    #endregion


    #region Methods
    public void OnDrop(PointerEventData eventData)
    {
        if (!GameManager.CanPlay || GameManager.CardsToDiscard <= 0) return;

        if (eventData.pointerDrag.gameObject.TryGetComponent<ActiveCard>(out lastDroppedCard))
        {
            GameManager.CardsToDiscard--;
            EventManager.DiscardCardFromHand?.Invoke(lastDroppedCard);
        }
    }
    #endregion
}
