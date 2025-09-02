<h1 align="center">
  <br>
  <a><img src="https://i.postimg.cc/L6j8hKKk/xbatt.png" alt="XBatt Logo" height="100px"></a>
</h1>
<h3 align="center">Monitor your Xbox (XInput) controllers directly from your Stream Deck!</h3>

<p align="center">
  <a href="#key-features">Key Features</a> •
  <a href="#requirements">Requirements</a> •
  <a href="#download">Download</a> •
  <a href="#support">Support</a> •
  <a href="#contributing">Contributing</a> •
  <a href="#thanks">Thanks</a> •
  <a href="#license">License</a>
</p>

<h1 align="center">
  <br>
  <img src="https://i.postimg.cc/SNPfQ94Y/Actions.png" alt="XBatt Actions">
</h1>

**xBatt** is a Stream Deck plugin written in C# that provides real-time information about your Xbox controllers through XInput.  
It lets you check battery levels, see which buttons are pressed, activate vibration, and monitor how many controllers are connected — all without leaving your Stream Deck.

---

## Key Features

- **Supports up to 4 Xbox controllers**: Monitor each controller individually.  
- **Battery status**: Displays battery as *Empty, Low, Medium,* or *Full*.  
- **Customizable battery display**: Choose which controller to show when disconnected.  
- **Controller actions**:  
  - 🔋 **xBatt Indicator** → Battery status of the selected controller.  
  - 🎮 **xBatt Buttons** → Shows pressed buttons in real-time.  
  - 💥 **xBatt Vibrate** → Trigger vibration on the selected controller.  
  - 👥 **xBatt Controllers** → See how many controllers are currently connected.  
- **Quick refresh**: Press the action to update controller info instantly.  

⚠️ Works best with the **official Xbox Wireless Dongle for PC**. Some Bluetooth adapters/controllers may not behave correctly due to XInput limitations.  

Made with ❤️ by **Unai González**.

---

## Requirements

- Windows 10 or later  
- Stream Deck 6.7 or later  
- .NET 6.0 Runtime  

---

## Download

👉 Get the latest release on the [Stream Deck Marketplace](https://apps.elgato.com/plugins)  
or from the [GitHub Releases Page](https://github.com/unaigonzalezz/xBatt/releases/tag/xBatt).

---

## Support

If you’d like to support development:  

<a href="https://www.buymeacoffee.com/unaiitxuu" target="_blank">
  <img src="https://www.buymeacoffee.com/assets/img/custom_images/purple_img.png" height="41">
</a>

<a href="https://ko-fi.com/unaigonzalez" target="_blank">
  <img src="https://user-images.githubusercontent.com/7586345/125668092-55af2a45-aa7d-4795-93ed-de0a9a2828c5.png" width="160">
</a>

---

## Contributing

Pull requests are welcome! For major changes, please open an issue first to discuss what you’d like to improve.  

---

## Thanks

- [Stream Deck Toolkit](https://github.com/FritzAndFriends/StreamDeckToolkit) for the amazing template.  
- [SharpDX DirectInput](https://www.nuget.org/packages/SharpDX.DirectInput) for controller logic.  
- [Nicolae (Xelu)](https://thoseawesomeguys.com/prompts/) for creating such an incredible set of button icons.

---

## License

[MIT](https://choosealicense.com/licenses/mit/)
