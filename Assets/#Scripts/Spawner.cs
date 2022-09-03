using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities;

namespace _Scripts {
    public class Spawner : StaticInstance<Spawner> {
        [SerializeField] private Tilemap map;
        [SerializeField] private Transform defenderParent;
        [SerializeField] private int defenderCount;
        [SerializeField] private Transform attackerParent;
        [SerializeField] private int attackerCount;
        [SerializeField] private GameObject defenderPrefab;
        [SerializeField] private GameObject attackerPrefab;
        
        //Debug
        public List<GameObject> defenders;
        public List<GameObject> attackers;
        private int _minX, _maxX, _minY, _maxY;
        private Camera _camera;
        public void SpawnAxies() {
            _camera = Camera.main;
            _minX = map.cellBounds.xMin;
            _maxX = map.cellBounds.xMax;
            _minY = map.cellBounds.yMin;
            _maxY = map.cellBounds.yMax;
            // Generate Defenders 
            GenerateAxie(defenderCount, defenderPrefab, defenderParent, defenders, "Defender");
            // Generate Attackers
            GenerateAxie(attackerCount, attackerPrefab, attackerParent, attackers, "Attacker");
        }
        // Start is called before the first frame update
        
        private void GenerateAxie(int axieCount, GameObject axiePrefab, Transform parent, List<GameObject> axieList, string type) {
            for (int i = 0; i < axieCount; i++) {
                var position = RandomizePosition();
                var axie = Instantiate(axiePrefab, parent);
                axie.name = $"{type}_{i}";
                var characterController = axie.GetComponentInChildren<AxieController>();
                characterController.map = map;
                var positionOffset = characterController.positionOffset;
                axie.transform.position = map.CellToWorld(position) - positionOffset;
                axieList.Add(axie);
            }
        }
        
        private Vector3Int RandomizePosition() {
            Vector3Int randomLocation = new Vector3Int {
                x = Random.Range(_minX, _maxX),
                y = Random.Range(_minY, _maxY)
            };
            if (map.HasTile(randomLocation) 
                && !IsPositionExisting(randomLocation, defenders) 
                    && !IsPositionExisting(randomLocation, attackers)) 
                        return randomLocation;
            return RandomizePosition();
        }

        private bool IsPositionExisting(Vector3Int position, List<GameObject> axieList) {
            foreach (var axie in axieList) {
                Vector3Int axiePosition = map.WorldToCell(axie.transform.position);
                if (Vector3Int.Distance(axiePosition, position) == 0) {
                    return true;
                }
            }
            return false;
        }
    }
}
