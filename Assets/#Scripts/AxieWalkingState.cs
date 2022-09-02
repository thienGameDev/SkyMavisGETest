namespace _Scripts {
    public class AxieWalkingState: AxieBaseState {
        public override void EnterState(AxieStateManager axie) {
            axie.animationName = "action/move-forward";
            axie.skeletonAnimation.state.SetAnimation(0, axie.animationName, true);
        }

        public override void UpdateState(AxieStateManager axie) {
            
        }
    }
}