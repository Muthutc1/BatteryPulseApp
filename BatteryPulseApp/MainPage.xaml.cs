using BatteryPulseApp.Viewmodels;
using System.ComponentModel;

namespace BatteryPulseApp
{
    public partial class MainPage : ContentPage
    {
        #region fields

        private BatteryViewModel _vm;
        private double _lastWidth = 0;
        private bool _progressVisible = false;

        #endregion fields

        #region Mainmethods
        /// <summary>
        /// 
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            BindingContext = _vm = new BatteryViewModel();
           
            _vm.PropertyChanged += Vm_PropertyChanged;
            AnimateBattery(_vm.BatteryLevel);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            AnimateGlow();
        }

        #endregion Mainmethods

        #region animation

        /// <summary>
        /// horizontal battery fill bar based on current battery level.
        /// </summary>
        /// <param name="batteryLevel"></param>
        private void AnimateBattery(int batteryLevel)
        {
            double totalWidth = BatteryFrame.Width;
            if (totalWidth == -1) return; // Not ready yet

            double targetWidth = totalWidth * batteryLevel / 100.0;

            var animation = new Animation(v =>
            {
                BatteryFill.WidthRequest = v;
            }, _lastWidth, targetWidth);

            animation.Commit(this, "BatteryFillAnim", 16, 600, Easing.CubicInOut);
            _lastWidth = targetWidth;
        }

        /// <summary>
        /// glowing animation around the circular battery
        /// </summary>
        private void AnimateGlow()
        {
            var animation = new Animation(v => GlowEllipse.Opacity = v, 0.3, 1);
            animation.Commit(this, "GlowPulse", length: 1000, repeat: () => true, easing: Easing.Linear);
        }
        #endregion animation

        #region Events
        /// <summary>
        /// Handles property changes in the ViewModel and updates the UI accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_vm.BatteryLevel))
            {
                if (_progressVisible)
                    AnimateBattery(_vm.BatteryLevel);
            }
        }

        /// <summary>
        /// Eventhandler to show/hide the battery view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            _progressVisible = !_progressVisible;

            BatteryFrame.IsVisible = _progressVisible;
            showbatteryText.Text = _progressVisible ? "Show Battery View" : "Hide Battery View";

            if (_progressVisible)
            {
                // Delay start animation slightly after layout
                this.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
                {
                        AnimateBattery(_vm.BatteryLevel);
                });
            }
            else
                this.AbortAnimation("BatteryFillAnim");
        }
        #endregion Events
    }
}
