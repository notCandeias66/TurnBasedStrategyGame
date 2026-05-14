using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Globalization;

public class generateExteriorTerrain : MonoBehaviour
{
    private Terrain terrain;

    public int width = 256;
    public int height = 256;

    public float scale = 2.0f;

    public float offsetX = 100.0f;
    public float offsetY = 100.0f;
    
    private Vector2 circleCenter;
    public float circleRadius = 20.0f;
    public float circleHeight = 0f;
    public float softness = 100.0f;

    public TextAsset environmentXML;
    private int depth = 0;

    public GameObject[] treeObjects;
    public GameObject[] rockObjects;
    public GameObject[] houseObjects;
    public Texture2D[] textures;

    public LayerMask layer;

    private string tipo = "village";

    void Start()
    {
        tipo = Loaderv4.attackedTileString;

        terrain = GetComponent<Terrain>();

        offsetX = Random.Range(0.0f, 9999.0f);
        offsetY = Random.Range(0.0f, 9999.0f);

        Vector2 terrainCenter = new Vector2(width / 2, height / 2);
        circleCenter = terrainCenter;

        TerrainLayer[] terrainLayers  = new TerrainLayer[1];
        TerrainLayer myTerrainLayer = new TerrainLayer();

        if (tipo == "forest")
            myTerrainLayer.diffuseTexture = textures[0];
        else if (tipo == "plain")
            myTerrainLayer.diffuseTexture = textures[1];
        else if (tipo == "desert")
            myTerrainLayer.diffuseTexture = textures[2];
        else if (tipo == "mountain")
            myTerrainLayer.diffuseTexture = textures[3];
        else if (tipo == "village")
            myTerrainLayer.diffuseTexture = textures[4];

        terrainLayers[0] = myTerrainLayer;

        terrain.terrainData.terrainLayers = terrainLayers;

        readTerrainFromXml(tipo);
    }

    void Update()
    {
        //terrain = GetComponent<Terrain>();
        //terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    void readTerrainFromXml(string terrainType)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(environmentXML.text);
        XmlNodeList squareNodes = xmlDoc.SelectNodes("environment/square");
        foreach (XmlNode squareNode in squareNodes)
        {
            string type = squareNode.Attributes["type"].Value;
            if (type == terrainType)
            {
                depth = int.Parse(squareNode.Attributes["maximum_elevation"].Value, CultureInfo.InvariantCulture);
                terrain.terrainData = GenerateTerrain(terrain.terrainData);
                XmlNodeList objectNodes = squareNode.ChildNodes;
                foreach (XmlNode objectNode in objectNodes)
                {
                    string objectType = objectNode.Attributes["type"].Value;
                    float densityLowAltitude = float.Parse(objectNode.Attributes["density_low_altitute"].Value, CultureInfo.InvariantCulture);
                    float densityHighAltitude = float.Parse(objectNode.Attributes["density_high_altitute"].Value, CultureInfo.InvariantCulture);
                    spawnObjectsToTerrain(objectType, densityLowAltitude, densityHighAltitude);
                }
            }
            
        }
    }

    void spawnObjectsToTerrain (string objectType, float densityLowAltitude, float densityHighAltitude)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float pointHeight = terrain.terrainData.GetHeight(x, y);
                float distanceToCenter = Vector2.Distance(new Vector2(x, y), circleCenter);
                float probabilityToSpawn = Random.value;
                float densityDivider = 100.0f;

                if (distanceToCenter > circleRadius)
                {
                    GameObject toBeAdded = null;
                    float radius = 0.0f;

                    switch (objectType)
                    {
                        case "tree":
                            int randomPick1 = Random.Range(0, treeObjects.Length);
                            toBeAdded = treeObjects[randomPick1];
                            radius = 2.0f;
                            break;
                        case "rock":
                            int randomPick2 = Random.Range(0, rockObjects.Length);
                            toBeAdded = rockObjects[randomPick2];
                            radius = 2.0f;
                            break;
                        case "house":
                            int randomPick3 = Random.Range(0, houseObjects.Length);
                            toBeAdded = houseObjects[randomPick3];
                            radius = 5.0f;
                            break;
                    }

                    Vector3 spawnPosition = new Vector3(x, pointHeight, y);

                    if (pointHeight >= depth * 0.8)
                    {
                        if (probabilityToSpawn <= densityHighAltitude / densityDivider)
                        {
                            if (!Physics.CheckSphere(spawnPosition, radius, layer))
                            {
                                Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                                Instantiate(toBeAdded, new Vector3(x, pointHeight, y), randomRotation);
                            }
                        }
                    }
                    else if (pointHeight <= depth * 0.2)
                    {
                        if (probabilityToSpawn <= densityLowAltitude / densityDivider)
                        {
                            if (!Physics.CheckSphere(spawnPosition, radius, layer))
                            {
                                Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                                Instantiate(toBeAdded, new Vector3(x, pointHeight, y), randomRotation);
                            }
                        }
                    }
                }
            }
        }
    }

    TerrainData GenerateTerrain (TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;

        terrainData.size = new Vector3(width, depth, height);

        terrainData.SetHeights(0, 0, GenerateHeights());
        return terrainData;
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = CalculateHeight(x, y);
            }
        }

        return heights;
    }

    float CalculateHeight (int x, int y)
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale + offsetY;

        float distanceToCenter = Vector2.Distance(new Vector2(x, y), circleCenter);

        float blendFactor = Mathf.Clamp01((distanceToCenter - circleRadius) / softness);

        float perlinNoiseHeight = Mathf.PerlinNoise(xCoord, yCoord);
        float blendedHeight = Mathf.Lerp(circleHeight, perlinNoiseHeight, blendFactor);

        return blendedHeight;
    }
}
