using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Extensions;
using BTDAdventure;
using BTDAdventure.Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    private GameObject? nodePrefab;
    private GameObject? LinePrefab;
    internal RectTransform? MapObjectsParent;

    private Vector2 Size;

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

    internal void GenerateMap(Vector2 size, int seed, int trailAmount)
    {
        Random.State state = Random.state;
        Random.InitState(seed);

        Size = size;

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

        MapNodes = new MapNode[Mathf.FloorToInt(Size.y), Mathf.FloorToInt(Size.x)];
        MapObjectsParent.sizeDelta = new Vector2(Size.y * 300, Size.x * 300);

        GeneratePaths(trailAmount);

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

        for (int i = 0; i < nodes.Count; i++)
        {
            var item = nodes[i];

            var newNode = GameObject.Instantiate(nodePrefab, Vector3.zero, Quaternion.identity, MapObjectsParent);
            var position = new Vector3(item.y - Size.y / 2, item.x - Size.x / 2) * 300 - new Vector3(0, 100);

            //var rdmAngle = Random.Range(0, 360);
            //position += new Vector3(Mathf.Cos(rdmAngle), Mathf.Sin(rdmAngle)) * Random.Range(0, 50);

            newNode.GetComponent<RectTransform>().localPosition = position;
            
            var mapNode = newNode.GetComponent<MapNode>();
            mapNode.NodeType = "normal";
            mapNode.index = i;

            Texture? texture = GetTexture(mapNode.NodeType);

            if (texture != null)
                mapNode.SetSprite(texture);
            else
                Log($"No texture was found for the Node type \'{mapNode.NodeType}\'");

            newNode.GetComponent<Button>().interactable = false;

            MapNodes[Mathf.FloorToInt(item.y), Mathf.FloorToInt(item.x)] = mapNode;
            nodes[i] = mapNode.transform.position;
        }

        Random.state = state;

        var linesParent = MapObjectsParent.Find("Lines");

        foreach (var path in paths)
        {
            var origin = GetNodePosition(path.OriginIndex);
            var destination = GetNodePosition(path.DestinationIndex);

            if (!origin.HasValue || !destination.HasValue)
                continue;

            //var mapNode = MapNodes[Mathf.FloorToInt(origin.Value.y), Mathf.FloorToInt(origin.Value.x)];
            var mapObject = GameObject.Instantiate(LinePrefab,
                origin.Value,
                Quaternion.identity,
                linesParent);

            Vector2 diff = origin.Value - destination.Value;

            float magnitude = Mathf.Sqrt(Mathf.Pow(diff.x, 2) + Mathf.Pow(diff.y, 2));

            float dirX = Mathf.Acos(diff.x / magnitude) * 180 / Mathf.PI;
            float diY = Mathf.Asin(diff.y / magnitude) * 180 / Mathf.PI;

            float angle;

            if (diff.y > 0 && diff.x < 0) // # 4
                angle = diY;
            else if (diff.y < 0 && diff.x < 0) // # 1
                angle = dirX;
            else if (diff.y > 0 && diff.x > 0) // # 3
                angle = 360 - dirX;
            else // # 2
                angle = dirX;

            angle -= 90;

            if (angle == 90)
            {
                angle -= 90;
            }
            else
            {
                mapObject.GetComponent<RectTransform>().sizeDelta = new Vector3(420, 10);
            }

            mapObject.transform.eulerAngles = new Vector3(0, 0, angle);

            //if (zRotation != 0)
            //    mapObject.transform.localScale = new Vector3(1 / Mathf.Abs(Mathf.Sin(zRotation)), 1);
        }
    }

    /// <summary>
    /// Generates a list of Path that leads from the start to the end
    /// </summary>
    private void GeneratePaths(int trailCount)
    {
        paths = new List<Path>();
        nodes = new List<Vector2>();

        // Create X trails
        for (int i = 0; i < trailCount; i++)
        {
            // Get a random starting X position
            int xPosition = Random.Range(0, Mathf.FloorToInt(Size.x));

            // For each layer
            for (int y = 0; y < Size.y - 1; y++)
            {
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
                xPosition = Mathf.Clamp(xPosition + modf[Random.Range(0, modf.Count)], 0, Mathf.FloorToInt(Size.x) - 1);

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

    private int GetNodeIndex(int x, int y)
    {
        int index = GetNode(x, y);

        if (index != -1)
            return index;

        nodes?.Add(new Vector2(x, y));
        return nodes == null ? -1 : nodes.Count - 1;
    }

    private int GetNode(int x, int y)
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

    private Texture? GetTexture(string type)
    {
        return type switch
        {
            "normal" => ModContent.GetTexture<Main>("map_normal_old"),
            "elite" => ModContent.GetTexture<Main>("map_elite_old"),
            "boss" => ModContent.GetTexture<Main>("map_boss_old"),
            "merchant" => ModContent.GetTexture<Main>("map_merchant_old"),
            "treasure" => ModContent.GetTexture<Main>("map_treasure_old"),
            _ => null,
        };
    }


    int layer = -1;
    int prevNodeIndex = 0;
    private void UnlockLayer(int layer)
    {
        if (paths == null)
        {
            Log("\'{paths}\' was not defined");
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
                GameManager.Instance.UiManager?.UseLoading(postLoad: new System.Action(() =>
                {
                    prevNodeIndex = mapNode.index;

                    // Start new fight
                    GameManager.Instance.UiManager?.GameUI?.SetActive(true);
                    GameManager.Instance.StartFight(mapNode.NodeType, "forest");

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
            Log("Thank you for playing ! This is the end of the game so far :)");
            // Next World
        }
    }
}
