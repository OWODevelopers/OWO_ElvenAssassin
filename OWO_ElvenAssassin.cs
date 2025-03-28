using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using MelonLoader;
using HarmonyLib;

using MyBhapticsTactsuit;

[assembly: MelonInfo(typeof(ElvenAssassin_bhaptics.OWO_ElvenAssassin), "ElvenAssassin_bhaptics", "2.0.0", "Florian Fahrenberger")]
[assembly: MelonGame("WenklyStudio", "Elven Assassin")]


namespace ElvenAssassin_bhaptics
{
    public class OWO_ElvenAssassin : MelonMod
    {
        public static TactsuitVR tactsuitVr;
        public static bool isRightHanded = true;

        public override void OnInitializeMelon()
        {
            tactsuitVr = new TactsuitVR();
            tactsuitVr.PlaybackHaptics("HeartBeat");
        }

        #region Shoot bow
        [HarmonyPatch(typeof(HandsDominanceSwitcher), "InitializeWithPlayer", new Type[] { typeof(bool) })]
        public class bhaptics_HandsDominance
        {
            [HarmonyPostfix]
            public static void Postfix(HandsDominanceSwitcher __instance, bool isLocalPlayer)
            {
                if (!isLocalPlayer) return;
                if (__instance.HandsDominance == HandsDominanceSwitcher.HandsDominanceType.Left) isRightHanded = false;
            }
        }

        [HarmonyPatch(typeof(WenklyStudio.BowController), "Shoot", new Type[] { })]
        public class bhaptics_ShootBow
        {
            [HarmonyPostfix]
            public static void Postfix(WenklyStudio.BowController __instance)
            {
                //if (!__instance.IsHandAttached) return;
                if (isRightHanded) tactsuitVr.PlaybackHaptics("BowRelease_R");
                else tactsuitVr.PlaybackHaptics("BowRelease_L");
            }
        }
        #endregion

        #region Get hit
        [HarmonyPatch(typeof(WenklyStudio.ElvenAssassin.DragonAttackControler), "KillPlayer", new Type[] { typeof(WenklyStudio.ElvenAssassin.PlayerController) })]
        public class bhaptics_DragonKillPlayer
        {
            [HarmonyPostfix]
            public static void Postfix(WenklyStudio.ElvenAssassin.PlayerController playerToBeKilled)
            {
                if (playerToBeKilled != PlayersManager.Instance.LocalPlayer) return;
                tactsuitVr.PlaybackHaptics("FlameThrower");
            }
        }

        [HarmonyPatch(typeof(WenklyStudio.ElvenAssassin.DeathMatchKillsController), "KillPlayer", new Type[] { typeof(PlayerControllerCore), typeof(PlayerControllerCore) })]
        public class bhaptics_PlayerKillPlayer
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerControllerCore victim)
            {
                if (victim != PlayersManager.Instance.LocalPlayer) return;
                tactsuitVr.PlaybackHaptics("Impact");
            }
        }

        [HarmonyPatch(typeof(WenklyStudio.ElvenAssassin.TrollAttackController), "AnimationEventKillPlayer", new Type[] {  })]
        public class bhaptics_TrollKillPlayer
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("Impact");
            }
        }

        [HarmonyPatch(typeof(WenklyStudio.ElvenAssassin.AxeController), "RpcPlayPlayerFleshSound", new Type[] { })]
        public class bhaptics_AxeHitPlayer
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("Impact");
            }
        }
        #endregion

        [HarmonyPatch(typeof(WenklyStudio.ElvenAssassin.TrollAttackController), "Shout", new Type[] { })]
        public class bhaptics_TrollShout
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("BellyRumble");
            }
        }

    }
}
