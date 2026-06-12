using CheckersGame.Client.ApiClient;
using CheckersGame.Client.GameEngine;
using CheckersGame.Client.ReplayDb;
using CheckersGame.Shared;
using CheckersGame.Shared.Dtos;
using CheckersGame.Shared.Enums;
using CheckersGame.Shared.Models;
using System.Text.Json;

namespace CheckersGame.Client.Forms;

/// <summary>
/// Main game board form.
/// Board is drawn using a Rectangle[8,4] matrix (spec §14).
/// All animation via Timer + Graphics class (spec §15).
/// Free-drawing on a Bitmap overlay (spec §15).
/// Win animation: pieces blink green 5 seconds (spec §16).
/// </summary>
public class BoardForm : Form
{
    // ── Layout constants ──────────────────────────────────────────────────
    private const int CellSize  = 70;
    private const int BoardLeft = 10;
    private const int BoardTop  = 10;

    // ── Game state ────────────────────────────────────────────────────────
    private BoardModel _board = new();
    private readonly CheckersApiClient _api;
    private int _gameId;
    private readonly List<PlayerDto> _players;
    private int _currentPlayerIndex = 0;
    private bool _myTurn = true;
    private bool _gameOver = false;
    private int _timePerMove = 10;
    private int _secondsLeft;

    // ── Selection state ───────────────────────────────────────────────────
    private int _selRow = -1, _selCol = -1;
    private List<(int Row, int Col, bool IsCapture, bool IsBackward,
                   int? CapRow, int? CapCol)> _legalTargets = new();

    // ── Rectangle matrix (spec §14) ───────────────────────────────────────
    private readonly Rectangle[,] _cells = new Rectangle[BoardModel.Rows, BoardModel.Cols];

    // ── Timers ────────────────────────────────────────────────────────────
    private readonly System.Windows.Forms.Timer _countdownTimer = new();  // per-turn countdown
    private readonly System.Windows.Forms.Timer _animTimer      = new();  // smooth animations
    private readonly System.Windows.Forms.Timer _winTimer       = new();  // win blink

    // ── Animation state ───────────────────────────────────────────────────
    private int   _animTick    = 0;
    private bool  _winBlink    = false;
    private bool  _winBlinkOn  = false;
    private int   _winBlinkCount = 0;
    private Side? _winnerSide  = null;

    // ── Highlight fade-in for moved squares ───────────────────────────────
    private (int Row, int Col)? _lastMovedFrom;
    private (int Row, int Col)? _lastMovedTo;
    private int _highlightAlpha = 0;

    // ── Free-drawing overlay (Bitmap, spec §15) ────────────────────────────
    private Bitmap   _drawBitmap   = null!;  // assigned in BuildDrawBitmap() before use
    private Graphics _drawGraphics = null!;
    private bool _isDrawing = false;
    private Point _lastDrawPoint;
    private readonly Color _drawColor = Color.Red;
    private int _drawWidth = 3;

    // ── UI controls ───────────────────────────────────────────────────────
    private Label   _lblStatus   = new();
    private Label   _lblTimer    = new();
    private Label   _lblPlayer   = new();
    private Button  _btnClear    = new();
    private Button  _btnNewGame  = new();
    private Button  _btnReplay   = new();
    private PictureBox _boardPB  = new();

    // ── Replay recording ─────────────────────────────────────────────────
    private readonly List<MoveDto> _moveHistory = new();

    public BoardForm(CheckersApiClient api, int gameId,
                     List<PlayerDto> players, int timePerMove,
                     BoardModel initialBoard)
    {
        _api          = api;
        _gameId       = gameId;
        _players      = players;
        _timePerMove  = timePerMove;
        _secondsLeft  = timePerMove;
        _board        = initialBoard;

        InitializeRectangleMatrix();
        BuildDrawBitmap();
        InitializeControls();
        WireTimers();
        StartCountdown();
    }

    // ── Build the Rectangle[8,4] matrix ──────────────────────────────────
    private void InitializeRectangleMatrix()
    {
        for (int r = 0; r < BoardModel.Rows; r++)
            for (int c = 0; c < BoardModel.Cols; c++)
                _cells[r, c] = new Rectangle(
                    BoardLeft + c * CellSize,
                    BoardTop  + r * CellSize,
                    CellSize, CellSize);
    }

    private void BuildDrawBitmap()
    {
        int w = BoardLeft * 2 + BoardModel.Cols * CellSize;
        int h = BoardTop  * 2 + BoardModel.Rows * CellSize;
        _drawBitmap   = new Bitmap(w, h);
        _drawGraphics = Graphics.FromImage(_drawBitmap);
        _drawGraphics.Clear(Color.Transparent);
    }

