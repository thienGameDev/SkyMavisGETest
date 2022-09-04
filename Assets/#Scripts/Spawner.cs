using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities;

namespace _Scripts {
    public class Spawner : StaticInstance<Spawner> {
        private const int MAX_AXIE_COUNT = 100;
        [SerializeField] private Tilemap map;
        [SerializeField] private Transform defenderParent;
        [SerializeField] private Transform attackerParent;
        [SerializeField] private GameObject defenderPrefab;
        [SerializeField] private GameObject attackerPrefab;

        //Debug
        public bool isReady;
        public List<GameObject> defenders;
        public List<GameObject> attackers;
        private List<Vector3Int> _cellPositionList = new List<Vector3Int>();

        protected override void Awake() {
            base.Awake();
            GetAllCellPosition();
            ShuffleCells();
        }

        private void GetAllCellPosition() {
            foreach (var position in map.cellBounds.allPositionsWithin) {
                if (!map.HasTile(position)) continue;
                _cellPositionList.Add(position);
            }
        }

        private void ShuffleCells() {
            var rand = new System.Random();
            _cellPositionList = _cellPositionList.OrderBy(_ => rand.Next()).ToList();
        }

        public void SpawnAxies(int attackerCount, int defenderCount) {
            isReady = false;
            ShuffleCells();
            if (attackerCount > MAX_AXIE_COUNT) attackerCount = MAX_AXIE_COUNT;
            if (defenderCount > MAX_AXIE_COUNT) defenderCount = MAX_AXIE_COUNT;
            // Generate Defenders 
            GenerateAxie(defenderCount, defenderPrefab, defenderParent, defenders, "Defender");
            // Generate Attackers
            GenerateAxie(attackerCount, attackerPrefab, attackerParent, attackers, "Attacker");
            isReady = true;
        }

        private void GenerateAxie(int axieCount, GameObject axiePrefab, Transform parent, List<GameObject> axieList, string type) {
            // Destroy current axies
            if (axieList.Count > 0) {
                foreach (var axie in axieList) {
                    DestroyImmediate(axie);
                }
                axieList.Clear();
            }
            
            //Generate new axies
            for (int i = 0; i < axieCount; i++) {
                var position = _cellPositionList[i];
                var axie = Instantiate(axiePrefab, parent);
                axie.name = $"{type}_{i}";
                var axieController = axie.GetComponent<AxieController>();
                axieController.map = map;
                var positionOffset = axieController.positionOffset;
                axie.transform.position = map.CellToWorld(position) - positionOffset;
                axieList.Add(axie);
            }
        }
    }
}
