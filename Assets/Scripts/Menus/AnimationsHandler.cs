using System.Collections;
using UnityEngine;


public class AnimationsHandler : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    RectTransform dropAreaRT;
    RectTransform discardPileRT;

    WaitForSeconds animTickrateDelay;
    WaitForSeconds useCardAnimTimeDelay;
    WaitForSeconds usingCardAnimDelay;
    WaitForSeconds discardCardAnimTimeDelay;

    Vector2 cardFinalScale;
    #endregion

    #region SerializeField
    [Space(25), Header("Animation Settings")]
    [SerializeField] float animationTickrate;

    [Space(10), SerializeField] float useCardAnimTime;
    [SerializeField] AnimationCurve useCardAnimSpeed;
    [SerializeField] float usingCardAnimDelayTime;
    [SerializeField] float discardCardAnimTime;
    [SerializeField] AnimationCurve discardCardAnimSpeed;
    [SerializeField] float discardedCardFinalScale;

    [Space(25), Header("References")]
    [SerializeField] GameObject dropArea;
    [SerializeField] GameObject drawPile;
    [SerializeField] GameObject discardPile;
    [SerializeField] RectTransform handRT;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        dropAreaRT = dropArea.GetComponent<RectTransform>();
        discardPileRT = discardPile.GetComponent<RectTransform>();

        animTickrateDelay = new WaitForSeconds(animationTickrate);
        useCardAnimTimeDelay = new WaitForSeconds(useCardAnimTime * animationTickrate);
        usingCardAnimDelay = new WaitForSeconds(usingCardAnimDelayTime);
        discardCardAnimTimeDelay = new WaitForSeconds(discardCardAnimTime * animationTickrate);

        cardFinalScale = new Vector2(discardedCardFinalScale, discardedCardFinalScale);
    }


    private void OnEnable()
    {
        EventManager.StartCardPlayedAnimation += OnStartCardPlayedAnimation;
        EventManager.StartCardDiscardedAnimation += OnStartCardDiscardedAnimation;
    }


    private void OnDisable()
    {
        EventManager.StartCardPlayedAnimation -= OnStartCardPlayedAnimation;
        EventManager.StartCardDiscardedAnimation -= OnStartCardDiscardedAnimation;
    }
    #endregion


    #region Methods
    private void OnStartCardPlayedAnimation(ActiveCard card)
    {
        StartCoroutine(CardPlayedAnimationCR(card));
    }


    private void OnStartCardDiscardedAnimation(ActiveCard card)
    {
        StartCoroutine(CardDiscardedAnimationCR(card));
    }


    private IEnumerator CardPlayedAnimationCR(ActiveCard card)
    {
        RectTransform cardRT = card.GetComponent<RectTransform>();
        float t = 0;
        Vector2 dropPosition = cardRT.position;

        while (t < useCardAnimTime)
        {
            t += useCardAnimTime * animationTickrate;
            cardRT.position = Vector2.Lerp(dropPosition, dropAreaRT.position, useCardAnimSpeed.Evaluate(t / useCardAnimTime));
            yield return useCardAnimTimeDelay;
        }

        yield return usingCardAnimDelay;

        yield return StartCoroutine(CardDiscardedAnimationCR(card));
    }


    private IEnumerator CardDiscardedAnimationCR(ActiveCard card)
    {
        RectTransform cardRT = card.GetComponent<RectTransform>();
        float t = 0;
        Vector2 startingPosition = cardRT.position;
        Vector2 startingScale = cardRT.transform.localScale;

        while (t < discardCardAnimTime)
        {
            t += discardCardAnimTime * animationTickrate;
            cardRT.position = Vector2.Lerp(startingPosition, discardPileRT.position, discardCardAnimSpeed.Evaluate(t / discardCardAnimTime));
            cardRT.transform.localScale = Vector2.Lerp(startingScale, cardFinalScale, t / discardCardAnimTime);
            yield return discardCardAnimTimeDelay;
        }

        cardRT.transform.localScale = startingScale;
        cardRT.gameObject.SetActive(false);
        EventManager.RequestDiscardPileUpdate?.Invoke(card);
    }
    #endregion
}
