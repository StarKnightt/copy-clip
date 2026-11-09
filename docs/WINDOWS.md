# Copy Clip for Windows

[한국어](WINDOWS.ko.md)

## Run

Extract the complete x64 ZIP into a folder and open `CopyClip.exe`. The app uses Microsoft Edge WebView2 Evergreen Runtime. If the runtime is missing, install the **Evergreen Bootstrapper** and reopen the app. [Microsoft WebView2 downloads](https://developer.microsoft.com/microsoft-edge/webview2/)

The packaged app was tested on Windows 10 22H2 x64. See Microsoft's support matrix for [.NET 10 on Windows](https://learn.microsoft.com/en-us/dotnet/core/install/windows#supported-versions).

## Run from source with npm

Install [Node.js 22 or newer](https://nodejs.org/), the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0), and WebView2 Evergreen. Then run:

```powershell
git clone https://github.com/StarKnightt/copy-clip.git
cd copy-clip
npm install
npm start
```

## Copy text

1. Click **Edit text** in the upper left and enter or paste text. Try two lines such as `Hello!` and `안녕하세요 📝`.
2. Click **Apply**, then **Copy**. Press **Ctrl+V** in the app where you want to paste it. The card previews the first eight lines and clips long lines to fit. Copy sends the full text, including text beyond the preview. Very long entries use plain colors to keep editing responsive.
3. Use **Use sample** in the editor to restore the example. Choose **한국어** from the language menu or use the speaker button in the upper right to mute the copy animation's sound effects.

If another program has locked the clipboard, wait briefly and select **Copy** again. You can also select the text in the editor and press **Ctrl+C**. Copy Clip keeps edited text in the current window. Windows manages the system clipboard and its history settings.

Click the card title to rename it. Press **Enter** or click away to apply, or **Esc** to cancel. Clear the title to restore `debounce.js`. The title stays with the current window as you edit the code.

## Advanced

- In **Language**, choose the **Display language** and **Code language** independently. **System language** follows Windows on the desktop and the browser's language in the browser. Copy Clip supports English, Korean, Czech, German, Spanish, French, Italian, Japanese, Polish, Brazilian Portuguese, Russian, Turkish, Simplified Chinese and Traditional Chinese. Your explicit choice is remembered.
- **Automatic** detects a language from the code first. A known extension in the card title then sets the language badge and syntax highlighting. For example, `app.py` sets Python and `main.go` sets Go. A language you select manually takes priority. **Plain text** uses the card's regular text color.
- Turn on **Advanced → Edit on the card** to type or paste directly on the card. The font, spacing, line numbers and code colors carry over from the preview, and highlighting updates as you type. Turn it off to return to **Edit text → Apply** with your current text. Each new window starts with direct editing off.
- Select **Advanced → Open in browser** to open the bundled page using the Windows association for HTML files. It opens from disk with its own editable text. To move text to the browser, use **Copy** in the app and **Ctrl+V** in the browser's editor or any webpage input.

- Select **Advanced → Desktop widget** to keep the code card and wooden clipboard above other windows. Type on the card and choose **Copy**. Drag the wooden clipboard, an empty part of the card header, or the background to move the widget. Right-click to adjust **Background transparency**, return to the **Normal window**, or **Quit** Copy Clip. The rounded background starts fully transparent unless you have a saved setting.

## Build a portable copy

Use the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) and run from the repository root.

```powershell
dotnet publish windows/CopyClip.csproj -c Release -r win-x64 --self-contained true
```

Run `windows/bin/Release/net10.0-windows/win-x64/publish/CopyClip.exe`. Keep the whole publish directory together when sharing it. This build includes the .NET Desktop Runtime and uses the separately installed WebView2 Evergreen Runtime.

Version 0.4.8 was built with SDK 10.0.403. Check the bundled runtime notices when updating the SDK.
