namespace _Scripts {
    public class AxieVictoryState: AxieBaseState{
        public override void EnterState(AxieStateManager axie) {
            var animationName = "activity/victory-pose-back-flip";
            axie.SetAnimation(animationName);
        }
    }
}