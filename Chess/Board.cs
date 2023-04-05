using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess {
  
  /// <summary>
  /// A chess board containing spaces which contain pieces
  /// </summary>
  public class Board {
    /// <summary> The side length of a standard chess board </summary>
    private static int STANDARD_DIMENSION = 8;
    
    /// <summary> The width of the board (# of files)</summary>
    private int width;
    /// <summary> The height of the board (# of ranks)</summary>
    private int height;
    /// <summary> A grid of spaces </summary>
    private Space[,] board;
    /// <summary> The color of the current player's pieces </summary>
    private string turn;

    /// <summary>
    /// Construct a new 8x8 board with pieces in the standard positions.
    /// </summary>
    public Board() {
      width = STANDARD_DIMENSION;
      height = STANDARD_DIMENSION;
      board = new Space[this.height, this.width];
      turn = "w";
      // The piece order in the back rank by increasing file
      string[] pieceOrder = {"rook", "knight", "bishop", "queen", "king", "bishop", "knight", "rook"};
      // Place the white pieces.
      for(int i = 0; i < this.width; i++) {
        board[0, i] = new Space(new Piece("w", pieceOrder[i]), 0, i);
      }
      // Place the white pawns.
      for (int i = 0; i < this.width; i++) {
        board[1, i] = new Space(new Piece("w", "pawn"), 1, i);
      }
      // Place the empty spaces.
      for (int i = 2; i < 6; i++) {
        for(int j = 0; j < this.width; j++) {
          board[i, j] = new Space(null, i, j);
        }
      }
      // Place the black pawns.
      for (int i = 0; i < this.width; i++) {
        board[6, i] = new Space(new Piece("b", "pawn"), 6, i);
      }
      // Place the white pieces.
      for (int i = 0; i < this.width; i++) {
        board[7, i] = new Space(new Piece("b", pieceOrder[i]), 7, i);
      }
    }

    /// <returns> The height of the board </returns>
    public int GetHeight() {
      return height;
    }

    /// <returns> The width of the board </returns>
    public int GetWidth() {
      return width;
    }

    /// <returns> The current turn </returns>
    public string GetTurn() {
      return turn;
    }

    /// <summary>
    /// Advance the turn to the next player.
    /// </summary>
    public void NextTurn() {
      turn = turn == "w" ? "b" : "w";
    }

    /// <returns> The piece at the given rank and file, null if not on the board. </returns>
    public Piece PieceAt(int rank, int file) {
      if (OnBoard(rank, file)) {
        return board[rank, file].GetPiece();
      }
      return null;
    }

    /// <returns> The space at the given rank and file, null if not on the board. </returns>
    public Space SpaceAt(int rank, int file) {
      if (OnBoard(rank, file)) {
        return board[rank, file];
      }
      return null;
    }

    /// <returns> Whether the given rank and file point to a space on the board </returns>
    public bool OnBoard(int rank, int file) {
      return rank >= 0 && rank < height && file >= 0 && file < width;
    }
  }
  
  /// <summary>
  /// Represents a space on the board which can hold a piece.
  /// </summary>
  public class Space {
    /// <summary> The piece occupying this space </summary>
    private Piece piece;
    /// <summary> The rank in which this space lies </summary>
    private int rank;
    /// <summary> The file in which this space lies </summary>
    private int file;
    /// <summary> The view corresponding to this space in the app </summary>
    private Grid view;

    /// <summary>
    /// Construct a new space with the give piece at the given rank and file.
    /// </summary>
    public Space(Piece piece, int rank, int file) {
      this.piece = piece;
      this.rank = rank;
      this.file = file;
      piece?.SetSpace(this);
    }

    /// <returns> The piece occupying this space </returns>
    public Piece GetPiece() {
      return piece;
    }

    public void SetPiece(Piece piece) {
      this.piece = piece;
    }

    /// <returns> The rank of this space </returns>
    public int GetRank() {
      return rank;
    }

    public void SetView(Grid view) {
      this.view = view;
    }

    /// <returns> The view representing this space </returns>
    public Grid GetView() {
      return view;
    }

    /// <returns> The file of this space </returns>
    public int GetFile() {
      return file;
    }

    /// <returns> Whether this space has no piece occupying it. </returns>
    public bool IsEmpty() {
      return piece == null;
    }
  }
}
