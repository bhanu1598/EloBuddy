namespace Rice.Modes
{
    using System.Linq;

    using EloBuddy;
    using EloBuddy.SDK;

    public sealed class LaneClear : ModeBase
    {
        public override bool ShouldBeExecuted()
        {
            return Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && Player.Instance.ManaPercent > Config.Modes.LaneClear.Mana;
        }

        public override void Execute()
        {
            var minions = EntityManager.MinionsAndMonsters.EnemyMinions.Where(x => x.IsValidTarget(Q.Range)).OrderBy(x => x.Health);

            var shouldQ = Config.Modes.LaneClear.UseQ && Q.IsReady();
            var shouldW = Config.Modes.LaneClear.UseW && W.IsReady() && ModeManager.LastSpell != SpellSlot.E;
            var shouldE = Config.Modes.LaneClear.UseE && E.IsReady() && ModeManager.LastSpell != SpellSlot.W;
            var shouldR = Config.Modes.LaneClear.UseR && R.IsReady();

            var stacks = ModeManager.PassiveCount + new[] { shouldQ, shouldW, shouldE, shouldR }.Count(x => x);

            foreach (var minion in minions)
            {
                switch (ModeManager.PassiveCount)
                {
                    case 2:
                        if (Q.IsReady() && W.IsReady() && E.IsReady())
                        {
                            if (shouldE)
                            {
                                E.Cast(minion);
                                if (shouldQ)
                                {
                                    Q.PredCast(minion);
                                }
                                return;
                            }
                            if (shouldQ && !Q.GetPrediction(minion).Collision)
                            {
                                Q.PredCast(minion);
                                return;
                            }
                        }
                        break;

                    case 4:
                        if (shouldW)
                        {
                            W.Cast(minion);
                            if (shouldQ && !Q.GetPrediction(minion).Collision)
                            {
                                Q.PredCast(minion);
                            }
                        }
                        if (shouldR
                            && (ModeManager.PassiveCharged || SpellDamage.GetTotalDamage(minion) > minion.Health))
                        {
                            R.Cast();
                            return;
                        }
                        break;
                }

                if (shouldW)
                {
                    W.Cast(minion);
                    if (shouldQ && !Q.GetPrediction(minion).Collision)
                    {
                        Q.PredCast(minion);
                    }
                    return;
                }
                if (shouldR && (stacks > 3 || ModeManager.PassiveCharged))
                {
                    R.Cast();
                    return;
                }
                if (shouldE)
                {
                    E.Cast(minion);
                    return;
                }
                if (shouldQ && !Q.GetPrediction(minion).Collision)
                {
                    Q.PredCast(minion);
                }
            }
        }
    }
}