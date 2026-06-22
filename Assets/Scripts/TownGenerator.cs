using UnityEngine;
using System.Collections.Generic;

// Attach to an empty GameObject in your scene.
// Use the Inspector button "Generate Town" to spawn the layout in Edit Mode (no Play required).
// Use "Clear Generated" to remove all generated children before regenerating.
//
// Unity 6000.4.5f1 / URP / WebGL compatible.
// All generation logic is public so TownGeneratorEditor can call it.
// No Start() or runtime generation — everything is Editor-driven.

public class TownGenerator : MonoBehaviour
{
    [Header("Ground")]
    public bool spawnGround = false;
    public Vector2 groundSize = new Vector2(80f, 80f);

    [Header("Enemy Spawning")]
    public GameObject enemyPivotPrefab;

    [Header("Colors (URP Lit)")]
    public Color groundColor = new Color(0.55f, 0.45f, 0.35f);
    public Color buildingColorA = new Color(0.72f, 0.60f, 0.42f);
    public Color buildingColorB = new Color(0.60f, 0.50f, 0.38f);
    public Color buildingColorC = new Color(0.80f, 0.70f, 0.55f);
    public Color saloonColor = new Color(0.65f, 0.48f, 0.30f);
    public Color towerColor = new Color(0.45f, 0.38f, 0.30f);
    public Color fenceColor = new Color(0.50f, 0.40f, 0.28f);

    public struct BuildingDef
    {
        public string name;
        public Vector3 position;
        public Vector3 size;
        public Color color;
    }

    public struct EnemySpawnDef
    {
        public string label;
        public Vector3 position;
        public bool isGroupMember;
    }

    // -----------------------------------------------------------------------
    // PUBLIC ENTRY POINTS (called by TownGeneratorEditor)
    // -----------------------------------------------------------------------

    public void GenerateTown()
    {
        if (spawnGround) SpawnGround();
        SpawnBuildings();
        SpawnWaterTower();
        SpawnFences();
        SpawnEnemies();
    }

    public void ClearGenerated()
    {
        // Collect all children first, then destroy — avoids modifying the
        // collection while iterating, which causes Unity editor errors.
        var children = new List<GameObject>();
        foreach (Transform child in transform)
            children.Add(child.gameObject);

        foreach (var child in children)
        {
            // Use DestroyImmediate in editor context (Destroy only works at runtime)
            DestroyImmediate(child);
        }
    }

    // -----------------------------------------------------------------------
    // GROUND
    // -----------------------------------------------------------------------

