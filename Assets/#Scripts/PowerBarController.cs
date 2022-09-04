using UnityEngine;

namespace _Scripts {
    public class PowerBarController : MonoBehaviour {
        [SerializeField] private RectTransform separator;
        [SerializeField] private RectTransform attackerPower;
        private float _attackerPowerBarAmount;
        private Spawner _spawner;
        private int _totalAttackerPower;
        private int _totalDefenderPower;

        private void Awake() {
            _spawner = Spawner.Instance;
        }

        private void FixedUpdate() {
            if (!_spawner.isReady) return;
            GetTotalPower();
            UpdatePowerBar();
        }

        private void GetTotalPower() {
            _totalAttackerPower = _spawner.GetCurrentTeamHitPoint(_spawner.attackers);
            _totalDefenderPower = _spawner.GetCurrentTeamHitPoint(_spawner.defenders);
            _attackerPowerBarAmount = (float) _totalAttackerPower / (_totalAttackerPower + _totalDefenderPower);
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
