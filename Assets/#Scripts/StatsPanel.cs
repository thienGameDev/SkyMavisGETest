using System.Collections;
using TMPro;
using UnityEngine;

namespace _Scripts {
    public class StatsPanel : MonoBehaviour {
        private const float DURATION = 5f;

        public AxieController followObject;

        //UI stats panel
        public TMP_Text typeText;
        public TMP_Text randNoText;
        public TMP_Text damageText;
        public TMP_Text hitPointText;

        private void Update() {
            if (followObject) UpdateStatsPanel();
        }

        private void OnEnable() {
            StartCoroutine(SelfDisappear());
        }

        private IEnumerator SelfDisappear() {
            yield return new WaitForSeconds(DURATION);
            gameObject.SetActive(false);
        }

        private void UpdateStatsPanel() {
            typeText.text = followObject.gameObject.tag;
            randNoText.text = $"Rand No.            {followObject.RandomNumber}";
            damageText.text = $"Damage             {followObject.damage}";
            var currentHpText = followObject.CurrentHitPoint < 10 ? " " + followObject.CurrentHitPoint : followObject.CurrentHitPoint.ToString();
            hitPointText.text = $"Hit Point       {currentHpText}/{followObject.maxHitPoint}";
            transform.position = followObject.gameObject.transform.position + new Vector3(.5f ,.5f, 0f);
        }
    }
}
