using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts {
    public class PowerBarController : MonoBehaviour {
        [SerializeField] private RectTransform separator;
        [SerializeField] private RectTransform attackerPower;
        private float _smoothTime = .5f;
        private Spawner _spawner;
        private int _totalAttackerPower;
        private int _totalDefenderPower;
        private Vector2 _vel2;
        private Vector3 _vel3;

        private void Awake() {
            _spawner = Spawner.Instance;
        }

        // Update is called once per frame
        private void Update()
        {
            _totalAttackerPower = GetTotalPower(_spawner.attackers);
            // Debug.LogWarning($"Total Attacker Power: {_totalAttackerPower}");
            _totalDefenderPower = GetTotalPower(_spawner.defenders);
            // Debug.LogWarning($"Total Defender Power {_totalDefenderPower}");
            if (_totalAttackerPower != 0 || _totalDefenderPower != 0) {
                UpdatePowerBar();
            }
        }

        private int GetTotalPower(List<GameObject> axieList) {
            int totalPower = 0;
            foreach (var axie in axieList) {
                var axieController = axie.GetComponent<AxieController>();
                var power = axieController.damage + axieController.CurrentHitPoint;
                totalPower += power;
            }
            return totalPower;
        }

        private void UpdatePowerBar() {
            var attackerPowerBarAmount = (float) _totalAttackerPower / (_totalAttackerPower + _totalDefenderPower);
            Debug.LogWarning(attackerPowerBarAmount);
            var localScale = attackerPower.localScale;
            localScale.x = attackerPowerBarAmount;
            attackerPower.localScale =
                Vector3.SmoothDamp(attackerPower.localScale, localScale, ref _vel3, _smoothTime);
            var separatorPosition = separator.anchoredPosition;
            separatorPosition.x = attackerPower.rect.width * attackerPowerBarAmount;
            separator.anchoredPosition =
                Vector2.SmoothDamp(separator.anchoredPosition, separatorPosition, ref _vel2, _smoothTime);
        }
    }
}
