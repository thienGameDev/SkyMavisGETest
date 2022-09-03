using AxieMixer.Unity;
using Spine.Unity;
using UnityEngine;

namespace _Scripts {
    public class AxieStateManager : MonoBehaviour {
        [SerializeField] private float scale = .5f;
        public SkeletonAnimation skeletonAnimation;
        public bool isDefender;
        private AxieBaseState _currentState;
        public AxieBaseState idleState = new AxieIdleState();
        public AxieBaseState walkingState = new AxieWalkingState();
        public AxieBaseState attackingState = new AxieAttackingState();
        public AxieBaseState victoryState = new AxieVictoryState();
        private void Start() {
            SetGenes();
            RandomFlipAxie();
            _currentState = idleState;
            _currentState.EnterState(this);
        }

        private void SetGenes() {
            var axieId = PlayerPrefs.GetString(isDefender ? "defenderId" : "attackerId");
            var genes = PlayerPrefs.GetString(isDefender ? "defenderGenes" : "attackerGenes");
            skeletonAnimation = GetComponent<SkeletonAnimation>();
            Mixer.SpawnSkeletonAnimation(skeletonAnimation, axieId, genes);
            GetComponent<MeshRenderer>().sortingLayerName = "Player";
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