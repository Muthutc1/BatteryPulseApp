
using System.ComponentModel;


namespace BatteryPulseApp.Viewmodels
{
    public class BatteryViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region private‑fields
        private int _batteryLevel;
        private bool _isCharging;
        private bool _popupActive;
        private System.Timers.Timer _lowBatteryTimer;
        private bool _isTimerRunning;
        private bool _isShowBatteryHorizontalView;
        #endregion private‑fields

        /// <summary>
        /// Constructor
        /// </summary>
        public BatteryViewModel()
        {
            UpdateBatteryStatus();
            Battery.BatteryInfoChanged += (s, e) =>
            {
                BatteryLevel = (int)(e.ChargeLevel * 100);
                IsCharging = e.State == BatteryState.Charging || e.State == BatteryState.Full;
                OnPropertyChanged(nameof(BatteryChargingLevel));
                OnPropertyChanged(nameof(ChargingStatus));
                OnPropertyChanged(nameof(BatterySaverStatus));
                OnPropertyChanged(nameof(BatteryStateText));
                MonitorBatteryLevel();
            };
            MonitorBatteryLevel();
        }

        #region public‑properties
        public int BatteryLevel
        {
            get => _batteryLevel;
            set
            {
                if (_batteryLevel != value)
                {
                    _batteryLevel = value;
                    OnPropertyChanged(nameof(BatteryLevel));
                    OnPropertyChanged(nameof(BatteryChargingLevel));
                    OnPropertyChanged(nameof(BatterySaverStatus));
                }
            }
        }

        public bool IsCharging
        {
            get => _isCharging;
            set
            {
                if (_isCharging != value)
                {
                    _isCharging = value;
                    OnPropertyChanged(nameof(IsCharging));
                    OnPropertyChanged(nameof(ChargingStatus));
                    OnPropertyChanged(nameof(BatterySaverStatus));
                }
            }
        }

        public string BatteryChargingLevel => $"{BatteryLevel}%";
        public string ChargingStatus => IsCharging ? "Yes" : "No";
        public string BatterySaverStatus => BatterySaverOn ? "On" : "Off";
        public string BatteryStateText => Battery.State.ToString();
        public bool BatterySaverOn => BatteryLevel < 20 && !IsCharging;

        #endregion public‑properties

        #region Methods

        /// <summary>
        /// Update the battery status
        /// </summary>
        private void UpdateBatteryStatus()
        {
            BatteryLevel = (int)(Battery.ChargeLevel * 100);
            IsCharging = Battery.State == BatteryState.Charging || Battery.State == BatteryState.Full;
        }

        /// <summary>
        /// Monitor battery status
        /// when if less than 10 % show a alert message
        /// </summary>
        public void MonitorBatteryLevel()
        {
            if (BatteryLevel < 10)
                StartLowBatteryTimer();
            else
                StopLowBatteryTimer();
        }

        /// <summary>
        /// Timer is used to alert message to every 1 min 
        /// When if battery level is below 10%
        /// </summary>
        private void StartLowBatteryTimer()
        {
            if (_isTimerRunning) return;

            _lowBatteryTimer = new System.Timers.Timer(60000); // 1 minute
            _lowBatteryTimer.Elapsed += (s, e) =>
            {
                if (BatteryLevel < 10 && !_popupActive)
                {
                    _popupActive = true;
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Shell.Current.DisplayAlert("Low Battery", "Battery is below 10%!", "OK");
                        _popupActive = false;
                    });
                }
            };

            _lowBatteryTimer.AutoReset = true;
            _lowBatteryTimer.Start();
            _isTimerRunning = true;
        }


        /// <summary>
        /// To stop the timer fun
        /// </summary>
        private void StopLowBatteryTimer()
        {
            if (_lowBatteryTimer != null)
            {
                _lowBatteryTimer.Stop();
                _lowBatteryTimer.Dispose();
                _lowBatteryTimer = null;
            }

            _popupActive = false;
            _isTimerRunning = false;
        }
        #endregion Methods

        /// <summary>
        /// raise PropertyChanged 
        /// </summary>
        /// <param name="name"></param>
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}