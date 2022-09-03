using System;
using AxieMixer.Unity;
using Spine.Unity;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts {
    public class AxieStateManager : MonoBehaviour {
        [SerializeField] private float scale = .5f;
        public SkeletonAnimation skeletonAnimation;
        private AxieBaseState _currentState;
        public AxieBaseState idleState = new AxieIdleState();
        public AxieBaseState walkingState = new AxieWalkingState();
        public AxieBaseState attackingState = new AxieAttackingState();
        public AxieBaseState victoryState = new AxieVictoryState();
        private bool _isAttacker;
        private void Awake() {
            _isAttacker = GetComponent<AxieController>().isAttacker;
        }

        private void Start() {
            SetGenes();
            RandomFlipAxie();
            _currentState = idleState;
            _currentState.EnterState(this);
        }

        private void SetGenes() {
            var axieId = PlayerPrefs.GetString(_isAttacker ? "attackerId" : "defenderId");
            var genes = PlayerPrefs.GetString(_isAttacker ? "attackerGenes" :"defenderGenes");
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
            Mixer.SpawnSkeletonAnimation(skeletonAnimation, axieId, genes);
            var meshRender = GetComponentInChildren<MeshRenderer>();
            meshRender.sortingLayerName = "Player";
            meshRender.sortingOrder = 1;
            skeletonAnimation.transform.localScale = scale * Vector3.one;
        }

        public void SetAnimation(string animationName) {
            skeletonAnimation.state.SetAnimation(0, animationName, true);
        }

        private void RandomFlipAxie() {
            int x = Random.Range(0, 100);
            if(x % 2 == 0) FlipAxie(-1f);
        }

        public void FlipAxie(float direction) {
            skeletonAnimation.skeleton.ScaleX = direction;
        }
        
        public void SwitchState(AxieBaseState newState) {
            _currentState = newState;
            _currentState.EnterState(this);
        }
    }
}