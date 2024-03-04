namespace Rice.Modes
{
    using EloBuddy;
    using EloBuddy.SDK;

    public sealed class Harass : ModeBase
    {
        public override bool ShouldBeExecuted()
        {
            return Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && Player.Instance.ManaPercent > Config.Modes.Harass.Mana;
        }

        public override void Execute()
        {
            if (!Config.Modes.Harass.UseQ || !Q.IsReady()) { return; }
            var target = TargetSelector.GetTarget(this.Q.Range, DamageType.Physical);
            if (target != null)
            {
                this.Q.PredCast(target);
            }
        }
    }
}
