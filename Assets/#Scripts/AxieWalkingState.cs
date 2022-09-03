namespace _Scripts {
    public class AxieWalkingState: AxieBaseState {
        public override void EnterState(AxieStateManager axie) {
            var animationName = "action/move-forward";
            axie.SetAnimation(animationName);
        }
        
    }
}