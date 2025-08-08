using UnityEngine;

namespace LegendsOfTianming.Core
{
    public class TerrainGenerator : MonoBehaviour
    {
        [Header("Terrain Settings")]
        public int terrainWidth = 100;
        public int terrainHeight = 100;
        public float terrainScale = 20f;
        public float heightMultiplier = 5f;
        
        [Header("Environment Objects")]
        public GameObject[] treePrefabs;
        public GameObject[] rockPrefabs;
        public GameObject[] grassPrefabs;
        public int treeCount = 50;
        public int rockCount = 30;
        public int grassCount = 100;
        
        private Terrain terrain;
        private TerrainData terrainData;

        private void Start()
        {
            GenerateTerrain();
            PlaceEnvironmentObjects();
        }

        private void GenerateTerrain()
        {
            GameObject terrainObject = Terrain.CreateTerrainGameObject(null);
            terrain = terrainObject.GetComponent<Terrain>();
            terrainData = terrain.terrainData;
            
            terrainData.heightmapResolution = terrainWidth + 1;
            terrainData.size = new Vector3(terrainWidth, heightMultiplier, terrainHeight);
            
            float[,] heights = GenerateHeights();
            terrainData.SetHeights(0, 0, heights);
            
            CreateTerrainTexture();
            
            Debug.Log("🌍 Generated terrain for Qingze Plains");
        }

        private float[,] GenerateHeights()
        {
            float[,] heights = new float[terrainWidth + 1, terrainHeight + 1];
            
            for (int x = 0; x <= terrainWidth; x++)
            {
                for (int y = 0; y <= terrainHeight; y++)
                {
                    float xCoord = (float)x / terrainWidth * terrainScale;
                    float yCoord = (float)y / terrainHeight * terrainScale;
                    
                    heights[x, y] = Mathf.PerlinNoise(xCoord, yCoord) * 0.1f;
                }
            }
            
            return heights;
        }

        private void CreateTerrainTexture()
        {
            Texture2D grassTexture = CreateSolidColorTexture(Color.green, 512, 512);
            
            TerrainLayer[] terrainLayers = new TerrainLayer[1];
            terrainLayers[0] = new TerrainLayer();
            terrainLayers[0].diffuseTexture = grassTexture;
            terrainLayers[0].tileSize = new Vector2(15, 15);
            
            terrainData.terrainLayers = terrainLayers;
        }

        private Texture2D CreateSolidColorTexture(Color color, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];
            
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            return texture;
        }

        private void PlaceEnvironmentObjects()
        {
            PlaceTrees();
            PlaceRocks();
            PlaceGrass();
        }

        private void PlaceTrees()
        {
            for (int i = 0; i < treeCount; i++)
            {
                Vector3 position = GetRandomTerrainPosition();
                position.y = GetTerrainHeight(position) + 0.5f;
                
                GameObject tree = CreatePlaceholderTree();
                tree.transform.position = position;
                tree.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                tree.transform.SetParent(transform);
            }
        }

        private void PlaceRocks()
        {
            for (int i = 0; i < rockCount; i++)
            {
                Vector3 position = GetRandomTerrainPosition();
                position.y = GetTerrainHeight(position);
                
                GameObject rock = CreatePlaceholderRock();
                rock.transform.position = position;
                rock.transform.rotation = Quaternion.Euler(Random.Range(-10, 10), Random.Range(0, 360), Random.Range(-10, 10));
                rock.transform.SetParent(transform);
            }
        }

        private void PlaceGrass()
        {
            for (int i = 0; i < grassCount; i++)
            {
                Vector3 position = GetRandomTerrainPosition();
                position.y = GetTerrainHeight(position);
                
                GameObject grass = CreatePlaceholderGrass();
                grass.transform.position = position;
                grass.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                grass.transform.SetParent(transform);
            }
        }

        private GameObject CreatePlaceholderTree()
        {
            GameObject tree = new GameObject("Tree");
            
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.SetParent(tree.transform);
            trunk.transform.localPosition = new Vector3(0, 2f, 0);
            trunk.transform.localScale = new Vector3(0.5f, 2f, 0.5f);
            
            Renderer trunkRenderer = trunk.GetComponent<Renderer>();
            if (trunkRenderer != null)
            {
                Material trunkMaterial = new Material(Shader.Find("Standard"));
                trunkMaterial.color = new Color(0.4f, 0.2f, 0.1f);
                trunkRenderer.material = trunkMaterial;
            }
            
            GameObject leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaves.transform.SetParent(tree.transform);
            leaves.transform.localPosition = new Vector3(0, 4f, 0);
            leaves.transform.localScale = new Vector3(3f, 2f, 3f);
            
            Renderer leavesRenderer = leaves.GetComponent<Renderer>();
            if (leavesRenderer != null)
            {
                Material leavesMaterial = new Material(Shader.Find("Standard"));
                leavesMaterial.color = new Color(0.2f, 0.6f, 0.2f);
                leavesRenderer.material = leavesMaterial;
            }
            
            return tree;
        }

        private GameObject CreatePlaceholderRock()
        {
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rock.name = "Rock";
            rock.transform.localScale = new Vector3(
                Random.Range(1f, 3f),
                Random.Range(0.5f, 2f),
                Random.Range(1f, 3f)
            );
            
            Renderer renderer = rock.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material rockMaterial = new Material(Shader.Find("Standard"));
                rockMaterial.color = new Color(0.5f, 0.5f, 0.5f);
                renderer.material = rockMaterial;
            }
            
            return rock;
        }

        private GameObject CreatePlaceholderGrass()
        {
            GameObject grass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grass.name = "Grass";
            grass.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);
            
            Renderer renderer = grass.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material grassMaterial = new Material(Shader.Find("Standard"));
                grassMaterial.color = new Color(0.3f, 0.7f, 0.3f);
                renderer.material = grassMaterial;
            }
            
            return grass;
        }

        private Vector3 GetRandomTerrainPosition()
        {
            float x = Random.Range(0, terrainWidth);
            float z = Random.Range(0, terrainHeight);
            return new Vector3(x, 0, z);
        }

        private float GetTerrainHeight(Vector3 position)
        {
            if (terrain != null)
            {
                return terrain.SampleHeight(position);
            }
            return 0f;
        }
    }
}
