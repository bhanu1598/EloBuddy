using Settings = Reborn_Leona.Config.Modes.Draw;

namespace Reborn_Leona
{
    using System;
    using System.Linq;

    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Rendering;

    using SharpDX;

    using Color = System.Drawing.Color;

    public static class DamageIndicator
    {
        private const int BarWidth = 104;

        private const int LineThickness = 9;

        public delegate float DamageToUnitDelegate(AIHeroClient hero);

        private static DamageToUnitDelegate DamageToUnit { get; set; }

        private static readonly Vector2 BarOffset = new Vector2(-9, 11);

        private static Color _drawingColor;

        public static Color DrawingColor
        {
            get
            {
                return _drawingColor;
            }
            set
            {
                _drawingColor = Color.FromArgb(170, value);
            }
        }

        public static bool HealthbarEnabled { get; set; }

        public static bool PercentEnabled { get; set; }

        public static void Initialize(DamageToUnitDelegate damageToUnit)
        {
            DamageToUnit = damageToUnit;
            DrawingColor = Config.Modes.Draw.colorHealth;
            HealthbarEnabled = Config.Modes.Draw.DrawHealth;

            Drawing.OnEndScene += OnEndScene;
        }

        private static void OnEndScene(EventArgs args)
        {
            if (HealthbarEnabled || PercentEnabled)
            {
                foreach (var unit in EntityManager.Heroes.Enemies.Where(u => u.IsValidTarget() && u.IsHPBarRendered))
                {
                    // Get damage to unit
                    var damage = DamageToUnit(unit);

                    // Continue on 0 damage
                    if (damage <= 0)
                    {
                        continue;
                    }

                    if (HealthbarEnabled)
                    {
                        // Get remaining HP after damage applied in percent and the current percent of health
                        var damagePercentage = ((unit.TotalShieldHealth() - damage) > 0
                                                    ? (unit.TotalShieldHealth() - damage)
                                                    : 0)
                                               / (unit.MaxHealth + unit.AllShield + unit.AttackShield + unit.MagicShield);
                        var currentHealthPercentage = unit.TotalShieldHealth()
                                                      / (unit.MaxHealth + unit.AllShield + unit.AttackShield
                                                         + unit.MagicShield);

                        // Calculate start and end point of the bar indicator
                        var startPoint =
                            new Vector2(
                                (int)(unit.HPBarPosition.X + BarOffset.X + damagePercentage * BarWidth),
                                (int)(unit.HPBarPosition.Y + BarOffset.Y) - 5);
                        var endPoint =
                            new Vector2(
                                (int)(unit.HPBarPosition.X + BarOffset.X + currentHealthPercentage * BarWidth) + 1,
                                (int)(unit.HPBarPosition.Y + BarOffset.Y) - 5);

                        // Draw the line
                        //Drawing.DrawLine(startPoint, endPoint, LineThickness, DrawingColor);

                        var pos = new Vector2(unit.HPBarPosition.X + 2, unit.HPBarPosition.Y + 9);
                        var fullbar = (BarWidth) * (unit.HealthPercent / 100);
                        damage = (BarWidth) * ((unit.TotalShieldHealth() - damage) > 0
                                                    ? (unit.TotalShieldHealth() - damage)
                                                    : 0)
                                               / (unit.MaxHealth + unit.AllShield + unit.AttackShield + unit.MagicShield);
                        Line.DrawLine(
                            DrawingColor,
                            9f,
                            new Vector2(pos.X, pos.Y),
                            new Vector2(pos.X + (damage > fullbar ? fullbar : damage), pos.Y));
                    }

                    if (PercentEnabled)
                    {
                        // Get damage in percent and draw next to the health bar
                        Drawing.DrawText(
                            unit.HPBarPosition,
                            Color.MediumVioletRed,
                            string.Concat(Math.Ceiling((damage / unit.TotalShieldHealth()) * 100), "%"),
                            10);
                    }
                }
            }
        }
    }
}