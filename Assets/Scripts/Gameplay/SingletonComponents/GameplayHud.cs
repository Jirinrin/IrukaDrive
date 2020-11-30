using TMPro;
using Tools.Commons;

namespace Gameplay.SingletonComponents
{
    public class GameplayHud : Singleton<GameplayHud>
    {
        public TMP_Text scoreField;

        private void OnScoreChange(int newScore) =>
            scoreField.text = newScore.ToString("D8");

        private void OnEnable()
        {
            GameplayManager.OnScoreChange += OnScoreChange;
        }
    }
}