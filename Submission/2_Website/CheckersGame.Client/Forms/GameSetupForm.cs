using CheckersGame.Client.ApiClient;
using CheckersGame.Shared;
using CheckersGame.Shared.Dtos;
using CheckersGame.Shared.Models;

namespace CheckersGame.Client.Forms;

/// <summary>
/// First form: player enters their ID(s), chooses time limit, starts game.
/// Also has a button to view replays.
/// </summary>
public class GameSetupForm : Form
{
    private readonly CheckersApiClient _api;
    private List<TextBox> _idBoxes  = new();
    private Label  _lblError        = new();
    private ComboBox _cboTime       = new();
    private NumericUpDown _numPlayers = new();
    private Panel _playerPanel      = new();
    private Button _btnStart        = new();
    private Button _btnReplay       = new();

    public GameSetupForm(CheckersApiClient api)
    {
        _api = api;
        BuildUi();
    }

    private void BuildUi()
    {
        Text        = "Checkers — New Game";
        Size        = new Size(400, 420);
        FormBorderStyle = FormBorderStyle.FixedDialog;

        int y = 15;

        AddLabel("Server URL:", 15, y);
        var tbUrl = new TextBox { Text = "https://localhost:7025", Location = new Point(130, y), Width = 230 };
        tbUrl.Tag = "url";
        Controls.Add(tbUrl);
        y += 35;

        AddLabel("Players (1-10):", 15, y);
        _numPlayers.Location = new Point(130, y);
        _numPlayers.Minimum  = 1; _numPlayers.Maximum = 10; _numPlayers.Value = 1;
        _numPlayers.Width    = 60;
        _numPlayers.ValueChanged += (_, __) => BuildPlayerBoxes();
        Controls.Add(_numPlayers);
        y += 35;

        AddLabel("Time per move:", 15, y);
        _cboTime.Location = new Point(130, y);
        _cboTime.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboTime.Items.AddRange(new object[] { 2, 5, 10, 15 });
        _cboTime.SelectedIndex = 2;  // default 10s
        Controls.Add(_cboTime);
        y += 35;

        AddLabel("Player IDs:", 15, y);
        y += 22;

        _playerPanel.Location = new Point(15, y);
        _playerPanel.Size     = new Size(360, 120);
        Controls.Add(_playerPanel);
        y += 130;

        _lblError.Location  = new Point(15, y);
        _lblError.Size      = new Size(360, 40);
        _lblError.ForeColor = Color.Red;
        Controls.Add(_lblError);
        y += 45;

        _btnStart.Text     = "Start Game";
        _btnStart.Location = new Point(15, y);
        _btnStart.Size     = new Size(120, 35);
        _btnStart.BackColor = Color.DarkGreen;
        _btnStart.ForeColor = Color.White;
        _btnStart.Click   += BtnStart_ClickAsync;
        Controls.Add(_btnStart);

        _btnReplay.Text     = "View Replays";
        _btnReplay.Location = new Point(145, y);
        _btnReplay.Size     = new Size(120, 35);
        _btnReplay.Click   += (_, __) => new ReplayListForm().ShowDialog();
        Controls.Add(_btnReplay);

        BuildPlayerBoxes();
    }

    private void AddLabel(string text, int x, int y)
    {
        Controls.Add(new Label { Text = text, Location = new Point(x, y), AutoSize = true });
    }

    private void BuildPlayerBoxes()
    {
        _playerPanel.Controls.Clear();
        _idBoxes.Clear();
        int count = (int)_numPlayers.Value;
        for (int i = 0; i < count; i++)
        {
            var lbl = new Label { Text = $"P{i + 1} ID:", Location = new Point(0, i * 26), AutoSize = true };
            var tb  = new TextBox { Location = new Point(55, i * 26 - 3), Width = 70 };
            _playerPanel.Controls.Add(lbl);
            _playerPanel.Controls.Add(tb);
            _idBoxes.Add(tb);
        }
    }

    private async void BtnStart_ClickAsync(object? sender, EventArgs e)
    {
        _lblError.Text  = "";
        _btnStart.Enabled = false;

        // Find the server URL textbox
        string url = "https://localhost:7001";
        foreach (Control c in Controls)
            if (c is TextBox tb && "url".Equals(tb.Tag)) { url = tb.Text.Trim(); break; }

        // Parse player IDs
        var playerIds = new List<int>();
        foreach (var tb in _idBoxes)
        {
            if (!int.TryParse(tb.Text.Trim(), out int id) || id < 1 || id > 1000)
            {
                _lblError.Text = "All player IDs must be whole numbers between 1 and 1000.";
                _btnStart.Enabled = true;
                return;
            }
            playerIds.Add(id);
        }

        // Verify each player is registered on the server
        var api      = new CheckersApiClient(url);
        var players  = new List<PlayerDto>();
        foreach (int id in playerIds)
        {
            try
            {
                var player = await api.GetPlayerAsync(id);
                if (player == null)
                {
                    _lblError.Text = $"Player ID {id} is not registered. Please register on the website first.";
                    _btnStart.Enabled = true;
                    return;
                }
                players.Add(player);
            }
            catch
            {
                _lblError.Text = "Cannot connect to server. Make sure the API and Web projects are running.";
                _btnStart.Enabled = true;
                return;
            }
        }

        // Start game via API
        int timeSec = (int)_cboTime.SelectedItem!;
        StartGameResponse? resp = null;
        try
        {
            resp = await api.StartGameAsync(new StartGameRequest
            {
                PlayerIds      = playerIds,
                TimePerMoveSec = timeSec
            });
        }
        catch (Exception ex)
        {
            _lblError.Text = "Failed to start game: " + ex.Message;
            _btnStart.Enabled = true;
            return;
        }

        // Show player details (spec §18: show at game start)
        string details = string.Join("\n", players.Select(p =>
            $"  {p.FirstName} | ID: {p.Id} | {p.CountryName}"));
        MessageBox.Show($"Game started!\n\n{details}", "Players",
                         MessageBoxButtons.OK, MessageBoxIcon.Information);

        // Deserialize initial board
        var board = BoardSerializer.Deserialize(resp!.InitialBoard);

        var boardForm = new BoardForm(api, resp.GameId, players, timeSec, board);
        boardForm.Show();
        Hide();
    }

}
