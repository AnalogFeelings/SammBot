using FlagPFP.Core.FlagMaking;

namespace SammBotNET.Services
{
    public class FlagService
    {
        public FlagCoreObject FlagMaker = new FlagCoreObject("Flags");
        public bool LoadedFlags = false;
        public int FlagNumber = 0;
    }
}
