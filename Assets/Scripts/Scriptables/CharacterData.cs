using UnityEngine;


[CreateAssetMenu(menuName = "Gameplay/Character", fileName = "CharacterName_Data")]

public class CharacterData : ScriptableObject
{
    #region Variables & Properties

    #region SerializeField
    [Header("Stats")]
    [SerializeField] int hp;
    [Space(10), SerializeField] int minDamage;
    [SerializeField] int maxDamage;
    [Space(10), SerializeField] int maxStartingEnergy;

    [Space(25), Header("Graphics")]
    [SerializeField] Sprite mainSprite;
    #endregion

    #region Properties
    public int Hp => hp;
    public int MaxStartingEnergy => maxStartingEnergy;
    public Sprite MainSprite => mainSprite;
    #endregion


    #region Methods
    public int GetDamage()
    {
        return Random.Range(minDamage, maxDamage + 1);
    }
    #endregion

    #endregion
}
