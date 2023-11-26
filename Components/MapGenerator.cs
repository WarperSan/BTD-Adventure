using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Extensions;
using BTDAdventure.Managers;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BTDAdventure.Components;

public class MapGenerator : MonoBehaviour
{
    #region Constants

    private const int MAP_SIZE_DEFAULT_X = 5;
    private const int MAP_SIZE_DEFAULT_Y = 15;
    private const uint TRAIL_COUNT_DEFAULT = 6;

    private const string PREFAB_NAME_NODE = "Node";
    private const string PREFAB_NAME_LINE = "Line";

    public const string NODE_TYPE_NORMAL = "normal";
    public const string NODE_TYPE_ELITE = "elite";
    public const string NODE_TYPE_BOSS = "boss";
    public const string NODE_TYPE_MERCHANT = "merchant";
    public const string NODE_TYPE_TREASURE = "treasure";

    public const string NODE_ICON_NORMAL = "map_normal_old";
    public const string NODE_ICON_ELITE = "map_elite_old";
    public const string NODE_ICON_BOSS = "map_boss_old";
    public const string NODE_ICON_MERCHANT = "map_merchant_old";
    public const string NODE_ICON_TREASURE = "map_treasure_old";

    #endregion Constants

    internal RectTransform? MapObjectsParent;

    private World? CurrentWorld;

    private struct Path
    {
        public int OriginIndex;
        public int DestinationIndex;
    }

    internal void SetUp(RectTransform? MapObjectsParent)
    {
        nodePrefab ??= GameObject.Instantiate(LoadAsset<GameObject>(PREFAB_NAME_NODE));
        LinePrefab ??= GameObject.Instantiate(LoadAsset<GameObject>(PREFAB_NAME_LINE));
        this.MapObjectsParent = MapObjectsParent;
    }

    internal void GenerateMap(int seed)
    {
        // Get current world
        CurrentWorld = GameManager.Instance.GetWorld();

        // Reset counters for the world
        CurrentWorld.ResetCounters();

        // Get the world size
        var worldSize = GetWorldSize();

        // Save current random state
        Random.State state = Random.state;

        // Set state to seed
        Random.InitState(seed);

        if (MapObjectsParent == null)
        {
            Log("\'MapObjectsParent\' was not defined");
            return;
        }

        if (nodePrefab == null)
        {
            Log("\'nodePrefab\' was not defined");
            return;
        }

        if (LinePrefab == null)
        {
            Log("\'LinePrefab\' was not defined");
            return;
        }

        MapNodes = new MapNode[worldSize.y + 1, worldSize.x];
        MapObjectsParent.sizeDelta = new Vector2(worldSize.y, worldSize.x * 4 / 3) * 300;

        GeneratePaths(GetTrailCount(), worldSize);

        if (nodes == null)
        {
            Log("\'nodes\' was not defined");
            return;
        }

        if (paths == null)
        {
            Log("\'paths\' was not defined");
            return;
        }

        CreateNodes(worldSize);

        Random.state = state; // Restore random state

        // Create lines
        CreateLines();

        // Destroy once done
        ReleaseResources();
    }

    #region Map Generation

    private GameObject? nodePrefab;
    private GameObject? LinePrefab;

    private List<Vector2>? nodes;
    private List<Path>? paths;

    private MapNode[,]? MapNodes;

    /// <summary>
    /// Generates a list of Path that leads from the start to the end
    /// </summary>
    private void GeneratePaths(uint trailCount, Vector2Int size)
    {
        paths = new List<Path>();
        nodes = new List<Vector2>();

        var endBossNode = GetNodeIndex(Mathf.RoundToInt(size.x / 2), size.y);

        // Create X trails
        for (uint i = 0; i < trailCount; ++i)
        {
            // Get a random starting X position
            int xPosition = Random.Range(0, Mathf.FloorToInt(size.x));

            // For each layer
            for (int y = 0; y < size.y; ++y)
            {
                if (y == size.y - 1)
                {
                    paths.Add(new Path()
                    {
                        OriginIndex = GetNodeIndex(xPosition, y),
                        DestinationIndex = endBossNode,
                    });
                    continue;
                }

                var modf = new List<int>() { -1, 0, 1 };

                // Remove paths that would cross other paths
                for (int j = modf.Count - 1; j >= 0; j--)
                {
                    if (modf[j] == 0)
                        continue;

                    // Remove movement if a path is in the way
                    int startIndex = GetNode(xPosition + modf[j], y);
                    int endIndex = GetNode(xPosition, y + 1);

                    if (startIndex == -1 || endIndex == -1)
                        continue;

                    foreach (var item in paths)
                    {
                        if (item.OriginIndex == startIndex && item.DestinationIndex == endIndex)
                        {
                            modf.RemoveAt(j);
                            break;
                        }
                    }
                }

                // Should never be called because 0 can't be crossed
                if (modf.Count == 0)
                    break;

                // Get the node index of the origin node
                int ogIndex = GetNodeIndex(xPosition, y);

                // Move the cursor to the next position while keeping it in bounds
                xPosition = Mathf.Clamp(xPosition + modf[Random.Range(0, modf.Count)], 0, Mathf.FloorToInt(size.x) - 1);

                // Get the node index of the destination node
                int destIndex = GetNodeIndex(xPosition, y + 1);

                var newPath = new Path()
                {
                    OriginIndex = ogIndex,
                    DestinationIndex = destIndex,
                };

                // If the path was already created
                if (!paths.Contains(newPath))
                    paths.Add(newPath);
            }
        }
    }

