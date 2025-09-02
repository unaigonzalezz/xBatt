using StreamDeckLib;
using StreamDeckLib.Messages;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace xbatt_deck
{
    [ActionUuid(Uuid = "com.unaigonzalez.xbatt.pressed-buttons")]
    public class xBattPressedButtons : BaseStreamDeckActionWithSettingsModel<Models.xBattModel>
    {
        private Controller _controller;
        private System.Timers.Timer _updateTimer;
        private CancellationTokenSource _cts;
        private readonly string buttonsPath = "images/buttons/";
        private readonly string  iconsPath = "images/icons/";

        public override async Task OnWillAppear(StreamDeckEventPayload args)
        {
            _cts = new CancellationTokenSource();
            _updateTimer = new System.Timers.Timer(100);
            _updateTimer.Elapsed += async (sender, e) => await UpdateButtonAsync(args.context, _cts.Token);
            _updateTimer.Start();

            GetController(SettingsModel.ControllerNumber);
            await UpdateButtonAsync(args.context, _cts.Token);

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

            _controller = controllers.ElementAtOrDefault(controllerNumber - 1);
        }

        private async Task UpdateButtonAsync(string context, CancellationToken token)
        {
            if (token.IsCancellationRequested) return;

            GetController(SettingsModel.ControllerNumber);

            if (_controller == null || !_controller.IsConnected)
            {
                string disconnectedPath = Path.Combine(iconsPath, "battery_disconnected.png");
                if (File.Exists(disconnectedPath))
                    await Manager.SetImageAsync(context, disconnectedPath);
                return;
            }

            try
            {
                var state = _controller.GetState();
                var buttons = state.Gamepad.Buttons;

                var pressedImages = new List<string>();

                if (buttons.HasFlag(GamepadButtonFlags.A)) pressedImages.Add("button_a.png");
                if (buttons.HasFlag(GamepadButtonFlags.B)) pressedImages.Add("button_b.png");
                if (buttons.HasFlag(GamepadButtonFlags.X)) pressedImages.Add("button_x.png");
                if (buttons.HasFlag(GamepadButtonFlags.Y)) pressedImages.Add("button_y.png");
                if (buttons.HasFlag(GamepadButtonFlags.LeftShoulder)) pressedImages.Add("button_lb.png");
                if (buttons.HasFlag(GamepadButtonFlags.RightShoulder)) pressedImages.Add("button_rb.png");
                if (buttons.HasFlag(GamepadButtonFlags.Start)) pressedImages.Add("button_start.png");
                if (buttons.HasFlag(GamepadButtonFlags.Back)) pressedImages.Add("button_back.png");
                if (buttons.HasFlag(GamepadButtonFlags.DPadUp)) pressedImages.Add("dpad_up.png");
                if (buttons.HasFlag(GamepadButtonFlags.DPadDown)) pressedImages.Add("dpad_down.png");
                if (buttons.HasFlag(GamepadButtonFlags.DPadLeft)) pressedImages.Add("dpad_left.png");
                if (buttons.HasFlag(GamepadButtonFlags.DPadRight)) pressedImages.Add("dpad_right.png");
                if (buttons.HasFlag(GamepadButtonFlags.LeftThumb)) pressedImages.Add("button_ls.png");
                if (buttons.HasFlag(GamepadButtonFlags.RightThumb)) pressedImages.Add("button_rs.png");

                string leftStickDir = GetThumbDirection(state.Gamepad.LeftThumbX, state.Gamepad.LeftThumbY, "ls");
                if (!string.IsNullOrEmpty(leftStickDir)) pressedImages.Add(leftStickDir);

                string rightStickDir = GetThumbDirection(state.Gamepad.RightThumbX, state.Gamepad.RightThumbY, "rs");
                if (!string.IsNullOrEmpty(rightStickDir)) pressedImages.Add(rightStickDir);

                const byte triggerThreshold = 30;
                if (state.Gamepad.LeftTrigger > triggerThreshold) pressedImages.Add("button_lt.png");
                if (state.Gamepad.RightTrigger > triggerThreshold) pressedImages.Add("button_rt.png");

                if (pressedImages.Count == 0)
                    pressedImages.Add("none.png");

                using (var finalImage = CreateOverlayImage("none.png", pressedImages))
                {
                    string tempPath = Path.Combine(Path.GetTempPath(), "streamdeck_buttons.png");
                    finalImage.Save(tempPath, ImageFormat.Png);

                    await Manager.SetImageAsync(context, tempPath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                string disconnectedPath = Path.Combine(buttonsPath, "battery_disconnected.png");
                if (File.Exists(disconnectedPath))
                    await Manager.SetImageAsync(context, disconnectedPath);
            }
        }

        private string GetThumbDirection(short x, short y, string prefix)
        {
            const short deadzone = 8000;
            if (Math.Abs(x) < deadzone && Math.Abs(y) < deadzone) return null;

            string dir = "";
            if (y > deadzone) dir += "up";
            else if (y < -deadzone) dir += "down";
            if (x > deadzone) dir += "right";
            else if (x < -deadzone) dir += "left";
            if (string.IsNullOrEmpty(dir)) return null;

            return $"{prefix}_{dir}.png";
        }

        private Bitmap CreateOverlayImage(string backgroundFile, List<string> overlayFiles)
        {
            var backgroundPath = Path.Combine(buttonsPath, backgroundFile);
            if (!File.Exists(backgroundPath))
                throw new FileNotFoundException($"Background not found: {backgroundPath}");

            var bg = new Bitmap(backgroundPath);

            using (var g = Graphics.FromImage(bg))
            {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

                int count = overlayFiles.Count;
                if (count == 0) return bg;

                List<Point> positions = CalculatePositions(count, bg.Width, bg.Height, out int targetSize);

                for (int i = 0; i < overlayFiles.Count; i++)
                {
                    var overlayPath = Path.Combine(buttonsPath, overlayFiles[i]);
                    if (!File.Exists(overlayPath)) continue;

                    using (var overlay = new Bitmap(overlayPath))
                    {
                        int newWidth = targetSize;
                        int newHeight = overlay.Height * targetSize / overlay.Width;
                        if (newHeight > targetSize)
                        {
                            newHeight = targetSize;
                            newWidth = overlay.Width * targetSize / overlay.Height;
                        }

                        var pos = positions[Math.Min(i, positions.Count - 1)];
                        int xOffset = pos.X - newWidth / 2;
                        int yOffset = pos.Y - newHeight / 2;

                        g.DrawImage(overlay, xOffset, yOffset, newWidth, newHeight);
                    }
                }
            }

            return bg;
        }

        private List<Point> CalculatePositions(int count, int width, int height, out int targetSize)
        {
            var list = new List<Point>();
            targetSize = 0;

            switch (count)
            {
                case 1:
                    list.Add(new Point(width / 2, height / 2));
                    targetSize = Math.Min(width, height) - 8;
                    break;

                case 2:
                    targetSize = (width - 12) / 2;
                    list.Add(new Point(width / 4, height / 2));
                    list.Add(new Point(3 * width / 4, height / 2));
                    break;

                case 3:
                    targetSize = (width - 12) / 2;
                    list.Add(new Point(width / 2, height / 4));
                    list.Add(new Point(width / 4, 3 * height / 4));
                    list.Add(new Point(3 * width / 4, 3 * height / 4));
                    break;

                case 4:
                    targetSize = (width - 16) / 2;
                    list.Add(new Point(width / 4, height / 4));
                    list.Add(new Point(3 * width / 4, height / 4));
                    list.Add(new Point(width / 4, 3 * height / 4));
                    list.Add(new Point(3 * width / 4, 3 * height / 4));
                    break;

                default:
                    int cols = (int)Math.Ceiling(Math.Sqrt(count));
                    int rows = (int)Math.Ceiling((double)count / cols);
                    targetSize = Math.Min(width / cols, height / rows) - 4;

                    for (int r = 0; r < rows; r++)
                    {
                        for (int c = 0; c < cols; c++)
                        {
                            int index = r * cols + c;
                            if (index >= count) break;
                            int x = width * (2 * c + 1) / (2 * cols);
                            int y = height * (2 * r + 1) / (2 * rows);
                            list.Add(new Point(x, y));
                        }
                    }
                    break;
            }

            return list;
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
