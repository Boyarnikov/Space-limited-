using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;
using System.Linq;
using System;
using UnityEngine.SceneManagement;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;


public enum TileType
{
    None = -1,
    Empty,
    Free,
    Bomb,
    Trash,
    House,
    Park,
    Utility,
    Hotel,
    Farm,
    Lab,
    Need_House,
    Need_Park,
    Need_Utility,
    Need_Hotel,
    Need_Farm,
    Need_Laboratory,
}


public static class TileConsts
{
    public const int buildingsOffset = 4;
    public const int buildingsCount = 6;
} 


public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [SerializeField] int level = -1;
    DialogueController controller;
    bool startDialog = false;
    bool firstPlacment = false;
    bool firstTaskDone = false;
    bool firstRecepyDone = false;

    [SerializeField] public bool trashed;
    [SerializeField] public Tile cell;
    [SerializeField] private Vector3 x_v = new Vector3(0.866f, -0.5f, 0);
    [SerializeField] private Vector3 y_v = new Vector3(0, 1.0f, 0);
    [SerializeField] DialogueDispellerTarget target;
    [SerializeField] GameObject frame;

    [SerializeField] private int board_size = 2;
    private Dictionary<Vector2, Tile> _grid;

    public Conveyor conv;
    public TaskManager tasks;

    List<Recepy> recepies = new List<Recepy>();

    void Awake()
    {
        Instance = this;
    }

    [SerializeField] public Dictionary<Tuple<TileType, TileType>, TileType> Conversions;

    void GenerateConversions()
    {
        Conversions = new Dictionary<Tuple<TileType, TileType>, TileType>();
        foreach (TileType t1 in Enum.GetValues(typeof(TileType)))
        {
            foreach (TileType t2 in Enum.GetValues(typeof(TileType)))
            {
                Tuple<TileType, TileType> key = new Tuple<TileType, TileType>(t1, t2);
                Conversions[key] = TileType.None;
                if (t1 == TileType.Free && t2 != TileType.None && (int)t2 < TileConsts.buildingsOffset + TileConsts.buildingsCount && t2 >= TileType.Trash)
                {
                    Conversions[new Tuple<TileType, TileType>(t1, t2)] = t2;
                }
                if ((int)t1 + TileConsts.buildingsCount == (int)t2 && (int)t2 >= TileConsts.buildingsOffset + TileConsts.buildingsCount)
                {
                    Conversions[new Tuple<TileType, TileType>(t1, t2)] = TileType.Free;
                }
                if (((int)t1 >= (int)TileType.Trash || t1 == TileType.Free) && t2 == TileType.Bomb)
                {
                    Conversions[new Tuple<TileType, TileType>(t1, t2)] = TileType.Free;
                }
            }
        }
    }

    void Shuffle(List<Tile> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n);
            Tile value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public void GenerateGrid()
    {
        _grid = new Dictionary<Vector2, Tile>();
        var offset_x = -board_size * x_v;
        var offset_y = -board_size * y_v;
        for (var i = 0; i < board_size * 2 + 1; i++)
            for (var j = 0; j < board_size * 2 + 1; j++)
                if (abs(i - j) <= board_size)
                {
                    var tile = Instantiate(cell, Vector3.zero, Quaternion.identity);
                    tile.GetComponent<SpriteRenderer>().sortingLayerName = "Board";
                    tile.name = $"Tile {i - board_size} {j - board_size}";
                    tile.transform.parent = this.transform;
                    tile.transform.position = offset_x + offset_y + x_v * i + y_v * j;
                    tile.Init(i - board_size, j - board_size, TileType.Free);
                    tile.moveToIdle = true;
                    tile.idle = tile.transform.position;
                    _grid[new Vector2(i - board_size, j - board_size)] = tile;
                }

        if (trashed)
        {
            List<Tile> l = _grid.Values.ToList();
            Shuffle(l);
            for (int i  = 0; i < 6; i++)
            {
                l[i].Init(TileType.Trash);
            }
        }
    }

    public List<Tile> GetAllTiles()
    {
        return _grid.Values.ToList(); ;
    }

    public Tile GetTile(Vector2 pos)
    {
        if (_grid.TryGetValue(pos, out var tile))
        {
            return tile;
        }
        return null;
    }

    public Vector3 GetMid()
    {
        return -board_size * x_v / 2 + (x_v + y_v) * board_size;
    }

    void Start()
    {
        float h = ((board_size + 1) * 2 - 1) * 1;
        float w = ((board_size * 2 + 1) / 2 * 0.76f + (board_size + 2) * 0.4f) * 2; //0.76 0.4
        frame.transform.localScale = new Vector2(w, h);
        target.Settings(new Vector2(0.25f, 0.25f));
        conv = FindAnyObjectByType<Conveyor>();
        tasks = FindAnyObjectByType<TaskManager>();

        recepies = FindObjectsOfType<Recepy>().ToList();

        GenerateGrid();
        GenerateConversions();

        controller = FindAnyObjectByType<DialogueController>();

        int filled = 0;
        HashSet<TileType> h2 = new HashSet<TileType>();
        foreach (var t in _grid.Values)
            if (t.type != TileType.Free)
            {
                filled++;
                h2.Add(t.type);
            }

        tasks.filled = filled;
        tasks.types = h2.Count();
    }

    private void Update()
    {
        if (!startDialog)
        {
            switch (level)
            {
                case (0):
                    //controller.SelectDialogue(0, () => { SceneManager.LoadScene("MainMenu", LoadSceneMode.Single); return 0; });
                    controller.SelectDialogue(0, () => { return 0; });
                    break;
                case (1):
                    //controller.SelectDialogue(0, () => { SceneManager.LoadScene("MainMenu", LoadSceneMode.Single); return 0; });
                    controller.SelectDialogue(4, () => { return 0; });
                    break;
                case (2):
                    //controller.SelectDialogue(0, () => { SceneManager.LoadScene("MainMenu", LoadSceneMode.Single); return 0; });
                    controller.SelectDialogue(6, () => { return 0; });
                    break;
                case (3):
                    //controller.SelectDialogue(0, () => { SceneManager.LoadScene("MainMenu", LoadSceneMode.Single); return 0; });
                    controller.SelectDialogue(9, () => { return 0; });
                    break;
                case (4):
                    //controller.SelectDialogue(0, () => { SceneManager.LoadScene("MainMenu", LoadSceneMode.Single); return 0; });
                    controller.SelectDialogue(12, () => { return 0; });
                    break;
            }
            startDialog = true;
        }
    }

    public void PlaceFigure(Figure f)
    {
        float d = 1000f;
        Vector2 ind = Vector2.zero;
        foreach (var key in _grid.Keys)
        {
            float dist = (_grid[key].transform.position - f.transform.position).magnitude;
            if (d > dist)
            {
                d = dist;
                ind = key;
            }
        }

        if (d < 0.5f)
        {
            bool is_placable = true;

            foreach (var key in f._grid.Keys)
            {
                if (_grid.ContainsKey(key + ind) && f._grid[key].type != TileType.Empty)
                {
                    TileType t1 = _grid[key + ind].type;
                    TileType t2 = f._grid[key].type;
                    Tuple<TileType, TileType> conv_key = new Tuple<TileType, TileType>(t1, t2);
                    is_placable &= (Conversions[conv_key] != TileType.None);
                    if (Conversions[conv_key] == TileType.None)
                    {
                        //Debug.Log("cannot convert " + (int)t1 + " to " + (int)t2);
                        _grid[key + ind].gameObject.GetComponent<TileAnimaitonManager>().UncorrectAnimation();
                    }
                    //_grid[key + ind].Init(f._grid[key].type);
                }
                else if (f._grid[key].type != TileType.Empty)
                {
                    is_placable = false;
                    //Debug.Log("not placable at l");
                }
            }

            if (is_placable)
            {
                tasks.turn += 1;

                foreach (var key in f._grid.Keys)
                {
                    if (_grid.ContainsKey(key + ind) && f._grid[key].type != TileType.Empty)
                    {
                        TileType t1 = _grid[key + ind].type;
                        TileType t2 = f._grid[key].type;
                        Tuple<TileType, TileType> conv_key = new Tuple<TileType, TileType>(t1, t2);
                        _grid[key + ind].transform.position += Vector3.down * 0.05f;
                        _grid[key + ind].Init(Conversions[conv_key]);
                    }
                }
                if (!firstPlacment)
                {
                    switch (level)
                    {
                        case (0):
                            //controller.SelectDialogue(0, () => { SceneManager.LoadScene("MainMenu", LoadSceneMode.Single); return 0; });
                            controller.SelectDialogue(1, () => { return 0; });
                            break;
                    }
                }
                firstPlacment = true;


                int filled = 0;
                HashSet<TileType> h = new HashSet<TileType>();
                foreach (var t in _grid.Values)
                    if (t.type != TileType.Free)
                    {
                        filled++;
                        h.Add(t.type);
                    }
                        
                tasks.filled = filled;
                tasks.types = h.Count();


                foreach (var r in recepies)
                {
                    for (int i = 0; i < r.queue.Count; i++)
                    {
                        if (r.queue[i].gameObject.GetInstanceID() == f.gameObject.GetInstanceID())
                        {
                            r.TaskDone();
                            r.queue[i] = null;

                            if (!firstRecepyDone)
                            {
                                switch (level)
                                {
                                    case (2):
                                        controller.SelectDialogue(8, () => { return 0; });
                                        break;
                                    case (3):
                                        controller.SelectDialogue(10, () => { return 0; });
                                        break;
                                }
                            }
                            firstRecepyDone = true;
                        }
                    }
                    r.UpdateQueue();

                }

                for (int i = 0; i < conv.figureList.Count; i++) 
                {
                    if (conv.figureList[i].gameObject.GetInstanceID() == f.gameObject.GetInstanceID())
                        conv.figureList[i] = null;

                }

                for (int i = 0; i < tasks.queue.Count; i++)
                {
                    if (tasks.queue[i].gameObject.GetInstanceID() == f.gameObject.GetInstanceID())
                    {
                        tasks.queue[i] = null;

                        // first done task of a mission
                        if (!firstTaskDone)
                        {
                            switch (level)
                            {
                                case (0):
                                    controller.SelectDialogue(2, () => { return 0; });
                                    break;
                                case (2):
                                    controller.SelectDialogue(7, () => { return 0; });
                                    break;
                                case (4):
                                    controller.SelectDialogue(13, () => { return 0; });
                                    break;
                            }
                        }
                        firstTaskDone = true;

                        // end of a mission
                        if (tasks.queue.Count == 1)
                        {
                            switch (level)
                            {
                                case (0):
                                    controller.SelectDialogue(3, () => { 
                                        if (Globals.levelsUnlocked < 2) { Globals.levelsUnlocked = 2; }
                                        if (Globals.levelsDone < 1) { Globals.levelsDone = 1; }
                                        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
                                        return 0; 
                                    });
                                    break;
                                case (1):
                                    controller.SelectDialogue(5, () => {
                                        if (Globals.levelsUnlocked < 3) { Globals.levelsUnlocked = 3; }
                                        if (Globals.levelsDone < 2) { Globals.levelsDone = 2; }
                                        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
                                        return 0;
                                    });
                                    break;
                                case (2):
                                    controller.SelectDialogue(16, () => {
                                        if (Globals.levelsUnlocked < 4) { Globals.levelsUnlocked = 4; }
                                        if (Globals.levelsDone < 3) { Globals.levelsDone = 3; }
                                        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
                                        return 0;
                                    });
                                    break;
                                case (3):
                                    controller.SelectDialogue(11, () => {
                                        if (Globals.levelsUnlocked < 5) { Globals.levelsUnlocked = 5; }
                                        if (Globals.levelsDone < 4) { Globals.levelsDone = 4; }
                                        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
                                        return 0;
                                    });
                                    break;
                                case (4):
                                    controller.SelectDialogue(14, () => {
                                        if (Globals.levelsUnlocked < 6) { Globals.levelsUnlocked = 6; }
                                        if (Globals.levelsDone < 5) { Globals.levelsDone = 5; }
                                        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
                                        return 0;
                                    });
                                    break;
                            }
                        }


                    }
                        
                }

                conv.UpdateConveyor();
                tasks.UpdateQueue();
                Destroy(f.gameObject);
            }
        }
    }
}