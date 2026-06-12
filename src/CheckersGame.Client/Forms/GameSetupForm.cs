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
        Text             = "Checkers — New Game";
        AutoScaleMode    = AutoScaleMode.None;   // prevent WinForms from rescaling positions
        ClientSize       = new Size(420, 620);
        FormBorderStyle  = FormBorderStyle.FixedDialog;
        MaximizeBox      = false;

        int y = 25;
        const int LBL_H = 30;   // label height — must be tall enough for 11pt bold (descenders like g,p,y)
        const int CTL_H = 32;   // control height   
        const int GAP   = 22;   // gap between sections

        // ── Server URL ───────────────────────────────────────────────────────
        AddLabel("Server URL:", 15, y, LBL_H);
        y += LBL_H + 5;
        var tbUrl = new TextBox
        {
            Text     = "https://localhost:7025",
            Location = new Point(15, y),
            Width    = 385,
            Height   = CTL_H,
            Font     = new Font("Segoe UI", 10)
        };
        tbUrl.Tag = "url";
        Controls.Add(tbUrl);
        y += CTL_H + GAP;

        // ── Number of players ────────────────────────────────────────────────
        AddLabel("Number of players (1–10):", 15, y, LBL_H);
        y += LBL_H + 5;
        _numPlayers.Location = new Point(15, y);
        _numPlayers.Width    = 100;
        _numPlayers.Height   = CTL_H;
        _numPlayers.Font     = new Font("Segoe UI", 10);
        _numPlayers.Minimum  = 1; _numPlayers.Maximum = 10; _numPlayers.Value = 1;
        _numPlayers.ValueChanged += (_, __) => BuildPlayerBoxes();
        Controls.Add(_numPlayers);
        y += CTL_H + GAP;

        // ── Time per move ────────────────────────────────────────────────────
        AddLabel("Time per move:", 15, y, LBL_H);
        y += LBL_H + 5;
        _cboTime.Location      = new Point(15, y);
        _cboTime.Width         = 170;
        _cboTime.Height        = CTL_H;
        _cboTime.Font          = new Font("Segoe UI", 10);
        _cboTime.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboTime.Items.AddRange(new object[] { "2 seconds", "5 seconds", "10 seconds", "15 seconds" });
        _cboTime.SelectedIndex = 2;   // default: 10 seconds
        Controls.Add(_cboTime);
        y += CTL_H + GAP;

        // ── Player IDs ───────────────────────────────────────────────────────
        AddLabel("Player IDs:", 15, y, LBL_H);
        y += LBL_H + 5;

        _playerPanel.Location = new Point(15, y);
        _playerPanel.Size     = new Size(385, 180);
        Controls.Add(_playerPanel);
        y += 190;

        // ── Error label ──────────────────────────────────────────────────────
        _lblError.Location  = new Point(15, y);
        _lblError.Size      = new Size(385, 48);
        _lblError.ForeColor = Color.Red;
        Controls.Add(_lblError);
        y += 55;

        // ── Buttons ──────────────────────────────────────────────────────────
        _btnStart.Text      = "Start Game";
        _btnStart.Location  = new Point(15, y);
        _btnStart.Size      = new Size(180, 42);
        _btnStart.BackColor = Color.DarkGreen;
        _btnStart.ForeColor = Color.White;
        _btnStart.Font      = new Font("Segoe UI", 10, FontStyle.Bold);
        _btnStart.Click    += BtnStart_ClickAsync;
        Controls.Add(_btnStart);

        _btnReplay.Text     = "View Replays";
        _btnReplay.Location = new Point(205, y);
        _btnReplay.Size     = new Size(180, 42);
        Controls.Add(_btnReplay);
        _btnReplay.Click   += (_, __) => new ReplayListForm().ShowDialog();

        BuildPlayerBoxes();
    }

    private void AddLabel(string text, int x, int y, int height)
    {
        Controls.Add(new Label
        {
            Text      = text,
            Location  = new Point(x, y),
            Size      = new Size(390, height),   
            Font      = new Font("Segoe UI", 11, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        });
    }

    private void BuildPlayerBoxes()
    {
        _playerPanel.Controls.Clear();
        _idBoxes.Clear();
        int count = (int)_numPlayers.Value;
        for (int i = 0; i < count; i++)
        {
            var lbl = new Label
            {
                Text      = $"Player {i + 1} ID:",
                Location  = new Point(0, i * 38 + 5),
                Size      = new Size(125, 30),
                Font      = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var tb = new TextBox
            {
                Location = new Point(125, i * 38),
                Width    = 130,
                Height   = 30,
                Font     = new Font("Segoe UI", 10)
            };
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
        int timeSec = int.Parse(_cboTime.SelectedItem!.ToString()!.Split(' ')[0]); // "10 seconds" → 10
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
