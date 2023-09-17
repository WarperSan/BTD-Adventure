using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Extensions;
using BTDAdventure.Managers;
using Il2CppAssets.Scripts.Unity.UI_New;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

namespace BTDAdventure.Components;

public class MapGenerator : MonoBehaviour
{
    #region Constants
    public const string NormalNode = "normal";
    public const string EliteNode = "elite";
    public const string BossNode = "boss";
    public const string MerchantNode = "merchant";
    public const string TreasureNode = "treasure";
    #endregion

    private GameObject? nodePrefab;
    private GameObject? LinePrefab;
    internal RectTransform? MapObjectsParent;

    private World? CurrentWorld;

    private List<Vector2>? nodes;
    private List<Path>? paths;

    private MapNode[,]? MapNodes;

    struct Path
    {
        public int OriginIndex;
        public int DestinationIndex;
    }

    internal void SetUp(RectTransform? MapObjectsParent)
    {
        nodePrefab ??= GameObject.Instantiate(LoadAsset<GameObject>("Node"));
        LinePrefab ??= GameObject.Instantiate(LoadAsset<GameObject>("Line"));
        this.MapObjectsParent = MapObjectsParent;
    }

    internal void GenerateMap(int seed)
    {
        CurrentWorld = GameManager.Instance.GetWorld();
        CurrentWorld.ResetCounters();

        var size = new Point(CurrentWorld.Size.x, CurrentWorld.Size.y);

        if (size.X <= 0)
            size.X = 5;

        if (size.Y <= 0)
            size.Y = 15;

        var trailAmount = CurrentWorld.TrailsCount;

        if (trailAmount == 0)
            trailAmount = 6;

        Random.State state = Random.state;
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

        MapNodes = new MapNode[size.Y + 1, size.X];
        MapObjectsParent.sizeDelta = new Vector2(size.Y, size.X * 4 / 3) * 300;

        GeneratePaths(trailAmount, size);

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

        CreateNodes(size);

        Random.state = state;

        // Create lines
        CreateLines();
    }

    /// <summary>
    /// Generates a list of Path that leads from the start to the end
    /// </summary>
    private void GeneratePaths(uint trailCount, Point size)
    {
        paths = new List<Path>();
        nodes = new List<Vector2>();

        var endBossNode = GetNodeIndex(Mathf.RoundToInt(size.X / 2), size.Y);

        // Create X trails
        for (uint i = 0; i < trailCount; ++i)
        {
            // Get a random starting X position
            int xPosition = Random.Range(0, Mathf.FloorToInt(size.X));

            // For each layer
            for (int y = 0; y < size.Y; ++y)
            {
                if (y == size.Y - 1)
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
                xPosition = Mathf.Clamp(xPosition + modf[Random.Range(0, modf.Count)], 0, Mathf.FloorToInt(size.X) - 1);

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

    private void CreateNodes(Point size)
    {
        if (nodes == null || nodePrefab == null || MapNodes == null || CurrentWorld == null)
            return;

        for (int i = 0; i < nodes.Count; i++)
        {
            var item = nodes[i];

            var newNode = GameObject.Instantiate(nodePrefab, Vector3.zero, Quaternion.identity, MapObjectsParent);

            var rdmAngle = Random.Range(0, 360);
            var position = new Vector3(item.y - size.Y / 2, item.x - size.X / 2) * 300 + new Vector3(Mathf.Cos(rdmAngle), Mathf.Sin(rdmAngle)) * 50;

            newNode.GetComponent<RectTransform>().localPosition = position;

            var mapNode = newNode.GetComponent<MapNode>();
            mapNode.NodeType = i == 0 ? "boss" : CurrentWorld.GetNodeType(Mathf.FloorToInt(item.y), Mathf.FloorToInt(item.x));
            mapNode.index = i;

            Texture? texture = GetTexture(mapNode.NodeType);

            if (texture != null)
                mapNode.SetSprite(texture);
            else
                Log($"No texture was found for the Node type \'{mapNode.NodeType}\'");

            newNode.GetComponent<Button>().interactable = false;

            if (i == 0)
            {
                MapNodes[size.Y, 0] = mapNode;
            }
            else
            {
                MapNodes[Mathf.FloorToInt(item.y), Mathf.FloorToInt(item.x)] = mapNode;
            }
            nodes[i] = position;
        }
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
        {
            var origin = GetNodePosition(path.OriginIndex);
            var destination = GetNodePosition(path.DestinationIndex);

            if (!origin.HasValue || !destination.HasValue)
                continue;

            var mapObject = GameObject.Instantiate(LinePrefab,
                Vector3.zero,
                Quaternion.identity,
                linesParent);

            mapObject.GetComponent<RectTransform>().localPosition = origin.Value;

            Vector2 diff = origin.Value - destination.Value;

            float magnitude = Mathf.Sqrt(Mathf.Pow(diff.x, 2) + Mathf.Pow(diff.y, 2));
            mapObject.GetComponent<RectTransform>().sizeDelta = new Vector3(magnitude, 10);

            var rotation = Quaternion.LookRotation(diff, Vector3.up).eulerAngles;
            var zRotation = Mathf.Sign(rotation.y - 180) == -1 ? 180 - rotation.x : rotation.x;

            mapObject.transform.rotation = Quaternion.Euler(0, 0, zRotation);
        }
    }

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
    #endregion

    private Texture? GetTexture(string type) => type switch
    {
        NormalNode => ModContent.GetTexture<Main>("map_normal_old"),
        EliteNode => ModContent.GetTexture<Main>("map_elite_old"),
        BossNode => ModContent.GetTexture<Main>("map_boss_old"),
        MerchantNode => ModContent.GetTexture<Main>("map_merchant_old"),
        TreasureNode => ModContent.GetTexture<Main>("map_treasure_old"),
        _ => CurrentWorld?.GetMapIcon(type), // If icon not found, ask world to give icon
    };

    #region Map Behavior
    int layer = -1;
    int prevNodeIndex = 0;
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
            btn.onClick.AddListener(new Function(() =>
            {
                GameManager.Instance.UiManager?.UseLoading(
                    preLoad: new System.Action(() =>
                    {
                        SoundManager.PlaySound("laugh", SoundManager.GeneralGroup);
                    }),

                    postLoad: new System.Action(() =>
                    {
                        prevNodeIndex = mapNode.index;

                        // Start new fight
                        GameManager.Instance.UiManager?.GameUI?.SetActive(true);
                        GameManager.Instance.StartFight(mapNode.NodeType);

                        MapObjectsParent?.gameObject.SetActive(false);

                    }));
            }));
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

    internal void ProgressLayer()
    {
        if (layer >= 0)
        {
            LockLayer(layer);
        }

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
    #endregion

    void OnDestroy()
    {
        if (nodePrefab != null)
            GameObject.Destroy(nodePrefab);

        if (LinePrefab != null)
            GameObject.Destroy(LinePrefab);
    }
}