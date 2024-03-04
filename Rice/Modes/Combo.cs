namespace Rice.Modes
{
    using System.Linq;

    using EloBuddy;
    using EloBuddy.SDK;

    public sealed class Combo : ModeBase
    {
        public override bool ShouldBeExecuted()
        {
            return Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && Player.Instance.ManaPercent > Config.Modes.Combo.Mana;
        }

        public override void Execute()
        {
            if (Config.Modes.Combo.blockAA) Orbwalker.DisableAttacking = true;

            var target = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            if (target == null)
            {
                return;
            }

            var shouldQ = Config.Modes.Combo.UseQ && Q.IsReady();
            var shouldW = Config.Modes.Combo.UseW && W.IsReady() && ModeManager.LastSpell != SpellSlot.E;
            var shouldE = Config.Modes.Combo.UseE && E.IsReady() && ModeManager.LastSpell != SpellSlot.W;
            var shouldR = Config.Modes.Combo.UseR && R.IsReady();

            var stacks = ModeManager.PassiveCount + new[] { shouldQ, shouldW, shouldE, shouldR}.Count(x => x);

            switch (ModeManager.PassiveCount)
            {
                case 2:
                    if (Q.IsReady() && W.IsReady() && E.IsReady())
                    {
                        if (shouldE)
                        {
                            E.Cast(target);
                            if (shouldQ)
                            {
                                Q.PredCast(target);
                            }
                            return;
                        }
                    }
                    break;

                case 4:
                    if (shouldW)
                    {
                        W.Cast(target);
                        if (shouldQ)
                        {
                            Q.PredCast(target);
                        }
                        
                    }
                    if (shouldR && (ModeManager.PassiveCharged || SpellDamage.GetTotalDamage(target) > target.Health))
                    {
                        R.Cast();
                        return;
                    }
                    break;
            }

            if (shouldW && ModeManager.LastSpell != SpellSlot.E)
            {
                W.Cast(target);
                if (shouldQ)
                {
                    Q.PredCast(target);
                }
                return;
            }
            if (shouldR
                && (stacks > 4 || ModeManager.PassiveCharged || SpellDamage.GetTotalDamage(target) > target.Health))
            {
                R.Cast();
                return;
            }
            if (shouldE && ModeManager.LastSpell != SpellSlot.W)
            {
                E.Cast(target);
                return;
            }
            if (shouldQ)
            {
                Q.PredCast(target);
            }
        }
    }
}