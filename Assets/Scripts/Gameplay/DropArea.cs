using UnityEngine;
using UnityEngine.EventSystems;


public class DropArea : MonoBehaviour, IDropHandler
{
    #region Variables & Properties

    #region Local
    ActiveCard lastDroppedCard;
    #endregion

    #endregion


    #region Methods
    public void OnDrop(PointerEventData eventData)
    {
        if (!GameManager.CanPlay || GameManager.CardsToDiscard > 0) return;

        if (eventData.pointerDrag.gameObject.TryGetComponent<ActiveCard>(out lastDroppedCard))
            EventManager.PlayCard?.Invoke(lastDroppedCard);
    }
    #endregion
}
