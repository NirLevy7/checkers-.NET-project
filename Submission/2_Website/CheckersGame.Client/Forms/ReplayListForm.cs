using CheckersGame.Client.ReplayDb;
using Microsoft.EntityFrameworkCore;

namespace CheckersGame.Client.Forms;

/// <summary>
/// Shows all saved replays. User picks one to watch.
/// </summary>
public class ReplayListForm : Form
{
    private ListBox _list = new();
    private Button  _btnWatch = new();
    private List<ReplayGame> _games = new();

    public ReplayListForm()
    {
        Text = "Saved Replays";
        Size = new Size(400, 400);

        _list.Location = new Point(10, 10);
        _list.Size     = new Size(360, 300);
        Controls.Add(_list);

        _btnWatch.Text     = "Watch Replay";
        _btnWatch.Location = new Point(10, 320);
        _btnWatch.Size     = new Size(150, 35);
        _btnWatch.Click   += BtnWatch_Click;
        Controls.Add(_btnWatch);

        LoadGames();
    }

    private void LoadGames()
    {
        try
        {
            using var db = new CheckersReplayDb();
            db.Database.EnsureCreated();
            _games = db.ReplayGames.OrderByDescending(g => g.StartTime).ToList();
            _list.Items.Clear();
            foreach (var g in _games)
                _list.Items.Add($"Game #{g.ServerGameId}  {g.StartTime:g}  Winner: {g.WinnerSide}");
        }
        catch
        {
            _list.Items.Add("No replays found.");
        }
    }

    private void BtnWatch_Click(object? sender, EventArgs e)
    {
        if (_list.SelectedIndex < 0 || _list.SelectedIndex >= _games.Count) return;

        var game = _games[_list.SelectedIndex];
        using var db = new CheckersReplayDb();
        var moves = db.ReplayMoves
            .Where(m => m.ReplayGameId == game.Id)
            .OrderBy(m => m.MoveNumber)
            .ToList();

        new ReplayBoardForm(game, moves).Show();
    }
}
