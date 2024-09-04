using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;


public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }
    public TetrominoData[] tetrominoes;
    public Vector3Int spawnPosition;
    public Vector2Int boardSize = new Vector2Int(10, 24);
    public Vector2Int boardSizeLock = new Vector2Int(10, 20);
    private bool hasNext = false;
    private TetrominoData nextData;
    public AudioSource tSong;
    public AudioSource t1;
    public AudioSource t2;
    public AudioSource t3;
    public AudioSource t4;
    public AudioSource t5;
    public AudioSource t6;
    public int clearStreak = 0;
    [SerializeField] GameObject SettingsPage;
    [SerializeField] Button CloseSettings;

    public AudioMixer music;
    public AudioMixer sFX;
    public Slider SongSlider;
    public Slider SFXSlider;
    Resolution[] resolutions;
    public Dropdown resolutionDropdown;
    private bool isPaused = false;

    public void TogglePause()
    {
        if (!activePiece.isPaused)
        {
            activePiece.isPaused = true;
        }
        else
        {
            activePiece.isPaused = false;
        }

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
    }
    public void Close() {
        activePiece.isPaused = false;
        Time.timeScale = 1;
    }


    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2 + 2);
            return new RectInt(position, this.boardSize);
        }
    }

    public RectInt BoundsLock
    {
        get
        {
            Vector2Int position = new Vector2Int(-this.boardSizeLock.x / 2, -this.boardSizeLock.y / 2);
            return new RectInt(position, this.boardSizeLock);
        }
    }

    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < this.tetrominoes.Length; i++)
        {
            this.tetrominoes[i].Initialize();
        }
    }

    private void Update()
    {
        CloseSettings.onClick.AddListener(Close);
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SettingsPage.SetActive(!SettingsPage.activeSelf);
            TogglePause();
        }
    }

    private void Start()
    {
        tSong.Play();
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        music.SetFloat("Volume", PlayerPrefs.GetFloat("Music", 0));
        sFX.SetFloat("SFX", PlayerPrefs.GetFloat("SFX", 0));
        SongSlider.value = PlayerPrefs.GetFloat("Music", 0);
        SFXSlider.value = PlayerPrefs.GetFloat("SFX", 0);
        SpawnPiece();
        int nextRandom = Random.Range(0, this.tetrominoes.Length);
        nextData = this.tetrominoes[nextRandom];
        DrawNextPiece(nextData);
    }
    public void SetMusic(float volume)
    {
        music.SetFloat("Volume", volume);
        PlayerPrefs.SetFloat("Music", volume);
    }

    public void SetSFX(float volume)
    {
        sFX.SetFloat("SFX", volume);
        PlayerPrefs.SetFloat("SFX", volume);
    }
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SpawnPiece()
    {
        TetrominoData data;
        if (!hasNext)
        {
            int random = Random.Range(0, this.tetrominoes.Length);
            data = this.tetrominoes[random];
            hasNext = true;
        }
        else
        {
            data = nextData;
            int nextRandom = Random.Range(0, this.tetrominoes.Length);
            nextData = this.tetrominoes[nextRandom];
            DrawNextPiece(nextData);
        }

        this.activePiece.Initialize(this, this.spawnPosition, data);

        if (IsValidPosition(this.activePiece, this.spawnPosition))
        {
            Set(this.activePiece);
        }
        else
        {
            GameOver();
        }
    }

    public void SpawnPieceHold(TetrominoData holdData)
    {
        TetrominoData data = holdData;

        this.activePiece.Initialize(this, this.spawnPosition, data);

        if (IsValidPosition(this.activePiece, this.spawnPosition))
        {
            Set(this.activePiece);
        }
        else
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        SceneManager.LoadSceneAsync(0);
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }
            if (this.tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }
        return true;
    }

    public void ClearLines()
    {
        RectInt bounds = this.Bounds;
        int row = bounds.yMin;
        int linesCleared = 0;

        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row);
                linesCleared++;
            }
            else
            {
                row++;
            }
        }

        if (linesCleared != 0)
        {
            PlaySound(clearStreak);
            Scoring.instance.AddPoints(linesCleared, clearStreak);
            clearStreak++;
        }
        else
        {
            clearStreak = 0;
        }
    }

    public void PlaySound(int clearStreak)
    {
        if (clearStreak == 0)
        {
            t1.Play();
        }
        else if (clearStreak == 1)
        {
            t2.Play();
        }
        else if (clearStreak == 2)
        {
            t3.Play();
        }
        else if (clearStreak == 3)
        {
            t4.Play();
        }
        else if (clearStreak == 4)
        {
            t5.Play();
        }
        else if (clearStreak > 4)
        {
            t6.Play();
        }

    }


    private bool IsLineFull(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            if (!this.tilemap.HasTile(position))
            {
                return false;
            }
        }
        return true;
    }

    private void LineClear(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            this.tilemap.SetTile(position, null);
        }

        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = this.tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                this.tilemap.SetTile(position, above);
            }

            row++;
        }
    }
    public void DrawHoldPiece(TetrominoData holdData)
    {
        Vector3Int holdPosition;

        if (holdData.tetromino == Tetromino.O || holdData.tetromino == Tetromino.I)
        {
            holdPosition = new Vector3Int(-10, 4, 0);
        }
        else
        {
            holdPosition = new Vector3Int(-9, 4, 0);
        }

        ClearHoldPiece(holdPosition);

        for (int i = 0; i < holdData.cells.Length; i++)
        {
            Vector3Int cellPosition = new Vector3Int(holdData.cells[i].x, holdData.cells[i].y, 0);
            Vector3Int tilePosition = cellPosition + holdPosition;
            this.tilemap.SetTile(tilePosition, holdData.tile);
        }
    }

    public void ClearHoldPiece(Vector3Int holdPosition)
    {
        for (int x = holdPosition.x - 2; x <= holdPosition.x + 2; x++)
        {
            for (int y = holdPosition.y - 1; y <= holdPosition.y + 3; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                this.tilemap.SetTile(position, null);
            }
        }
    }

    public void DrawNextPiece(TetrominoData nextData)
    {
        Vector3Int nextPosition;

        if (nextData.tetromino == Tetromino.O || nextData.tetromino == Tetromino.I)
        {
            nextPosition = new Vector3Int(8, 7, 0);
        }
        else
        {
            nextPosition = new Vector3Int(8, 7, 0);
        }

        ClearNextPiece(nextPosition);

        for (int i = 0; i < nextData.cells.Length; i++)
        {
            Vector3Int cellPosition = new Vector3Int(nextData.cells[i].x, nextData.cells[i].y, 0);
            Vector3Int tilePosition = cellPosition + nextPosition;
            this.tilemap.SetTile(tilePosition, nextData.tile);
        }
    }

    public void ClearNextPiece(Vector3Int nextPosition)
    {
        for (int x = nextPosition.x - 2; x <= nextPosition.x + 3; x++)
        {
            for (int y = nextPosition.y - 1; y <= nextPosition.y + 4; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                this.tilemap.SetTile(position, null);
            }
        }
    }
}