    private void InitializeControls()
    {
        Text          = "Checkers Game";
        ClientSize    = new Size(BoardLeft * 2 + BoardModel.Cols * CellSize + 260,
                                  BoardTop  * 2 + BoardModel.Rows * CellSize + 20);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        DoubleBuffered  = true;

        int boardW = BoardLeft + BoardModel.Cols * CellSize;

        _boardPB.Location = new Point(0, 0);
        _boardPB.Size     = new Size(boardW + BoardLeft, ClientSize.Height);
        _boardPB.Paint   += BoardPB_Paint;
        _boardPB.MouseDown  += BoardPB_MouseDown;
        _boardPB.MouseMove  += BoardPB_MouseMove;
        _boardPB.MouseUp    += BoardPB_MouseUp;
        Controls.Add(_boardPB);

        int cx = boardW + 20;

        _lblPlayer.Location  = new Point(cx, 15);
        _lblPlayer.Size      = new Size(200, 45);
        _lblPlayer.Font      = new Font("Segoe UI", 11, FontStyle.Bold);
        Controls.Add(_lblPlayer);

        _lblTimer.Location   = new Point(cx, 70);
        _lblTimer.Size       = new Size(200, 35);
        _lblTimer.Font       = new Font("Segoe UI", 14, FontStyle.Bold);
        _lblTimer.ForeColor  = Color.DarkRed;
        Controls.Add(_lblTimer);

        _lblStatus.Location  = new Point(cx, 115);
        _lblStatus.Size      = new Size(200, 70);
        _lblStatus.Font      = new Font("Segoe UI", 10);
        Controls.Add(_lblStatus);

        // Two separate instruction labels — one per line, prevents text cut-off
        Controls.Add(new Label
        {
            Text      = "Right-click + drag = draw",
            Location  = new Point(cx, 195),
            Size      = new Size(220, 22),
            Font      = new Font("Segoe UI", 9),
            ForeColor = Color.Gray
        });
        Controls.Add(new Label
        {
            Text      = "Left-click = move piece",
            Location  = new Point(cx, 217),
            Size      = new Size(220, 22),
            Font      = new Font("Segoe UI", 9),
            ForeColor = Color.Gray
        });

        _btnClear.Location   = new Point(cx, 248);
        _btnClear.Size       = new Size(220, 35);
        _btnClear.Text       = "Clear Drawing";
        _btnClear.Click     += (_, __) => { _drawGraphics.Clear(Color.Transparent); _boardPB.Invalidate(); };
        Controls.Add(_btnClear);

        _btnNewGame.Location = new Point(cx, 293);
        _btnNewGame.Size     = new Size(220, 35);
        _btnNewGame.Text     = "New Game";
        _btnNewGame.Click   += BtnNewGame_Click;
        Controls.Add(_btnNewGame);

        _btnReplay.Location  = new Point(cx, 338);
        _btnReplay.Size      = new Size(220, 35);
        _btnReplay.Text      = "View Replays";
        _btnReplay.Click    += (_, __) => new ReplayListForm().ShowDialog();
        Controls.Add(_btnReplay);

        UpdateStatusLabels();
    }

    // ── Timers ────────────────────────────────────────────────────────────
    private void WireTimers()
    {
        _countdownTimer.Interval = 1000;
        _countdownTimer.Tick    += CountdownTimer_Tick;

        _animTimer.Interval = 50;   // 20 fps — smooth color fade
        _animTimer.Tick    += AnimTimer_Tick;
        _animTimer.Start();

        _winTimer.Interval = 300;   // blink every 300 ms
        _winTimer.Tick    += WinTimer_Tick;
    }

    private void StartCountdown()
    {
        _secondsLeft = _timePerMove;
        _countdownTimer.Start();
        UpdateStatusLabels();
    }

    private async void CountdownTimer_Tick(object? sender, EventArgs e)
    {
        _secondsLeft--;
        UpdateStatusLabels();
        if (_secondsLeft <= 0)
        {
            _countdownTimer.Stop();
            _gameOver = true;
            _lblStatus.Text = "Time out! Server wins.";
            await _api.TimeoutAsync(_gameId, _players[_currentPlayerIndex].Id);
        }
    }

    // ── Smooth animation tick: fades the "last move" highlight ─────────────
    private void AnimTimer_Tick(object? sender, EventArgs e)
    {
        if (_highlightAlpha > 0)
        {
            _highlightAlpha = Math.Max(0, _highlightAlpha - 15);
            _boardPB.Invalidate();
        }
        _animTick++;
    }

