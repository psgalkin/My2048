using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FieldView : MonoBehaviour
{
    [SerializeField] private int _sizeX;
    [SerializeField] private int _sizeY;
    [SerializeField] private int _startLevel;

    [SerializeField] private float _moveTimeInterval;

    [SerializeField] private Transform _cells;
    [SerializeField] private Transform _tiles;

    [SerializeField] private InGameUi _inGameUi;
    [SerializeField] private SoundController _sounds;

    private Dictionary<Pos, GameObject> _tileViewsByPos;
    private List<GameObject> _myTiles;

    private int _score;
    private int _highScore;

    private float _lastMoveTime;

    private void Awake()
    {
        Field.OnAddTile += AddTile;
        Field.OnMoveTile += MoveTile;
        Field.OnSetTileLevel += SetTileLevel;
        Field.OnDestroyTile += DestroyTile;
        Field.OnMatch += MatchTiles;

        Field.Load(_sizeX, _sizeY);

        _myTiles = new List<GameObject>();
        _tileViewsByPos = new Dictionary<Pos, GameObject>();

        _score = 0;
        if (!PlayerPrefs.HasKey("highScore"))
            PlayerPrefs.SetInt("highScore", 0);
        else
            _highScore = PlayerPrefs.GetInt("highScore");
    }

    void Start()
    {
        DrawField();
        Field.AddRandomTile();
        _lastMoveTime = Time.time;
        _inGameUi.SetScore(_score);
        _inGameUi.SetHighScore(_highScore);
    }

    private void DrawField()
    {
        for (int y = 0; y < _sizeY; ++y)
        {
            for (int x = 0; x < _sizeX; ++x)
            {
                GameObject cellView = (GameObject)Resources.Load("Cell");
                cellView.transform.position = new Vector3Int(x, y, 0);
                Instantiate(cellView, _cells);
            }
        }
    }

    private void AddTile(Pos pos)
    {
        GameObject tileView = Instantiate(Resources.Load("Tile"), _tiles) as GameObject;
        tileView.transform.position = new Vector3Int(pos.X, pos.Y, 0);
        tileView.GetComponentInChildren<TMP_Text>().text = SetLevelText(_startLevel);
        _tileViewsByPos[pos] = tileView;
        _myTiles.Add(tileView);
    }

    private void MoveTile(Pos fromPos, Pos toPos)
    {
        if (_tileViewsByPos.ContainsKey(toPos))
        {
            Debug.Log("Cell to move is not empty");
            return;
        }

        Debug.Log($"View: Try move tile from ({fromPos.X}, {fromPos.Y}) to ({toPos.X}, {toPos.Y})");

        _tileViewsByPos[fromPos].transform.DOMove(new Vector3Int(toPos.X, toPos.Y, 0), 0.3f);
        _tileViewsByPos[toPos] = _tileViewsByPos[fromPos];
        _tileViewsByPos.Remove(fromPos);

        if (Time.time - _lastMoveTime > _moveTimeInterval)
        {
            _sounds.PlaySwapSound();
            _lastMoveTime = Time.time;
        }
    }

    private void SetTileLevel(Pos pos, int level)
    {
        if (!_tileViewsByPos.ContainsKey(pos))
        {
            Debug.Log("Cell to up level is empty");
            return;
        }

        _tileViewsByPos[pos].GetComponentInChildren<TMP_Text>().text = SetLevelText(level);
        _tileViewsByPos[pos].transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.4f);
        _tileViewsByPos[pos].transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.2f);
        _sounds.PlayMatchSound(level);
        AddAndShowScore((int)Mathf.Pow(2.0f, (float)level));
    }

    private void DestroyTile(Pos pos)
    {

        if (!_tileViewsByPos.ContainsKey(pos))
        {
            Debug.Log("Cell to destroy is empty");
            return;
        }
        
        _tileViewsByPos[pos].transform.DOKill();
        Destroy(_tileViewsByPos[pos]);
        _tileViewsByPos.Remove(pos);
    }
     
    private void MatchTiles(Pos fromPos, Pos toPos, int level)
    {
        //if (_tileViewsByPos.ContainsKey(toPos))
        //{
        //    Debug.Log("Cell to move is not empty");
        //    return;
        //}

        Debug.Log($"View: Try move tile from ({fromPos.X}, {fromPos.Y}) to ({toPos.X}, {toPos.Y})");

        StartCoroutine(WaitForDestroy(fromPos, toPos, level));

        //_tileViewsByPos[fromPos].transform.DOMove(new Vector3Int(toPos.X, toPos.Y, 0), 0.3f);
        //_tileViewsByPos[fromPos].GetComponentInChildren<Canvas>().sortingOrder = -1;
        
        //_tileViewsByPos[toPos] = _tileViewsByPos[fromPos];
        //_tileViewsByPos.Remove(fromPos);

        //if (Time.time - _lastMoveTime > _moveTimeInterval)
        //{
        //    _sounds.PlaySwapSound();
        //    _lastMoveTime = Time.time;
        //}
    }
    
    private IEnumerator WaitForDestroy(Pos fromPos, Pos toPos, int level)
    {
        GameObject tile = _tileViewsByPos[fromPos];
        Tween myTween = tile.transform.DOMove(new Vector3Int(toPos.X, toPos.Y, 0), 0.3f);
        myTween.Play();
        tile.GetComponentInChildren<Canvas>().sortingOrder = 1;
        _tileViewsByPos.Remove(fromPos);
        yield return myTween.WaitForCompletion();
        Destroy(tile);
        SetTileLevel(toPos, level);
        
    }


        

    private string SetLevelText(int level)
    {
        return $"{Mathf.Pow(2.0f, (float)level)}";
    }

    private void AddAndShowScore(int scoreIncriment)
    {
        _score += scoreIncriment;
        _inGameUi.SetScore(_score);

        if (_score > _highScore)
        {
            _highScore = _score;
            _inGameUi.SetHighScore(_score);
        }     
    }

    private void OnDestroy()
    {
        Field.OnAddTile -= AddTile;
        Field.OnMoveTile -= MoveTile;
        Field.OnSetTileLevel -= SetTileLevel;
        Field.OnDestroyTile -= DestroyTile;
        Field.OnMatch -= MatchTiles;

        if (PlayerPrefs.GetInt("highScore") < _highScore)
            PlayerPrefs.SetInt("highScore", _highScore);

        _tileViewsByPos.Clear();
        _myTiles.Clear();
    }
}
