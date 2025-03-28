using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MelonLoader;
using OWOGame;

namespace OWO_ElvenAssassin
{
    public class OWOSkin
    {
        public bool suitDisabled = true;
        public bool systemInitialized = false;
        private static bool heartBeatIsActive = false;
        private static bool waterIsActive = false;
        private static bool chokingIsActive = false;

        public Dictionary<String, Sensation> FeedbackMap = new Dictionary<String, Sensation>();

        public OWOSkin()
        {
            RegisterAllSensationsFiles();
            InitializeOWO();
        }

        #region Skin Configuration

        private void RegisterAllSensationsFiles()
        {
            string configPath = Directory.GetCurrentDirectory() + "\\Mods\\OWO";
            DirectoryInfo d = new DirectoryInfo(configPath);
            FileInfo[] Files = d.GetFiles("*.owo", SearchOption.AllDirectories);
            for (int i = 0; i < Files.Length; i++)
            {
                string filename = Files[i].Name;
                string fullName = Files[i].FullName;
                string prefix = Path.GetFileNameWithoutExtension(filename);
                if (filename == "." || filename == "..")
                    continue;
                string tactFileStr = File.ReadAllText(fullName);
                try
                {
                    Sensation test = Sensation.Parse(tactFileStr);
                    FeedbackMap.Add(prefix, test);
                }
                catch (Exception e) { LOG(e.Message); }

            }

            systemInitialized = true;
        }

        private async void InitializeOWO()
        {
            LOG("Initializing OWO skin");

            var gameAuth = GameAuth.Create(AllBakedSensations()).WithId("38940112");

            OWO.Configure(gameAuth);
            string[] myIPs = GetIPsFromFile("OWO_Manual_IP.txt");
            if (myIPs.Length == 0) await OWO.AutoConnect();
            else
            {
                await OWO.Connect(myIPs);
            }

            if (OWO.ConnectionState == OWOGame.ConnectionState.Connected)
            {
                suitDisabled = false;
                LOG("OWO suit connected.");
                Feel("Heartbeat");
            }
            if (suitDisabled) LOG("OWO is not enabled?!?!");
        }

        public BakedSensation[] AllBakedSensations()
        {
            var result = new List<BakedSensation>();

            foreach (var sensation in FeedbackMap.Values)
            {
                if (sensation is BakedSensation baked)
                {
                    LOG("Registered baked sensation: " + baked.name);
                    result.Add(baked);
                }
                else
                {
                    LOG("Sensation not baked? " + sensation);
                    continue;
                }
            }
            return result.ToArray();
        }

        public string[] GetIPsFromFile(string filename)
        {
            List<string> ips = new List<string>();
            string filePath = Directory.GetCurrentDirectory() + "\\BepinEx\\Plugins\\OWO" + filename;
            if (File.Exists(filePath))
            {
                LOG("Manual IP file found: " + filePath);
                var lines = File.ReadLines(filePath);
                foreach (var line in lines)
                {
                    if (IPAddress.TryParse(line, out _)) ips.Add(line);
                    else LOG("IP not valid? ---" + line + "---");
                }
            }
            return ips.ToArray();
        }

        ~OWOSkin()
        {
            LOG("Destructor called");
            DisconnectOWO();
        }

        public void DisconnectOWO()
        {
            LOG("Disconnecting OWO skin.");
            OWO.Disconnect();
        }
        #endregion

        public void Feel(String key, int Priority = 0, float intensity = 1.0f, float duration = 1.0f)
        {
            if (FeedbackMap.ContainsKey(key))
            {
                OWO.Send(FeedbackMap[key].WithPriority(Priority));
            }

            else LOG("Feedback not registered: " + key);
        }

        public void FeelWithHand(String key, int priority = 0, bool isRightHand = true, float intensity = 1.0f)
        {

            if (isRightHand)
            {
                key += " R";
            }
            else
            {
                key += " L";
            }

            Feel(key, priority, intensity);
        }

        public void LOG(string logStr)
        {
            MelonLogger.Msg(logStr);
        }

        #region heart beat loop
        public void StartHeartBeat()
        {
            if (heartBeatIsActive) return;

            heartBeatIsActive = true;
            HeartBeatFuncAsync();
        }

        public void StopHeartBeat()
        {
            heartBeatIsActive = false;
        }

        public async Task HeartBeatFuncAsync()
        {
            while (heartBeatIsActive)
            {
                Feel("Heartbeat", 0);
                await Task.Delay(1000);
            }
        }
        #endregion

        #region water loop
        public void StartWater()
        {
            if (waterIsActive) return;

            waterIsActive = true;
            WaterFuncAsync();
        }

        public void StopWater()
        {
            waterIsActive = false;
        }

