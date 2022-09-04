using UnityEngine;

namespace _Scripts {
    public class PowerBarController : MonoBehaviour {
        [SerializeField] private RectTransform separator;
        [SerializeField] private RectTransform attackerPower;
        private float _attackerPowerBarAmount;
        private bool _isLoaded;
        private Spawner _spawner;
        private int _totalAttackerPower;
        private int _totalDefenderPower;

        private void Awake() {
            _spawner = Spawner.Instance;
            EventManager.StartListening("UpdatePower", UpdatePower);
            EventManager.StartListening("EndGame", EndGame);
        }

        private void Update() {
            if(!_isLoaded && _spawner.isReady) GetTotalMaxPower();
        }

        private void EndGame(int param) {
            _isLoaded = false;
        }

        private void GetTotalMaxPower() {
            _totalAttackerPower = _spawner.attackers.Count * _spawner.maxAttackerHp;
            _totalDefenderPower = _spawner.defenders.Count * _spawner.maxDefenderHp;
            _isLoaded = true;
        }

        private void UpdatePower(int value) {
            _totalAttackerPower += value;
            _totalDefenderPower -= value;
            _attackerPowerBarAmount = (float) _totalAttackerPower / (_totalAttackerPower + _totalDefenderPower);
            UpdatePowerBar();
        }

        private void UpdatePowerBar() {
            var localScale = attackerPower.localScale;
            localScale.x = _attackerPowerBarAmount;
            attackerPower.localScale = localScale;
            var separatorPosition = separator.anchoredPosition;
            separatorPosition.x = attackerPower.rect.width * _attackerPowerBarAmount;
            separator.anchoredPosition = separatorPosition;
        }
    }
}
