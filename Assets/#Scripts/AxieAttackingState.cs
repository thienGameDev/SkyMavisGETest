namespace _Scripts {
    public class AxieAttackingState: AxieBaseState {
        
        public override void EnterState(AxieStateManager axie) {
            var animationName = "attack/melee/normal-attack";
            axie.SetAnimation(animationName);

        }
    }
}