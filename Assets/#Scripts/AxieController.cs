using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AxieMixer.Unity;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace _Scripts {
    public class AxieController : MonoBehaviour {
        [SerializeField] private AxieStateManager axieStateManager;
        public bool isAttacker;
        public int maxHitPoint;
        public int currentHitPoint;
        public Tilemap map;
        public Vector3 positionOffset;
        public int RandomNumber {
            get {
                if (_randomNumber == -1) _randomNumber = Random.Range(0, 3);
                return _randomNumber;
            }
        }
        private int _randomNumber = -1;
        private Camera _camera;
        private bool _isFindingTarget;
        //Debug
        public List<GameObject> _enemyList;
        public List<GameObject> _ignoreEnemyList = new List<GameObject>();
        public GameObject _currentEnemy;
        
        private Queue<Vector3Int> _pathToEnemy = new Queue<Vector3Int>();
        private Spawner _spawner;
        private string _eventDealDamage;
        private string _instanceId;
        private Coroutine _checkActionRoutine;
        private bool _battleEnded;

        private void Awake() {
            Mixer.Init();
            _camera = Camera.main;
            _spawner = GameObject.FindWithTag("Spawner").GetComponent<Spawner>();
            maxHitPoint = isAttacker ? 16 : 32;
            currentHitPoint = maxHitPoint;
            _instanceId = gameObject.GetInstanceID().ToString();
            _eventDealDamage = $"DealDamage{_instanceId}";
            EventManager.StartListening(_eventDealDamage, DealDamage);
        }
        
        // Start is called before the first frame update
        private void Start() {
            _enemyList = isAttacker ? _spawner.defenders : _spawner.attackers;
            _checkActionRoutine = StartCoroutine(CheckForAction());
        }

        private void DealDamage(int damage) {
            currentHitPoint -= damage;
            var eventUpdateHealthBar = $"UpdateHealthBar{_instanceId}";
            EventManager.TriggerEvent(eventUpdateHealthBar, currentHitPoint);
            if (currentHitPoint < 0) {
                currentHitPoint = 0;
                EventManager.StopListening(_eventDealDamage, DealDamage);
                if (isAttacker) _spawner.attackers.Remove(gameObject);
                else _spawner.defenders.Remove(gameObject);
                DestroyImmediate(gameObject);
            }
            
        }

        private void Update() {
            if (_battleEnded) return;
            if (_spawner.attackers.Count == 0 || _spawner.defenders.Count == 0) {
                StopCoroutine(_checkActionRoutine);
                axieStateManager.SwitchState(axieStateManager.victoryState);
                _battleEnded = true;
            }
        }

        private IEnumerator CheckForAction() {
            while (true) {
                yield return new WaitForSeconds(1f);
                // Debug.Log("Check action");
                var adjacentEnemy = GetAdjacentEnemy();
                if (adjacentEnemy is not null) Attack(adjacentEnemy);
                else {
                    if (isAttacker && _enemyList.Count > 0) {
                        MoveToClosestEnemy();
                    } 
                    else axieStateManager.SwitchState(axieStateManager.idleState);
                }
            }
        }

        private void MoveToClosestEnemy() {
            // Find closest enemy
            if (_pathToEnemy.Count == 0) {
                if(_isFindingTarget) return;
                _isFindingTarget = true;
                _currentEnemy = FindClosestEnemy();
                if (_currentEnemy is null) return;
                FindPathToClosestEnemy(_currentEnemy);
                _isFindingTarget = false;
            }
            else {
                // Start moving to the next path
                var nextCell = _pathToEnemy.Dequeue();
                var axieAtNextCell = GetAxieAt(nextCell);
                if (axieAtNextCell is not null 
                    && (axieAtNextCell.CompareTag("Attacker") 
                        || axieAtNextCell.CompareTag("Defender"))) {
                            axieStateManager.SwitchState(axieStateManager.idleState);
                }
                else {
                    // keep moving there
                    axieStateManager.SwitchState(axieStateManager.walkingState);
                    if (isTargetOnTheLeft(nextCell)) {
                        axieStateManager.FlipAxie(1f);
                    }
                    else {
                        axieStateManager.FlipAxie(-1f);
                    }
                    // Debug.Log($"{tag} moved to {targetPosition}");
                    transform.position = map.CellToWorld(nextCell) - positionOffset;
                }
            }
        }

        private void ChangeTarget() {
            _ignoreEnemyList.Add(_currentEnemy);
            axieStateManager.SwitchState(axieStateManager.idleState);
            Debug.LogWarning($"{name} has changed target");
        }
        private bool isTargetOnTheLeft(Vector3Int destination) {
            var currentPosition = map.WorldToCell(transform.position);
            var direction = destination - currentPosition;
            // Debug.Log($"Direction: {direction}");
            return direction.x < 0 || direction.y > 0;
        }
        
        private GameObject GetAdjacentEnemy() {
            var currentCell = map.WorldToCell(transform.position);
            var adjacentCells = GetAdjacentCells(currentCell);
            foreach (var cell in adjacentCells) {
                var axie = GetAxieAt(cell);
                if (axie is null || axie.CompareTag(tag)) continue;
                return axie;
            }
            // Debug.LogWarning("No adjacent enemy");
            return null;
        }
        
        private GameObject FindClosestEnemy() {
            if (_enemyList.Count == 0) {
                return null;
            }
            GameObject closestEnemy = null;
            var thisPosition = map.WorldToCell(transform.position);
            float closestDistance = float.MaxValue;
            foreach (var enemy in _enemyList) {
                if (enemy is null || _ignoreEnemyList.Contains(enemy)) continue;
                var enemyPosition = map.WorldToCell(enemy.transform.position);
                var currentDistance = Vector3Int.Distance(thisPosition, enemyPosition);
                if (currentDistance <= closestDistance) {
                    closestDistance = currentDistance;
                    closestEnemy = enemy;
                }
            }
            return closestEnemy;
        }
        
        private void FindPathToClosestEnemy(GameObject closestEnemy) {
            _pathToEnemy.Clear();
            var enemyPosition = map.WorldToCell(closestEnemy.transform.position);
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
                new (currentCell.x + 1, currentCell.y - 1, 0),
                new (currentCell.x - 1, currentCell.y + 1, 0)
            };
            return adjacentCells;
        }
        
        private void Attack(GameObject enemy) {
            var eventDealDamageEnemy = $"DealDamage{enemy.GetInstanceID()}";
            var enemyController = enemy.GetComponent<AxieController>();
            var targetNumber = enemyController.RandomNumber;
            axieStateManager.SwitchState(axieStateManager.attackingState);
            var enemyPosition = map.WorldToCell(enemy.transform.position);
            if (isTargetOnTheLeft(enemyPosition)) 
                axieStateManager.FlipAxie(1f);
            else axieStateManager.FlipAxie(-1f);
            var damage = GetDamage(RandomNumber, targetNumber);
            _randomNumber = -1;
            EventManager.TriggerEvent(eventDealDamageEnemy, damage);
            if (_enemyList.Contains(enemy)) return;
            if (isAttacker && _enemyList.Count > 0) ChangeTarget();
            else axieStateManager.SwitchState(axieStateManager.idleState);
        }

        private int GetDamage(int attackerNumber, int targetNumber) {
            var calculation = (3 + attackerNumber - targetNumber) % 3;
            return calculation switch {
                0 => 4,
                1 => 5,
                _ => 3
            };
        }
        private GameObject GetAxieAt(Vector3Int position) {
            var worldPosition = map.GetCellCenterWorld(position);
            var screenPoint = _camera.WorldToScreenPoint(worldPosition);
            Ray ray = _camera.ScreenPointToRay(screenPoint);
            int layerMask = LayerMask.NameToLayer("AxieOnTile");
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, layerMask);
            if (hit.collider is not null) {
                var axie = hit.collider.transform.parent.gameObject;
                if (axie.CompareTag("Attacker") || axie.CompareTag("Defender")) {
                    // Debug.LogWarning($"{name} found axie {axie.tag} at {position}");
                    return axie;
                }
            }
            return null;
        }
    }
}
