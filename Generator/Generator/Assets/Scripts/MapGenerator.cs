using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.CompositeCollider2D;

/*
 * Based on "Procedural Cave Generation" by Sebastian Lague
 * https://www.youtube.com/watch?v=v7yyZZjF1z4&list=PLFt_AvWsXl0eZgMK_DT5_biRkWXftAOf9
 */
public class MapGenerator : MonoBehaviour
{
    [Header("Map Basic Settings")]
    public int width;
	public int height;
	public string seed;
	public bool useRandomSeed;
    public GameObject playerPrefab;
    public GameObject labPrefab;
    public GameObject bridgePrefab;
    [Range(0, 100)]
	public int randomFillPercent;
    public enum MapGenerationType
    {
        RandomFill,
        IslandArchipelago,
    }
    public MapGenerationType generationType = MapGenerationType.RandomFill;

    [Header("Island Generator Settings")]
    public int island_Count = 8;
    public int minIsland_Radius = 6;
    public int maxIsland_Radius = 10;
    public int minCenter_Distance = 15;

    [Header("Perlin Noise Settings")]
    [Range(0.01f, 0.3f)] public float perlinFrequency = 0.08f;
    [Range(0f, 1f)] public float perlinAmplitude = 0.4f;

    [Header("Difficulty Settings")]
    public bool hardMode = true; // true = Difficult mode, generate the farthest island pair; false = Normal mode, generate the nearest island pair
    public int fakeBridgeNums = 3;
    public int zombieNums = 3;

    [Header("Agent Settings")]
    public GameObject playerAgentPrefab;
    public GameObject zombieAgentPrefab;
    public GameObject gameManagerPrefab;


    int[,] map; // A two-dimensional array for storing map data (0=sea, 1=wall, 2)