    // ── Win blink: all winner pieces flash green ────────────────────────
    private void WinTimer_Tick(object? sender, EventArgs e)
    {
        _winBlinkOn = !_winBlinkOn;
        _winBlinkCount++;
        _boardPB.Invalidate();
        if (_winBlinkCount >= 17)  // 5 seconds: 17 ticks × 300 ms = 5 100 ms ≈ 5 seconds
        {
            _winTimer.Stop();
            _winBlink   = false;
            _winBlinkOn = false;
            _boardPB.Invalidate();
        }
    }

    // ── Drawing the board (Graphics) ──────────────────────────────────────
    private void BoardPB_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        for (int r = 0; r < BoardModel.Rows; r++)
        {
            for (int c = 0; c < BoardModel.Cols; c++)
            {
                var rect = _cells[r, c];

                // Alternating dark/light squares — all pieces live on dark squares
                bool isDark = (r + c) % 2 == 0;
                g.FillRectangle(isDark ? Brushes.SaddleBrown : Brushes.Wheat, rect);

                // Highlight selected square
                if (r == _selRow && c == _selCol)
                    g.FillRectangle(new SolidBrush(Color.FromArgb(120, Color.Yellow)), rect);

                // Highlight legal target squares (green tint)
                if (_legalTargets.Any(t => t.Row == r && t.Col == c))
                    g.FillRectangle(new SolidBrush(Color.FromArgb(100, Color.LimeGreen)), rect);

                // Fade highlight on last-moved squares
                if (_highlightAlpha > 0)
                {
                    if (_lastMovedFrom.HasValue && _lastMovedFrom.Value == (r, c))
                        g.FillRectangle(new SolidBrush(Color.FromArgb(_highlightAlpha, Color.CornflowerBlue)), rect);
                    if (_lastMovedTo.HasValue && _lastMovedTo.Value == (r, c))
                        g.FillRectangle(new SolidBrush(Color.FromArgb(_highlightAlpha, Color.CornflowerBlue)), rect);
                }

                // Draw piece
                //var piece = _board.Cells[r, c];
                var player = _lastMovedFrom.HasValue && _lastMovedFrom.Value == (r, c) ? _board.Cells[_lastMovedTo!.Value.Row, _lastMovedTo.Value.Col] : _board.Cells[r, c];
                if (player != null)
                {
                    Color pieceColor;
                    if (_winBlink && _winBlinkOn && player == _winnerSide)
                        pieceColor = Color.LimeGreen;  // blink green for winner
                    else
                        pieceColor = player == Side.Human ? Color.Red : Color.DodgerBlue;

                    int margin = 8;
                    var pieceRect = new Rectangle(
                        rect.X + margin, rect.Y + margin,
                        rect.Width - margin * 2, rect.Height - margin * 2);

                    // Pulsing radius for animation (spec §15 — shape changes over time)
                    int pulse = (int)(Math.Sin(_animTick * 0.15) * 20); /////////////////////4
                    pieceRect.Inflate(pulse, pulse);

                    using var brush = new SolidBrush(pieceColor);
                    g.FillEllipse(brush, pieceRect);
                    g.DrawEllipse(Pens.Black, pieceRect);

                    // Tiny marker if backward move was used
                    if (_board.UsedBackward[r, c])
                        g.DrawString("B", new Font("Arial", 7), Brushes.White,
                                      rect.X + 4, rect.Y + 4);
                }
            }
        }

