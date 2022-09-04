using AxieMixer.Unity;
using Spine.Unity;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts {
    public class AxieStateManager : MonoBehaviour {
        [SerializeField] private float scale = .5f;
        public SkeletonAnimation skeletonAnimation;
        public bool isAttacker;
        public bool isReady;
        public AxieBaseState attackingState = new AxieAttackingState();
        public AxieBaseState currentState;
        public AxieBaseState idleState = new AxieIdleState();
        public AxieBaseState victoryState = new AxieVictoryState();
        public AxieBaseState walkingState = new AxieWalkingState();

        private void Start() {
            SetGenes();
            RandomFlipAxie();
            currentState = idleState;
            currentState.EnterState(this);
            isReady = true;
        }

        private void SetGenes() {
            var axieId = PlayerPrefs.GetString(isAttacker ? "attackerId" : "defenderId");
            var genes = PlayerPrefs.GetString(isAttacker ? "attackerGenes" :"defenderGenes");
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
            currentState = newState;
            currentState.EnterState(this);
        }
    }
}