using UnityEngine;


public class AudioManager : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    AudioSource[] sources;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        sources = GetComponents<AudioSource>();
    }


    private void OnEnable()
    {
        EventManager.PlayCardDrawnAudio += OnCardDrawn;
        EventManager.PlayHitAudio += OnHit;
        EventManager.PlayBlockAudio += OnBlock;
        EventManager.PlayNextTurnAudio += OnNextTurn;
    }


    private void OnDisable()
    {
        EventManager.PlayCardDrawnAudio -= OnCardDrawn;
        EventManager.PlayHitAudio -= OnHit;
        EventManager.PlayBlockAudio -= OnBlock;
        EventManager.PlayNextTurnAudio -= OnNextTurn;
    }
    #endregion


    #region Methods
    private void OnCardDrawn()
    {
        sources[1].Play();
    }


    private void OnHit()
    {
        sources[2].Play();
    }


    private void OnBlock()
    {
        sources[3].Play();
    }


    public void OnNextTurn()
    {
        sources[4].Play();
    }
    #endregion
}
