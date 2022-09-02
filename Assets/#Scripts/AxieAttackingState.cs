namespace _Scripts {
    public class AxieAttackingState: AxieBaseState {
        
        public override void EnterState(AxieStateManager axie) {
            axie.animationName = "attack/melee/normal-attack";
        }

        public override void UpdateState(AxieStateManager axie) {
            axie.skeletonAnimation.state.SetAnimation(0, axie.animationName, false);

        }
    }
}