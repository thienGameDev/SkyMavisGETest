using System.Collections;
using System.Collections.Generic;
using AxieMixer.Unity;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace _Scripts {
    public class CharacterController : MonoBehaviour {
        [SerializeField] private float movementSpeed = 1f;
        [SerializeField] private AxieStateManager axieStateManager;
        public Tilemap map;
        public Vector3 positionOffset;
        private Camera _camera;
        private bool _isClosestEnemyFound;
        public bool isDefender;
        private bool _isSelected;
        private bool _isEnemyOnTheLeft;
        private bool _isMoving;
        private List<GameObject> _enemyList;
        private GameObject _closestEnemy;
        private Queue<Vector3Int> _pathToEnemy = new Queue<Vector3Int>();
        private Spawner _spawner;
        private void Awake() {
            Mixer.Init();
            _camera = Camera.main;
            _spawner = GameObject.FindWithTag("Spawner").GetComponent<Spawner>();
            
        }

        // Start is called before the first frame update
        private void Start() {
            _enemyList = isDefender ? _spawner.attackers : _spawner.defenders;
            StartCoroutine(CheckForAction());
        }

        // Update is called once per frame
        private void Update()
        {
            // if (Input.GetMouseButtonDown(0)) {
            //     MouseClick();
            // }
            
        }
        
        private IEnumerator CheckForAction() {
            while (true) {
                yield return new WaitForSeconds(1f);
                // Debug.Log("Check action");
                if (IsAdjacentEnemy(out GameObject enemy)) Attack(enemy);
                else {
                    if (!isDefender) {
                        if (!_isClosestEnemyFound) {
                            FindClosestEnemy();
                            FindPathToClosestEnemy();
                        }
                        else MoveToClosestEnemy();
                    }
                }
            }
        }

        private void MoveToClosestEnemy() {
            if (!_closestEnemy || _pathToEnemy.Count == 0) return;
            var targetPosition = _pathToEnemy.Dequeue();
            var axieAtTarget = GetAxieAt(targetPosition);
            if (axieAtTarget is not null && axieAtTarget.CompareTag("Attacker")) {
                // Change target
                _enemyList.Remove(_closestEnemy);
                _closestEnemy = null;
                _pathToEnemy.Clear();
                _isClosestEnemyFound = false;
                axieStateManager.SwitchState(axieStateManager.IdleState);
            }
            else {
                // keep moving there
                if (isTargetOnTheLeft(targetPosition)) {
                    axieStateManager.FlipAxie(1f);
                }
                else {
                    axieStateManager.FlipAxie(-1f);
                }
                axieStateManager.SwitchState(axieStateManager.WalkingState);
                Debug.Log($"{tag} moved to {targetPosition}");
                transform.position = map.CellToWorld(targetPosition) - positionOffset;
            }
        }

        private bool isTargetOnTheLeft(Vector3Int destination) {
            var currentPosition = map.WorldToCell(transform.position);
            var direction = destination - currentPosition;
            // Debug.Log($"Direction: {direction}");
            if (direction.x < 0 || direction.y > 0) return true;
            return false;
        }
        
        private bool IsAdjacentEnemy(out GameObject enemy) {
            enemy = null;
            var currentCell = map.WorldToCell(transform.position);
            var adjacentCells = GetAdjacentCells(currentCell);
            foreach (var cell in adjacentCells) {
                var axie = GetAxieAt(cell);
                if (axie is not null && !axie.CompareTag(tag)) {
                    enemy = axie;
                    return true;
                }
            }
            return false;
        }
        
        private void FindClosestEnemy() {
            var thisPosition = transform.position;
            float closestDistance = float.MaxValue;
            foreach (var enemy in _enemyList) {
                var enemyPosition = enemy.transform.position;
                var currentDistance = Vector2.Distance(thisPosition, enemyPosition);
                if (currentDistance <= closestDistance) {
                    closestDistance = currentDistance;
                    _closestEnemy = enemy;
                    _isClosestEnemyFound = true;
                }
            }
        }
        
        private void FindPathToClosestEnemy() {
            var enemyPosition = map.WorldToCell(_closestEnemy.transform.position);
            var adjacentCells = GetAdjacentCells(enemyPosition);
            Vector3Int targetPosition = isTargetOnTheLeft(enemyPosition) ? adjacentCells[0] : adjacentCells[1];
            var currentPosition = map.WorldToCell(transform.position);
            while (currentPosition != targetPosition) {
                var directionList = new List<Vector3Int>() {
                    new Vector3Int(currentPosition.x, currentPosition.y + 1, 0),
                    new Vector3Int(currentPosition.x, currentPosition.y - 1, 0),
                    new Vector3Int(currentPosition.x - 1, currentPosition.y, 0),
                    new Vector3Int(currentPosition.x + 1, currentPosition.y, 0)
                };
                float shortestDistance = float.MaxValue;
                var tempPath = new Vector3Int();
                // Check four direction
                foreach (var dir in directionList) {
                    if (map.HasTile(dir)) {
                        var distance = Vector3Int.Distance(dir, targetPosition);
                        if (distance <= shortestDistance) {
                            shortestDistance = distance;
                            tempPath = dir;
                        }
                    }
                }
                currentPosition = tempPath;
                _pathToEnemy.Enqueue(currentPosition);
            }
            
        }

        private List<Vector3Int> GetAdjacentCells(Vector3Int currentCell) {
            List<Vector3Int> adjacentCells = new List<Vector3Int>() {
                new Vector3Int(currentCell.x + 1, currentCell.y - 1, 0),
                new Vector3Int(currentCell.x - 1, currentCell.y + 1, 0)
            };
            return adjacentCells;
        }
        
        private void Attack(GameObject enemy) {
            axieStateManager.SwitchState(axieStateManager.AttackingState);
            var enemyPosition = map.WorldToCell(enemy.transform.position);
            if (isTargetOnTheLeft(enemyPosition)) 
                axieStateManager.FlipAxie(1f);
            else axieStateManager.FlipAxie(-1f);
                Debug.LogWarning($"Attack {enemy.tag}");
        }
        
        private GameObject GetAxieAt(Vector3Int position) {
            var worldPosition = map.GetCellCenterWorld(position);
            var screenPoint = _camera.WorldToScreenPoint(worldPosition);
            Ray ray = _camera.ScreenPointToRay(screenPoint);
            int layerMask = LayerMask.NameToLayer("AxieOnTile");
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, layerMask);
            if (hit.collider != null) {
                var axie = hit.collider.transform.parent.gameObject;
                if (axie.CompareTag("Attacker") || axie.CompareTag("Defender")) {
                    Debug.Log($"Found axie {axie.tag} at {position}");
                    return axie;
                }
            }
            return null;
        }
        
        // private void MouseClick() {
        //     Vector2 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        //     Vector3Int gridPosition = map.WorldToCell(mousePosition);
        //     map.GetTile(gridPosition);
        //     // Debug.Log($"MousePosition: {mousePosition} - GridPosition: {gridPosition}");
        //     if (map.HasTile(gridPosition)) {
        //         destination = map.CellToWorld(gridPosition) - positionOffset;
        //         if (isTargetOnTheLeft()) axieStateManager.FlipAxie(1f);
        //         else axieStateManager.FlipAxie(-1f);
        //     }
        // }

        // public void OnMouseDown() {
        //     _isSelected = !_isSelected;
        //     Debug.Log($"Axie {gameObject.tag} is selected: {_isSelected}");
        // }
        
    }
}
