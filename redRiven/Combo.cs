namespace redRiven
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Events;
    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK.Menu.Values;

    internal class Combo
    {
        #region Static Fields

        private static readonly Spell.Skillshot E = Riven.E;

        private static readonly Spell.Skillshot Q = Riven.Q;

        private static readonly Spell.Active R = Riven.R;

        private static readonly Spell.Skillshot R2 = Riven.R2;

        private static readonly Spell.Active W = Riven.W;

        #endregion

        #region Public Methods and Operators

        public static double CalcDmg(Obj_AI_Base target, bool useR, bool onlyR)
        {
            if (target == null)
            {
                return 0;
            }

            double dmg = 0;
            double[] passivedmg = { 0.2, 0.25, 0.3, 0.35, 0.4, 0.45, 0.5 };
            if (UseItem(true, false))
            {
                dmg = dmg + Riven.Player.GetAutoAttackDamage(target) * 0.7;
            }

            if (W.IsReady() && GetOption(Riven.CMenu, "w"))
            {
                dmg = dmg + Riven.Player.GetSpellDamage(target, SpellSlot.W);
            }

            if (Q.IsReady() && GetOption(Riven.CMenu, "q"))
            {
                dmg = dmg + Riven.Player.GetSpellDamage(target, SpellSlot.Q) * 3
                      + Riven.Player.GetAutoAttackDamage(target) * 3 * (1 + passivedmg[Riven.Player.Level / 3]);
            }

            dmg = dmg + Riven.Player.GetAutoAttackDamage(target) * (1 + passivedmg[Riven.Player.Level / 3]) * 2;
            if (R2.IsReady() && useR)
            {
                double health = target.Health;
                if (!onlyR)
                {
                    if (Riven.CanR2())
                    {
                        health = target.Health - (dmg * 1.2);
                    }
                    else if (!Riven.CanR2())
                    {
                        health = target.Health - dmg;
                    }
                }

                var missinghealth = (target.MaxHealth - health) / target.MaxHealth > 0.75
                                        ? 0.75
                                        : (target.MaxHealth - health) / target.MaxHealth;
                var pluspercent = missinghealth * (8.0 / 3.0);

                var rawdmg = new double[] { 80, 120, 160 }[R.Level - 1] + 0.6 * Riven.Player.FlatPhysicalDamageMod;
                return Riven.Player.CalculateDamageOnUnit(
                    target, 
                    DamageType.Physical, 
                    (float)(rawdmg * (1 + pluspercent)));
            }

            return dmg;
        }

        public static void Clear()
        {
            var minion =
                EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Riven.Player.Position, W.Range)
                    .OrderByDescending(x => 1 - x.Distance(Riven.Player.Position))
                    .FirstOrDefault();
            var monster =
                EntityManager.MinionsAndMonsters.GetJungleMonsters(Riven.Player.Position, W.Range)
                    .OrderByDescending(x => 1 - x.Distance(Riven.Player.Position))
                    .FirstOrDefault();

            if (minion != null && Riven.Player.Distance(minion) <= W.Range && W.IsReady() && Orbwalker.CanMove
                && GetOption(Riven.WMenu, "w"))
            {
                UseItem(true, true);
                W.Cast();
            }

            if (monster != null && Riven.Player.Distance(monster) <= W.Range && W.IsReady() && Orbwalker.CanMove
                && GetOption(Riven.JMenu, "w"))
            {
                UseItem(true, true);
                W.Cast();
            }

            if (minion != null && Riven.Player.Distance(minion) <= W.Range && E.IsReady() && Orbwalker.CanMove
                && GetOption(Riven.WMenu, "e"))
            {
                Riven.Player.Spellbook.CastSpell(SpellSlot.E, minion.Position);
            }

            if (monster != null && Riven.Player.Distance(monster) <= W.Range && E.IsReady() && Orbwalker.CanMove
                && GetOption(Riven.JMenu, "e"))
            {
                Riven.Player.Spellbook.CastSpell(SpellSlot.E, monster.Position);
            }
        }

        public static void DoCombo(bool useR = true)
        {
            if (Q.IsReady() && Orbwalker.CanMove && !Riven.Player.IsDashing()
                && (GetOption(Riven.CMenu, "q") && useR || GetOption(Riven.HMenu, "q") && !useR))
            {
                var target =
                    EntityManager.Heroes.Enemies.Where(
                        x => x.Distance(Game.CursorPos) <= 375 && x.Distance(Riven.Player.ServerPosition) <= 1200)
                        .OrderBy(x => x.Distance(Game.CursorPos))
                        .FirstOrDefault(x => x.IsEnemy);

                if (!Riven.Player.IsDashing() && Environment.TickCount - Riven.LastQ >= 1000 && target.IsValidTarget() && target != null)
                {
                    if (Riven.Player.AttackRange + Q.Range >= Riven.Player.Distance(target.Position)
                        && !Riven.Player.IsInAutoAttackRange(target))
                    {
                        Riven.Player.Spellbook.CastSpell(SpellSlot.Q, target.Position);
                    }
                }
            }

            if (W.IsReady() && Orbwalker.CanMove
                && (GetOption(Riven.CMenu, "w") && useR || GetOption(Riven.HMenu, "w") && !useR))
            {
                var targets =
                    EntityManager.Heroes.Enemies.Where(
                        x => x.IsValidTarget() && !x.IsZombie && Riven.Player.Distance(x) <= W.Range);
                if (targets.Any() && Riven.QStacks == 0)
                {
                    UseItem(true, true);
                    W.Cast();
                }
            }

            if (E.IsReady() && Orbwalker.CanMove
                && (GetOption(Riven.CMenu, "e") && useR || GetOption(Riven.HMenu, "e") && !useR))
            {
                var target = TargetSelector.GetTarget(325 + Riven.Player.AttackRange + 70, DamageType.Physical);
                if (target.IsValidTarget() && !target.IsZombie && Riven.QStacks == 0
                    && E.Range > Riven.Player.Distance(target.Position))
                {
                    Riven.Player.Spellbook.CastSpell(SpellSlot.E, target.Position);
                }
            }

            if (R.IsReady() && useR && !Riven.CanR2() && GetOption(Riven.CMenu, "r"))
            {
                var targetR = TargetSelector.GetTarget(200 + Riven.Player.BoundingRadius + 70, DamageType.Physical);
                if (targetR.IsValidTarget() && !targetR.IsZombie)
                {
                    if (!(CalcDmg(targetR, false, false) > targetR.Health))
                    {
                        R.Cast();
                    }
                    else if (Riven.Player.CountEnemiesInRange(800) >= GetOption(Riven.CMenu, "r1"))
                    {
                        R.Cast();
                    }
                }
            }

            if (R2.IsReady() && useR && Riven.CanR2() && GetOption(Riven.CMenu, "r"))
            {
                var targets = EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(R.Range) && !x.IsZombie && !x.IsMinion);
                foreach (var target in targets)
                {
                    if (CalcDmg(target, true, true) > target.Health)
                    {
                        R2.Cast(target);
                    }
                    else if (target.Health / target.MaxHealth <= GetOption(Riven.CMenu, "r2"))
                    {
                        R2.Cast(target);
                    }
                }
            }
        }

        public static dynamic GetOption(Menu menu, string option)
        {
            if (menu[option] is CheckBox)
            {
                return menu[option].Cast<CheckBox>().CurrentValue;
            }

            if (menu[option] is Slider)
            {
                return menu[option].Cast<Slider>().CurrentValue;
            }

            return null;
        }

        public static dynamic UseItem(bool useItem, bool item)
        {
            if (item)
            {
                if (Item.HasItem((int)ItemId.Tiamat_Melee_Only) || Item.HasItem((int)ItemId.Ravenous_Hydra_Melee_Only))
                {
                    if (useItem)
                    {
                        if (Item.CanUseItem((int)ItemId.Tiamat_Melee_Only))
                        {
                            Item.UseItem((int)ItemId.Tiamat_Melee_Only);
                        }

                        if (Item.HasItem((int)ItemId.Ravenous_Hydra_Melee_Only))
                        {
                            Item.UseItem((int)ItemId.Ravenous_Hydra_Melee_Only);
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (Item.HasItem((int)ItemId.Youmuus_Ghostblade))
                {
                    if (useItem)
                    {
                        if (Item.CanUseItem((int)ItemId.Youmuus_Ghostblade))
                        {
                            Item.UseItem((int)ItemId.Youmuus_Ghostblade);
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        public static void UseQ()
        {
            var targets =
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(x => x.IsValidTarget() && !x.IsZombie && Riven.Player.Distance(x) <= Q.Range);
            if (Riven.WaitQ && targets.Any())
            {
                Riven.Player.Spellbook.CastSpell(SpellSlot.Q, Riven.mainTarget.Position);
            }

            if (Q.IsReady() && !Riven.Player.IsRecalling() && !Riven.Player.Spellbook.IsChanneling && !Riven.Player.IsDead
                && Riven.QStacks != 0 && Environment.TickCount - Riven.LastQ >= 3650 && GetOption(Riven.MMenu, "q"))
            {
                Riven.Player.Spellbook.CastSpell(SpellSlot.Q, Game.CursorPos);
            }
            else if (Riven.Player.IsDead || Environment.TickCount - Riven.LastQ >= 3651)
            {
                Riven.QStacks = 0;
            }
        }

        #endregion
    }
}