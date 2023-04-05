using Microsoft.Maui.Layouts;

namespace Chess;

/// <summary>
/// The main page of the application
/// </summary>
public partial class MainPage : ContentPage {
	/// <summary> The model for the current game </summary>
	private readonly Chess game;
	/// <summary> The ImageButtons representing the valid moves for the clicked piece </summary>
	private List<ImageButton> moveIndicators;
	/// <summary> The ImageButtons for each piece  </summary>
	private List<ImageButton> pieceViews;

	/// <summary>
	/// Initialize the application.
	/// </summary>
	public MainPage()	{
		game = new Chess(this);
    moveIndicators = new List<ImageButton>();
    pieceViews = new List<ImageButton>();
    InitializeComponent();
		// Build the views for the chess board.
		for (int i = 7; i >= 0; i--) {
			HorizontalStackLayout rank = new HorizontalStackLayout();
			for (int j = 0; j < 8; j++) {
				Grid spaceView = new Grid();
        string src = (i + j) % 2 == 0 ? "b_space.png" : "w_space.png";
				spaceView.Add(new ImageButton {
					Source = ImageSource.FromFile(src),
					MaximumWidthRequest = 80,
					MaximumHeightRequest = 80,
					Margin = new Thickness(0, 0, 0, 0)
				});
				Piece piece = game.PieceAt(i, j);
				if (piece != null) {
					ImageButton pieceView = piece.CreatePieceView();
          pieceView.Clicked += ShowMoves;
          spaceView.Add(pieceView);
					pieceViews.Add(pieceView);
				}
				rank.Add(spaceView);
				game.SpaceAt(i, j).SetView(spaceView);
      }
			board.Add(rank);
		}
		ActivatePieces();
  }

	/// <summary>
	/// Enable the pieces to be clickable based on the current turn.
	/// </summary>
	private void ActivatePieces() {
		string turn = game.GetTurn();
		foreach(ImageButton pieceView in pieceViews) {
			pieceView.IsEnabled = ((Piece)pieceView.BindingContext).GetColor() == turn;
		}
	}

	/// <summary>
	/// Show the moves for the clicked piece.
	/// </summary>
	/// <param name="sender"> The piece the user clicked </param>
	/// <param name="e"> The event arguments (none for ShowMoves) </param>
	private void ShowMoves(object sender, EventArgs e) {
		ClearMoveIndicators();
    ImageButton source = (ImageButton)sender;
		Move[] moves = game.ValidMoves((Piece) source.BindingContext);
		foreach(Move move in moves) {
			ImageButton indicator = new ImageButton {
				Source = ImageSource.FromFile("indicator.png"),
				MaximumWidthRequest = 80,
				MaximumHeightRequest = 80,
				Margin = new Thickness(0, 0, 0, 0),
				BindingContext = move
			};
			indicator.Clicked += MovePiece;
      move.destination.GetView().Add(indicator);
			moveIndicators.Add(indicator);
		}
	}

	/// <summary>
	/// Clear all move inidcators from the board.
	/// </summary>
	private void ClearMoveIndicators() {
		foreach(ImageButton indicator in moveIndicators) {
			((Grid) indicator.Parent).Remove(indicator);
		}
		moveIndicators = new List<ImageButton>();
	}

	/// <summary>
	/// Make the given move visually and in the game model.
	/// </summary>
	/// <param name="move">The move to be made</param>
	private async void MovePiece(Move move) {
    ClearMoveIndicators();
		// Visually move the pieces around.
    Grid sourceView = move.piece.GetSpace().GetView();
    Grid destView = move.destination.GetView();
    sourceView.Remove(move.piece.GetView());
    pieceViews.Remove(move.piece.GetView());
    if (!move.destination.IsEmpty()) {
      ImageButton destPieceView = move.destination.GetPiece().GetView();
      destView.Remove(destPieceView);
      pieceViews.Remove(destPieceView);
    }
		// Make the move in the game model.
    await game.MakeMove(move);
    destView.Add(move.piece.GetView());
    pieceViews.Add(move.piece.GetView());
		// Initialize the new piece view in the event of a promotion.
    if (move.type == MoveType.promotion) {
      move.piece.GetView().Clicked += ShowMoves;
    }
		// Move the corresponding rook if the king castles.
    else if (move.type == MoveType.castle) {
      Piece rook = move.partner;
      int newFile = rook.GetSpace().GetFile() == 0 ? 3 : 5;
      Move rookMove = new Move(rook, game.SpaceAt(rook.GetSpace().GetRank(), newFile), MoveType.standard);
      MovePiece(rookMove);
			game.nextTurn();
    }
    ActivatePieces();
  }

	/// <summary>
	/// The event handler for moving pieces
	/// </summary>
	/// <param name="sender"> The move indicator </param>
	/// <param name="e"> The event arguments (none for MovePiece) </param>
	private void MovePiece(object sender, EventArgs e) {
		ImageButton indicator = (ImageButton)sender;
    Move move = (Move)indicator.BindingContext;
		MovePiece(move);
  }
}
