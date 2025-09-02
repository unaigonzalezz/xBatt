using StreamDeckLib;
using StreamDeckLib.Messages;
using SharpDX.XInput;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace xbatt_deck
{
    [ActionUuid(Uuid = "com.unaigonzalez.xbatt.connected-controllers")]
    public class xBattConnectedControllersImage : BaseStreamDeckActionWithSettingsModel<Models.xBattModel>
    {
        private System.Timers.Timer _updateTimer;
        private CancellationTokenSource _cts;
        private readonly string imagesPath = "images/controllers/";

        public override async Task OnWillAppear(StreamDeckEventPayload args)
        {
            _cts = new CancellationTokenSource();
            _updateTimer = new System.Timers.Timer(1000);
            _updateTimer.Elapsed += async (sender, e) => await UpdateControllerImageAsync(args.context, _cts.Token);
            _updateTimer.Start();

            await UpdateControllerImageAsync(args.context, _cts.Token);

            await base.OnWillAppear(args);
        }

        private async Task UpdateControllerImageAsync(string context, CancellationToken token)
        {
            if (token.IsCancellationRequested) return;

            var controllers = new[]
            {
                new Controller(UserIndex.One),
                new Controller(UserIndex.Two),
                new Controller(UserIndex.Three),
                new Controller(UserIndex.Four)
            };

            int connectedCount = controllers.Count(c => c.IsConnected);
            if (connectedCount > 4) connectedCount = 4;

            string imageFile = Path.Combine(imagesPath, $"controllers_{connectedCount}.png");

            if (!File.Exists(imageFile))
            {
                System.Diagnostics.Debug.WriteLine($"Image not found: {imageFile}");
                return;
            }

            await Manager.SetImageAsync(context, imageFile);
        }

        public override async Task OnWillDisappear(StreamDeckEventPayload args)
        {
            _updateTimer?.Stop();
            _cts?.Cancel();
            _updateTimer?.Dispose();
            await base.OnWillDisappear(args);
        }
    }
}
