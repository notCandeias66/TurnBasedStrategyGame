using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using System.Xml;
using System;

using UnityEngine.UI;
using System.IO;
using TMPro;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using static UnityEngine.UI.CanvasScaler;
using System.Buffers;
using System.Linq;
//using static CharacterSkinController;
using System.Threading.Tasks;
using static UnityEngine.ParticleSystem;
using UnityEngine.EventSystems;

public class Loaderv4 : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextAsset xmlFile;

    private XmlDocument xmlDoc;
    private XmlElement root;

    private XmlNodeList roles;
    public string player1, player2;

    [SerializeField] public GameObject square;

    // Terrain specific variables
    public float Width = 0.4f;
    public float Height = 0.4f;
    private XmlElement boardElement;
    public int xmlWidth, xmlHeight;
    [SerializeField] private Tile plainTile, florestTile, villageTile, waterTile, mountainTile, desertTile;
    private Dictionary<Vector2, Tile> tilePlacements;

    // Turn specific variables
    private int currentTurnIndex = -1;
    XmlNodeList turnList;
    private Dictionary<int, Dictionary<Vector2, List<BaseUnit>>> turnUnitPlacements;

    bool isRunning = false;

    // Unit specific variables
    [SerializeField] private BaseUnit archer1, archer2, soldier1, soldier2, catapult1, catapult2, mage1, mage2;
    private XmlNodeList unitList;

    // Throwable specific variables
    [SerializeField] private Arrow arrow;
    [SerializeField] private CannonBall cannonBall;

    // UnitClicker specific variables
    private List<Vector2> pastPositions = new List<Vector2>();
    private LineRenderer lineRenderer;

    public bool battleView = false;

    public static string attackedTileString = "";

    public static GameObject invisible = null;

    public static bool inBattle = false;

    public TextMeshProUGUI text;

    public TextMeshProUGUI isGameOver;

    //[SerializeField] public SceneSwitcher sceneswitcher;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] mainObject = GameObject.FindGameObjectsWithTag("mainObject");

        foreach (GameObject g in mainObject)
        {
            invisible = g;
        }

        LoadXmlDocument();

        SetupGlobalVariables();

        Quaternion squareRotation = square.transform.rotation;
        Vector3 squarePosition = square.transform.position;
        Vector3 squareScale = transform.localScale;

        GenerateTerrain(squareRotation, squarePosition, squareScale);
    }

    private void LoadXmlDocument()
    {
        if (xmlFile != null)
        {
            xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlFile.text);
        }
        else
        {
            Debug.LogError("No XML file selected.");
        }
    }

    private void SetupGlobalVariables()
    {
        root = xmlDoc.DocumentElement;
        if (root == null)
        {
            Debug.LogError("Root element is null. Aborting game generation.");
            return;
        }

        boardElement = root.SelectSingleNode("board") as XmlElement;
        if (boardElement == null)
        {
            Debug.LogError("Board element not found. Aborting board generation.");
            return;
        }

        roles = root.SelectNodes("roles/role");
        player1 = roles[0].Attributes["name"].Value;
        player2 = roles[1].Attributes["name"].Value;

        xmlWidth = int.Parse(boardElement.GetAttribute("width"));
        xmlHeight = int.Parse(boardElement.GetAttribute("height"));

        tilePlacements = new Dictionary<Vector2, Tile>();

        turnUnitPlacements = new Dictionary<int, Dictionary<Vector2, List<BaseUnit>>>();

        turnList = root.SelectNodes("turns/turn");
        if (turnList == null)
        {
            Debug.Log("Turn elements not founded. Aborting turn generation.");
            return;
        }
    }

    public void GenerateTerrain(Quaternion q, Vector3 v, Vector3 s)
    {
        XmlNodeList boardTiles = boardElement.ChildNodes;

        float tilesInWidth = (float)(Width / xmlWidth) * s.x;
        float tilesInHeight = (float)(Height / xmlHeight) * s.y;

        int tileCounter = 0;
        int xIndex = 0;

        for (int y = 1; y <= xmlHeight; y++)
        {
            int reversedY = xmlHeight - y + 1;

            for (int x = 1; x <= xmlWidth; x++)
            {
                XmlNode tile = boardTiles[tileCounter];
                string tileType = tile.Name;

                Tile tileName = GetTileType(tileType);
                if (tileName == null)
                {
                    Debug.LogError("Unknown tile type: " + tileType);
                    return;
                }

                Vector3 positionRelativeToSpawn = q * new Vector3((x - 0.5f) * tilesInWidth - (Width / 2f) * s.x, (reversedY - 0.5f) * tilesInHeight - (Height / 2f) * s.y, 0);
                Vector3 finalPosition = v + positionRelativeToSpawn;

                var spawnedTile = Instantiate(tileName, finalPosition, q);

                //spawnedTile.transform.parent = square.transform.parent;

                spawnedTile.name = $"Tile {x} {y}";
                spawnedTile.transform.localScale = new Vector3(tilesInWidth, tilesInHeight, 1f);

                spawnedTile.tag = "Tile";

                //spawnedTile.Init(x, reversedY);

                tilePlacements[new Vector2(x, y)] = spawnedTile;

                tileCounter++;

                xIndex++;
                if (xIndex >= xmlWidth)
                {
                    xIndex = 0;

                }
            }
        }
    }

    public void StartTurn()
    {
        if (!isRunning)
        {
            isRunning = true;
            InvokeRepeating("NextTurn", 1, 5);

            Debug.Log("Game started or resumed!");
        }
    }

    public void PauseTurn()
    {
        if (isRunning)
        {
            isRunning = false;
            CancelInvoke("NextTurn");

            if (currentTurnIndex == turnList.Count - 1)
            {
                Debug.Log("End of the game reached. Last turn is " + (currentTurnIndex + 1));
                return;

            }
            else
            {
                Debug.Log("Game paused on turn " + (currentTurnIndex + 1));
                return;
            }
        }
    }

    public void RestartGame()
    {
        if(currentTurnIndex >= 0)
        {
            Dictionary<Vector2,List<BaseUnit>> unitsPerPosition = turnUnitPlacements[currentTurnIndex];

            List<BaseUnit> listaDasUnidades = new ();

            foreach (var position in unitsPerPosition)
            {
                foreach (BaseUnit unit in position.Value)
                {
                    listaDasUnidades.Add(unit);
                }
            }

            // Destroy all found game objects
            foreach (BaseUnit unit in listaDasUnidades)
            {
                Destroy(unit.gameObject);
            }
        }

        turnUnitPlacements.Clear();
        currentTurnIndex = -1;
    }

    public void NextTurn()
    {
        // While we are not in the last turn
        if (currentTurnIndex < turnList.Count - 1 && turnList != null)
        {
            // Need this to be able to delete dead units since they are still there but invisible (only needed when we are atlest in the fisrt turn)
            if (currentTurnIndex >= 0)
            {
                Dictionary<Vector2,List<BaseUnit>> unitsPerPosition = turnUnitPlacements[currentTurnIndex];

                List<BaseUnit> listaDasUnidades = new ();

                foreach (var position in unitsPerPosition)
                {
                    foreach (BaseUnit unit in position.Value)
                    {
                        listaDasUnidades.Add(unit);
                    }
                }

                // Destroy all found game objects
                foreach (BaseUnit unit in listaDasUnidades)
                {
                    Destroy(unit.gameObject);
                }
            }

            currentTurnIndex++;

            isGameOver.text = "Playing";
            text.text = currentTurnIndex.ToString();

            // Only need to care about dead units from the second turn onwards, as it doesn't make sense to have dead units at the beginning of the game
            if (currentTurnIndex > 0)
            {
                // Building the dead units but setting then inactive
                foreach (var position in turnUnitPlacements[currentTurnIndex - 1])
                {
                    foreach (BaseUnit unit in position.Value)
                    {
                        if (unit.unitStatus == UnitStatus.Died)
                        {
                            SpawnUnit(unit.unitId, unit.unitRole, unit.unitType, (int)position.Key.x, (int)position.Key.y);

                            foreach (BaseUnit unitNow in turnUnitPlacements[currentTurnIndex][position.Key])
                            {
                                if (unitNow.unitId == unit.unitId)
                                {
                                    unitNow.gameObject.SetActive(false);
                                    unitNow.unitStatus = UnitStatus.Dead;
                                    unitNow.unitTurnAction = UnitTurnAction.Holded;
                                }
                            }
                        }
                    }
                }
            }

            ExecuteCurrentTurn(square.transform.rotation, square.transform.position);

        } else {

            if (isRunning == false)
            {
                Debug.Log("You are in the most recent turn. Can't go forward");
                isGameOver.text = "GAME OVER";
                return;

            } else {
                PauseTurn();
            }
        }
    }

    public void SwitchBattleCheck()
    {
        if (battleView)
        {
            battleView = false;
        
        } else {
            battleView = true;
        }
    }

    private void SpawnGhosts()
    {
        if (currentTurnIndex > 0)
        {
            foreach (var positionBefore in turnUnitPlacements[currentTurnIndex - 1])
            {
                foreach (BaseUnit unit in positionBefore.Value)
                {
                    Vector2? currentPos = GetCurrentPosition(unit.unitId);

                    BaseUnit unitNow = GetUnitById(unit.unitId);

                    if (unit.unitId == unit.unitId)
                    {
                        if (unitNow.unitStatus == UnitStatus.Alive && currentPos != positionBefore.Key)
                        {
                            BaseUnit unitName = GetUnitType(unit.unitRole, unit.unitType);
                            if (unitName == null)
                            {
                                Debug.LogError("Unknown unit role: " + unit.unitRole + " or unknown unit type: " + unit.unitType);
                                return;
                            }

                            var ghostUnit = Instantiate(unitName);
                            ghostUnit.unitId = 1000 + unit.unitId;
                            ghostUnit.name = $"GhostUnit {unit.unitId + 1000} ({positionBefore.Key.x} , {positionBefore.Key.y})";
                            ghostUnit.tag = "BaseUnit";

                            float tileCenterX = ((int)positionBefore.Key.x - (xmlWidth / 2.0f) - 0.5f) * (Width / (float)xmlWidth) * transform.localScale.x;
                            float tileCenterY = (xmlHeight / 2.0f - (int)positionBefore.Key.y + 0.5f) * (Height / (float)xmlHeight) * transform.localScale.y;

                            float bottomOffset = unitNow.gameObject.GetComponent<MeshRenderer>().bounds.extents.y;

                            Quaternion rotation = square.transform.rotation * Quaternion.Euler(-90f, -90f, 90f);
                            Vector3 position = square.transform.position + square.transform.rotation * new Vector3(tileCenterX, tileCenterY, -bottomOffset);

                            ghostUnit.transform.position = position;
                            ghostUnit.transform.rotation = rotation;

                            float minTileSize = Mathf.Min(Width / xmlWidth, Height / xmlHeight) * 0.5f;
                            Vector3 tileSize = new Vector3(minTileSize * 0.7f, minTileSize * 0.7f, minTileSize * 0.7f);
                            Vector3 unitScale = new Vector3(tileSize.x / ghostUnit.transform.localScale.x * transform.localScale.x, tileSize.y / ghostUnit.transform.localScale.y * transform.localScale.y, tileSize.z / ghostUnit.transform.localScale.z * transform.localScale.z);

                            // Apply the calculated scale to the unit
                            ghostUnit.transform.localScale = unitScale;

                            SetUnitToGhost(ghostUnit, 0.5f);

                            ghostUnit.transform.parent = square.transform.parent;

                            AddUnitToTurn(positionBefore.Key, ghostUnit);
                        }
                    }
                }
            }
        }
    }

    // ---------------------------------------------------LUCAS----------------------------------------------------------

    public void PreviousTurn()
    {
        // If we are trying to go back even after having the first turn done
        if (currentTurnIndex < 0)
        {
            Debug.Log("You didn't even started the first turn!!!");
            return;
        }

        // When we are in the turn that is not the first one
        if (currentTurnIndex > 0 && turnList != null)
        {
            // Need this to be able to delete dead units (they are still there but invisible)
            Dictionary<Vector2,List<BaseUnit>> unitsPerPosition = turnUnitPlacements[currentTurnIndex];

            List<BaseUnit> listaDasUnidades = new ();

            foreach (var position in unitsPerPosition)
            {
                foreach (BaseUnit unit in position.Value)
                {
                    listaDasUnidades.Add(unit);
                }
            }

            // Destroy all found game objects
            foreach (BaseUnit unit in listaDasUnidades)
            {
                Destroy(unit.gameObject);
            }

            // Remove all units from turnUnitPlacements in current turn and in the previous turn
            turnUnitPlacements[currentTurnIndex].Clear();
            turnUnitPlacements[currentTurnIndex - 1].Clear();

            currentTurnIndex--;

            text.text = currentTurnIndex.ToString();
            isGameOver.text = "Playing";

            ExecuteCurrentTurn(square.transform.rotation, square.transform.position);
        } else { // if we are on the fisrt turn
            Debug.Log("You are in the first turn. Can't go backwards");
            return;
        }
    }

    public void ExecuteCurrentTurn(Quaternion q, Vector3 v)
    {
        XmlNode currentTurnNode = turnList[currentTurnIndex];
        unitList = currentTurnNode.SelectNodes("unit");
        if (unitList == null)
        {
            Debug.Log("Unit elements not founded. Aborting unit generation");
        }

        int priority = 0;

        while (priority < 2)
        {
        foreach (XmlNode unitNode in unitList)
        {
            int unitId = int.Parse(unitNode.Attributes["id"].Value);
            string unitRole = unitNode.Attributes["role"].Value;
            string unitType = unitNode.Attributes["type"].Value;
            string unitAction = unitNode.Attributes["action"].Value;
            int unitX = int.Parse(unitNode.Attributes["x"].Value);
            int unitY = int.Parse(unitNode.Attributes["y"].Value);

            switch (unitAction)
            {
                case "spawn":
                        if (priority == 0) SpawnUnit(unitId, unitRole, unitType, unitX, unitY);
                    break;
                case "hold":
                        if (priority == 0) HoldFunction(unitId, unitX, unitY);
                    break;
                case "move_to":
                        if (priority == 0) MoveUnit(unitId, unitX, unitY, q, v);
                    break;
                case "attack":
                        if (priority == 1) AttackFunction(unitId, unitRole, unitType, unitX, unitY);
                    break;
                default:
                    Debug.Log("Unknown unit action: " + unitAction);
                    break;
            }
            }

            priority++;
        }

        // Spawning the ghosts
        SpawnGhosts();

        // Spawning multiple units in the same position (we will consider that in the same tile can only 4 units max, alive or ghosts)
        SpawnMultipleUnitsInSamePosition();


        // Setting up the movement annimation for the units that have moved using their ghosts
        if (currentTurnIndex > 0)
        {
            foreach (var position in turnUnitPlacements[currentTurnIndex])
            {
                foreach (BaseUnit unit in position.Value)
                {
                    if (unit.unitStatus == UnitStatus.Alive && unit.unitTurnAction == UnitTurnAction.Moved)
                    {
                        int idToGet = unit.unitId + 1000;

                        BaseUnit ghostUnit = GetGhostById(idToGet);

                        Vector3 startPos = ghostUnit.transform.position;
                        Vector3 endPos = unit.transform.position;

                        // Start the movement animation
                        StartCoroutine(MoveUnitOverTime(ghostUnit, startPos, endPos, 1.0f));
                    }
                }
            }
        }
    }

    private void SpawnUnit(int unitId, string unitRole, string unitType, int unitX, int unitY)
    {
        BaseUnit unitName = GetUnitType(unitRole, unitType);
        if (unitName == null)
        {
            Debug.LogError("Unknown unit role: " + unitRole + " or unknown unit type: " + unitType);
            return;
        }

        // Calculate the position of the unit relative to the table
        float tileCenterX = (unitX - (xmlWidth / 2.0f) - 0.5f) * (Width / (float)xmlWidth) * transform.localScale.x;
        float tileCenterY = (xmlHeight / 2.0f - unitY + 0.5f) * (Height / (float)xmlHeight) * transform.localScale.y;

        // Use the square's rotation and position to adjust the unit's position and rotation
        Quaternion rotation = square.transform.rotation * Quaternion.Euler(-90f, -90f, 90f);
        Vector3 position = square.transform.position + square.transform.rotation * new Vector3(tileCenterX, tileCenterY, 0);

        // Spawn units in the center of the tile in an inicial phase (it is ajusted later)
        var spawnedUnit = Instantiate(unitName, position, rotation);
        spawnedUnit.name = $"Unit {unitId} ({unitX} , {unitY})";
        spawnedUnit.tag = "BaseUnit";
        spawnedUnit.unitId = unitId;
        spawnedUnit.unitRole = unitRole;
        spawnedUnit.unitType = unitType;
        spawnedUnit.unitStatus = UnitStatus.Alive;
        spawnedUnit.unitTurnAction = UnitTurnAction.Spawned;

        // Calculate the scale of the unit based on the smaller size of the tile and the model size of the unit
        float minTileSize = Mathf.Min(Width / xmlWidth, Height / xmlHeight) * 0.5f;
        Vector3 tileSize = new Vector3(minTileSize * 0.7f, minTileSize * 0.7f, minTileSize * 0.7f);
        Vector3 unitScale = new Vector3(tileSize.x / spawnedUnit.transform.localScale.x * transform.localScale.x, tileSize.y / spawnedUnit.transform.localScale.y * transform.localScale.y, tileSize.z / spawnedUnit.transform.localScale.z * transform.localScale.z);

        // Apply the calculated scale to the unit
        spawnedUnit.transform.localScale = unitScale;

        // Add the spawned unit to the dictionary for tracking
        Vector2 whereToAdd = new Vector2(unitX, unitY);

        spawnedUnit.transform.parent = square.transform.parent;


        AddUnitToTurn(whereToAdd, spawnedUnit);
    }

    private void MoveUnit(int unitId, int unitX, int unitY, Quaternion q, Vector3 v)
    {
        if (currentTurnIndex > 0)
        {
            foreach (var position in turnUnitPlacements[currentTurnIndex - 1])
            {
                foreach (BaseUnit unit in position.Value)
                {
                    if (unit.unitId == unitId)
                    {
                        string unitRole = unit.unitRole;
                        string unitType = unit.unitType;

                        SpawnUnit(unitId, unitRole, unitType, unitX, unitY);

                        BaseUnit unitNow = GetUnitById(unitId);
                        unitNow.unitTurnAction = UnitTurnAction.Moved;
                    }
                }
            }
        }
    }

    public void HoldFunction(int unitId, int unitX, int unitY)
    {
        if (currentTurnIndex > 0)
        {
            foreach (var position in turnUnitPlacements[currentTurnIndex - 1])
            {
                foreach (BaseUnit unit in position.Value)
                {
                    if (unit.unitId == unitId)
                    {
                        string unitRole = unit.unitRole;
                        string unitType = unit.unitType;

                        SpawnUnit(unitId, unitRole, unitType, unitX, unitY);

                        BaseUnit unitNow = GetUnitById(unitId);

                        unitNow.unitTurnAction = UnitTurnAction.Holded;
                    }
                }
            }
        }
    }

    public void AttackFunction(int unitId, string unitRole, string unitType, int unitX, int unitY)
    {
        Vector2 whereToAttack = new Vector2(unitX, unitY);

        BaseUnit attackingUnit = null;

        // Only care about units attacking starting at the second turn, beacuse we need to spawn units first
        if (currentTurnIndex > 0)
        {
            // Spawning the attacking unit
            foreach (var position in turnUnitPlacements[currentTurnIndex - 1])
            {
                foreach (BaseUnit unit in position.Value)
                {
                    if (unit.unitId == unitId)
                    {
                        int x = (int)position.Key.x;
                        int y = (int)position.Key.y;

                        SpawnUnit(unitId, unitRole, unitType, x, y);

                        BaseUnit unitNow = GetUnitById(unitId);
                        unitNow.unitTurnAction = UnitTurnAction.Attacked;

                        attackingUnit = unitNow;
                    }
                }
            }

            foreach (var position in turnUnitPlacements[currentTurnIndex])
            {
                if (position.Key == whereToAttack)
                {
                    foreach (BaseUnit unit in position.Value)
                    {
                        if (unit.unitRole != unitRole)
                        {
                            unit.unitStatus = UnitStatus.Died;
                            
                            // If the attacking unit is an archer
                            if (attackingUnit.unitType == "archer") 
                            {
                                Vector3 arrowSize = new Vector3(0.01f, 0.01f, 0.01f); // Vector to adjust the size of the arrow 
                                LaunchArrow(attackingUnit.transform.position, unit.transform.position, arrowSize); // Function to launch the arrow
                            }

                            // If the attacking unit is a catapult
                            if (attackingUnit.unitType == "catapult")
                            {
                                Vector3 cannonBallSize = new Vector3(0.03f, 0.03f, 0.03f); // Vector to adjust the size of the cannon ball 
                                LaunchCannonBall(attackingUnit.transform.position, unit.transform.position, cannonBallSize); // Function to launch the cannon ball
                            }

                            // If the battle check is true, it means the the respective unit battles will be shown 
                            if (battleView)
                            {
                                inBattle = true;

                                // We need to check if the both attacking and attacked units are soldiers from diferente players
                                if ((attackingUnit.unitType == "soldier" && unit.unitType == "soldier"))
                                {
                                    Tile attackedTile = GetTileAtPosition(whereToAttack);
                                    attackedTileString = attackedTile.tileNameString;

                                    GameObject[] extraObjects = GameObject.FindGameObjectsWithTag("extras");

                                    foreach (GameObject g in extraObjects)
                                    {
                                        Destroy(g);
                                    }

                                    invisible.SetActive(false);
                                    SceneManager.LoadScene("Cutscene", LoadSceneMode.Additive);
                                }
                            }

                            if (!inBattle)
                            {
                                StartCoroutine(UnitDeath(unit, 2f));
                            }
                        }
                    }
                }
            }
        }
    }

    void LaunchCannonBall(Vector3 startPosition, Vector3 targetPosition, Vector3 cannonBallSize)
    {
        CannonBall cannonBallInstance = Instantiate(cannonBall, startPosition, Quaternion.identity); // Instanciate the cannon ball using the specific prefab
        CannonBall cannonBallScript = cannonBallInstance.GetComponent<CannonBall>(); // Get the script of the created prefab
        cannonBallScript.targetPosition = targetPosition; // Set the target position for the cannon ball

        cannonBallInstance.transform.localScale = cannonBallSize; // Adjust the cannon ball scale
    }


    void LaunchArrow(Vector3 startPosition, Vector3 targetPosition, Vector3 arrowSize)
    {
        Arrow arrowInstance = Instantiate(arrow, startPosition, Quaternion.identity); // Instanciate the arrow using the specific prefab
        Arrow arrowScript = arrowInstance.GetComponent<Arrow>(); // Get the script of the created prefab
        arrowScript.targetPosition = targetPosition; // Set the target position for the arrow

        arrowInstance.transform.localScale = arrowSize; // Adjust the arrow scale
    }

    public void SetUnitToGhost(BaseUnit unit, float opacity)
    {
        unit.unitStatus = UnitStatus.Ghost;

        Renderer renderer = unit.GetComponent<Renderer>();
        if (renderer != null)
        {
            foreach (Material material in renderer.materials)
            {
                SetMaterialTransparent(material);
                Color color = material.color;
                color.a = opacity;
                material.color = color;
            }
        }
    }

    // Function used to set the material transparent (to have less opacity)
    public void SetMaterialTransparent(Material material)
    {
        if (material != null)
        {
            material.SetFloat("_Mode", 3);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }
    }

    private void SpawnMultipleUnitsInSamePosition()
    {
        foreach (var position in turnUnitPlacements[currentTurnIndex])
        {
            float tilesInWidth = Width / (float)xmlWidth;
            float tilesInHeight = Height / (float)xmlHeight;

            // Calculating the centre position of the tile 
            float tileCenterX = (position.Key.x - (xmlWidth / 2.0f) - 0.5f) * (Width / (float)xmlWidth) * transform.localScale.x;
            float tileCenterY = (xmlHeight / 2.0f - position.Key.y + 0.5f) * (Height / (float)xmlHeight) * transform.localScale.y;

            // Used to the bottem of the unit to be in the tile and not its center
            float bottomOffset = position.Value[0].gameObject.GetComponent<MeshRenderer>().bounds.extents.y;

            // We get the units that can be seen in that tile, sorted by the material name, that way we can get the units from the same player (same material) closer together
            List<BaseUnit> unitsInPos = turnUnitPlacements[currentTurnIndex][position.Key]
                .Where(unit => unit.unitStatus == UnitStatus.Alive)
                .OrderBy(unit => unit.gameObject.GetComponent<Renderer>().material.name)
                .ToList();

            // If there isn't any unit in the tile
            if (unitsInPos.Count == 0)
            {
                continue;
            }
            else if (unitsInPos.Count == 1) // If there is only one unit in the tile
            {
                float unit1PosX = tileCenterX;
                float unit1PosY = tileCenterY;
                Vector3 pos1 = square.transform.position + square.transform.rotation * new Vector3(unit1PosX, unit1PosY, -bottomOffset); // Center of the tile
                unitsInPos[0].transform.position = pos1;
            }
            else if (unitsInPos.Count == 2) // If there are two units in the tile
            {
                float unit1PosX = tileCenterX - tilesInWidth / 4;
                float unit1PosY = tileCenterY;
                Vector3 pos1 = square.transform.position + square.transform.rotation * new Vector3(unit1PosX, unit1PosY, -bottomOffset); // Center of the left half of the tile 
                unitsInPos[0].transform.position = pos1;

                float unit2PosX = tileCenterX + tilesInWidth / 4;
                float unit2PosY = tileCenterY;
                Vector3 pos2 = square.transform.position + square.transform.rotation * new Vector3(unit2PosX, unit2PosY, -bottomOffset); // Center of the right half of the tile
                unitsInPos[1].transform.position = pos2;
            }
            else if (unitsInPos.Count == 3) // If there are three units in the tile
            {
                float unit1PosX = tileCenterX - tilesInWidth / 4;
                float unit1PosY = tileCenterY;
                Vector3 pos1 = square.transform.position + square.transform.rotation * new Vector3(unit1PosX, unit1PosY, -bottomOffset); // Center of the left half of the tile
                unitsInPos[0].transform.position = pos1;

                float unit2PosX = tileCenterX;
                float unit2PosY = tileCenterY;
                Vector3 pos2 = square.transform.position + square.transform.rotation * new Vector3(unit2PosX, unit2PosY, -bottomOffset); // Center of the tile
                unitsInPos[1].transform.position = pos2;

                float unit3PosX = tileCenterX + tilesInWidth / 4;
                float unit3PosY = tileCenterY;
                Vector3 pos3 = square.transform.position + square.transform.rotation * new Vector3(unit3PosX, unit3PosY, -bottomOffset); // Center of the right half of the tile
                unitsInPos[2].transform.position = pos3;
            }
            else if (unitsInPos.Count == 4) // If there are four units in the tile
            {
                float unit1PosX = tileCenterX - tilesInWidth / 4;
                float unit1PosY = tileCenterY + tilesInHeight / 4;
                Vector3 pos1 = square.transform.position + square.transform.rotation * new Vector3(unit1PosX, unit1PosY, -bottomOffset); // Upper left of the tile
                unitsInPos[0].transform.position = pos1;

                float unit2PosX = tileCenterX + tilesInWidth / 4;
                float unit2PosY = tileCenterY + tilesInHeight / 4;
                Vector3 pos2 = square.transform.position + square.transform.rotation * new Vector3(unit2PosX, unit2PosY, -bottomOffset); // Upper right of the tile
                unitsInPos[1].transform.position = pos2;

                float unit3PosX = tileCenterX - tilesInWidth / 4;
                float unit3PosY = tileCenterY - tilesInHeight / 4;
                Vector3 pos3 = square.transform.position + square.transform.rotation * new Vector3(unit3PosX, unit3PosY, -bottomOffset); // Lower left of the tile
                unitsInPos[2].transform.position = pos3;

                float unit4PosX = tileCenterX + tilesInWidth / 4;
                float unit4PosY = tileCenterY - tilesInHeight / 4;
                Vector3 pos4 = square.transform.position + square.transform.rotation * new Vector3(unit4PosX, unit4PosY, -bottomOffset); // Lower right of the tile
                unitsInPos[3].transform.position = pos4;
            }
        }
    }

    // ---------------------------------------------------Afonso----------------------------------------------------------

    private Tile GetTileType(string tiletype)
    {
        switch (tiletype)
        {
            case "forest":
                return florestTile;
            case "plain":
                return plainTile;
            case "village":
                return villageTile;
            case "sea":
                return waterTile;
            case "mountain":
                return mountainTile;
            case "desert":
                return desertTile;
            default:
                return null;
        }
    }

    private BaseUnit GetUnitType(string unitRole, string unitType)
    {
        switch (unitType)
        {
            case "soldier":
                return unitRole == player1 ? soldier1 : soldier2;
            case "archer":
                return unitRole == player1 ? archer1 : archer2;
            case "catapult":
                return unitRole == player1 ? catapult1 : catapult2;
            case "mage":
                return unitRole == player1 ? mage1 : mage2;
            default:
                return null;
        }
    }

    public Tile GetTileAtPosition(Vector3 pos)
    {
        if (tilePlacements.TryGetValue(pos, out var tile))
        {
            return tile;
        }

        return null;
    }

    private Vector2? GetPreviousPosition(int unitId)
    {
        int previousTurnIndex = currentTurnIndex - 1;

        if (turnUnitPlacements.ContainsKey(previousTurnIndex))
        {
            Dictionary<Vector2, List<BaseUnit>> previousTurnUnits = turnUnitPlacements[previousTurnIndex];

            foreach (var position in previousTurnUnits.Keys)
            {
                List<BaseUnit> unitsAtPosition = previousTurnUnits[position];

                foreach (BaseUnit unit in unitsAtPosition)
                {
                    if (unit.unitId == unitId)
                    {
                        return position;
                    }
                }
            }
        }

        return null;
    }

    private Vector2? GetCurrentPosition(int unitId)
    {
        if (turnUnitPlacements.ContainsKey(currentTurnIndex))
        {
            Dictionary<Vector2, List<BaseUnit>> currentTurnUnits = turnUnitPlacements[currentTurnIndex];

            foreach (var position in currentTurnUnits.Keys)
            {
                List<BaseUnit> unitsAtPosition = currentTurnUnits[position];

                foreach (BaseUnit unit in unitsAtPosition)
                {
                    if (unit.unitId == unitId)
                    {
                        return position;
                    }
                }
            }
        }

        return null;
    }

    private BaseUnit GetUnitById(int unitId)
    {
        for (int turnIndex = currentTurnIndex; turnIndex >= 0; turnIndex--)
        {
            if (turnUnitPlacements.ContainsKey(turnIndex))
            {
                foreach (var position in turnUnitPlacements[turnIndex])
                {
                    foreach (BaseUnit unit in position.Value)
                    {
                        if (unit.unitId == unitId)
                        {
                            return unit;
                        }
                    }
                }
            }
        }

        return null;
    }

    public List<Vector2> GetUnitPastPositions(BaseUnit toGet)
    {
        List<Vector2> allPos = new List<Vector2>();

        for (int i = 0; i <= currentTurnIndex - 1; i++)
        {
            if (turnUnitPlacements.ContainsKey(i))
            {
                Vector2 pos;

                foreach (var position in turnUnitPlacements[i])
                {
                    foreach (BaseUnit unit in position.Value)
                    {
                        if (unit.unitId == toGet.unitId)
                        {
                            if (unit.unitStatus != UnitStatus.Dead)
                            {
                                pos = new Vector2((int)position.Key.x, (int)position.Key.y);
                                allPos.Add(pos);
                            }
                        }
                    }
                }
            }
        }

        return allPos;
    }

    public void AddUnitToTurn(Vector2 postion, BaseUnit unit)
    {
        if (!turnUnitPlacements.ContainsKey(currentTurnIndex))
        {
            turnUnitPlacements[currentTurnIndex] = new Dictionary<Vector2, List<BaseUnit>>();
        }

        if (!turnUnitPlacements[currentTurnIndex].ContainsKey(postion))
        {
            turnUnitPlacements[currentTurnIndex][postion] = new List<BaseUnit>();
        }

        turnUnitPlacements[currentTurnIndex][postion].Add(unit);
    }

    private BaseUnit GetGhostById(int idToGet)
    {    
        BaseUnit ghostUnit = null;

        foreach (var position in turnUnitPlacements[currentTurnIndex])
        {
            foreach (BaseUnit unit in position.Value)
            {
                if ((unit.unitId == idToGet) && unit.unitStatus == UnitStatus.Ghost)
                {
                    ghostUnit = unit;
                }
            }
        }

        return ghostUnit;
    }

    public IEnumerator MoveUnitOverTime(BaseUnit unit, Vector3 startPosition, Vector3 endPosition, float duration)
    {
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            unit.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        unit.transform.position = endPosition;

        unit.gameObject.SetActive(false);
    } 
    
    public IEnumerator UnitDeath(BaseUnit unit, float duration)
    {
        MeshRenderer meshRenderer = unit.gameObject.GetComponent<MeshRenderer>();
        Color startColor = meshRenderer.material.color;

        Vector3 startScale = unit.transform.localScale;

        // Create a new material with a shader suitable for particles
        Material particleMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
        particleMaterial.color = Color.gray; // Set the desired start color

        // Create the particle system
        ParticleSystem particles = new GameObject("DeathParticles").AddComponent<ParticleSystem>();

        // Assign the material to the particle system renderer
        particles.GetComponent<ParticleSystemRenderer>().material = particleMaterial;

        // Set up the main module
        var main = particles.main;
        main.startLifetime = 1f;
        main.startSpeed = 0.002f;  // Lower speed to keep particles close
        main.startSize = 0.02f * Mathf.Max(startScale.x, startScale.y, startScale.z); // Adjust size based on unit scale
        main.loop = true;  // Loop the particle system

        // Set up the emission module
        var emission = particles.emission;
        emission.rateOverTime = 50f;  // Adjust rate for desired density

        // Set up the shape module
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere; // Sphere shape to keep particles close
        shape.radius = 0.005f * Mathf.Max(startScale.x, startScale.y, startScale.z); // Adjust radius based on unit scale

        // Set up the velocity over lifetime module to make particles move away after the coroutine
        var velocityOverLifetime = particles.velocityOverLifetime;
        velocityOverLifetime.enabled = false;  // Initially disabled

        particles.transform.position = unit.transform.position;
        particles.transform.parent = unit.transform;

        float elapsedTime = 0;

        particles.Play();

        while (elapsedTime < duration)
        {  
            elapsedTime += Time.deltaTime;

            float alpha = Mathf.Lerp(1, 0, elapsedTime / duration);       
            meshRenderer.material.color = new Color(startColor.r * alpha, startColor.g * alpha, startColor.b * alpha);

            // Inflation and deflation effect
            float inflateAmount = Mathf.Sin(elapsedTime / duration * Mathf.PI) * 0.5f;
            unit.transform.localScale = startScale * (1 + inflateAmount);

            yield return null;
        }
                    
        // Stop emission but keep particles playing until they move away
        emission.enabled = false;
                    
        Destroy(particles.gameObject);

        unit.gameObject.SetActive(false);
    }

    void HandlleUnitClick(BaseUnit clickedUnit)
    {
        List<Vector3> unitPos3D = new List<Vector3>();

        pastPositions.Clear();

        // Getting all the tile positions where the unit has been
        pastPositions = GetUnitPastPositions(clickedUnit);

        foreach (Vector2 pos2D in pastPositions)
        {
            int x = (int)pos2D.x; 
            int y = (int)pos2D.y;

            // Calculate the position of the unit relative to the table
            float tileCenterX = (x - (xmlWidth / 2.0f) - 0.5f) * (Width / (float)xmlWidth) * transform.localScale.x;
            float tileCenterY = (xmlHeight / 2.0f - y + 0.5f) * (Height / (float)xmlHeight) * transform.localScale.y;

            Vector3 position = square.transform.position + square.transform.rotation * new Vector3(tileCenterX, tileCenterY, -0.05f);

            unitPos3D.Add(position);
        }

        // Getting the exact position in the game world of the unit
        Vector3 currentPos3D = clickedUnit.transform.position;
        unitPos3D.Add(currentPos3D);

        DestroyLineRenderer();

        lineRenderer = new GameObject("LineRenderer").AddComponent<LineRenderer>();
        lineRenderer.positionCount = unitPos3D.Count;
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.005f;
        lineRenderer.SetPositions(unitPos3D.ToArray());
    }

    void DestroyLineRenderer()
    {
        if (lineRenderer != null)
        {
            Destroy(lineRenderer.gameObject);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        DestroyLineRenderer();

        GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;

        Debug.Log(clickedObject);

        if (clickedObject.CompareTag("BaseUnit")) // If the hit obejct is a unit
        {
            foreach (var position in turnUnitPlacements[currentTurnIndex])
            {
                foreach (BaseUnit unit in position.Value)
                {
                    if (unit.gameObject == clickedObject)
                    {
                        if (unit != null && unit.unitStatus == UnitStatus.Alive) // If the unit is alive and not null
                        {
                            HandlleUnitClick(unit); // Function to handle the unit click
                        }
                    }
                }
            }
        }
    }
}
