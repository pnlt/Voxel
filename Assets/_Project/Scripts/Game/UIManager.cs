using TMPro;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack._Project.Scripts.Game
{
    public class UIManager : MonoBehaviour

    {
        [SerializeField] private TextMeshProUGUI playersInGameText;

        private void Update()
        {
            playersInGameText.text = $"Players in game: {PlayersManager.Instance.PlayersInGame}";
        }
    }
}