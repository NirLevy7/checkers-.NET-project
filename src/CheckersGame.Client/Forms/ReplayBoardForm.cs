using CheckersGame.Client.ReplayDb;
using CheckersGame.Shared.Enums;
using CheckersGame.Shared.Models;

namespace CheckersGame.Client.Forms;

/// <summary>
/// Replays a saved game: same board rendering as BoardForm, but read-only.
/// Moves play back at even intervals via a Timer (spec §20).
/// </summary>
public class ReplayBoardForm : Form
{
    private const int CellSize = 70;
    private const int BoardLeft = 10;
    private const int BoardTop = 10;

    private BoardModel _board = new();
    private readonly Rectangle[,] _cells = new Rectangle[BoardModel.Rows, BoardModel.Cols];
    private readonly List<ReplayMove> _moves;
    private int _moveIndex = 0;

    private readonly System.Windows.Forms.Timer _replayTimer = new();
    private readonly System.Windows.Forms.Timer _animTimer = new();
    private int _animTick = 0;

    private Label _lblInfo = new();
    private PictureBox _boardPB = new();

    public ReplayBoardForm(ReplayGame game, List<ReplayMove> moves)
    {
        _moves = moves;
        Text = $"Replay — Game #{game.ServerGameId}  |  Winner: {game.WinnerSide}";

        InitializeRectangleMatrix();
        BuildControls();
        WireTimers();
        _replayTimer.Start();
    }


    private void InitializeRectangleMatrix()
    {
        for (int r = 0; r < BoardModel.Rows; r++)
            for (int c = 0; c < BoardModel.Cols; c++)
                _cells[r, c] = new Rectangle(
                    BoardLeft + c * CellSize,
                    BoardTop + r * CellSize,
                    CellSize, CellSize);
    }

    private void BuildControls()
    {
        ClientSize = new Size(BoardLeft * 2 + BoardModel.Cols * CellSize + 200,
                               BoardTop * 2 + BoardModel.Rows * CellSize + 20);

        _boardPB.Location = new Point(0, 0);
        _boardPB.Size = new Size(BoardLeft + BoardModel.Cols * CellSize + BoardLeft,
                                       ClientSize.Height);
        _boardPB.Paint += BoardPB_Paint;
        Controls.Add(_boardPB);

        _lblInfo.Location = new Point(_boardPB.Width + 10, 10);
        _lblInfo.Size = new Size(170, 60);
        _lblInfo.Font = new Font("Segoe UI", 10);
        _lblInfo.Text = "Replaying...";
        Controls.Add(_lblInfo);
    }

    private void WireTimers()
    {
        _replayTimer.Interval = 1200;  // 1.2s per move — reasonable pace
        _replayTimer.Tick += ReplayTimer_Tick;

        _animTimer.Interval = 50;
        _animTimer.Tick += (_, __) => { _animTick++; _boardPB.Invalidate(); };
        _animTimer.Start();
    }

    private void ReplayTimer_Tick(object? sender, EventArgs e)
    {
        if (_moveIndex >= _moves.Count)
        {
            _replayTimer.Stop();
            _lblInfo.Text = "Replay complete.";
            return;
        }

        var m = _moves[_moveIndex++];
        _lblInfo.Text = $"Move {_moveIndex}/{_moves.Count}\n{m.Side}";

        // Apply: move piece, remove captured
        _board.Cells[m.ToRow, m.ToCol] = _board.Cells[m.FromRow, m.FromCol];
        _board.Cells[m.FromRow, m.FromCol] = null;
        if (m.CapturedRow.HasValue && m.CapturedCol.HasValue)
            _board.Cells[m.CapturedRow.Value, m.CapturedCol.Value] = null;
        if (m.IsBackward)
            _board.UsedBackward[m.ToRow, m.ToCol] = true;

        _boardPB.Invalidate();
    }

    private void BoardPB_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        for (int r = 0; r < BoardModel.Rows; r++)
            for (int c = 0; c < BoardModel.Cols; c++)
            {
                var rect = _cells[r, c];
                bool isDark = (r + c) % 2 == 0;
                g.FillRectangle(isDark ? Brushes.SaddleBrown : Brushes.Wheat, rect);

                var piece = _board.Cells[r, c];
                if (piece != null)
                {
                    Color pieceColor = piece == Side.Human ? Color.Red : Color.DodgerBlue;
                    int margin = 8;
                    var pr = new Rectangle(rect.X + margin, rect.Y + margin,
                                            rect.Width - margin * 2, rect.Height - margin * 2);
                    int pulse = (int)(Math.Sin(_animTick * 0.15) * 4);
                    pr.Inflate(pulse, pulse);

                    g.FillEllipse(new SolidBrush(pieceColor), pr);
                    g.DrawEllipse(Pens.Black, pr);
                }
            }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _replayTimer.Dispose();
        _animTimer.Dispose();
        base.OnFormClosed(e);
    }
}
