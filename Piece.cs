using UnityEngine;
using UnityEngine.SceneManagement;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }
    public int rotationIndex { get; private set; }
    public float stepDelay = 1.1f;
    public float lockDelay = 0.5f;
    private float stepTime;
    private float lockTime;
    private bool hasHold = false;
    private bool hasHeld = false;
    TetrominoData holdData;

    private Scoring scoring;

    private float moveDelay = 0.2f; // Delay before movement ramps up
    private float moveSpeed = 0.02f; // Speed of movement ramping
    private float moveSpeedDown = 0.03f; // Speed of movement ramping
    private float moveTimeLeft; // Time left before moving left
    private float moveTimeRight; // Time left before moving right
    private float moveTimeDown; // Time left before moving down
    private bool isMovingLeft = false;
    private bool isMovingRight = false;
    private bool isMovingDown = false;
    public AudioSource DropSond;
    public bool isPaused = false;

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;
        this.stepTime = Time.time + this.stepDelay;
        this.lockTime = 0f;
        this.scoring = Scoring.instance;

        if (this.cells == null)
        {
            this.cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < data.cells.Length; i++)
        {
            this.cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private void Update()
    {
        if (isPaused)
        {
            return;
        }
        this.board.Clear(this);

        this.lockTime += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.C))
        {
            HoldPiece();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Rotate(-1);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Rotate(1);
        }

        HandleMovement();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }

        if (scoring != null)
        {
            stepDelay = Mathf.Max(0.1f, 1.1f - 0.1f * (scoring.level - 1));
        }

        if (Time.time >= this.stepTime)
        {
            Step();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadSceneAsync(1);
        }

        this.board.Set(this);
    }

    private void HandleMovement()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            isMovingLeft = true;
            moveTimeLeft = moveDelay;
            Move(Vector2Int.left); // Initial move
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            isMovingRight = true;
            moveTimeRight = moveDelay;
            Move(Vector2Int.right); // Initial move
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            isMovingDown = true;
            moveTimeDown = moveDelay;
            Move(Vector2Int.down);
            this.stepTime = Time.time + this.stepDelay;
        }

        // Check for key release
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            isMovingLeft = false;
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            isMovingRight = false;
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            isMovingDown = false;
        }

        // Handle ramping speed movement
        if (isMovingLeft)
        {
            moveTimeLeft -= Time.deltaTime;
            if (moveTimeLeft <= 0f)
            {
                Move(Vector2Int.left);
                moveTimeLeft = moveSpeed; // Set the next move time based on speed
            }
        }
        else if (isMovingRight)
        {
            moveTimeRight -= Time.deltaTime;
            if (moveTimeRight <= 0f)
            {
                Move(Vector2Int.right);
                moveTimeRight = moveSpeed; // Set the next move time based on speed
            }
        }
        else if (isMovingDown)
        {
            moveTimeDown -= Time.deltaTime;
            if (moveTimeDown <= 0f)
            {
                Move(Vector2Int.down);
                moveTimeDown = moveSpeedDown; // Set the next move time based on speed
            }
        }
    }

    private void Step()
    {
        this.stepTime = Time.time + this.stepDelay;

        if (!Move(Vector2Int.down))
        {
            Lock();
        }

        if (this.lockTime >= this.lockDelay)
        {
            Lock();
        }
    }

    private void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            continue;
        }

        Lock();
    }

    private void Lock()
    {
        this.board.Set(this);
        DropSond.Play();
        if (IsPieceOutsideBoard())
        {
            this.board.GameOver();
            return;
        }
        this.board.ClearLines();
        this.board.SpawnPiece();
        hasHeld = false;
    }

    private bool IsPieceOutsideBoard()
    {
        RectInt bounds = this.board.BoundsLock;
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePosition = this.cells[i] + this.position;
            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return true;
            }
        }
        return false;
    }

    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = this.position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = this.board.IsValidPosition(this, newPosition);

        if (valid)
        {
            this.position = newPosition;
            this.lockTime = 0f;
        }

        return valid;
    }

    private void Rotate(int direction)
    {
        int originalRotation = this.rotationIndex;
        this.rotationIndex = Wrap(this.rotationIndex + direction, 0, 4);

        ApplyRotationMatrix(direction);

        if (!TestWallKicks(this.rotationIndex, direction))
        {
            this.rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }

    }

    private void ApplyRotationMatrix(int direction)
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3 cell = cells[i];

            int x, y;

            switch (this.data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;
            }

            this.cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < this.data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = this.data.wallKicks[wallKickIndex, i];

            if (Move(translation))
            {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;
        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, this.data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }

    private void HoldPiece()
    {
        if (hasHeld)
        {
            return;
        }
        if (!hasHold)
        {
            holdData = data;
            board.SpawnPiece();
            hasHold = true;
            hasHeld = true;
        }
        else
        {
            TetrominoData newSpawnData = holdData;
            holdData = data;
            board.SpawnPieceHold(newSpawnData);
            hasHeld = true;
        }

        board.DrawHoldPiece(holdData);
    }
}

