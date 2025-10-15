using System.Collections.Generic;
using UnityEngine;


public class ActiveCharacter
{
    #region Variables & Properties

    #region Local
    CharacterData charData;

    int hp;
    int energy;
    int block;

    List<ST_ActiveStatusEffect> activeStatusEffects;
    #endregion

    #region Properties
    public CharacterData CharData => charData;
    public int Hp => hp;
    public int Block => block;
    public int Energy => energy;
    public List<ST_ActiveStatusEffect> ActiveStatusEffects => activeStatusEffects;
    #endregion

    #endregion


    #region Methods
    public ActiveCharacter(CharacterData newCharData)
    {
        charData = newCharData;
        hp = charData.Hp;
        energy = charData.MaxStartingEnergy;
        activeStatusEffects = new List<ST_ActiveStatusEffect>();
    }


    public void ApplyStatusEffects(ST_UpgradeStatusEffect newStatusEffect, bool isUpgraded, bool isPlayer)
    {
        bool found = false;
        for (int i = 0; i < activeStatusEffects.Count && !found; i++)
        {
            if (activeStatusEffects[i].statusEffect == newStatusEffect.statusEffect)
            {
                found = true;
                IncreaseStatusEffect(newStatusEffect, i, isUpgraded, isPlayer);
            }
        }

        if (!found)
            AddNewStatusEffect(newStatusEffect, isUpgraded, isPlayer);
    }


    private void IncreaseStatusEffect(ST_UpgradeStatusEffect newStatusEffect, int index, bool isUpgraded, bool isPlayer)
    {
        int updatedDuration = 0;
        int updatedStacks = 0;

        switch (newStatusEffect.statusEffect.EffectStacking)
        {
            case E_EffectStackingType.DURATION:
                updatedDuration = isUpgraded ? newStatusEffect.stacks.upgradedAmount : newStatusEffect.stacks.defaultAmount;
                updatedDuration *= newStatusEffect.remove ? -1 : 1;
                break;

            case E_EffectStackingType.INTENSITY:
                updatedStacks = isUpgraded ? newStatusEffect.stacks.upgradedAmount : newStatusEffect.stacks.defaultAmount;
                updatedStacks *= newStatusEffect.remove ? -1 : 1;
                break;

            default:
                Debug.LogError("No effect stacking has been defined!");
                break;
        }

        updatedDuration += activeStatusEffects[index].duration;
        updatedStacks += activeStatusEffects[index].stacks;

        if (newStatusEffect.statusEffect.EffectStacking == E_EffectStackingType.DURATION && updatedDuration <= 0)
        {
            activeStatusEffects.RemoveAt(index);
            EventManager.UpdateStatusEffectIcon?.Invoke(true, activeStatusEffects[index], isPlayer);
        }
        else
        {
            ST_ActiveStatusEffect newActiveStatusEffect = new ST_ActiveStatusEffect()
            {
                duration = updatedDuration,
                stacks = updatedStacks,
                statusEffect = newStatusEffect.statusEffect
            };

            activeStatusEffects[index] = newActiveStatusEffect;
            EventManager.UpdateStatusEffectIcon?.Invoke(false, newActiveStatusEffect, isPlayer);
        }
    }



    private void AddNewStatusEffect(ST_UpgradeStatusEffect newStatusEffect, bool isUpgraded, bool isPlayer)
    {
        int duration = 0;
        int stacks = 0;

        switch (newStatusEffect.statusEffect.EffectStacking)
        {
            case E_EffectStackingType.DURATION:
                duration = isUpgraded ? newStatusEffect.stacks.upgradedAmount : newStatusEffect.stacks.defaultAmount;
                duration *= newStatusEffect.remove ? -1 : 1;
                if (duration <= 0)
                    return;
                break;

            case E_EffectStackingType.INTENSITY:
                stacks = isUpgraded ? newStatusEffect.stacks.upgradedAmount : newStatusEffect.stacks.defaultAmount;
                stacks *= newStatusEffect.remove ? -1 : 1;
                break;

            default:
                Debug.LogError("No effect stacking has been defined!");
                break;
        }

        ST_ActiveStatusEffect newActiveStatusEffect = new ST_ActiveStatusEffect()
        {
            duration = duration,
            stacks = stacks,
            statusEffect = newStatusEffect.statusEffect
        };

        activeStatusEffects.Add(newActiveStatusEffect);
        EventManager.AddStatusEffectIcon?.Invoke(newActiveStatusEffect, isPlayer);
    }


    public void TickActiveStatusEffects(bool isPlayer)
    {
        List<ST_ActiveStatusEffect> trash = new List<ST_ActiveStatusEffect>();

        int count = activeStatusEffects.Count;
        for (int i = 0; i < count; i++)
        {
            if (activeStatusEffects[i].statusEffect.EffectStacking == E_EffectStackingType.DURATION)
            {
                ST_ActiveStatusEffect tickedActiveStatusEffect = activeStatusEffects[i];
                tickedActiveStatusEffect.duration--;
                activeStatusEffects[i] = tickedActiveStatusEffect;

                if (tickedActiveStatusEffect.duration <= 0)
                {
                    EventManager.UpdateStatusEffectIcon?.Invoke(true, tickedActiveStatusEffect, isPlayer);
                    trash.Add(tickedActiveStatusEffect);
                }
                else
                {
                    EventManager.UpdateStatusEffectIcon?.Invoke(false, tickedActiveStatusEffect, isPlayer);
                    activeStatusEffects[i] = tickedActiveStatusEffect;
                }
            }
        }

        for (int i = 0; i < trash.Count; i++)
            activeStatusEffects.Remove(trash[i]);
    }


