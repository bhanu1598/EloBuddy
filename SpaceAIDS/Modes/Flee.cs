namespace SpaceAIDS.Modes
{
    using EloBuddy.SDK;

    public sealed class Flee : ModeBase
    {
        public override bool ShouldBeExecuted()
        {
            return Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee);
        }

        public override void Execute()
        {
            // TODO: Add flee logic here
        }
    }
}