        // Draw the free-drawing overlay Bitmap on top
        g.DrawImage(_drawBitmap, 0, 0);
    }

    // ── Mouse: click to select piece or move; drag to draw ──────────────
    private async void BoardPB_MouseDown(object? sender, MouseEventArgs e)
    {
        var (row, col) = HitTest(e.X, e.Y);
        if (row < 0) return;

        if (e.Button == MouseButtons.Right)
        {
            // Right-click = free drawing start
            _isDrawing   = true;
            _lastDrawPoint = e.Location;
            return;
        }

        if (_gameOver || !_myTurn) return;

        // If a piece is already selected, try to move to clicked square
        if (_selRow >= 0)
        {
            var target = _legalTargets.FirstOrDefault(t => t.Row == row && t.Col == col);
            if (target != default)
            {
                await SendMoveAsync(new MoveDto
                {
                    FromRow     = _selRow,  FromCol     = _selCol,
                    ToRow       = target.Row, ToCol    = target.Col,
                    IsCapture   = target.IsCapture,
                    IsBackward  = target.IsBackward,
                    CapturedRow = target.CapRow,
                    CapturedCol = target.CapCol
                });
                return;
            }
        }

        // Select a piece
        if (_board.Cells[row, col] == Side.Human)
        {
            _selRow      = row;
            _selCol      = col;
            _legalTargets = ClientMoveValidator.GetLegalTargets(_board, row, col);
        }
        else
        {
            _selRow = -1; _selCol = -1; _legalTargets.Clear();
        }

        _boardPB.Invalidate();
    }

    private void BoardPB_MouseMove(object? sender, MouseEventArgs e)
    {
        if (!_isDrawing) return;

        using var pen = new Pen(_drawColor, _drawWidth);
        _drawGraphics.DrawLine(pen, _lastDrawPoint, e.Location);
        _lastDrawPoint = e.Location;
        _boardPB.Invalidate();
    }

    private void BoardPB_MouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right) _isDrawing = false;
    }

    // ── Send human move to server ─────────────────────────────────────────
    private async Task SendMoveAsync(MoveDto move)
    {
        _countdownTimer.Stop();
        _myTurn = false;
        _legalTargets.Clear();
        _selRow = -1; _selCol = -1;

        var req = new MakeMoveRequest
        {
            PlayerId = _players[_currentPlayerIndex].Id,
            Move     = move
        };

        var resp = await _api.MakeMoveAsync(_gameId, req);
        if (resp == null) { _lblStatus.Text = "Server error."; return; }

        if (!resp.Accepted)
        {
            _lblStatus.Text = resp.RejectReason ?? "Move rejected.";
            _myTurn = true;
            StartCountdown();
            return;
        }

        // Apply human move visually
        _lastMovedFrom = (move.FromRow, move.FromCol);
        _lastMovedTo   = (move.ToRow, move.ToCol);
        _highlightAlpha = 200;

        // Deserialize updated board from server
        _board = BoardSerializer.Deserialize(resp.BoardJson);
        _moveHistory.Add(move);
        if (resp.ServerMove != null) _moveHistory.Add(resp.ServerMove);

        if (resp.GameOver)
        {
            HandleGameOver(resp.Winner!.Value);
            return;
        }

        // Advance to next human player (round-robin)
        _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
        _myTurn = true;
        StartCountdown();
        UpdateStatusLabels();
        _boardPB.Invalidate();
    }

    private void HandleGameOver(GameStatus winner)
    {
        _gameOver = true;
        _countdownTimer.Stop();
        _winnerSide  = winner == GameStatus.HumanWon ? Side.Human : Side.Server;
        _winBlink    = true;
        _winBlinkOn  = true;
        _winBlinkCount = 0;
        _winTimer.Start();
        _lblStatus.Text = winner == GameStatus.HumanWon ? "Human wins!" : "Server wins!";
        SaveReplayToDb(winner);
        _boardPB.Invalidate();
    }

    // ── Persist game to local replay DB ──────────────────────────────────
    private void SaveReplayToDb(GameStatus winner)
    {
        using var db = new CheckersReplayDb();
        db.Database.EnsureCreated();

        var replay = new ReplayGame
        {
            ServerGameId    = _gameId,
            PlayersJson     = JsonSerializer.Serialize(_players.Select(p => p.FirstName)),
            StartTime       = DateTime.Now,
            DurationSec     = _timePerMove * _moveHistory.Count,
            TimePerMoveSec  = _timePerMove,
            WinnerSide      = winner == GameStatus.HumanWon ? "Human" : "Server"
        };

        int num = 1;
        foreach (var m in _moveHistory)
        {
            replay.Moves.Add(new ReplayDb.ReplayMove
            {
                MoveNumber  = num++,
                Side        = num % 2 == 0 ? "Server" : "Human",
                FromRow     = m.FromRow, FromCol = m.FromCol,
                ToRow       = m.ToRow,   ToCol   = m.ToCol,
                CapturedRow = m.CapturedRow, CapturedCol = m.CapturedCol,
                IsBackward  = m.IsBackward
            });
        }

        db.ReplayGames.Add(replay);
        db.SaveChanges();
    }

    // ── Button handlers ────────────────────────────────────────────────────
    private void BtnNewGame_Click(object? sender, EventArgs e)
    {
        // Re-open the new game setup form
        var setup = new GameSetupForm(_api);
        setup.Show();
        Close();
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    private (int row, int col) HitTest(int x, int y)
    {
        for (int r = 0; r < BoardModel.Rows; r++)
            for (int c = 0; c < BoardModel.Cols; c++)
                if (_cells[r, c].Contains(x, y))
                    return (r, c);
        return (-1, -1);
    }

    private void UpdateStatusLabels()
    {
        if (_players.Count > 0 && _currentPlayerIndex < _players.Count)
            _lblPlayer.Text = $"Turn: {_players[_currentPlayerIndex].FirstName}";

        _lblTimer.Text = _myTurn && !_gameOver
            ? $"Time left: {_secondsLeft}s"
            : "";
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _countdownTimer.Dispose();
        _animTimer.Dispose();
        _winTimer.Dispose();
        _drawGraphics.Dispose();
        _drawBitmap.Dispose();
        base.OnFormClosed(e);
    }
}