    public void ResetEnergy(int additionalEnergy)
    {
        energy = charData.MaxStartingEnergy + additionalEnergy;
        EventManager.UpdateEnergyDisplay?.Invoke(energy, charData.MaxStartingEnergy);
    }


    public void ApplyEnergy(int addedEnergy)
    {
        energy = Mathf.Clamp(energy + addedEnergy, 0, int.MaxValue);
        EventManager.UpdateEnergyDisplay?.Invoke(energy, charData.MaxStartingEnergy);
    }


    public void ResetBlock()
    {
        block = 0;
        EventManager.UpdateBlockDisplay?.Invoke(block);
    }


    public void ApplyBlock(int appliedBlock, bool blockMultiplied)
    {
        int dexterity = 0;

        if (appliedBlock > 0)
        {
            EventManager.PlayBlockAudio?.Invoke();

            if (!blockMultiplied)
            {
                /*Tmp code block*/
                for (int i = 0; i < activeStatusEffects.Count; i++)
                {
                    if (activeStatusEffects[i].statusEffect.EffectName == "Dexterity")
                    {
                        dexterity = activeStatusEffects[i].stacks;
                        break;
                    }
                }
            }
        }

        block = Mathf.Clamp(block + appliedBlock + dexterity, 0, int.MaxValue);
        EventManager.UpdateBlockDisplay?.Invoke(block);
    }


    public void ApplyDamage(bool isPlayer, int damage, bool bypassBlock = false)
    {
        if (damage > 0)
        {
            int extraDamageTaken = 0;
            if (GetVulnerable(out extraDamageTaken))
            {
                Debug.Log("Receiving damage: " + damage + " (+ " + (int)((float)damage / 100 * extraDamageTaken) + ")");

                damage += (int)((float)damage / 100 * extraDamageTaken);
            }
        }

        if (bypassBlock || !isPlayer)
        {
            if (damage > 0)
                EventManager.PlayHitAudio?.Invoke();
            hp = Mathf.Clamp(hp - damage, 0, int.MaxValue);
        }
        else
        {
            int lastBlock = block;
            int damageReceived = Mathf.Clamp(damage - block, 0, int.MaxValue);
            block = Mathf.Clamp(block - damage, 0, int.MaxValue);
            hp = Mathf.Clamp(hp - damageReceived, 0, int.MaxValue);

            if (damage > 0)
            {
                if (damageReceived > 0)
                {
                    if (lastBlock > 0)
                        EventManager.PlayBlockAudio?.Invoke();
                    EventManager.PlayHitAudio?.Invoke();
                }
                else
                    EventManager.PlayBlockAudio?.Invoke();
                EventManager.UpdateBlockDisplay?.Invoke(block);
            }
        }

        EventManager.UpdateHealth?.Invoke(isPlayer, hp, charData.Hp);
        if (hp <= 0)
            EventManager.EndGame?.Invoke();
    }


    public int GetDamage()
    {
        int damageDealt = charData.GetDamage();
        int extraDamageDealtPercentage;

        if (GetWeak(out extraDamageDealtPercentage))
        {
            Debug.Log("Dealing " + damageDealt + " + " + GetStrength() + " damage + " + (int)(((float)damageDealt + GetStrength()) / 100 * extraDamageDealtPercentage) +
                ". TOT: " + Mathf.Clamp(damageDealt + GetStrength() + (int)(((float)damageDealt + GetStrength()) / 100 * extraDamageDealtPercentage), 1, int.MaxValue));

            return Mathf.Clamp(damageDealt + GetStrength() + (int)(((float)damageDealt + GetStrength()) / 100 * extraDamageDealtPercentage), 1, int.MaxValue);
        }
        else
        {
            Debug.Log("Dealing " + damageDealt + " + " + GetStrength() + " damage");

            return Mathf.Clamp(damageDealt + GetStrength(), 1, int.MaxValue);
        }
    }


    public int GetStrength()
    {
        int strength = 0;
        /*Tmp code block*/
        for (int i = 0; i < activeStatusEffects.Count; i++)
        {
            if (activeStatusEffects[i].statusEffect.EffectName == "Strength")
            {
                strength = activeStatusEffects[i].stacks;
                break;
            }
        }

        return strength;
    }


    private bool GetVulnerable(out int value)
    {
        value = 0;
        /*Tmp code block*/
        for (int i = 0; i < activeStatusEffects.Count; i++)
        {
            if (activeStatusEffects[i].statusEffect.EffectName == "Vulnerable")
            {
                value = activeStatusEffects[i].statusEffect.DamageTakenIncreasePercentage;
                return true;
            }
        }

        return false;
    }


    private bool GetWeak(out int value)
    {
        value = 0;
        /*Tmp code block*/
        for (int i = 0; i < activeStatusEffects.Count; i++)
        {
            if (activeStatusEffects[i].statusEffect.EffectName == "Weak")
            {
                value = activeStatusEffects[i].statusEffect.DamageDealtIncreasePercentage;
                return true;
            }
        }

        return false;
    }
    #endregion
}
