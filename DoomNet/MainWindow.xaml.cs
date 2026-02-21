using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace DoomNet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Jendela utama yang menampung engine game berbasis web
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var serviceCollection = new ServiceCollection();

            InitializeComponent();
            serviceCollection.AddWpfBlazorWebView();
            serviceCollection.AddBlazorWebViewDeveloperTools();
            Resources.Add("services", serviceCollection.BuildServiceProvider());
			AppConstants.State.OnExit += (s, e) => Application.Current.Shutdown();
            // Inisialisasi status
            StatusText.Text = "DoomNet Engine Initialized - Loading Assets...";
			// Wait until WebView2 is ready before opening DevTools
			webView1.BlazorWebViewInitialized += BlazorWebView_WebViewInitialized;
			

		}
		private async void BlazorWebView_WebViewInitialized(object? sender, EventArgs e)
		{
			try
			{
				// Ensure CoreWebView2 is initialized
				await webView1.WebView.EnsureCoreWebView2Async();

				// Open the browser console (DevTools)
				webView1.WebView.CoreWebView2.OpenDevToolsWindow();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error initializing WebView2: {ex.Message}");
			}
		}
    }
}
