namespace Chess {
  
  /// <summary>
  /// A chess piece
  /// </summary>
  public class Piece {
    /// <summary> The color of this piece </summary>
    private string color;
    /// <summary> The type of this piece (e.g. pawn, knight, etc.) </summary>
    private string pieceType;
    /// <summary> The space this piece occupies </summary>
    private Space space;
    /// <summary> Whether this piece has been moved in the game (useful for double-moves and castling) </summary>
    private bool moved;
    /// <summary> The view representing this piece in the app </summary>
    private ImageButton view;
    /// <summary> Whether this piece has just double-moved (useful for en passant captures) </summary>
    private bool doubleMoved;

    /// <summary>
    /// Construct a new piece with the given color and type.
    /// </summary>
    public Piece(string color, string pieceType) {
      this.color = color;
      this.pieceType = pieceType;
      moved = false;
      doubleMoved = false;
    }

    /// <returns> This piece's color </returns>
    public string GetColor() {
      return color;
    }

    /// <returns> This piece's type </returns>
    public string GetPieceType() {
      return pieceType;
    }

    /// <summary>
    /// Set this piece's type to the given type.
    /// </summary>
    public void setPieceType(string pieceType) {
      this.pieceType = pieceType;
    }

    /// <summary>
    /// Set this piece's space to the given space.
    /// </summary>
    public void SetSpace(Space space) {
      this.space = space;
    }

    /// <returns> This piece's view </returns>
    public ImageButton GetView() {
      return view;
    }

    /// <summary>
    /// Set this piece's view to the given view.
    /// </summary>
    public void SetView(ImageButton view) {
      this.view = view;
    }

    /// <returns> This space this piece occupies </returns>
    public Space GetSpace() {
      return space;
    }

    /// <returns> The image file for this piece based on its color and type </returns>
    public string GetImgFile() {
      return color + "_" + pieceType + ".png";
    }

    /// <summary>
    /// Create a new view for this piece based on its color and type.
    /// </summary>
    /// <returns> The newly created view. </returns>
    public ImageButton CreatePieceView() {
      view = new ImageButton {
        Source = GetImgFile(),
        MaximumWidthRequest = 80,
        MaximumHeightRequest = 80,
        Margin = new Thickness(0, 0, 0, 0),
        BindingContext = this,
      };
      return view;
    }

    /// <returns> Whether this piece has moved </returns>
    public bool HasMoved() {
      return moved;
    }

    /// <returns> Whether this piece double-moved last turn </returns>
    public bool HasDoubleMoved() {
      return doubleMoved;
    }

    /// <summary>
    /// Set whether this piece double-moved last turn.
    /// </summary>
    public void SetDoubleMoved(bool doubleMoved) {
      this.doubleMoved = doubleMoved;
    }

    /// <summary>
    /// Move this piece to the given space.
    /// </summary>
    public void MoveTo(Space space) {
      this.space.SetPiece(null);
      SetSpace(space);
      space.SetPiece(this);
      moved = true;
    }
  }
}
