using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverHomeCommand
{
    public class Config : IRocketPluginConfiguration
    {
        public string MessageColor;
        public int HomeDelay;
        public string BypassDelayPermission;
        public bool CancelOnHarm;
        public bool CancelOnMove;
        public ushort HomeSucceedEffect;
        public int EffectRadius;
        public bool PlayEffectOnRespawnBed;
        public bool PlayEffectOnRespawnNatural;
        public void LoadDefaults()
        {
            MessageColor = "ffff00";
            HomeDelay = 5;
            BypassDelayPermission = "home.bypassdelay";
            CancelOnHarm = true;
            CancelOnMove = true;
            HomeSucceedEffect = 1956;
            EffectRadius = 150;
            PlayEffectOnRespawnBed = true;
            PlayEffectOnRespawnNatural = true;
        }
    }
}