    private void SpawnGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.SetParent(transform);
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(groundSize.x / 10f, 1f, groundSize.y / 10f);
        SetMaterialColor(ground, groundColor);
    }

    // -----------------------------------------------------------------------
    // BUILDINGS
    // -----------------------------------------------------------------------

    private void SpawnBuildings()
    {
        List<BuildingDef> buildings = new List<BuildingDef>
        {
            new BuildingDef { name = "EntryWest",
                position = new Vector3(-9f, 0f, -12f),
                size = new Vector3(7f, 3.5f, 6f),
                color = buildingColorA },

            new BuildingDef { name = "EntryEast",
                position = new Vector3(9f, 0f, -12f),
                size = new Vector3(5f, 5f, 5f),
                color = buildingColorB },

            new BuildingDef { name = "EntryEast2",
                position = new Vector3(9.5f, 0f, -4f),
                size = new Vector3(4f, 4f, 5f),
                color = buildingColorC },

            new BuildingDef { name = "EastAlleyNorth",
                position = new Vector3(12f, 0f, 4f),
                size = new Vector3(8f, 4f, 3f),
                color = buildingColorA },

            new BuildingDef { name = "Saloon",
                position = new Vector3(-13f, 0f, +13f),
                size = new Vector3(13f, 5.5f, 12f),
                color = saloonColor },

            new BuildingDef { name = "SaloonAnnex",
                position = new Vector3(-9f, 0f, +5f),
                size = new Vector3(4f, 3f, 4f),
                color = new Color(saloonColor.r * 0.85f, saloonColor.g * 0.85f, saloonColor.b * 0.85f) },

            new BuildingDef { name = "MidEast",
                position = new Vector3(9f, 0f, +9f),
                size = new Vector3(5f, 4.5f, 6f),
                color = buildingColorB },

            new BuildingDef { name = "MidEast2",
                position = new Vector3(10f, 0f, +17f),
                size = new Vector3(6f, 3.5f, 5f),
                color = buildingColorC },

            new BuildingDef { name = "WestBack",
                position = new Vector3(-9.5f, 0f, +26f),
                size = new Vector3(6f, 4f, 5f),
                color = buildingColorA },

            new BuildingDef { name = "NorthWest",
                position = new Vector3(-9f, 0f, +35f),
                size = new Vector3(7f, 4.5f, 6f),
                color = buildingColorB },

            new BuildingDef { name = "NorthEast",
                position = new Vector3(9f, 0f, +32f),
                size = new Vector3(5f, 5f, 7f),
                color = buildingColorA },

            new BuildingDef { name = "NorthEast2",
                position = new Vector3(9f, 0f, +41f),
                size = new Vector3(5f, 3.5f, 4f),
                color = buildingColorC },

            new BuildingDef { name = "NorthWestCorner",
                position = new Vector3(-8f, 0f, +43f),
                size = new Vector3(5f, 4f, 4f),
                color = buildingColorC },
        };

        foreach (var def in buildings)
            SpawnBuilding(def);
    }

    private void SpawnBuilding(BuildingDef def)
    {
        GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
        building.name = def.name;
        building.transform.SetParent(transform);
        building.transform.position = def.position + new Vector3(0f, def.size.y * 0.5f, 0f);
        building.transform.localScale = def.size;
        SetMaterialColor(building, def.color);

        var col = building.GetComponent<BoxCollider>();
        if (col != null) col.isTrigger = false;
    }

    // -----------------------------------------------------------------------
    // WATER TOWER
    // -----------------------------------------------------------------------

    private void SpawnWaterTower()
    {
        Vector3 towerBase = new Vector3(0f, 0f, +50f);

        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = "WaterTower_Base";
        platform.transform.SetParent(transform);
        platform.transform.position = towerBase + new Vector3(0f, 0.4f, 0f);
        platform.transform.localScale = new Vector3(3.5f, 0.8f, 3.5f);
        SetMaterialColor(platform, towerColor);

        GameObject legs = GameObject.CreatePrimitive(PrimitiveType.Cube);
        legs.name = "WaterTower_Legs";
        legs.transform.SetParent(transform);
        legs.transform.position = towerBase + new Vector3(0f, 3.5f, 0f);
        legs.transform.localScale = new Vector3(0.6f, 6f, 0.6f);
        SetMaterialColor(legs, towerColor);

        GameObject tank = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tank.name = "WaterTower_Tank";
        tank.transform.SetParent(transform);
        tank.transform.position = towerBase + new Vector3(0f, 8f, 0f);
        tank.transform.localScale = new Vector3(3f, 1.5f, 3f);
        SetMaterialColor(tank, new Color(towerColor.r * 0.75f, towerColor.g * 0.75f, towerColor.b * 0.75f));
    }

    // -----------------------------------------------------------------------
    // FENCES
    // -----------------------------------------------------------------------

    private void SpawnFences()
    {
        SpawnFenceSegment(new Vector3(-5f, 0f, -6.5f), new Vector3(0.3f, 1.2f, 3f));
        SpawnFenceSegment(new Vector3(-5f, 0f, +2.5f), new Vector3(0.3f, 1.2f, 3f));
        SpawnFenceSegment(new Vector3(-5f, 0f, +22.5f), new Vector3(0.3f, 1.2f, 3f));
        SpawnFenceSegment(new Vector3(-5f, 0f, +30.5f), new Vector3(0.3f, 1.2f, 3f));
        SpawnFenceSegment(new Vector3(5f, 0f, -7.5f), new Vector3(0.3f, 1.2f, 2.5f));
        SpawnFenceSegment(new Vector3(5f, 0f, +1f), new Vector3(0.3f, 1.2f, 2f));
        SpawnFenceSegment(new Vector3(5f, 0f, +13.5f), new Vector3(0.3f, 1.2f, 4f));
        SpawnFenceSegment(new Vector3(5f, 0f, +25.5f), new Vector3(0.3f, 1.2f, 3f));
        SpawnFenceSegment(new Vector3(5f, 0f, +38f), new Vector3(0.3f, 1.2f, 3f));
    }

    private void SpawnFenceSegment(Vector3 centerBase, Vector3 size)
    {
        GameObject fence = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fence.name = "Fence";
        fence.transform.SetParent(transform);
        fence.transform.position = centerBase + new Vector3(0f, size.y * 0.5f, 0f);
        fence.transform.localScale = size;
        SetMaterialColor(fence, fenceColor);
    }

    // -----------------------------------------------------------------------
    // ENEMY SPAWNING
    // -----------------------------------------------------------------------

    private void SpawnEnemies()
    {
        if (enemyPivotPrefab == null)
        {
            Debug.LogWarning("[TownGenerator] No Enemy Pivot Prefab assigned. Skipping enemy spawns.");
            return;
        }

        List<EnemySpawnDef> spawns = new List<EnemySpawnDef>
        {
            new EnemySpawnDef { label = "Solo_1_Entry",       position = new Vector3(-2f, 0f, -8f),  isGroupMember = false },
            new EnemySpawnDef { label = "Solo_2_EastAlley",   position = new Vector3(8f,  0f, +2f),  isGroupMember = false },
            new EnemySpawnDef { label = "Solo_3_SaloonPlaza", position = new Vector3(-1f, 0f, +13f), isGroupMember = false },
            new EnemySpawnDef { label = "Group_A",            position = new Vector3(-3f, 0f, +22f), isGroupMember = true  },
            new EnemySpawnDef { label = "Group_B",            position = new Vector3(+3f, 0f, +22f), isGroupMember = true  },
            new EnemySpawnDef { label = "Solo_4_North",       position = new Vector3(0f,  0f, +38f), isGroupMember = false },
            new EnemySpawnDef { label = "Wanted_Billy",       position = new Vector3(0f,  0f, +47f), isGroupMember = false },
        };

        foreach (var spawn in spawns)
        {
            GameObject enemy = Instantiate(enemyPivotPrefab, spawn.position, Quaternion.identity);
            enemy.name = spawn.label + (spawn.isGroupMember ? " [GROUP]" : "");
            enemy.transform.SetParent(transform);
        }
    }

    // -----------------------------------------------------------------------
    // MATERIAL UTILITY
    // -----------------------------------------------------------------------

    public void SetMaterialColor(GameObject obj, Color color)
    {
        var renderer = obj.GetComponent<Renderer>();
        if (renderer == null) return;

        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            Debug.LogWarning("[TownGenerator] Could not find URP/Lit shader.");
            renderer.material.color = color;
            return;
        }

        Material mat = new Material(urpLit);
        mat.color = color;
        renderer.material = mat;
    }
}