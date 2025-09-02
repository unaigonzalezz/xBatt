using StreamDeckLib;
using StreamDeckLib.Messages;
using SharpDX.XInput;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace xbatt_deck
{
    [ActionUuid(Uuid = "com.unaigonzalez.xbatt.vibrate")]
    public class xBattVibrateAction : BaseStreamDeckActionWithSettingsModel<Models.xBattModel>
    {
        private Controller _controller;
        private CancellationTokenSource _vibrationCts;

        private const string ImageNormal = "images/controllers/vibrate_normal.png";
        private const string ImageActive = "images/controllers/vibrate_active.png";
        private const string ImageDisconnected = "images/Icons/battery_disconnected.png";

        public override async Task OnWillAppear(StreamDeckEventPayload args)
        {
            GetController(SettingsModel.ControllerNumber);

            if (_controller == null || !_controller.IsConnected)
                await SetButtonStateAsync(args.context, false, true);
            else
                await SetButtonStateAsync(args.context, false, false);

            await base.OnWillAppear(args);
        }

        private void GetController(int controllerNumber)
        {
            if (controllerNumber <= 0 || controllerNumber > 4)
                controllerNumber = 1;

            var controllers = new[]
            {
                new Controller(UserIndex.One),
                new Controller(UserIndex.Two),
                new Controller(UserIndex.Three),
                new Controller(UserIndex.Four)
            };

            _controller = controllers[controllerNumber - 1];
        }

        public override async Task OnKeyDown(StreamDeckEventPayload args)
        {
            GetController(SettingsModel.ControllerNumber);

            if (_controller == null || !_controller.IsConnected)
            {
                await SetButtonStateAsync(args.context, false, true);
                return;
            }

            _vibrationCts?.Cancel();
            _vibrationCts = new CancellationTokenSource();

            _ = VibrateLoopAsync(_controller, _vibrationCts.Token);

            await SetButtonStateAsync(args.context, true, false);
        }

        public override async Task OnKeyUp(StreamDeckEventPayload args)
        {
            _vibrationCts?.Cancel();

            if (_controller != null && _controller.IsConnected)
                _controller.SetVibration(new Vibration { LeftMotorSpeed = 0, RightMotorSpeed = 0 });

            if (_controller == null || !_controller.IsConnected)
                await SetButtonStateAsync(args.context, false, true);
            else
                await SetButtonStateAsync(args.context, false, false);
        }

        private async Task VibrateLoopAsync(Controller controller, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    controller.SetVibration(new Vibration
                    {
                        LeftMotorSpeed = 65535,
                        RightMotorSpeed = 65535
                    });
                    await Task.Delay(50, token);
                }
            }
            catch (TaskCanceledException) { }
            finally
            {
                controller.SetVibration(new Vibration { LeftMotorSpeed = 0, RightMotorSpeed = 0 });
            }
        }

        private async Task SetButtonStateAsync(string context, bool active, bool disconnected)
        {
            string imagePath;

            if (disconnected)
                imagePath = ImageDisconnected;
            else
                imagePath = active ? ImageActive : ImageNormal;

            if (File.Exists(imagePath))
                await Manager.SetImageAsync(context, imagePath);
        }

        public override async Task OnWillDisappear(StreamDeckEventPayload args)
        {
            _vibrationCts?.Cancel();

            if (_controller != null && _controller.IsConnected)
                _controller.SetVibration(new Vibration { LeftMotorSpeed = 0, RightMotorSpeed = 0 });

            await SetButtonStateAsync(args.context, false, false);

            await base.OnWillDisappear(args);
        }
    }
}
