﻿using FlagPFP.Core.FlagMaking;

namespace SammBotNET.Services
{
    public class FlagService
    {
        public FlagCoreObject FlagMaker = new("Flags");
        public bool LoadedFlags = false;
        public int FlagNumber = 0;
    }
}
