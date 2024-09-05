using StreamDeckLib;
using StreamDeckLib.Messages;
using SharpDX.XInput;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace xbatt_deck
{
    [ActionUuid(Uuid = "com.unaigonzalez.xBatt.checkBatt")]
    public class xBattAction : BaseStreamDeckActionWithSettingsModel<Models.xBattModel>
    {
        private Controller _controller;
        private System.Timers.Timer _updateTimer;
        private CancellationTokenSource _cts;

        public override async Task OnWillAppear(StreamDeckEventPayload args)
        {
            _cts = new CancellationTokenSource();
            _updateTimer = new System.Timers.Timer(1000); // Update status every 1 sec
            _updateTimer.Elapsed += async (sender, e) => await UpdateStatusAsync(args.context, _cts.Token);
            _updateTimer.Start();

            // Initialize controller status
            GetController(SettingsModel.ControllerNumber);

            // Establish controller status
            await UpdateStatusAsync(args.context, _cts.Token);

            await base.OnWillAppear(args);
        }

        private async Task UpdateStatusAsync(string context, CancellationToken token)
        {
            if (token.IsCancellationRequested) return;

            // Obtain selected controller
            GetController(SettingsModel.ControllerNumber);

            // Obtain status (status, icon, number)
            var (batteryStatus, iconPath, controllerNumber) = GetBatteryStatus();

            // Verify controller status
            if (_controller != null && _controller.IsConnected)
            {
                // Show controller number
                if (batteryStatus != "Controller disconnected" || iconPath != "Images/Icons/battery_disconnected.png")
                {
                    await Manager.SetTitleAsync(context, $"    {controllerNumber}");

                    // Update tile icon
                    if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
                    {
                        try
                        {
                            await Manager.SetImageAsync(context, iconPath);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error al cargar la imagen: {ex.Message}");
                        }
                    }
                }
            }
            else
            {
                // Show if the controller is disconnected
                await Manager.SetTitleAsync(context, "");
                await Manager.SetImageAsync(context, "Images/Icons/battery_disconnected.png");
            }
        }

        public override async Task OnKeyUp(StreamDeckEventPayload args)
        {
            // Update status every keypress
            await UpdateStatusAsync(args.context, CancellationToken.None);
            await Manager.SetSettingsAsync(args.context, SettingsModel);
        }

        public override async Task OnWillDisappear(StreamDeckEventPayload args)
        {
            // Stop counter and process
            _updateTimer?.Stop();
            _cts?.Cancel();
            _updateTimer?.Dispose();

            await base.OnWillDisappear(args);
        }

        private void GetController(int controllerNumber)
        {
            // Create controllers instance
            var controllers = new[]
            {
                new Controller(UserIndex.One),
                new Controller(UserIndex.Two),
                new Controller(UserIndex.Three),
                new Controller(UserIndex.Four)
            };
            _controller = controllers.ElementAtOrDefault(controllerNumber - 1); // userindex starts on 0
        }

        private (string batteryStatus, string iconPath, int controllerNumber) GetBatteryStatus()
        {
            if (_controller == null || !_controller.IsConnected)
            {
                return ("Controller disconnected", "Images/Icons/battery_disconnected.png", SettingsModel.ControllerNumber);
            }

            var batteryInfo = _controller.GetBatteryInformation(BatteryDeviceType.Gamepad);
            string iconPath;
            string batteryStatus;
            int controllerNumber = (int)_controller.UserIndex + 1; // 1,2,3,4 controllers number

            if (batteryInfo.BatteryType == BatteryType.Wired)
            {
                // USB Status
                batteryStatus = "Controller wired";
                iconPath = "Images/Icons/battery_wired.png";
            }
            else if (batteryInfo.BatteryType == BatteryType.Disconnected)
            {
                batteryStatus = "Controller disconnected";
                iconPath = "Images/Icons/battery_disconnected.png";
            }
            else if (batteryInfo.BatteryType == BatteryType.Unknown)
            {
                batteryStatus = "Unknown battery status";
                iconPath = "Images/Icons/battery_unknown.png";
            }
            else
            {
                // Show battery status
                switch (batteryInfo.BatteryLevel)
                {
                    case BatteryLevel.Empty:
                        batteryStatus = "Battery: Empty";
                        iconPath = "Images/Icons/battery_empty.png";
                        break;
                    case BatteryLevel.Low:
                        batteryStatus = "Battery: Low";
                        iconPath = "Images/Icons/battery_low.png";
                        break;
                    case BatteryLevel.Medium:
                        batteryStatus = "Battery: Medium";
                        iconPath = "Images/Icons/battery_medium.png";
                        break;
                    case BatteryLevel.Full:
                        batteryStatus = "Battery: Full";
                        iconPath = "Images/Icons/battery_full.png";
                        break;
                    default:
                        batteryStatus = "Unknown battery level";
                        iconPath = "Images/Icons/battery_unknown.png";
                        break;
                }
            }

            return (batteryStatus, iconPath, controllerNumber);
        }
    }
}
