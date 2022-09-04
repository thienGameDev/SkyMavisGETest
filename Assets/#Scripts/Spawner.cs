using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities;

namespace _Scripts {
    public class Spawner : StaticInstance<Spawner> {
        private const int MAX_AXIE_COUNT = 50;
        [SerializeField] private Tilemap map;
        [SerializeField] private Transform defenderParent;
        [SerializeField] private Transform attackerParent;
        [SerializeField] private GameObject defenderPrefab;
        [SerializeField] private GameObject attackerPrefab;

        //Debug
        public bool isReady;
        public List<GameObject> defenders;
        public List<GameObject> attackers;
        private List<Vector3Int> _axieExistingPositions = new List<Vector3Int>();
        private int _minX, _maxX, _minY, _maxY;

        protected override void Awake() {
            base.Awake();
            _minX = map.cellBounds.xMin;
            _maxX = map.cellBounds.xMax;
            _minY = map.cellBounds.yMin;
            _maxY = map.cellBounds.yMax;
        }

        public void SpawnAxies(int attackerCount, int defenderCount) {
            isReady = false;
            if (attackerCount > MAX_AXIE_COUNT) attackerCount = MAX_AXIE_COUNT;
            if (defenderCount > MAX_AXIE_COUNT) defenderCount = MAX_AXIE_COUNT;
            // Generate Defenders 
            GenerateAxie(defenderCount, defenderPrefab, defenderParent, defenders, "Defender");
            // Generate Attackers
            GenerateAxie(attackerCount, attackerPrefab, attackerParent, attackers, "Attacker");
            isReady = true;
        }
        // Start is called before the first frame update

        private void GenerateAxie(int axieCount, GameObject axiePrefab, Transform parent, List<GameObject> axieList, string type) {
            // Destroy current axies
            if (axieList.Count > 0) {
                foreach (var axie in axieList) {
                    DestroyImmediate(axie);
                }
                axieList.Clear();
                _axieExistingPositions.Clear();
            }
            
            //Generate new axies
            for (int i = 0; i < axieCount; i++) {
                var position = RandomizePosition();
                var axie = Instantiate(axiePrefab, parent);
                axie.name = $"{type}_{i}";
                var axieController = axie.GetComponent<AxieController>();
                axieController.map = map;
                var positionOffset = axieController.positionOffset;
                axie.transform.position = map.CellToWorld(position) - positionOffset;
                axieList.Add(axie);
            }
        }

        private Vector3Int RandomizePosition() {
            Vector3Int randomLocation = new Vector3Int {
                x = Random.Range(_minX, _maxX),
                y = Random.Range(_minY, _maxY)
            };
            if (map.HasTile(randomLocation) && !IsPositionExisting(randomLocation)) {
                _axieExistingPositions.Add(randomLocation);
                return randomLocation;
            }
            return RandomizePosition();
        }

        private bool IsPositionExisting(Vector3Int position) {
            return _axieExistingPositions.Contains(position);
        }
    }
}
