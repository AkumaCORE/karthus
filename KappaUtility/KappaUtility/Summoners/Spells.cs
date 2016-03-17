﻿namespace KappaUtility
{
    using System.Linq;

    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK.Menu.Values;

    internal class Spells
    {
        public static Spell.Active Heal;

        public static Spell.Active Barrier;

        public static Spell.Targeted Ignite;

        public static Spell.Targeted Smite;

        public static Spell.Targeted Exhaust;

        public static Menu SummMenu { get; private set; }

        internal static void OnLoad()
        {
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Obj_AI_Base.OnBasicAttack += OnBasicAttack;
            Game.OnUpdate += Game_OnUpdate;
            
            SummMenu = Load.UtliMenu.AddSubMenu("Summoner Spells");
            SummMenu.AddGroupLabel("Summoners Settings");

            if (Player.Spells.FirstOrDefault(o => o.SData.Name.Contains("SummonerDot")) != null)
            {
                SummMenu.AddGroupLabel("Ignite Settings");
                SummMenu.Add("ignite", new CheckBox("Ignite"));
                SummMenu.AddGroupLabel("Don't Use Ignite On:");
                foreach (var enemy in ObjectManager.Get<AIHeroClient>())
                {
                    CheckBox cb = new CheckBox(enemy.BaseSkinName) { CurrentValue = false };
                    if (enemy.Team != Player.Instance.Team)
                    {
                        SummMenu.Add("DontIgnite" + enemy.BaseSkinName, cb);
                    }
                }

                SummMenu.AddSeparator();
                Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("SummonerDot"), 600);
            }

            if (Player.Spells.FirstOrDefault(o => o.SData.Name.Contains("SummonerBarrier")) != null)
            {
                SummMenu.AddGroupLabel("Barrier Settings");
                SummMenu.Add("barrier", new CheckBox("Barrier"));
                SummMenu.Add("barrierme", new Slider("Use On My Health %", 30, 0, 100));
                SummMenu.AddSeparator();
                Barrier = new Spell.Active(ObjectManager.Player.GetSpellSlotFromName("SummonerBarrier"));
            }

            if (Player.Spells.FirstOrDefault(o => o.SData.Name.Contains("SummonerHeal")) != null)
            {
                SummMenu.AddGroupLabel("Heal Settings");
                SummMenu.Add("Heal", new CheckBox("Heal"));
                SummMenu.Add("Healally", new Slider("Use On Ally Health %", 25, 0, 100));
                SummMenu.Add("Healme", new Slider("Use On My Health %", 30, 0, 100));
                SummMenu.AddGroupLabel("Don't Use Heal On:");
                foreach (var ally in ObjectManager.Get<AIHeroClient>())
                {
                    CheckBox cb = new CheckBox(ally.BaseSkinName) { CurrentValue = false };
                    if (ally.Team == Player.Instance.Team)
                    {
                        SummMenu.Add("DontHeal" + ally.BaseSkinName, cb);
                    }
                }

                SummMenu.AddSeparator();
                Heal = new Spell.Active(ObjectManager.Player.GetSpellSlotFromName("SummonerHeal"), 850);
            }

            if (Player.Spells.FirstOrDefault(o => o.SData.Name.Contains("SummonerExhaust")) != null)
            {
                SummMenu.AddGroupLabel("Exhaust Settings");
                SummMenu.Add("exhaust", new CheckBox("Exhaust"));
                SummMenu.Add("exhaustally", new Slider("Use When Ally/Self Health %", 35, 0, 100));
                SummMenu.Add("exhaustenemy", new Slider("Use When Enemy Health %", 40, 0, 100));
                SummMenu.AddGroupLabel("Don't Use Exhaust On:");
                foreach (var enemy in ObjectManager.Get<AIHeroClient>())
                {
                    CheckBox cb = new CheckBox(enemy.BaseSkinName) { CurrentValue = false };
                    if (enemy.Team != Player.Instance.Team)
                    {
                        SummMenu.Add("DontExhaust" + enemy.BaseSkinName, cb);
                    }
                }

                Exhaust = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("SummonerExhaust"), 650);
            }

            if (Player.Spells.FirstOrDefault(o => o.SData.Name.Contains("SummonerSmite")) != null)
            {
                SummMenu.AddGroupLabel("Smite Settings");
                SummMenu.Add("smitemob", new CheckBox("Smite Monsters"));
                SummMenu.Add("smitecombo", new CheckBox("Smite Combo"));
                SummMenu.Add("smiteks", new CheckBox("Smite KillSteal"));
                SummMenu.AddGroupLabel("Don't Use Smite On:");
                foreach (var enemy in ObjectManager.Get<AIHeroClient>())
                {
                    CheckBox cb = new CheckBox(enemy.BaseSkinName) { CurrentValue = false };
                    if (enemy.Team != Player.Instance.Team)
                    {
                        SummMenu.Add("DontSmite" + enemy.BaseSkinName, cb);
                    }
                }

                SummMenu.AddGroupLabel("Use Smite On Monster:");
                SummMenu.Add("blue", new CheckBox(" Blue "));
                SummMenu.Add("red", new CheckBox(" Red "));
                SummMenu.Add("baron", new CheckBox(" Baron "));
                SummMenu.Add("drake", new CheckBox(" Dragon "));
                SummMenu.Add("gromp", new CheckBox(" Gromp "));
                SummMenu.Add("krug", new CheckBox(" Krug "));
                SummMenu.Add("razorbeak", new CheckBox(" Razorbeak "));
                SummMenu.Add("crab", new CheckBox(" Crab "));
                SummMenu.Add("murkwolf", new CheckBox(" Murkwolf "));
                SummMenu.AddSeparator();
                Smite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("SummonerSmite"), 500);
            }
        }

        private static void Game_OnUpdate(System.EventArgs args)
        {
            if (Ignite != null)
            {
                var ignitec = SummMenu["ignite"].Cast<CheckBox>().CurrentValue && Ignite.IsReady();
                var target = ObjectManager.Get<AIHeroClient>()
                        .FirstOrDefault(
                            enemy => enemy.IsValid && enemy.IsEnemy && enemy.IsVisible);

                if (ignitec && target != null && Player.Instance.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite) >= target.TotalShieldHealth() + (target.HPRegenRate * 4))
                {
                    if (target.IsValidTarget(Ignite.Range) && !target.IsDead && !SummMenu["DontIgnite" + target.BaseSkinName].Cast<CheckBox>().CurrentValue)
                    {
                        Ignite.Cast(target);
                    }
                }
            }
        }

        public static void OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender == null || sender.IsAlly || sender is Obj_AI_Minion || args.Target is Obj_AI_Minion
                || args.Target == null)
            {
                return;
            }

            var caster = sender;
            var target = (AIHeroClient)args.Target;

            if (!caster.IsEnemy || target == null)
            {
                return;
            }

            if (target.IsAlly && !target.IsMe)
            {
                if (Exhaust != null && !SummMenu["DontExhaust" + caster.BaseSkinName].Cast<CheckBox>().CurrentValue)
                {
                    
                }

                if (Heal != null && !SummMenu["DontHeal" + target.BaseSkinName].Cast<CheckBox>().CurrentValue)
                {
                    var healc = SummMenu["Heal"].Cast<CheckBox>().CurrentValue && Heal.IsReady();
                    var healally = SummMenu["Healally"].Cast<Slider>().CurrentValue;
                    if (healc)
                    {
                        if (target.IsValidTarget(Heal.Range))
                        {
                            if (target.TotalShieldHealth() <= healally)
                            {
                                Heal.Cast();
                            }

                            if (caster.GetAutoAttackDamage(target) > target.TotalShieldHealth())
                            { 
                                Heal.Cast();
                            }
                        }
                    }
                }
            }

            if (target.IsMe)
            {
                if (Heal != null && !SummMenu["DontHeal" + target.BaseSkinName].Cast<CheckBox>().CurrentValue)
                {
                    var healc = SummMenu["Heal"].Cast<CheckBox>().CurrentValue && Heal.IsReady();
                    var healme = SummMenu["Healme"].Cast<Slider>().CurrentValue;
                    if (healc)
                    {
                        if (target.TotalShieldHealth() <= healme)
                        {
                            Heal.Cast();
                        }

                        if (caster.GetAutoAttackDamage(target) > target.TotalShieldHealth())
                        {
                            Heal.Cast();
                        }
                    }
                }

                if (Barrier != null)
                {
                    var barrierc = SummMenu["barrier"].Cast<CheckBox>().CurrentValue && Barrier.IsReady();
                    var barrierme = SummMenu["barrierme"].Cast<Slider>().CurrentValue;
                    if (barrierc)
                    {
                        if (target.TotalShieldHealth() <= barrierme)
                        {
                            Barrier.Cast();
                        }

                        if (caster.GetAutoAttackDamage(target) > target.TotalShieldHealth())
                        {
                            Barrier.Cast();
                        }
                    }
                }
            }
        }

        public static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsAlly || !args.Target.IsValid || args.Target.IsEnemy || sender is Obj_AI_Minion
                || args.Target is Obj_AI_Minion || args.Target == null)
            {
                return;
            }


            var caster = sender;
            var target = (AIHeroClient)args.Target;

            if (!caster.IsEnemy || target == null)
            {
                return;
            }

            if (target.IsAlly && !target.IsMe)
            {
                if (Heal != null && !SummMenu["DontHeal" + target.BaseSkinName].Cast<CheckBox>().CurrentValue)
                {
                    var healc = SummMenu["Heal"].Cast<CheckBox>().CurrentValue && Heal.IsReady();
                    var healally = SummMenu["Healally"].Cast<Slider>().CurrentValue;
                    if (healc)
                    {
                        if (target.IsValidTarget(Heal.Range))
                        {
                            if (target.TotalShieldHealth() <= healally)
                            {
                                Heal.Cast();
                            }
                        }

                        if (caster.BaseAttackDamage > target.TotalShieldHealth()
                            || caster.BaseAbilityDamage > target.TotalShieldHealth())
                        {
                            Heal.Cast();
                        }
                    }
                }
            }

            if (target.IsMe)
            {
                if (Heal != null && !SummMenu["DontHeal" + target.BaseSkinName].Cast<CheckBox>().CurrentValue)
                {
                    var healc = SummMenu["Heal"].Cast<CheckBox>().CurrentValue && Heal.IsReady();
                    var healme = SummMenu["Healme"].Cast<Slider>().CurrentValue;
                    if (healc)
                    {
                        if (target.TotalShieldHealth() <= healme)
                        {
                            Heal.Cast();
                        }

                        if (caster.BaseAttackDamage > target.TotalShieldHealth()
                            || caster.BaseAbilityDamage > target.TotalShieldHealth())
                        {
                            Heal.Cast();
                        }
                    }
                }

                if (Barrier != null)
                {
                    var barrierc = SummMenu["barrier"].Cast<CheckBox>().CurrentValue && Barrier.IsReady();
                    var barrierme = SummMenu["barrierme"].Cast<Slider>().CurrentValue;
                    if (barrierc)
                    {
                        if (target.TotalShieldHealth() <= barrierme)
                        {
                            Barrier.Cast();
                        }

                        if (caster.BaseAttackDamage > target.TotalShieldHealth()
                            || caster.BaseAbilityDamage > target.TotalShieldHealth())
                        {
                            Barrier.Cast();
                        }
                    }
                }
            }
        }
    }
}