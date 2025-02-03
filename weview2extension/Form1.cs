using Microsoft.Web.WebView2.Core;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace weview2extension
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            CoreWebView2EnvironmentOptions environmentOptions = new CoreWebView2EnvironmentOptions()
            {
                AreBrowserExtensionsEnabled = true,
            };

            CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync("", "", environmentOptions);

            CoreWebView2ControllerOptions controllerOptions = environment.CreateCoreWebView2ControllerOptions();

            controllerOptions.IsInPrivateModeEnabled = true;


            await this.webView21.EnsureCoreWebView2Async(environment, controllerOptions);
        }

        private async void webView21_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            var extension = await this.webView21.CoreWebView2.Profile.GetBrowserExtensionsAsync();

            if (!extension.Any(i => i.Name == "SomeExtension"))
            {
                await this.webView21.CoreWebView2.Profile.AddBrowserExtensionAsync("..\\..\\..\\..\\..\\SomeExtension");
            }

            await TryAllowExtensionIncognito();

            this.webView21.CoreWebView2.Navigate("https://portal.azure.com");

        }

        private async Task TryAllowExtensionIncognito()
        {
            var extension = await this.webView21.CoreWebView2.Profile.GetBrowserExtensionsAsync();


            // Edit the Preferences file
            var filePath = Path.Combine(this.webView21.CoreWebView2.Profile.ProfilePath, "Preferences");

            string? extensionId = extension.First(i => i.Name == "SomeExtension")?.Id;

            if (!string.IsNullOrEmpty(extensionId))
            {
                try
                {
                    // Read the JSON file
                    string jsonString = File.ReadAllText(filePath);
                    var jsonDocument = JsonNode.Parse(jsonString);

                    if (jsonDocument != null)
                    {
                        // Navigate to the correct JSON path
                        var extensions = jsonDocument["extensions"]?["settings"];
                        if (extensions != null && extensions[extensionId] != null)
                        {
                            // Set the "incognito" property to true
                            extensions[extensionId]["incognito"] = true;

                            // Save the modified JSON back to the file
                            File.WriteAllText(filePath,
                                jsonDocument.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

        }
    }
}
