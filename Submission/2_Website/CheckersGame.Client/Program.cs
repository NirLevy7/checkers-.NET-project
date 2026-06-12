using CheckersGame.Client.ApiClient;
using CheckersGame.Client.Forms;

namespace CheckersGame.Client;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        // API URL can be changed in the GameSetupForm text box at runtime
        var api = new CheckersApiClient("https://localhost:7025");
        Application.Run(new GameSetupForm(api));
    }
}