    private void CreateNodes(Vector2Int size)
    {
        if (nodes == null || nodePrefab == null || MapNodes == null || CurrentWorld == null)
            return;

        for (int i = 0; i < nodes.Count; i++)
            nodes[i] = CreateNode(size, i);
    }

    private Vector3 CreateNode(Vector2Int size, int index)
    {
#nullable disable
        Vector2 item = nodes[index];
#nullable restore

        var newNode = GameObject.Instantiate(nodePrefab, Vector3.zero, Quaternion.identity, MapObjectsParent);

        if (newNode == null)
            return item;

        var position = new Vector3(item.y - size.y / 2, item.x - size.x / 2) * 300 + GetRandomOffset();

        newNode.GetComponent<RectTransform>().localPosition = position;

        var clampedPosition = new Vector2Int(Mathf.FloorToInt(item.y), Mathf.FloorToInt(item.x));

        var mapNode = newNode.GetComponent<MapNode>();
        mapNode.NodeType = GetNodeType(index, clampedPosition);
        mapNode.index = index;

        Texture? texture = GetTexture(mapNode.NodeType);

        if (texture == null)
            Log($"No texture was found for the node type \'{mapNode.NodeType}\'.");
        else
            mapNode.SetSprite(texture);

        newNode.GetComponent<Button>().interactable = false;

        if (index == 0)
        {
            clampedPosition.x = size.y;
            clampedPosition.y = 0;
        }

        if (MapNodes != null)
            MapNodes[clampedPosition.x, clampedPosition.y] = mapNode;
        return position;
    }

    private string GetNodeType(int index, Vector2Int position)
    {
        if (index == 0)
            return NODE_TYPE_BOSS;
        return CurrentWorld?.GetNodeType(position.x, position.y) ?? NODE_TYPE_NORMAL;
    }

    private Vector3 GetRandomOffset()
    {
        if (Settings.GetSettingValue(Settings.SETTING_RANDOM_MAP_OFFSET, false))
            return Vector3.zero;

        var rdmAngle = Random.Range(0, 360);
        return new Vector3(Mathf.Cos(rdmAngle), Mathf.Sin(rdmAngle));
    }

    /// <summary>
    /// Instantiates the lines on the map
    /// </summary>
    private void CreateLines()
    {
        if (MapObjectsParent == null || paths == null || LinePrefab == null)
            return;

        var linesParent = MapObjectsParent.Find("Lines");

        foreach (var path in paths)
            CreatePath(path, linesParent);
    }

    private void CreatePath(Path path, Transform linesParent)
    {
        // Get origin position
        var origin = GetNodePosition(path.OriginIndex);

        // Get destination position
        var destination = GetNodePosition(path.DestinationIndex);

        if (!origin.HasValue || !destination.HasValue)
            return;

        var mapObject = GameObject.Instantiate(LinePrefab, Vector3.zero, Quaternion.identity,
            linesParent);

        if (mapObject == null)
            return;

        mapObject.GetComponent<RectTransform>().localPosition = origin.Value;

        AdaptLine(mapObject, origin.Value, destination.Value);
    }

    private void AdaptLine(GameObject mapObject, Vector2 origin, Vector2 destination)
    {
        Vector2 diff = origin - destination;

        float magnitude = Mathf.Sqrt(Mathf.Pow(diff.x, 2) + Mathf.Pow(diff.y, 2));
        mapObject.GetComponent<RectTransform>().sizeDelta = new Vector3(magnitude, 10);

        var rotation = Quaternion.LookRotation(diff, Vector3.up).eulerAngles;
        var zRotation = Mathf.Sign(rotation.y - 180) == -1 ? 180 - rotation.x : rotation.x;

        mapObject.transform.rotation = Quaternion.Euler(0, 0, zRotation);
    }


    #endregion Map Generation

    #region Node related

