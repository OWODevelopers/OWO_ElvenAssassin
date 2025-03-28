﻿using System;
using MelonLoader;
using HarmonyLib;
using UnityEngine;
using WenklyStudio.Payload;
using WenklyStudio.ElvenAssassin;

[assembly: MelonInfo(typeof(OWO_ElvenAssassin.OWO_ElvenAssassin), "OWO_ElvenAssassin", "1.0.0", "OWOGame")]
[assembly: MelonGame("WenklyStudio", "Elven Assassin")]


namespace OWO_ElvenAssassin
{
    public class OWO_ElvenAssassin : MelonMod
    {
        public static OWOSkin owoSkin;
        public static bool isRightHanded = true;

        public override void OnInitializeMelon()
        {
            owoSkin = new OWOSkin();
        }

        #region Shoot bow
        [HarmonyPatch(typeof(HandsDominanceSwitcher), "InitializeWithPlayer", new Type[] { typeof(bool) })]
        public class HandsDominance
        {
            [HarmonyPostfix]
            public static void Postfix(HandsDominanceSwitcher __instance, bool isLocalPlayer)
            {
                if (!isLocalPlayer) return;
                if (__instance.HandsDominance == HandsDominanceSwitcher.HandsDominanceType.Left) isRightHanded = false;
            }
        }

        [HarmonyPatch(typeof(WenklyStudio.BowController), "UpdateBowTensionValue", new Type[] { })]
        public class UpdateBowTensionValue
        {
            [HarmonyPostfix]
            public static void Postfix(WenklyStudio.BowController __instance)
            {
                if (!owoSkin.suitEnabled) return;

                if(__instance.BowAnimationNormalizedTime >= 0.3)
                {
                    owoSkin.StartChoking();
                }

                //owoSkin.LOG($"UpdateBowTensionValue: {__instance.BowAnimationNormalizedTime}");
            }
        }
        
        [HarmonyPatch(typeof(WenklyStudio.BowController), "Shoot", new Type[] { })]
        public class ShootBow
        {
            [HarmonyPostfix]
            public static void Postfix(WenklyStudio.BowController __instance)
            {
                //if (!__instance.IsHandAttached) return;

                owoSkin.StopChoking();
                owoSkin.FeelWithHand("Bow Release", 2, isRightHanded);
            }
        }
        #endregion

        #region Get hit
        [HarmonyPatch(typeof(WenklyStudio.ElvenAssassin.DragonAttackControler), "KillPlayer", new Type[] { typeof(WenklyStudio.ElvenAssassin.PlayerController) })]
        public class DragonKillPlayer
        {
            [HarmonyPostfix]
            public static void Postfix(WenklyStudio.ElvenAssassin.PlayerController playerToBeKilled)
            {
                if (playerToBeKilled != PlayersManager.Instance.LocalPlayer) return;
                owoSkin.Feel("Flame Thrower", 3);
            }
        }

        [HarmonyPatch(typeof(WenklyStudio.ElvenAssassin.DeathMatchKillsController), "KillPlayer", new Type[] { typeof(PlayerControllerCore), typeof(PlayerControllerCore) })]
        public class PlayerKillPlayer
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerControllerCore victim)
            {
                if (victim != PlayersManager.Instance.LocalPlayer) return;
                owoSkin.Feel("Impact", 3);
            }
        }

        [HarmonyPatch(typeof(WenklyStudio.ElvenAssassin.TrollAttackController), "AnimationEventKillPlayer", new Type[] {  })]
        public class TrollKillPlayer
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.Feel("Impact", 3);
            }
        }

        [HarmonyPatch(typeof(WenklyStudio.ElvenAssassin.AxeController), "RpcPlayPlayerFleshSound", new Type[] { })]
        public class AxeHitPlayer
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.Feel("Impact", 3);
            }
        }
        #endregion

        [HarmonyPatch(typeof(WenklyStudio.ElvenAssassin.TrollAttackController), "Shout")]
        public class TrollShout
        {
            [HarmonyPostfix]
            public static void Postfix()
            {                
                owoSkin.Feel("Belly Rumble", 3);
            }
        }

        [HarmonyPatch(typeof(TeleportController), "TeleportLocalPlayer")]
        public class TeleportLocalPlayer
        {
            [HarmonyPostfix]
            public static void Postfix()
            {                
                if (!owoSkin.suitEnabled) return;

                owoSkin.Feel("Teleport", 2);
            }
        }

        #region RPG MODE
        [HarmonyPatch(typeof(GateController), "DamageGate")]
        public class DamageGate
        {
            static bool gateDestroyed = false;

            [HarmonyPrefix]
            public static void Prefix(GateController __instance)
            {
                gateDestroyed = Traverse.Create(__instance).Field("gateAlreadyDestroyed").GetValue<bool>();
            }

            [HarmonyPostfix]
            public static void Postfix(GateController __instance)
            {
                if (!owoSkin.suitEnabled) return;


                if (__instance.EnemiesThatCanEnter == __instance.MaxEnemiesThatCanEnter) {
                    gateDestroyed = false;
                }


                if (!gateDestroyed) {
                    owoSkin.Feel("Gate Damage", 3);

                    //If remaining life less than 5 start heartbeat
                    if (__instance.EnemiesThatCanEnter <= __instance.MaxEnemiesThatCanEnter / 4)
                    {
                        owoSkin.StartHeartBeat();
                    }

                    //If dead stop heatbeat
                    if (__instance.EnemiesThatCanEnter == 0)
                    {
                        gateDestroyed = true;
                        owoSkin.StopAllHapticFeedback();
                        owoSkin.Feel("Death", 3);
                    }

                    //owoSkin.LOG($"Damage Gate {__instance.EnemiesThatCanEnter}");
                }
            }
        }

        [HarmonyPatch(typeof(CartController), "CallOnCartDiedEvent")]
        public class CallOnCartDiedEvent
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                //if (!owoSkin.suitEnabled) return;

                owoSkin.Feel("Death", 2);
            }
        }
        #endregion

        #region Interactable

        [HarmonyPatch(typeof(CannonController), "FireCannon")]
        public class FireCannon
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!owoSkin.suitEnabled) return;

                owoSkin.Feel("Fire Cannon", 2);
            }
        }

        [HarmonyPatch(typeof(CatapultController), "ThrowRock")]
        public class ThrowRock
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!owoSkin.suitEnabled) return;

                owoSkin.Feel("Fire Catapult", 2);
            }
        }

        [HarmonyPatch(typeof(BalistaShootController), "Shoot")]
        public class Shoot
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!owoSkin.suitEnabled) return;

                owoSkin.Feel("Fire Balista", 2);
            }
        }

        [HarmonyPatch(typeof(HandPickUpController), "UseItem")]
        public class UseItem
        {
            [HarmonyPostfix]
            public static void Postfix(HandPickUpController __instance)
            {
                if (!owoSkin.suitEnabled) return;

                owoSkin.LOG($"UseITem hover:{Traverse.Create(__instance).Field("pickableInteracterHovered").GetValue<PickableInteracter>().name}");
                owoSkin.LOG($"UseITem grabbed:{Traverse.Create(__instance).Field("pickableInteracterGrabbed").GetValue<PickableInteracter>().name}");

                //owoSkin.Feel("Use Item", 2);
            }
        }

        #endregion
    }
}
