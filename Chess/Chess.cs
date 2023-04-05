using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess {
  
/// <summary>
/// A model class handing the data and rules for a standard game of chess.
/// </summary>
  public class Chess {
    /// <summary> The current game board </summary>
    private Board activeBoard;
    /// <summary> The pawn that did a double move last turn </summary>
    private Piece doubleMovedPawn;
    /// <summary> The app page </summary>
    private MainPage appPage;

    /// <summary>
    /// Construct a new game with the given page
    /// </summary>
    public Chess(MainPage appPage) {
      activeBoard = new Board();
      doubleMovedPawn = null;
      this.appPage = appPage;
    }

    /// <returns> The piece at the given rank and file </returns>
    public Piece PieceAt(int rank, int file) {
      return activeBoard.PieceAt(rank, file);
    }

    /// <returns> The space at the given rank and file </returns>
    public Space SpaceAt(int rank, int file) {
      return activeBoard.SpaceAt(rank, file);
    }

    /// <returns> The current turn </returns>
    public string GetTurn() {
      return activeBoard.GetTurn();
    }

    /// <summary>
    /// Advance the turn.
    /// </summary>
    public void nextTurn() {
      activeBoard.NextTurn();
    }

    /// <summary>
    /// Make the given move.
    /// </summary>
    public async Task MakeMove(Move move) {
      // Move the piece.
      move.piece.MoveTo(move.destination);
      // Clear the double-moved pawn reference.
      if (doubleMovedPawn != null) {
        doubleMovedPawn.SetDoubleMoved(false);
        doubleMovedPawn = null;
      }
      // Handle a double-move.
      if (move.type == MoveType.doubleMove) {
        move.piece.SetDoubleMoved(true);
        doubleMovedPawn = move.piece;
      // Handle an en passant capture.
      } else if(move.type == MoveType.enPassant) {
        Piece target = move.partner;
        target.GetSpace().SetPiece(null);
        ImageButton targetView = target.GetView();
        ((Grid)targetView.Parent).Remove(targetView);
      // Handle a pawn promotion.
      } else if(move.type == MoveType.promotion) {
        string selection = await appPage.DisplayActionSheet("Promotion piece type", null, null, new string[]
                                                     {"Knight", "Bishop", "Rook", "Queen"});
        selection = selection.ToLower();
        ImageButton oldView = move.piece.GetView();
        move.piece.setPieceType(selection);
        move.piece.CreatePieceView();
      }
      activeBoard.NextTurn();
    }

    /// <returns> All the moves the given piece can make based on the current game state. </returns>
    public Move[] ValidMoves(Piece piece) {
      return ValidMoves(piece, activeBoard);
    }

    /// <param name="board"> Any board, not just the current board (for looking for checks) </param>
    /// <returns>All the moves the given piece can make based on the given board.</returns>
    public Move[] ValidMoves(Piece piece, Board board) {
      Move[] baseMoves = new Move[] { };
      string pieceType = piece.GetPieceType();
      if (pieceType == "pawn") {
        baseMoves = ValidPawnMoves(piece, board);
      } else if (pieceType == "knight") {
        baseMoves = ValidKnightMoves(piece, board);
      } else if (pieceType == "bishop") {
        baseMoves = ValidBishopMoves(piece, board);
      } else if (pieceType == "rook") {
        baseMoves = ValidRookMoves(piece, board);
      }  else if (pieceType == "queen") {
        baseMoves = ValidQueenMoves(piece, board);
      }  else if (pieceType == "king") {
        baseMoves = ValidKingMoves(piece, board);
      }
      // Remove all null elements in the moves array.
      int numNull = 0;
      foreach(Move move in baseMoves) {
        if(move == null) {
          numNull++;
        }
      }
      Move[] moves = new Move[baseMoves.Length - numNull];
      int moveIdx = 0;
      foreach (Move move in baseMoves) {
        if (move != null) {
          moves[moveIdx++] = move;
        }
      }
      return moves;
    }

    /// <returns>  All the valid moves a pawn can make based on the given board </returns>
    public Move[] ValidPawnMoves(Piece piece, Board board) {
      Move[] moves = new Move[4];
      int pr = piece.GetSpace().GetRank();
      int pf = piece.GetSpace().GetFile();
      int dir = piece.GetColor() == "w" ? 1 : -1;
      //Double and single forward moves
      Space space = SpaceAt(pr + 1 * dir, pf);
      if (space != null && space.IsEmpty()) {
        moves[0] = new Move(piece, space, MoveType.standard);
        space = SpaceAt(pr + 2 * dir, pf);
        if (!piece.HasMoved() && space != null && space.IsEmpty()) {
          moves[1] = new Move(piece, space, MoveType.doubleMove);
        }
      }
      //Captures
      for (int dpf = -1; dpf <= 1; dpf += 2) {
        space = SpaceAt(pr + 1 * dir, pf + dpf);
        int moveIdx = dpf == -1 ? 2 : 3;
        if (space != null) {
          //En passant
          if (space.IsEmpty()) {
            Space adjacentSpace = SpaceAt(pr, pf + dpf);
            Piece adjPiece = adjacentSpace.GetPiece();
            if (!adjacentSpace.IsEmpty() && adjPiece.GetColor() != piece.GetColor() && adjPiece.HasDoubleMoved()) {
              moves[moveIdx] = new Move(piece, space, MoveType.enPassant, adjPiece);
            }
          } else {
            if (space.GetPiece().GetColor() != piece.GetColor()) {
              moves[moveIdx] = new Move(piece, space, MoveType.standard);
            }
          }
        }
      }
      //Promotion
      foreach(Move move in moves) {
        if(move != null) {
          int rank = move.destination.GetRank();
          if(rank == 0 || rank == 7) {
            move.type = MoveType.promotion;
          }
        }
      }
      return moves;
    }

    /// <returns>  All the valid moves a knight can make based on the given board </returns>
    public Move[] ValidKnightMoves(Piece piece, Board board) {
      Move[] moves = new Move[8];
      int moveNo = 0;
      // Nested loop handles all 8 possible Ls for the knight
      for(int rSign = -1; rSign <= 1; rSign += 2) {
        for(int fSign = -1; fSign <= 1; fSign += 2) {
          for(int lType = 0; lType <= 1; lType++) {
            Space space = SpaceAt(piece.GetSpace().GetRank() + (2 - lType) * rSign,
                                  piece.GetSpace().GetFile() + (1 + lType) * fSign);
            if(space != null && (space.IsEmpty() || space.GetPiece().GetColor() != piece.GetColor())) {
              moves[moveNo++] = new Move(piece, space, MoveType.standard);
            }
          }
        }
      }
      return moves;
    }

    /// <returns>  All the valid moves a bishop can make based on the given board </returns>
    public Move[] ValidBishopMoves(Piece piece, Board board) {
      Move[] moves = new Move[14];
      int moveNo = 0;
      for (int dr = -1; dr <= 1; dr += 2) {
        for (int df = -1; df <= 1; df += 2) {
          int r = piece.GetSpace().GetRank() + dr;
          int f = piece.GetSpace().GetFile() + df;
          Space space = SpaceAt(r, f);
          while(space != null) {
            if(!space.IsEmpty()) {
              if(space.GetPiece().GetColor() != piece.GetColor()) {
                moves[moveNo++] = new Move(piece, space, MoveType.standard);
              }
              break;
            } else {
              moves[moveNo++] = new Move(piece, space, MoveType.standard);
            }
            r += dr;
            f += df;
            space = SpaceAt(r, f);
          }
        }
      }
      return moves;
    }

    /// <returns>  All the valid moves a rook can make based on the given board </returns>
    public Move[] ValidRookMoves(Piece piece, Board board) {
      Move[] moves = new Move[14];
      int moveNo = 0;
      int[,] directions = { { -1, 0 }, { 0, -1 }, { 0, 1 }, { 1, 0 } };
      for(int i = 0; i < 4; i++) {
        int dr = directions[i,0];
        int df = directions[i,1];
        int r = piece.GetSpace().GetRank() + dr;
        int f = piece.GetSpace().GetFile() + df;
        Space space = SpaceAt(r, f);
        while (space != null) {
          if (!space.IsEmpty()) {
            if (space.GetPiece().GetColor() != piece.GetColor()) {
              moves[moveNo++] = new Move(piece, space, MoveType.standard);
            }
            break;
          }
          else {
            moves[moveNo++] = new Move(piece, space, MoveType.standard);
          }
          r += dr;
          f += df;
          space = SpaceAt(r, f);
        }
      }
      return moves;
    }

    /// <returns>  All the valid moves a queen can make based on the given board </returns>
    public Move[] ValidQueenMoves(Piece piece, Board board) {
      // Queens can make any move vaid for a rook or bishop in their position.
      Move[] moves = new Move[28];
      ValidBishopMoves(piece, board).CopyTo(moves, 0);
      ValidRookMoves(piece, board).CopyTo(moves, 14);
      return moves;
    }

    /// <returns>  All the valid moves a king can make based on the given board </returns>
    public Move[] ValidKingMoves(Piece piece, Board board) {
      Move[] moves = new Move[10];
      int moveNo = 0;
      int r = piece.GetSpace().GetRank();
      int f = piece.GetSpace().GetFile();
      for (int i = -1; i <= 1; i++) {
        for (int j = -1; j <= 1; j++) {
          Space space = SpaceAt(r + i, f + j);
          if(space != null && (space.IsEmpty() || space.GetPiece().GetColor() != piece.GetColor())) {
            moves[moveNo++] = new Move(piece, space, MoveType.standard);
          }
        }
      }
      //Castling
      if (!piece.HasMoved()) {
        foreach(int file in new int[]{0, 7}) {
          Piece rook = PieceAt(r, file);
          if (rook != null && rook.GetPieceType() == "rook" && !rook.HasMoved()) {
            int df = file - f > 0 ? 1 : -1;
            bool flag = true;
            for(int i = f + df; i != file; i += df) {
              if(!SpaceAt(r, i).IsEmpty()) {
                flag = false;
                break;
              }
            }
            if(flag) {
              int moveIdx = file == 0 ? 8 : 9;
              moves[moveIdx] = new Move(piece, SpaceAt(r, f + 2 * df), MoveType.castle, rook);
            }
          }
        }
      }
      return moves;
    }
  }

  /// <summary>
  /// A move with a piece, destination, type, and possible partner (for 2-piece moves)
  /// </summary>
  public class Move {
    /// <summary> The piece to be moved </summary>
    public Piece piece;
    /// <summary> The destination for the piece </summary>
    public Space destination;
    /// <summary> The type of move </summary>
    public MoveType type;
    /// <summary> The optional partner (for en passant and castling) </summary>
    public Piece partner;

    /// <summary>
    /// Construct a move with the give piece, destination, type, and partner.
    /// </summary>
    public Move(Piece piece, Space destination, MoveType type, Piece partner) { 
      this.piece = piece;
      this.destination = destination;
      this.type = type;
      this.partner = partner;
    }

    /// <summary>
    /// Construct a move with the give piece, destination, and type (partner is null).
    /// </summary>
    public Move(Piece piece, Space destination, MoveType type) : this(piece, destination, type, null) {}
  }

  /// <summary>
  /// An enumeration for the different types of moves.
  /// </summary>
  public enum MoveType {
    /// <summary> A non-special, default move </summary>
    standard,
    /// <summary> A pawn double-move from the starting position </summary>
    doubleMove,
    /// <summary> A castle move </summary>
    castle,
    /// <summary> A pawn promotion </summary>
    promotion,
    /// <summary> An en passant capture </summary>
    enPassant
  }
}
