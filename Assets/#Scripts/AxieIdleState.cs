namespace _Scripts {
    public class AxieIdleState: AxieBaseState {
        public override void EnterState(AxieStateManager axie) {
            var animationName = "action/idle/normal";
            axie.SetAnimation(animationName);
        }
    }
}