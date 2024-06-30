namespace BoxHitsBOIII.MemoryUtils
{
    public static class Addresses
    {
        private static IntPtr _BaseGameAddr;
        public static IntPtr BaseGameAddr
        {
            get => _BaseGameAddr;
            set
            {
                _BaseGameAddr = value;
            }
        }

        private static IntPtr _PointsAddr;
        public static IntPtr PointsAddr
        {
            get => _PointsAddr;
            set
            {
                _PointsAddr = value;
            }
        }

        private static IntPtr _BoxCoverAddr; // 1 opened | 2 closed
        public static IntPtr BoxCoverAddr
        {
            get => _BoxCoverAddr;
            set
            {
                _BoxCoverAddr = value;
            }
        }

        private static IntPtr _GamePausedAddr; // 1 paused | 2 playing
        public static IntPtr GamePausedAddr
        {
            get => _GamePausedAddr;
            set
            {
                _GamePausedAddr = value;
            }
        }

        private static IntPtr _MapNameAddr;
        public static IntPtr MapNameAddr
        {
            get => _MapNameAddr;
            set
            {
                _MapNameAddr = value;
            }
        }

        private static IntPtr _BoxGunAddr;
        public static IntPtr BoxGunAddr
        {
            get => _BoxGunAddr;
            set
            {
                _BoxGunAddr = value;
            }
        }

        private static IntPtr _RoundNumber;
        public static IntPtr RoundNumber
        {
            get => _RoundNumber;
            set
            {
                _RoundNumber = value;
            }
        }
    }
}