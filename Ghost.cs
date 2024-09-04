using UnityEngine;
using UnityEngine.Tilemaps;

public class Ghost : MonoBehaviour
{
    public Tile tile;
    public Board board;
    public Piece trackingPiece;

    public Tilemap tilemap { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }

    private void Awake()
    {
        // Initialize the tilemap and cell array
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.cells = new Vector3Int[4];
    }

    private void LateUpdate()
    {
        // Update the ghost piece's position and appearance
        Clear();  // Clear the previous ghost piece tiles
        Copy();   // Copy the shape of the tracking piece
        Drop();   // Drop the ghost piece to the bottom of the board
        Set();    // Set the ghost piece tiles at the new position
    }

    private void Clear()
    {
        // Clear the tiles from the previous ghost piece position
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePosition = this.cells[i] + this.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    private void Copy()
    {
        // Copy the cell positions from the tracking piece
        for (int i = 0; i < this.cells.Length; i++)
        {
            this.cells[i] = this.trackingPiece.cells[i];
        }
    }

    public void Drop()
    {
        Vector3Int position = this.trackingPiece.position;

        int current = position.y;
        int bottom = -this.board.boardSize.y / 2 - 1;

        this.board.Clear(this.trackingPiece);

        // Drop the ghost piece until it can't move further down
        for (int row = current; row >= bottom; row--)
        {
            position.y = row;

            if (this.board.IsValidPosition(this.trackingPiece, position))
            {
                this.position = position;
            }
            else
            {
                break; // Stop if the ghost piece can't go further down
            }
        }

        this.board.Set(this.trackingPiece);
    }

    public void Set()
    {
        // Set the ghost piece tiles at the new position
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePosition = this.cells[i] + this.position;
            this.tilemap.SetTile(tilePosition, this.tile);
        }
    }
}
