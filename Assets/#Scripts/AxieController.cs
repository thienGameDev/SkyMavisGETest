using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace _Scripts {
    public class AxieController : MonoBehaviour {
        private const float COUNT_DOWN = 1f;
        [SerializeField] private AxieStateManager axieStateManager;
        public bool isAttacker;
        public int maxHitPoint;
        public Tilemap map;
        public Vector3 positionOffset;
        public int damage;

        private StatsPanel _axieStatsPanel;
        private bool _battleEnded;
        private Camera _camera;
        private GameObject _currentEnemy;
        private int _currentHitPoint;

        //Debug
        private List<GameObject> _enemyList;
        private string _eventDealDamage;
        private List<GameObject> _ignoreEnemyList = new List<GameObject>();
        private int _instanceId;
        private bool _isFindingTarget;

        private Queue<Vector3Int> _pathToEnemy = new Queue<Vector3Int>();

        private int _randomNumber = -1;
        private Spawner _spawner;
        private float _timeRemaining;
        public string CurrentTarget => _currentEnemy ? _currentEnemy.name : "";

        public string CurrentState {
            get {
                var currentStateString = axieStateManager.currentState.GetType().ToString();
                return currentStateString.Replace("_Scripts.Axie", "").Replace("State", "");
            }
        }

        public int CurrentHitPoint => _currentHitPoint < 0 ? 0 : _currentHitPoint;

        public int RandomNumber {
            get {
                if (_randomNumber == -1) _randomNumber = Random.Range(0, 3);
                return _randomNumber;
            }
        }

        private void Awake() {
            _camera = Camera.main;
            _spawner = Spawner.Instance;
            maxHitPoint = isAttacker ? 16 : 32;
            _currentHitPoint = maxHitPoint;
            _instanceId = gameObject.GetInstanceID();
            _eventDealDamage = $"DealDamage{_instanceId}";
            EventManager.StartListening(_eventDealDamage, DealDamage);
            SetupStatsPanel();
        }

        // Start is called before the first frame update
        private void Start() {
            _enemyList = isAttacker ? _spawner.defenders : _spawner.attackers;
        }

        private void Update() {
            if (!_spawner.isReady) return;
            if (_timeRemaining > 0) {
                //Start countdown
                _timeRemaining -= Time.deltaTime;
            }
            else {
                CheckForAction();
                //Reset Countdown
                _timeRemaining = COUNT_DOWN;
            }
            if (_battleEnded) return;
            if (_spawner.attackers.Count == 0 || _spawner.defenders.Count == 0) {
                axieStateManager.SwitchState(axieStateManager.victoryState);
                _battleEnded = true;
                EventManager.TriggerEvent("EndGame", 0);
            }
        }

        private void OnMouseDown() {
            _axieStatsPanel.followObject = this;
            _axieStatsPanel.gameObject.SetActive(true);
        }

        private void SetupStatsPanel() {
            var canvas = GameObject.FindGameObjectWithTag("UICanvas");
            _axieStatsPanel = canvas.transform.Find("AxieStats").gameObject.GetComponent<StatsPanel>();
        }

        private void DealDamage(int dmg) {
            _currentHitPoint -= dmg;
            var eventUpdateHealthBar = $"UpdateHealthBar{_instanceId}";
            EventManager.TriggerEvent(eventUpdateHealthBar, CurrentHitPoint);
            if (_currentHitPoint <= 0) {
                Dead();
            }
        }

        private void Dead() {
            if (isAttacker) _spawner.attackers.Remove(gameObject);
            else _spawner.defenders.Remove(gameObject);
            Destroy(gameObject);
            EventManager.StopListening(_eventDealDamage, DealDamage);
        }

        private void CheckForAction() {
            var adjacentEnemy = GetAdjacentEnemy();
            if (adjacentEnemy is not null) Attack(adjacentEnemy);
            else {
                if (isAttacker && _enemyList.Count > 0) {
                    MoveToClosestEnemy();
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
            if (!isAttacker) _currentEnemy = enemy;
            var eventDealDamageEnemy = $"DealDamage{enemy.GetInstanceID()}";
            var enemyController = enemy.GetComponentInChildren<AxieController>();
            var targetNumber = enemyController.RandomNumber;
            axieStateManager.SwitchState(axieStateManager.attackingState);
            var enemyPosition = map.WorldToCell(enemy.transform.position);
            if (isTargetOnTheLeft(enemyPosition)) 
                axieStateManager.FlipAxie(1f);
            else axieStateManager.FlipAxie(-1f);
            damage = GetDamage(RandomNumber, targetNumber);
            _randomNumber = -1;
            EventManager.TriggerEvent(eventDealDamageEnemy, damage);
            if (_enemyList.Contains(enemy)) return;
            if (isAttacker && _enemyList.Count > 0) ChangeTarget();
            else axieStateManager.SwitchState(axieStateManager.idleState);
        }

        private static int GetDamage(int attackerNumber, int targetNumber) {
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
