using System.Collections;
using System.Collections.Generic;
using AxieMixer.Unity;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace _Scripts {
    public class AxieController : MonoBehaviour {
        [SerializeField] private AxieStateManager axieStateManager;
        public bool isDefender;
        public int maxHitPoint;
        public int currentHitPoint;
        public Tilemap map;
        public Vector3 positionOffset;
        private int RandomNumber => Random.Range(0, 3);
        private Camera _camera;
        private bool _isClosestEnemyFound;
        private bool _isSelected;
        private bool _isEnemyOnTheLeft;
        private bool _isMoving;
        private List<GameObject> _enemyList;
        private GameObject _closestEnemy;
        private Queue<Vector3Int> _pathToEnemy = new Queue<Vector3Int>();
        private Spawner _spawner;
        private string _eventDealDamage;
        private string _instanceId;
        private void Awake() {
            Mixer.Init();
            _camera = Camera.main;
            _spawner = GameObject.FindWithTag("Spawner").GetComponent<Spawner>();
            maxHitPoint = isDefender ? 32 : 16;
            currentHitPoint = maxHitPoint;
            _instanceId = gameObject.GetInstanceID().ToString();
            _eventDealDamage = $"DealDamage{_instanceId}";
            EventManager.StartListening(_eventDealDamage, DealDamage);
        }
        
        // Start is called before the first frame update
        private void Start() {
            _enemyList = isDefender ? _spawner.attackers : _spawner.defenders;
            StartCoroutine(CheckForAction());
        }

        private void DealDamage(int damage) {
            currentHitPoint -= damage;
            var eventUpdateHealthBar = $"UpdateHealthBar{_instanceId}";
            EventManager.TriggerEvent(eventUpdateHealthBar, currentHitPoint);
            if (currentHitPoint >= 0) return;
            currentHitPoint = 0;
            EventManager.StopListening(_eventDealDamage, DealDamage);
            DestroyImmediate(gameObject);
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
                ChangeTarget();
            }
            else {
                // keep moving there
                if (isTargetOnTheLeft(targetPosition)) {
                    axieStateManager.FlipAxie(1f);
                }
                else {
                    axieStateManager.FlipAxie(-1f);
                }
                axieStateManager.SwitchState(axieStateManager.walkingState);
                Debug.Log($"{tag} moved to {targetPosition}");
                transform.position = map.CellToWorld(targetPosition) - positionOffset;
            }
        }

        private void ChangeTarget() {
            axieStateManager.SwitchState(axieStateManager.idleState);
            _enemyList.Remove(_closestEnemy);
            _closestEnemy = null;
            _pathToEnemy.Clear();
            _isClosestEnemyFound = false;
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
            axieStateManager.SwitchState(axieStateManager.idleState);
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
            var eventDealDamageEnemy = $"DealDamage{enemy.GetInstanceID()}";
            var enemyController = enemy.GetComponent<AxieController>();
            var targetNumber = enemyController.RandomNumber;
            axieStateManager.SwitchState(axieStateManager.attackingState);
            var enemyPosition = map.WorldToCell(enemy.transform.position);
            if (isTargetOnTheLeft(enemyPosition)) 
                axieStateManager.FlipAxie(1f);
            else axieStateManager.FlipAxie(-1f);
            var damage = GetDamage(RandomNumber, targetNumber);
            EventManager.TriggerEvent(eventDealDamageEnemy, damage);
            if (enemy == null) {
                if (!isDefender)
                    ChangeTarget();
                else {
                    axieStateManager.SwitchState(axieStateManager.idleState);
                }
            }
        }

        private int GetDamage(int attackerNumber, int targetNumber) {
            var calculation = (3 + attackerNumber - targetNumber) % 3;
            switch (calculation) {
                case 0:
                    return 4;
                case 1:
                    return 5;
                case 2:
                    return 3;
                default:
                    return 0;
            }
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
