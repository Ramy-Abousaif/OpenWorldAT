using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GenerateMap : MonoBehaviour
{
    public Material ground;
    public int groundLayer;
    private GameObject player;
    private Camera cam;
    public float distanceToActive = 50.0f;

    [System.Serializable]
    public class GameData
    {
        public Orb.SaveToken[] orb;
    }

    private GameData gameData = new GameData();
    List<Orb.SaveToken> orbList = new List<Orb.SaveToken>();

    [System.Serializable]
    public class JsonTrees
    {
        public JsonTree[] tree;
    }

    [System.Serializable]
    public class JsonTree
    {
        public int id;
        public bool active;
        public Vector3 position;
        public Vector3 scale;
    }

    [System.Serializable]
    public class JsonOrbs
    {
        public JsonOrb[] orb;
    }

    [System.Serializable]
    public class JsonOrb
    {
        public int id;
        public bool active;
        public Vector3 position;
        public Vector3 scale;
    }

    [System.Serializable]
    public class JsonUniques
    {
        public JsonUnique[] unique;
    }

    [System.Serializable]
    public class JsonUnique
    {
        public int id;
        public string path;
        public bool active;
        public Vector3 position;
        public float renderDistance;
    }

    [System.Serializable]
    public class JsonTerrainTiles
    {
        public JsonTerrainTile[] terrain;
    }

    [System.Serializable]
    public class JsonTerrainTile
    {
        public int id;
        public Vector3 position;
        public Vector3 scale;
        public float spawnrate;
    }

    private string terrainJSON;
    private string treeJSON;
    private string lorbJSON;
    private string sorbJSON;
    private string uniqueJSON;
    JsonTerrainTiles myJsonTerrainTiles = new JsonTerrainTiles();
    JsonTrees myJsonTrees = new JsonTrees();
    JsonOrbs myJsonOrbs = new JsonOrbs();
    JsonUniques myJsonUniques = new JsonUniques();
    private List<Renderer> visibleRenderers = new List<Renderer>();
    private List<Terrain> visibleTerrain = new List<Terrain>();

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        SetJSONFiles();

        StartCoroutine(ChunkTerrain());
        StartCoroutine(ChunkTrees());
        StartCoroutine(ChunkUniques());
        StartCoroutine(ChunkOrbs());
        StartCoroutine(CullMap());
        //StartCoroutine(FPSDebug());
    }

    /*
     int fpsDebug = 0;
     float deltaTime;
    
    
     IEnumerator FPSDebug()
     {
         while (fpsDebug <= 31)
         {
             float fps = 1.0f / deltaTime;
             Debug.Log(new Vector2(Mathf.Ceil(fps), fpsDebug - 1));
             fpsDebug++;
             yield return new WaitForSeconds(1.0f);
         }
     }
    
     private void Update()
     {
         deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
     }
     */

    IEnumerator CullMap()
    {
        while(true)
        {
            Renderer[] sceneRenderers = FindObjectsOfType<Renderer>();
            Terrain[] terrainRenderers = FindObjectsOfType<Terrain>();

            visibleRenderers.Clear();
            visibleTerrain.Clear();
            for (int i = 0; i < sceneRenderers.Length; i++)
            {
                if (IsObjVisible(sceneRenderers[i]))
                    visibleRenderers.Add(sceneRenderers[i]);
                else
                    sceneRenderers[i].enabled = false;
            }

            for (int i = 0; i < terrainRenderers.Length; i++)
            {
                if (IsTerrainVisible(terrainRenderers[i]))
                    visibleTerrain.Add(terrainRenderers[i]);
                else
                    terrainRenderers[i].enabled = false;
            }

            foreach (Renderer renderer in visibleRenderers)
            {
                renderer.enabled = true;
            }

            foreach (Terrain terrain in visibleTerrain)
            {
                terrain.enabled = true;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    bool IsObjVisible(Renderer renderer)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        return (GeometryUtility.TestPlanesAABB(planes, renderer.bounds)) ? true : false;
    }

    bool IsTerrainVisible(Terrain terrain)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        return (GeometryUtility.TestPlanesAABB(planes, terrain.GetComponent<TerrainCollider>().bounds)) ? true : false;
    }

    IEnumerator ChunkTerrain()
    {
        while (true)
        {
            foreach (JsonTerrainTile t in myJsonTerrainTiles.terrain)
            {
                Vector2 dist = new Vector2
                    (
                    Mathf.Abs(t.position.x - player.transform.position.x),
                    Mathf.Abs(t.position.z - player.transform.position.z)
                    );
                if (Mathf.Sqrt((dist.x * dist.x) + (dist.y * dist.y)) > distanceToActive)
                {
                    Destroy(GameObject.Find("Terrain" + t.id));
                }
                else
                {
                    MakeTerrainChunk(t.id, t.position, t.scale, t.spawnrate, ground, 25f, 25f);
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator ChunkTrees()
    {
        while(true)
        {
            foreach (JsonTree tr in myJsonTrees.tree)
            {
                Vector2 dist = new Vector2
                    (
                    Mathf.Abs(tr.position.x - player.transform.position.x),
                    Mathf.Abs(tr.position.z - player.transform.position.z)
                    );
                if (Mathf.Sqrt((dist.x * dist.x) + (dist.y * dist.y)) > distanceToActive)
                {
                    Destroy(GameObject.Find("Tree" + tr.id));
                }
                else
                {
                    if (!GameObject.Find("Tree" + tr.id))
                    {
                        GameObject newTree = Instantiate(Resources.Load("Prefabs/Tree")) as GameObject;
                        newTree.transform.position = tr.position;
                        newTree.transform.localScale = tr.scale;
                        newTree.isStatic = true;
                        newTree.name = "Tree" + tr.id;
                    }
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator ChunkOrbs()
    {
        while(true)
        {
            foreach (JsonOrb o in myJsonOrbs.orb)
            {
                Vector2 dist = new Vector2
                    (
                    Mathf.Abs(o.position.x - player.transform.position.x),
                    Mathf.Abs(o.position.z - player.transform.position.z)
                    );
                if (Mathf.Sqrt((dist.x * dist.x) + (dist.y * dist.y)) > distanceToActive)
                {
                    Destroy(GameObject.Find("Orb" + o.id));
                }
                else
                {
                    if (!GameObject.Find("Orb" + o.id) && o.active)
                    {
                        GameObject newOrb = Instantiate(Resources.Load("Prefabs/EnergyOrb")) as GameObject;
                        newOrb.transform.position = o.position;
                        newOrb.transform.localScale = o.scale;
                        newOrb.isStatic = true;
                        newOrb.AddComponent<Orb>();
                        newOrb.GetComponent<Orb>().id = o.id;
                        newOrb.GetComponent<Orb>().active = o.active;
                        newOrb.name = "Orb" + o.id;
                    }
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator ChunkUniques()
    {
        while(true)
        {
            foreach (JsonUnique u in myJsonUniques.unique)
            {
                Vector2 dist = new Vector2
                    (
                    Mathf.Abs(u.position.x - player.transform.position.x),
                    Mathf.Abs(u.position.z - player.transform.position.z)
                    );
                if (Mathf.Sqrt((dist.x * dist.x) + (dist.y * dist.y)) > myJsonUniques.unique[u.id].renderDistance)
                {
                    Destroy(GameObject.Find("Unique" + u.id));
                }
                else
                {
                    if (!GameObject.Find("Unique" + u.id))
                    {
                        GameObject newUnique = Instantiate(Resources.Load(u.path)) as GameObject;
                        newUnique.transform.position = u.position;
                        newUnique.isStatic = true;
                        newUnique.name = "Unique" + u.id;
                    }
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    void MakeTerrainChunk(int id, Vector3 position, Vector3 scale, float spawnRate, Material mat, float width, float height)
    {
        if (!GameObject.Find("Terrain" + id))
        {
            TerrainData td = new TerrainData();
            td.heightmapResolution = 513;
            td.size = new Vector3(width, 600.0f, height);

            string rawFile = Application.streamingAssetsPath + "/Raw/Terrain" + position + ".raw";

            int r = td.heightmapResolution;
            float[,] data = new float[r, r];

            using (var file = File.OpenRead(rawFile))
            using (var reader = new BinaryReader(file))
            {
                for (int y = 0; y < r; y++)
                {
                    for (int x = 0; x < r; x++)
                    {
                        float v = (float)reader.ReadUInt16() / 0xFFFF;
                        data[y, x] = v;
                    }
                }
            }

            td.SetHeights(0, 0, data);

            GameObject newTerrain = Terrain.CreateTerrainGameObject(td);
            newTerrain.transform.position = new Vector3(position.x - (width / 2), position.y, position.z - (height / 2));
            newTerrain.transform.localScale = scale;
            newTerrain.GetComponent<Terrain>().materialTemplate = mat;
            newTerrain.name = "Terrain" + id;
            SpawnEnemy(spawnRate, newTerrain);
        }
    }

    void SpawnEnemy(float chance, GameObject terrain)
    {
        if (Random.value <= (chance / 100))
        {
            GameObject newEnemy = Instantiate(Resources.Load("Prefabs/Enemy")) as GameObject;
            newEnemy.transform.position = new Vector3(Random.Range(terrain.transform.position.x - terrain.GetComponent<Terrain>().terrainData.size.x, 
                terrain.transform.position.x + terrain.GetComponent<Terrain>().terrainData.size.x), 
                terrain.transform.position.y + terrain.GetComponent<Terrain>().terrainData.GetHeight((int)newEnemy.transform.position.x, (int)newEnemy.transform.position.z),
                Random.Range(terrain.transform.position.z - terrain.GetComponent<Terrain>().terrainData.size.z,
                terrain.transform.position.z + terrain.GetComponent<Terrain>().terrainData.size.z));
        }
    }

    void SetJSONFiles()
    {
        terrainJSON = File.ReadAllText(Application.streamingAssetsPath + "/Load/Terrain.JSON");
        treeJSON = File.ReadAllText(Application.streamingAssetsPath + "/Load/Tree.JSON");
        lorbJSON = File.ReadAllText(Application.streamingAssetsPath + "/Load/Orb.JSON");
        sorbJSON = File.ReadAllText(Application.streamingAssetsPath + "/Save/OrbList.JSON");
        uniqueJSON = File.ReadAllText(Application.streamingAssetsPath + "/Load/Unique.JSON");
        File.WriteAllText(Application.streamingAssetsPath + "/Save/OrbList.JSON", lorbJSON);
        myJsonTerrainTiles = JsonUtility.FromJson<JsonTerrainTiles>(terrainJSON);
        myJsonTrees = JsonUtility.FromJson<JsonTrees>(treeJSON);
        myJsonOrbs = JsonUtility.FromJson<JsonOrbs>(sorbJSON);
        myJsonUniques = JsonUtility.FromJson<JsonUniques>(uniqueJSON);
    }

    public void SaveOrbs()
    {
        orbList.Clear();
        foreach (var orb in FindObjectsOfType<Orb>())
        {
            orbList.Add(orb.Tokenize());
        }
        gameData.orb = orbList.ToArray();
        Save(Application.streamingAssetsPath + "/Save/OrbList.JSON");
        Load(Application.streamingAssetsPath + "/Save/OrbList.JSON");
        sorbJSON = File.ReadAllText(Application.streamingAssetsPath + "/Save/OrbList.JSON");
        myJsonOrbs = JsonUtility.FromJson<JsonOrbs>(sorbJSON);
    }

    void Save(string path)
    {
        string jsonString = JsonUtility.ToJson(gameData, true);
        StreamWriter writer = new StreamWriter(path);
        writer.Write(jsonString);
        writer.Close();
    }

    void Load(string path)
    {
        StreamReader reader = new StreamReader(path);
        string jsonString = reader.ReadToEnd();
        JsonUtility.FromJsonOverwrite(jsonString, gameData);
        reader.Close();
    }

    void OnApplicationQuit()
    {
        File.WriteAllText(Application.streamingAssetsPath + "/Save/OrbList.JSON", lorbJSON);
    }
}