    private int GetNodeIndex(float x, float y)
    {
        int index = GetNode(x, y);

        if (index != -1)
            return index;

        nodes?.Add(new Vector2(x, y));
        return nodes == null ? -1 : nodes.Count - 1;
    }

    private int GetNode(float x, float y)
    {
        for (int i = 0; i < nodes?.Count; i++)
        {
            var item = nodes[i];

            if (item.x == x && item.y == y)
                return i;
        }
        return -1;
    }

    private Vector2? GetNodePosition(int index) => nodes?.Count <= index ? null : nodes?[index];

    #endregion Node related

    #region Map Behavior

    private int prevNodeIndex = 0;

    private void UnlockLayer(int layer)
    {
        if (paths == null)
        {
            Log($"\'{paths}\' was not defined");
            return;
        }

        List<int> validIndex = new();
        foreach (var item in paths)
        {
            if (item.OriginIndex == prevNodeIndex || layer == 0)
            {
                validIndex.Add(layer == 0 ? item.OriginIndex : item.DestinationIndex);
            }
        }

        for (int i = 0; i < MapNodes?.GetLength(1); i++)
        {
            var mapNode = MapNodes[layer, i];

            if (mapNode == null)
                continue;

            if (!validIndex.Contains(mapNode.index))
                continue;

            Button btn = mapNode.GetComponent<Button>();

            btn.interactable = true;
            btn.onClick.AddListener(() =>
            {
                UIManager.UseLoading(preLoad: () =>
                {
                    SoundManager.PlaySound(SoundManager.SOUND_FIGHT_STARTED, SoundManager.GeneralGroup);
                },
                postLoad: () =>
                {
                    prevNodeIndex = mapNode.index;

                    // Start new fight
                    UIManager.SetGameUIActive(true);
                    GameManager.Instance.StartFight(mapNode.NodeType);

                    MapObjectsParent?.gameObject.SetActive(false);
                });
            });
        }
    }

    private void LockLayer(int layer)
    {
        for (int i = 0; i < MapNodes?.GetLength(1); i++)
        {
            var mapNode = MapNodes[layer, i];

            if (mapNode == null)
                continue;

            if (mapNode.index == prevNodeIndex)
            {
                mapNode.transform.Find("Slash").GetComponent<RawImage>().enabled = true;
            }

            Button btn = mapNode.gameObject.GetComponent<Button>();

            btn.interactable = false;
            btn.onClick.RemoveAllListeners();
        }
    }

    #endregion Map Behavior

    #region Layers

    private int layer = -1;

    internal void ProgressLayer()
    {
        // If this layer isn't the first layer
        if (layer >= 0)
            LockLayer(layer);

        layer++;

        if (layer < MapNodes?.GetLength(0))
            UnlockLayer(layer);
        else
        {
            InGame.instance.QuitToMainMenu();
            Log("Thank you for playing ! This is the end of the game so far :)");
            // Next World
        }
    }

    #endregion Layers

    #region Get Values

    private Vector2Int GetWorldSize()
    {
        if (CurrentWorld == null)
            return new Vector2Int(MAP_SIZE_DEFAULT_X, MAP_SIZE_DEFAULT_Y);

        // Get the world size
        var worldSize = CurrentWorld.GetWorldSize();

        (worldSize.x, worldSize.y) = (worldSize.y, worldSize.x);

        if (worldSize.x <= 0)
            worldSize.x = MAP_SIZE_DEFAULT_X;

        if (worldSize.y <= 0)
            worldSize.y = MAP_SIZE_DEFAULT_Y;

        return worldSize;
    }

    private uint GetTrailCount()
    {
        if (CurrentWorld == null)
            return TRAIL_COUNT_DEFAULT;

        var trailCount = CurrentWorld.GetTrailCount();

        return trailCount == 0 ? TRAIL_COUNT_DEFAULT : trailCount;
    }

    private Texture? GetTexture(string type)
    {
        Texture? texture = CurrentWorld?.GetMapIcon(type);

        if (texture != null)
            return texture;

        string textureName = type switch
        {
            NODE_TYPE_ELITE => NODE_ICON_ELITE,
            NODE_TYPE_BOSS => NODE_ICON_BOSS,
            NODE_TYPE_MERCHANT => NODE_ICON_MERCHANT,
            NODE_TYPE_TREASURE => NODE_ICON_TREASURE,
            _ => NODE_ICON_NORMAL // Default to normal
        };
        return ModContent.GetTexture<Main>(textureName);
    }

    #endregion Get Values

    private void OnDestroy()
    {
        ReleaseResources();
    }

    private void ReleaseResources()
    {
        if (nodePrefab != null)
            GameObject.Destroy(nodePrefab);

        if (LinePrefab != null)
            GameObject.Destroy(LinePrefab);
    }
}