    public struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }

    class CoordWithCost
    {
        public Coord coord;
        public int gCost; // The actual cost from the starting point to the current node
        public int hCost; // Estimate the cost from the current node to the target node
        public int fCost => gCost + hCost;
        public CoordWithCost parent;

        public CoordWithCost(Coord c, int g, int h, CoordWithCost parent = null)
        {
            this.coord = c;
            this.gCost = g;
            this.hCost = h;
            this.parent = parent;
        }
    }

    class PriorityQueue<T>
    {
        private List<(T item, int priority)> elements = new List<(T, int)>();

        public int Count => elements.Count;

        public void Enqueue(T item, int priority)
        {
            elements.Add((item, priority));
        }

        public T Dequeue()
        {
            int bestIndex = 0;
            for (int i = 1; i < elements.Count; i++)
            {
                if (elements[i].priority < elements[bestIndex].priority)
                {
                    bestIndex = i;
                }
            }

            T bestItem = elements[bestIndex].item;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }
    }

    /*
	 * Generate the map on start, on mouse click
	 */
    void Start()
	{
		GenerateMap();
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			GenerateMap();
		}
	}

	void GenerateMap()
	{
        // Clean up old point markers (to prevent accumulation)
        ClearTaggedObjects("BridgeTile");
        ClearTaggedObjects("PlayerSpawnPoint");
        ClearTaggedObjects("LabPoint");
        ClearTaggedObjects("Zombie");

        map = new int[width, height];

        // Stage 1: populate the grid cells
        PopulateMap();

        // Stage 2: apply cellular automata rules
        for (int i = 0; i < 5; i++)
		{
			SmoothMap();
		}

        // Stage 3: finalise the map
        if(generationType == MapGenerationType.IslandArchipelago)
        {
            ProcessMap();
        }

		AddMapBorder();

		// Generate mesh
		MeshGenerator meshGen = GetComponent<MeshGenerator>();
		meshGen.GenerateMesh(map, 1);
	}

    /*
	 * STAGE 1: Populate the map
	 */
    void PopulateMap()
    {
		switch(generationType)
		{
            case MapGenerationType.RandomFill:
                RandomFillMap();
                break;
            case MapGenerationType.IslandArchipelago:
                GenerateIslandArchipelagoMap();
                break;
        }
    }

    // MODE 1: Random
    void RandomFillMap()
	{
		if (useRandomSeed)
		{
			seed = Time.time.ToString();
		}

		System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
                // Ensure that the edges of the map are all walls
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
				{
					map[x, y] = 1;
				}
				else
				{
                    // Set the probability of the wall based on 'randomFillPercent'
                    map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
				}
			}
		}
	}

    // MODE 2: Archipelago
    void GenerateIslandArchipelagoMap()
    {
        // 初始化整张地图为海洋（0 表示水）
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = 0;
            }
        }

        // 岛屿生成参数
        int islandCount = island_Count; // 想要生成的岛屿数量
        int minIslandRadius = minIsland_Radius;
        int maxIslandRadius = maxIsland_Radius;
        int minCenterDistance = minCenter_Distance; // 岛屿间最小距离，避免粘连
        int maxAttempts = 100; // 每个岛尝试放置次数（防止死循环）

        // Perlin Noise 控制参数
        float frequency = perlinFrequency;    // 控制岛屿边缘的起伏粗细，越小越平滑
        float amplitude = perlinAmplitude;     // 控制边缘最大扰动程度，越大越破碎

        // 创建一个伪随机数生成器
        System.Random rand = useRandomSeed ? new System.Random(Time.time.GetHashCode()) : new System.Random(seed.GetHashCode());

        // 每个岛屿都加一个 Perlin 的偏移，让每个岛不同
        float perlinOffsetX = rand.Next(0, 10000);
        float perlinOffsetY = rand.Next(0, 10000);

        // 用于记录已成功放置的岛屿中心
        List<Vector2Int> islandCenters = new List<Vector2Int>();

        // 生成指定数量的岛屿
        for (int i = 0; i < islandCount; i++)
        {
            bool placed = false;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // 随机生成岛屿中心点（不贴边）
                int centerX = rand.Next(10, width - 10);
                int centerY = rand.Next(10, height - 10);
                Vector2Int candidate = new Vector2Int(centerX, centerY);

                // 检查该点是否与已存在岛屿太近
                bool tooClose = false;
                foreach (Vector2Int existing in islandCenters)
                {
                    if (Vector2Int.Distance(existing, candidate) < minCenterDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose) continue;

                // 如果通过距离检测，准备生成这个岛屿
                islandCenters.Add(candidate);
                placed = true;

                // 随机半径，决定岛屿大小和形状
                int radiusX = rand.Next(minIslandRadius, maxIslandRadius);
                int radiusY = rand.Next(minIslandRadius, maxIslandRadius);

                // 遍历椭圆区域中的每个格子，判断是否属于岛屿
                for (int x = -radiusX; x <= radiusX; x++)
                {
                    for (int y = -radiusY; y <= radiusY; y++)
                    {
                        int drawX = centerX + x;
                        int drawY = centerY + y;

                        if (IsInMapRange(drawX, drawY))
                        {
                            // 归一化坐标，用于计算椭圆范围
                            float normX = x / (float)radiusX;
                            float normY = y / (float)radiusY;
                            float distanceFactor = normX * normX + normY * normY;

                            // 获取 Perlin Noise 值，用于扰动边缘
                            float sampleX = (drawX + perlinOffsetX) * frequency;
                            float sampleY = (drawY + perlinOffsetY) * frequency;
                            float noise = (Mathf.PerlinNoise(sampleX, sampleY) - 0.5f) * 2f * amplitude; // [-ampl, +ampl]

                            // 如果该点在“扰动后的椭圆范围”内，则为陆地
                            if (distanceFactor <= 1f + noise)
                            {
                                map[drawX, drawY] = 1;
                            }
                        }
                    }
                }

                break; // 成功放置岛屿，跳出尝试循环
            }

            if (!placed)
            {
                Debug.LogWarning($"Failed to place island {i + 1} due to proximity constraints.");
            }
        }
    }



    /*
	 * STAGE 2: Smooth map with CA
	 */
    void SmoothMap()
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
                // 计算该点周围的墙壁数量
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                // 根据周围墙壁的数量决定当前点是墙壁还是空地
                if (neighbourWallTiles > 4)
					map[x, y] = 1;
				else if (neighbourWallTiles < 4)
					map[x, y] = 0;

			}
		}
	}

    // Calculate the number of walls around the current point (x, y)
    int GetSurroundingWallCount(int gridX, int gridY)
	{
		int wallCount = 0;
		for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
		{
			for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
			{
				if (IsInMapRange(neighbourX, neighbourY))
				{
					if (neighbourX != gridX || neighbourY != gridY)
					{
						wallCount += map[neighbourX, neighbourY];
					}
				}
				else
				{
                    wallCount++;
				}
			}
		}

		return wallCount;
	}

	bool IsInMapRange(int x, int y)
	{
		return x >= 0 && x < width && y >= 0 && y < height;
	}



    /*
	 * Stage 3: produce the finished map
	 */
    void ProcessMap()
    {
        // Obtain islands & preprocess
        List<List<Coord>> landRegions = GetCleanedLandRegions(1);

        if (landRegions.Count < 2)
        {
            Debug.LogWarning("Not enough valid islands.");
            return;
        }

        // Clearing too small islands  
        RemoveSmallIslands(landRegions, 20);

        // Generate PlayerSpawnPoint and Lab
        var spawnInfo = ChooseSpawnAndLab(landRegions, hardMode);
        if (!spawnInfo.Item1)
        {
            Debug.LogWarning("Failed to select spawn and lab islands.");
            return;
        }

        Coord playerSpawn = spawnInfo.Item2;
        Coord labLocation = spawnInfo.Item3;
        List<Coord> playerIsland = spawnInfo.Item4;
        List<Coord> labIsland = spawnInfo.Item5;

        map[playerSpawn.tileX, playerSpawn.tileY] = 2;
        map[labLocation.tileX, labLocation.tileY] = 3;

        InstantiateSpawnPrefabs(playerSpawn, labLocation);

        CreateBridgeBetween(playerSpawn, labLocation);

        GenerateFakeBridges(landRegions, playerSpawn, labLocation, fakeBridgeNums);

        List<Coord> bridgePath = FindPathAStar(playerSpawn, labLocation);
        InstantiateAgentsAndAI(landRegions, playerSpawn, labLocation, bridgePath);

        InstantiateBridgeTiles();

    }

    // 1. Clean the boundaries and obtain island areas
    List<List<Coord>> GetCleanedLandRegions(int tileType)
    {
        var regions = GetRegions(tileType);
        return regions.Where(region =>
            !region.Any(coord =>
                coord.tileX <= 1 || coord.tileX >= width - 2 ||
                coord.tileY <= 1 || coord.tileY >= height - 2)
        ).ToList();
    }

    // 2. Remove islands that are too small
    void RemoveSmallIslands(List<List<Coord>> regions, int minSize)
    {
        foreach (var region in regions)
        {
            if (region.Count < minSize)
            {
                foreach (var tile in region)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }
    }

    // 3. Instantiate prefab
    void InstantiateSpawnPrefabs(Coord playerSpawn, Coord labLocation)
    {
        Vector3 playerWorldPos = new Vector3(playerSpawn.tileX - width / 2 + 0.5f, -0.5f, playerSpawn.tileY - height / 2 + 0.5f);
        Vector3 labWorldPos = new Vector3(labLocation.tileX - width / 2 + 0.5f, -0.5f, labLocation.tileY - height / 2 + 0.5f);
        Instantiate(playerPrefab, playerWorldPos, Quaternion.identity);
        Instantiate(labPrefab, labWorldPos, Quaternion.identity);
    }

    // 4. Laying bridge paths
    void CreateBridgeBetween(Coord start, Coord end)
    {
        var path = FindPathAStar(start, end);
        foreach (var tile in path)
        {
            if (map[tile.tileX, tile.tileY] == 0)
            {
                map[tile.tileX, tile.tileY] = 4;
            }
        }
    }

    // 5. Instantiate bridge prefab
    void InstantiateBridgeTiles()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 4)
                {
                    Vector3 pos = new Vector3(x - width / 2 + 0.5f, -0.5f, y - height / 2 + 0.5f);
                    Instantiate(bridgePrefab, pos, Quaternion.identity);
                }
            }
        }
    }

    // 6. Select the PlayerSpawnPoint and lab——According to the Difficulty Mode
    (bool, Coord, Coord, List<Coord>, List<Coord>) ChooseSpawnAndLab(List<List<Coord>> landRegions, bool hardMode)
    {
        if (landRegions.Count < 2)
            return (false, new Coord(), new Coord(), null, null);

        // Calculate the 'center point' of all islands
        Vector2Int[] centers = landRegions.Select(region =>
        {
            int sumX = 0, sumY = 0;
            foreach (var c in region)
            {
                sumX += c.tileX;
                sumY += c.tileY;
            }
            return new Vector2Int(sumX / region.Count, sumY / region.Count);
        }).ToArray();

        // Construct all island pairs and their distances
        var islandPairs = new List<(int, int, float)>();
        for (int i = 0; i < centers.Length; i++)
        {
            for (int j = i + 1; j < centers.Length; j++)
            {
                float dist = Vector2Int.Distance(centers[i], centers[j]);
                islandPairs.Add((i, j, dist));
            }
        }

        // Set the minimum distance threshold in normal mode
        float minAllowedDistance = 40f;

        // Select candidate pairs based on difficulty mode
        var validPairs = hardMode
            ? islandPairs.OrderByDescending(p => p.Item3).ToList()
            : islandPairs.Where(p => p.Item3 >= minAllowedDistance).OrderBy(p => p.Item3).ToList();

        // If there are no matching pairs in the normal mode, choose the nearest two islands as the second best option
        if (validPairs.Count == 0)
        {
            validPairs = islandPairs.OrderBy(p => p.Item3).ToList();
        }

        var chosen = validPairs.First();
        var islandA = landRegions[chosen.Item1];
        var islandB = landRegions[chosen.Item2];

        System.Random rand = useRandomSeed ? new System.Random(Time.time.GetHashCode()) : new System.Random(seed.GetHashCode());
        Coord player = islandA[rand.Next(islandA.Count)];
        Coord lab = islandB[rand.Next(islandB.Count)];

        return (true, player, lab, islandA, islandB);
    }

    // 7. Generate FakeBridges
    /// <summary>
    /// Generate misleading fake bridges from player's island to other islands
    ///  , to increase exploration difficulty.
    /// </summary>
    /// 
    void GenerateFakeBridges(List<List<Coord>> landRegions, Coord playerSpawn, Coord labLocation, int numberOfFakeBridges)
    {
        // Get the index of the player's island and lab's island in the landRegions list
        int playerIslandIndex = landRegions.FindIndex(region => region.Contains(playerSpawn));
        int labIslandIndex = landRegions.FindIndex(region => region.Contains(labLocation));

        // Sanity check
        if (playerIslandIndex == -1 || labIslandIndex == -1)
        {
            Debug.LogWarning("Cannot find player or lab island in land regions.");
            return;
        }

        // Get the actual island region where the player starts
        List<Coord> playerIsland = landRegions[playerIslandIndex];

        // Initialize random number generator with seed
        System.Random rand = useRandomSeed
            ? new System.Random(Time.time.GetHashCode())
            : new System.Random(seed.GetHashCode());

        int fakeBridgeCount = 0;
        int attempts = 0;
        int maxAttempts = 200;      // Max number of attempts to find valid fake bridge pairs
        int minPathLength = 1;      // Minimum acceptable path length
        int maxPathLength = 200;    // Maximum acceptable path length

        while (fakeBridgeCount < numberOfFakeBridges && attempts < maxAttempts)
        {
            attempts++;

            // Randomly choose a target island (must NOT be the player or lab island)
            int targetIndex = rand.Next(landRegions.Count);
            if (targetIndex == playerIslandIndex || targetIndex == labIslandIndex)
                continue;

            List<Coord> targetIsland = landRegions[targetIndex];

            // Randomly choose a point on the player island and target island to attempt connection
            Coord start = playerIsland[rand.Next(playerIsland.Count)];
            Coord end = targetIsland[rand.Next(targetIsland.Count)];

            // Use A* to find a valid path
            List<Coord> path = FindPathAStar(start, end);

            // Validate path: skip if path is empty or does not meet length constraints
            if (path.Count == 0)
            {
                Debug.Log($"[FakeBridge] No path between island {playerIslandIndex} and {targetIndex}");
                continue;
            }
            if (path.Count < minPathLength)
            {
                Debug.Log($"[FakeBridge] Path too short ({path.Count})");
                continue;
            }
            if (path.Count > maxPathLength)
            {
                Debug.Log($"[FakeBridge] Path too long ({path.Count})");
                continue;
            }

            // Mark path tiles that are on water as bridge (map value = 4)
            foreach (Coord tile in path)
            {
                if (map[tile.tileX, tile.tileY] == 0) // 0 = water
                {
                    map[tile.tileX, tile.tileY] = 4;  // 4 = bridge
                }
            }

            fakeBridgeCount++;
        }

        // Final summary log
        Debug.Log($"Generated {fakeBridgeCount}/{numberOfFakeBridges} fake bridges after {attempts} attempts.");
    }

    // 8. Instantiate Agent
    void InstantiateAgentsAndAI(List<List<Coord>> landRegions, Coord playerSpawn, Coord labLocation, List<Coord> path)
    {
        // 1. Instantiate GameManager
        GameObject gmObj = Instantiate(gameManagerPrefab);
        GameManager gm = gmObj.GetComponent<GameManager>();

        // 2. Instantiate PlayerAgent
        Vector3 playerWorldPos = new Vector3(playerSpawn.tileX - width / 2 + 0.5f, 1.0f, playerSpawn.tileY - height / 2 + 0.5f);
        GameObject playerAgentObj = Instantiate(playerAgentPrefab, playerWorldPos, Quaternion.identity);
        PlayerAgent playerAgent = playerAgentObj.GetComponent<PlayerAgent>();
        playerAgent.gameManager = gm;

        // Convert the path from tile coordinates to world coordinates
        List<Vector3> worldPath = path.ConvertAll(c =>
            new Vector3(c.tileX - width / 2 + 0.5f, 0.5f, c.tileY - height / 2 + 0.5f));
        playerAgent.SetPath(worldPath);

        // 3. Spawn ZombieAgent
        List<ZombieAgentPathing> zombieList = new List<ZombieAgentPathing>();

        foreach (var region in landRegions)
        {
            // Only generated on islands containing players or labs (reachable islands)
            if (!region.Contains(playerSpawn) && !region.Contains(labLocation)) continue;

            int spawnCount = zombieNums; // Number of zombies in each region (adjustable)
            for (int i = 0; i < spawnCount; i++)
            {
                Coord spawnTile = region[Random.Range(0, region.Count)];
                Vector3 zombiePos = new Vector3(spawnTile.tileX - width / 2 + 0.5f, 0.5f, spawnTile.tileY - height / 2 + 0.5f);
                GameObject zombieObj = Instantiate(zombieAgentPrefab, zombiePos, Quaternion.identity);

                ZombieAgentPathing zombie = zombieObj.GetComponent<ZombieAgentPathing>();
                zombie.player = playerAgent.transform;
                zombie.mapGen = this;
                zombie.gameManager = gm;
                zombieList.Add(zombie);
            }
        }

        gm.zombies = zombieList.ToArray();
    }

    // Clean last time objects
    void ClearTaggedObjects(string tag)
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(tag))
        {
            Destroy(go);
        }
    }

    void AddMapBorder()
	{
		int borderSize = 1;
		int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

		for (int x = 0; x < borderedMap.GetLength(0); x++)
		{
			for (int y = 0; y < borderedMap.GetLength(1); y++)
			{
				if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
				{
					borderedMap[x, y] = map[x - borderSize, y - borderSize];
				}
				else
				{
					borderedMap[x, y] = 1;
				}
			}
		}
		map = borderedMap;
	}

    public List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int tileType = map[startX, startY];
        bool[,] visited = new bool[width, height];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int nx = tile.tileX + x;
                    int ny = tile.tileY + y;

                    if ((x == 0 || y == 0) && IsInMapRange(nx, ny) && !visited[nx, ny] && map[nx, ny] == tileType)
                    {
                        visited[nx, ny] = true;
                        queue.Enqueue(new Coord(nx, ny));
                    }
                }
            }
        }

        return tiles;
    }

    // Get all the islands
    public List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        bool[,] visited = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!visited[x, y] && map[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        visited[tile.tileX, tile.tileY] = true;
                    }
                }
            }
        }

        return regions;
    }

    // A*
    public List<Coord> FindPathAStar(Coord start, Coord end)
    {
        int[,] costMap = new int[width, height];
        bool[,] closed = new bool[width, height];
        PriorityQueue<CoordWithCost> open = new PriorityQueue<CoordWithCost>();

        int Heuristic(Coord a, Coord b)
        {
            return Mathf.Abs(a.tileX - b.tileX) + Mathf.Abs(a.tileY - b.tileY);
        }

        CoordWithCost startNode = new CoordWithCost(start, 0, Heuristic(start, end));
        open.Enqueue(startNode, startNode.fCost);

        while (open.Count > 0)
        {
            CoordWithCost current = open.Dequeue();
            Coord c = current.coord;
            if (closed[c.tileX, c.tileY])
                continue;

            closed[c.tileX, c.tileY] = true;

            if (c.tileX == end.tileX && c.tileY == end.tileY)
            {
                // 路径到达终点，回溯构造路径
                List<Coord> path = new List<Coord>();
                while (current != null)
                {
                    path.Add(current.coord);
                    current = current.parent;
                }
                path.Reverse();
                return path;
            }

            // 遍历四个方向（上下左右）
            foreach (Vector2Int dir in new Vector2Int[] {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                int nx = c.tileX + dir.x;
                int ny = c.tileY + dir.y;

                if (!IsInMapRange(nx, ny)) continue;
                if (closed[nx, ny]) continue;

                int terrainCost = map[nx, ny] == 0 ? 10 : 1; // 海洋代价高
                int gCost = current.gCost + terrainCost;
                int hCost = Heuristic(new Coord(nx, ny), end);

                CoordWithCost neighbor = new CoordWithCost(new Coord(nx, ny), gCost, hCost, current);
                open.Enqueue(neighbor, neighbor.fCost);
            }
        }

        // 如果没有路径
        return new List<Coord>();
    }



}