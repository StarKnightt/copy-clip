using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace CopyClip;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new CopyClipWindow());
    }
}

internal sealed class CopyClipWindow : Form
{
    private const string Host = "copyclip.example";
    private const string AppPage = "https://" + Host + "/index.html";
    private readonly WebView2 view = new() { Dock = DockStyle.Fill, DefaultBackgroundColor = Color.Transparent };
    private readonly string pagePath = Path.Combine(AppContext.BaseDirectory, "ui", "index.html");
    public CopyClipWindow()
    {
        Text = "Copy Clip";
        ClientSize = new Size(880, 720);
        MinimumSize = new Size(440, 550);
        StartPosition = FormStartPosition.CenterScreen;
        Controls.Add(view);
        Shown += InitializeView;
    }

    private async void InitializeView(object? sender, EventArgs args)
    {
        try
        {
            if (!File.Exists(pagePath))
            {
                ShowStartupError("Extract the entire Copy Clip ZIP into one folder and try again.\nZIP 파일 전체를 한 폴더에 압축 해제한 뒤 다시 실행하세요.");
                return;
            }

            var profile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CopyClip", "WebView2");
            var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: profile);
            if (IsDisposed)
                return;

            await view.EnsureCoreWebView2Async(environment);
            if (IsDisposed)
                return;

            var browser = view.CoreWebView2;
            browser.Settings.AreDevToolsEnabled = false;
            browser.Settings.AreHostObjectsAllowed = false;
            browser.Settings.AreBrowserAcceleratorKeysEnabled = false;
            browser.Settings.IsGeneralAutofillEnabled = false;
            browser.Settings.IsPasswordAutosaveEnabled = false;
            browser.NavigationStarting += (_, e) => e.Cancel = e.Uri != AppPage;
            browser.NewWindowRequested += (_, e) => e.Handled = true;
            browser.PermissionRequested += (_, e) => e.State = CoreWebView2PermissionState.Deny;
            browser.NavigationCompleted += (_, e) =>
            {
                if (!e.IsSuccess)
                    ShowStartupError("The app page failed to load. Extract a fresh copy of the ZIP and try again.\n앱 화면을 열 수 없습니다. ZIP을 다시 압축 해제한 뒤 실행하세요.");
            };
            foreach (var pattern in new[] { "http://*", "https://*" })
                browser.AddWebResourceRequestedFilter(pattern, CoreWebView2WebResourceContext.All, CoreWebView2WebResourceRequestSourceKinds.All);
            browser.WebResourceRequested += (_, e) =>
            {
                if (!e.Request.Uri.StartsWith("https://" + Host + "/", StringComparison.Ordinal))
                    e.Response = environment.CreateWebResourceResponse(null, 403, "Forbidden", "");
            };
            browser.WebMessageReceived += HandleWebMessage;
            browser.SetVirtualHostNameToFolderMapping(Host, Path.GetDirectoryName(pagePath)!, CoreWebView2HostResourceAccessKind.Deny);
            var uiCulture = JsonSerializer.Serialize(CultureInfo.CurrentUICulture.Name);
            await browser.AddScriptToExecuteOnDocumentCreatedAsync(
                $"if (location.href === '{AppPage}') window.copyClipSystemLanguage = {uiCulture};");
            if (!IsDisposed)
                browser.Navigate(AppPage);
        }
        catch (WebView2RuntimeNotFoundException)
        {
            if (!IsDisposed)
                ShowStartupError("Install Microsoft Edge WebView2 Runtime and reopen Copy Clip.\nMicrosoft Edge WebView2 런타임을 설치하고 앱을 다시 여세요.\n\nhttps://developer.microsoft.com/microsoft-edge/webview2/");
        }
        catch (Exception)
        {
            if (!IsDisposed)
                ShowStartupError("Copy Clip could not start. Repair or update Microsoft Edge WebView2 Runtime and try again.\n앱을 시작할 수 없습니다. Microsoft Edge WebView2 런타임을 복구하거나 업데이트한 뒤 다시 실행하세요.");
        }
    }

    private void ShowStartupError(string message)
    {
        MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        Close();
    }

    private void HandleWebMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        if (sender is not CoreWebView2 browser || args.Source != AppPage || browser.Source != AppPage)
            return;

        using var message = JsonDocument.Parse(args.WebMessageAsJson);
        var request = message.RootElement;
        if (request.ValueKind != JsonValueKind.Object ||
            !request.TryGetProperty("id", out var id) || id.ValueKind != JsonValueKind.Number ||
            !id.TryGetInt32(out var requestId) ||
            !request.TryGetProperty("type", out var type) || type.ValueKind != JsonValueKind.String)
            return;

        if (type.ValueEquals("open-browser"))
        {
            browser.PostWebMessageAsJson(JsonSerializer.Serialize(new { id = requestId, opened = OpenBrowser() }));
        }
        else if (type.ValueEquals("copy") && request.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
        {
            browser.PostWebMessageAsJson(JsonSerializer.Serialize(new { id = requestId, copied = CopyToClipboard(text) }));
        }
    }

    private bool OpenBrowser()
    {
        var editorPath = Path.Combine(AppContext.BaseDirectory, "ui", "editor.html");
        if (!File.Exists(editorPath))
            return false;

        try
        {
            using var process = Process.Start(new ProcessStartInfo(editorPath) { UseShellExecute = true });
            return true;
        }
        catch (Win32Exception)
        {
            return false;
        }
    }

    private static bool CopyToClipboard(JsonElement text)
    {
        try
        {
            var value = text.GetString()!;
            if (value.Length > 0 && !value.Contains('\0'))
            {
                var data = new DataObject();
                data.SetText(value, TextDataFormat.UnicodeText);
                Clipboard.SetDataObject(data, copy: true, retryTimes: 3, retryDelay: 40);
                return true;
            }
        }
        catch (ExternalException)
        {
            // Another process can hold the Windows clipboard open.
        }
        catch (InvalidOperationException)
        {
            // The string can contain malformed Unicode escapes.
        }

        return false;
    }
}
