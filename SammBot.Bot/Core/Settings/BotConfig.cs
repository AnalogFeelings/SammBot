using System;
using System.Collections.Generic;
using SammBot.Bot.Classes;

namespace SammBot.Bot.Core
{
    public class BotConfig
    {
        public delegate void TagDistanceModifiedEventHandler(object Sender, EventArgs Args);
        public event TagDistanceModifiedEventHandler TagDistanceModified;
        
        private int _TagDistance = 3;
        
        public int TagDistance
        {
            get => _TagDistance;
            set
            {
                _TagDistance = value;
                TagDistanceModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void MagicBallAnswersModifiedEventHandler(object Sender, EventArgs Args);
        public event MagicBallAnswersModifiedEventHandler MagicBallAnswersModified;

        private List<string> _MagicBallAnswers = null;

        public List<string> MagicBallAnswers
        {
            get => _MagicBallAnswers;
            set
            {
                _MagicBallAnswers = value;
                MagicBallAnswersModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void HugKaomojisModifiedEventHandler(object Sender, EventArgs Args);
        public event HugKaomojisModifiedEventHandler HugKaomojisModified;

        private List<string> _HugKaomojis = null;

        public List<string> HugKaomojis
        {
            get => _HugKaomojis;
            set
            {
                _HugKaomojis = value;
                HugKaomojisModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void KillMessagesModifiedEventHandler(object Sender, EventArgs Args);
        public event KillMessagesModifiedEventHandler KillMessagesModified;

        private List<string> _KillMessages = null;

        public List<string> KillMessages
        {
            get => _KillMessages;
            set
            {
                _KillMessages = value;
                KillMessagesModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void StatusListModifiedEventHandler(object Sender, EventArgs Args);
        public event StatusListModifiedEventHandler StatusListModified;

        private List<BotStatus> _StatusList = null;

        public List<BotStatus> StatusList
        {
            get => _StatusList;
            set
            {
                _StatusList = value;
                StatusListModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void OnlyOwnerModeModifiedEventHandler(object Sender, EventArgs Args);
        public event OnlyOwnerModeModifiedEventHandler OnlyOwnerModeModified;

        private bool _OnlyOwnerMode = false;

        public bool OnlyOwnerMode
        {
            get => _OnlyOwnerMode;
            set
            {
                _OnlyOwnerMode = value;
                OnlyOwnerModeModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void ShipBarStartEmptyModifiedEventHandler(object Sender, EventArgs Args);
        public event ShipBarStartEmptyModifiedEventHandler ShipBarStartEmptyModified;

        private string _ShipBarStartEmpty = null;

        public string ShipBarStartEmpty
        {
            get => _ShipBarStartEmpty;
            set
            {
                _ShipBarStartEmpty = value;
                ShipBarStartEmptyModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void ShipBarStartFullModifiedEventHandler(object Sender, EventArgs Args);
        public event ShipBarStartFullModifiedEventHandler ShipBarStartFullModified;

        private string _ShipBarStartFull = null;

        public string ShipBarStartFull
        {
            get => _ShipBarStartFull;
            set
            {
                _ShipBarStartFull = value;
                ShipBarStartFullModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void ShipBarHalfEmptyModifiedEventHandler(object Sender, EventArgs Args);
        public event ShipBarHalfEmptyModifiedEventHandler ShipBarHalfEmptyModified;

        private string _ShipBarHalfEmpty = null;

        public string ShipBarHalfEmpty
        {
            get => _ShipBarHalfEmpty;
            set
            {
                _ShipBarHalfEmpty = value;
                ShipBarHalfEmptyModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void ShipBarHalfFullModifiedEventHandler(object Sender, EventArgs Args);
        public event ShipBarHalfFullModifiedEventHandler ShipBarHalfFullModified;

        private string _ShipBarHalfFull = null;

        public string ShipBarHalfFull
        {
            get => _ShipBarHalfFull;
            set
            {
                _ShipBarHalfFull = value;
                ShipBarHalfFullModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void ShipBarEndEmptyModifiedEventHandler(object Sender, EventArgs Args);
        public event ShipBarEndEmptyModifiedEventHandler ShipBarEndEmptyModified;

        private string _ShipBarEndEmpty = null;

        public string ShipBarEndEmpty
        {
            get => _ShipBarEndEmpty;
            set
            {
                _ShipBarEndEmpty = value;
                ShipBarEndEmptyModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void ShipBarEndFullModifiedEventHandler(object Sender, EventArgs Args);
        public event ShipBarEndFullModifiedEventHandler ShipBarEndFullModified;

        private string _ShipBarEndFull = null;

        public string ShipBarEndFull
        {
            get => _ShipBarEndFull;
            set
            {
                _ShipBarEndFull = value;
                ShipBarEndFullModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void RotatingStatusModifiedEventHandler(object Sender, EventArgs Args);
        public event RotatingStatusModifiedEventHandler RotatingStatusModified;

        private bool _RotatingStatus = true;

        public bool RotatingStatus
        {
            get => _RotatingStatus;
            set
            {
                _RotatingStatus = value;
                RotatingStatusModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void RotatingAvatarModifiedEventHandler(object Sender, EventArgs Args);
        public event RotatingAvatarModifiedEventHandler RotatingAvatarModified;

        private bool _RotatingAvatar = true;

        public bool RotatingAvatar
        {
            get => _RotatingAvatar;
            set
            {
                _RotatingAvatar = value;
                RotatingAvatarModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void WaitForDebuggerModifiedEventHandler(object Sender, EventArgs Args);

        public event WaitForDebuggerModifiedEventHandler WaitForDebuggerModified;

        private bool _WaitForDebugger = false;

        [NeedsReboot]
        public bool WaitForDebugger
        {
            get => _WaitForDebugger;
            set
            {
                _WaitForDebugger = value;
                WaitForDebuggerModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void BotTokenModifiedEventHandler(object Sender, EventArgs Args);
        public event BotTokenModifiedEventHandler BotTokenModified;

        private string _BotToken = "";

        [NeedsReboot]
        public string BotToken
        {
            get => _BotToken;
            set
            {
                _BotToken = value;
                BotTokenModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void CatKeyModifiedEventHandler(object Sender, EventArgs Args);
        public event CatKeyModifiedEventHandler CatKeyModified;

        private string _CatKey = "";

        public string CatKey
        {
            get => _CatKey;
            set
            {
                _CatKey = value;
                CatKeyModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void DogKeyModifiedEventHandler(object Sender, EventArgs Args);
        public event DogKeyModifiedEventHandler DogKeyModified;

        private string _DogKey = "";

        public string DogKey
        {
            get => _DogKey;
            set
            {
                _DogKey = value;
                DogKeyModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void OpenWeatherKeyModifiedEventHandler(object Sender, EventArgs Args);
        public event OpenWeatherKeyModifiedEventHandler OpenWeatherKeyModified;

        private string _OpenWeatherKey = "";

        public string OpenWeatherKey
        {
            get => _OpenWeatherKey;
            set
            {
                _OpenWeatherKey = value;
                OpenWeatherKeyModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void TwitchUrlModifiedEventHandler(object Sender, EventArgs Args);

        public event TwitchUrlModifiedEventHandler TwitchUrlModified;

        private string _TwitchUrl = "https://www.twitch.tv/coreaesthetics";

        public string TwitchUrl
        {
            get => _TwitchUrl;
            set
            {
                _TwitchUrl = value;
                TwitchUrlModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void CommandLogFormatModifiedEventHandler(object Sender, EventArgs Args);
        public event CommandLogFormatModifiedEventHandler CommandLogFormatModified;

        private string _CommandLogFormat = "Executing command \"{0}\". Channel: #{1}. User: @{2}.";

        public string CommandLogFormat
        {
            get => _CommandLogFormat;
            set
            {
                _CommandLogFormat = value;
                CommandLogFormatModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void AvatarRotationTimeModifiedEventHandler(object Sender, EventArgs Args);
        public event AvatarRotationTimeModifiedEventHandler AvatarRotationTimeModified;

        private int _AvatarRotationTime = 1;

        public int AvatarRotationTime
        {
            get => _AvatarRotationTime;
            set
            {
                _AvatarRotationTime = value;
                AvatarRotationTimeModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void AvatarRecentQueueSizeModifiedEventHandler(object Sender, EventArgs Args);
        public event AvatarRecentQueueSizeModifiedEventHandler AvatarRecentQueueModified;

        private int _AvatarRecentQueueSize = 10;

        public int AvatarRecentQueueSize
        {
            get => _AvatarRecentQueueSize;
            set
            {
                _AvatarRecentQueueSize = value;
                AvatarRecentQueueModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public delegate void MessageCacheSizeModifiedEventHandler(object Sender, EventArgs Args);
        public event MessageCacheSizeModifiedEventHandler MessageCacheSizeModified;

        private int _MessageCacheSize = 2000;

        [NeedsReboot]
        public int MessageCacheSize
        {
            get => _MessageCacheSize;
            set
            {
                _MessageCacheSize = value;
                MessageCacheSizeModified?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}