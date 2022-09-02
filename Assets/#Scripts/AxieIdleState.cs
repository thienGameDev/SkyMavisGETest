namespace _Scripts {
    public class AxieIdleState: AxieBaseState {
        public override void EnterState(AxieStateManager axie) {
            axie.animationName = "action/idle/normal";
            axie.skeletonAnimation.state.SetAnimation(0, axie.animationName, true);
        }

        public override void UpdateState(AxieStateManager axie) {
            
        }
    }
}