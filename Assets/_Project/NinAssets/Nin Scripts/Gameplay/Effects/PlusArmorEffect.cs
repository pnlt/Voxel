using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu (fileName = "Effect", menuName = "Data/Effect/Plus armor")]
public class PlusArmorEffect : Effect, IOutDateEffect
{
    [SerializeField] private int armorPlus;

    public override void BuffEffect(PlayerData stat)
    {
        //plus armor
        stat.armor = armorPlus;
    }

    public void UpdateEffectUse(PlayerData data)
    {
        if (data.armor <= 0)
        {
            StartCooldown(data).Forget();
        }
    }
}