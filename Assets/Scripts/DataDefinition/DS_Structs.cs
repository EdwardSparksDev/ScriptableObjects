[System.Serializable]
public struct ST_UpgradeAmount
{
    public int defaultAmount;
    public int upgradedAmount;
}


[System.Serializable]
public struct ST_UpgradeStatusEffect
{
    public E_TargetType target;
    public bool remove;
    public ST_UpgradeAmount stacks;
    public StatusEffectData statusEffect;
}


public struct ST_ActiveStatusEffect
{
    public int duration;
    public int stacks;
    public StatusEffectData statusEffect;
}