        public async Task WaterFuncAsync()
        {
            while (waterIsActive)
            {
                Feel("Water Slushing", 0);
                await Task.Delay(5050);
            }
        }
        #endregion

        #region choking loop
        public void StartChoking()
        {
            if (chokingIsActive) return;

            chokingIsActive = true;
            ChokingFuncAsync();
        }

        public void StopChoking()
        {
            chokingIsActive = false;
        }

        public async Task ChokingFuncAsync()
        {
            while (chokingIsActive)
            {
                Feel("Choking", 0);
                await Task.Delay(1050);
            }
        }
        #endregion        

        //public void PlayBackHit(String key, float xzAngle, float yShift)
        //{
        //    bHapticsLib.ScaleOption scaleOption = new bHapticsLib.ScaleOption(1f, 1f);
        //    bHapticsLib.RotationOption rotationOption = new bHapticsLib.RotationOption(xzAngle, yShift);
        //    bHapticsLib.bHapticsManager.PlayRegistered(key, key, scaleOption, rotationOption);
        //}

        //public void GunRecoil(bool isRightHand, float intensity = 1.0f )
        //{
        //    float duration = 1.0f;
        //    var scaleOption = new bHapticsLib.ScaleOption(intensity, duration);
        //    var rotationFront = new bHapticsLib.RotationOption(0f, 0f);
        //    string postfix = "_L";
        //    if (isRightHand) { postfix = "_R"; }
        //    string keyArm = "Recoil" + postfix;
        //    string keyVest = "RecoilVest" + postfix;
        //    bHapticsLib.bHapticsManager.PlayRegistered(keyArm, keyArm, scaleOption, rotationFront);
        //    bHapticsLib.bHapticsManager.PlayRegistered(keyVest, keyVest, scaleOption, rotationFront);
        //}
        //public void SwordRecoil(bool isRightHand, float intensity = 1.0f)
        //{
        //    float duration = 1.0f;
        //    var scaleOption = new bHapticsLib.ScaleOption(intensity, duration);
        //    var rotationFront = new bHapticsLib.RotationOption(0f, 0f);
        //    string postfix = "_L";
        //    if (isRightHand) { postfix = "_R"; }
        //    string keyArm = "Sword" + postfix;
        //    string keyVest = "SwordVest" + postfix;
        //    bHapticsLib.bHapticsManager.PlayRegistered(keyArm, keyArm, scaleOption, rotationFront);
        //    bHapticsLib.bHapticsManager.PlayRegistered(keyVest, keyVest, scaleOption, rotationFront);
        //}

        //public bool isMinigunPlaying()
        //{
        //    if (IsPlaying("Minigun_L")) { return true; }
        //    if (IsPlaying("Minigun_R")) { return true; }
        //    if (IsPlaying("MinigunDual_L")) { return true; }
        //    if (IsPlaying("MinigunDual_R")) { return true; }
        //    return false;
        //}

        //public void FireMinigun(bool isRightHand, bool twoHanded)
        //{
        //    if (isMinigunPlaying()) { return; }

        //    string postfix = "";
        //    if (twoHanded) { postfix += "Dual"; }
        //    if (isRightHand) { postfix += "_R"; }
        //    else { postfix += "_L"; }
        //    string key = "Minigun" + postfix;
        //    string keyVest = "MinigunVest" + postfix;
        //    PlaybackHaptics(key);
        //    PlaybackHaptics(keyVest);
        //}

        //public void StopMinigun(bool isRightHand, bool twoHanded)
        //{
        //    string postfix = "";
        //    if (twoHanded) { postfix += "Dual"; }
        //    if (isRightHand) { postfix += "_R"; }
        //    else { postfix += "_L"; }
        //    string key = "Minigun" + postfix;
        //    string keyVest = "MinigunVest" + postfix;
        //    StopHapticFeedback(key);
        //    StopHapticFeedback(keyVest);
        //}

        //public void HeadShot()
        //{
        //    if (bHapticsLib.bHapticsManager.IsDeviceConnected(bHapticsLib.PositionID.Head)) { PlaybackHaptics("HitInTheFace"); }
        //    else { PlaybackHaptics("HeadShotVest"); }
        //}

        //public void FootStep(bool isRightFoot)
        //{
        //    if (!bHapticsLib.bHapticsManager.IsDeviceConnected(bHapticsLib.PositionID.FootLeft)) { return; }
        //    string postfix = "_L";
        //    if (isRightFoot) { postfix = "_R"; }
        //    string key = "FootStep" + postfix;
        //    PlaybackHaptics(key);
        //}       

        public void StopAllHapticFeedback()
        {
            StopHeartBeat();
            StopWater();
            StopChoking();

            OWO.Stop();
        }


    }
}
