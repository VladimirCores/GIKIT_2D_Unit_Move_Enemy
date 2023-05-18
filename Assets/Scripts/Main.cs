using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleGraphQL;

[System.Serializable]
public class LeaderboardsResult
{
    public Leaderboard data;
}

[System.Serializable]
public class Leaderboard
{
    public LeadVO[] leaderboard;
}

public class Main : MonoBehaviour
{
    public GameObject healthPrefab;
    public GameObject enemyPrefab;
    public GameObject unitPrefab;
    public GameObject bulletPrefab;

    public GameObject enemiesContainer;
    public GameObject healthsContainer;

    public Image healthsBar;
    
    public Text textEnemyKilled;
    public Text textGameOver;
    public Text textGameWin;

    public GraphQLConfig Config;

    [Range(3, 10)]
    public int healthCounter = 3;

    [Range(3, 15)]
    public int enemyCounter = 3;

    public Transform ContentContainer;
    public GameObject LeadItemPrefab;

    private List<GameObject> _enemyLists;
    private List<GameObject> _healthLists;

    private int _enemyKilledCounter = 0;

    private GameObject getButtonCreateMarineGO
    {
        get { return GameObject.Find("button_Create_Marine"); }
    }

    private GameObject getTextEnemyKilledGO
    {
        get { return textEnemyKilled.gameObject; }
    }

    private GameObject getTextGameOverGO
    {
        get { return textGameOver.gameObject; }
    }
    private GameObject getTextGameWinGO
    {
        get { return textGameWin.gameObject; }
    }

    private GraphQLClient _gql;
    private float _timerStart;

    // Start is called before the first frame update
    void Start()
    {
        var uiButtonCreateUnit = getButtonCreateMarineGO;
        uiButtonCreateUnit.GetComponent<Button>().onClick.AddListener(OnCreatePressed);
        Debug.Log("> Button on UI: " + uiButtonCreateUnit.name);

        _enemyLists = new List<GameObject>(enemyCounter);
        Debug.Log("> _enemyLists: " + _enemyLists.Count + " | " + healthCounter);
        
        getTextEnemyKilledGO.SetActive(false);
        getTextGameOverGO.SetActive(false);
        getTextGameWinGO.SetActive(false);

        _healthLists = spawnPrefabWithUniquePositionOnStageInContainer(healthCounter, healthPrefab, healthsContainer);
        _enemyLists = spawnPrefabWithUniquePositionOnStageInContainer(enemyCounter, enemyPrefab, enemiesContainer);

        _gql = new GraphQLClient(Config);
        _CallQueryCoroutine();
    }

    Text GetChildTextByName(GameObject obj, string name) => obj.transform.Find(name)?.gameObject.GetComponent<Text>();

    public async void _CallQueryCoroutine() 
    {
        // https://github.com/NavidK0/SimpleGraphQL-For-Unity
        Query query = _gql.FindQuery("leaderboard", "GetLeaderboard", OperationType.Query);
        string results = await _gql.Send(query.ToRequest());
        LeaderboardsResult result = JsonUtility.FromJson<LeaderboardsResult>(results);
        Debug.Log("results = " + results);
        foreach(Transform child in ContentContainer.transform) Destroy(child.gameObject);
        foreach (LeadVO leadVO in result.data.leaderboard)
        {
            Debug.Log("> leaderboard.leadVO = " + leadVO.score);
            var leadItem = Instantiate(LeadItemPrefab);
            // do something with the instantiated item -- for instance
            GetChildTextByName(leadItem, "txt_leaderboard_user_name").text = "User: " + (leadVO.user?.name ?? "unknown");
            GetChildTextByName(leadItem, "txt_leaderboard_time").text = "Time: " + leadVO.time;
            //parent the item to the content container
            leadItem.transform.SetParent(ContentContainer);
            //reset the item's scale -- this can get munged with UI prefabs
            leadItem.transform.localScale = Vector2.one;
        }
    }

    public void OnCallQueryCoroutineComplete(string result) 
    {
        Debug.Log("GraphQL Result: " + result);
    }

    List<GameObject> spawnPrefabWithUniquePositionOnStageInContainer(int counter, GameObject prefab, GameObject container) {
        int CALCULATION_LIMIT = 100;

        var instance = Instantiate(prefab);
        double DEFAULT_RADIUS = instance.GetComponent<CircleCollider2D>().radius;
        
        Destroy(instance);

        var result = new List<GameObject>(counter);
        
        int limit = 0;

        bool checkCalculationLimit() => ++limit < CALCULATION_LIMIT;

        while(counter-- > 0) {
            Vector2 position = Vector2.zero;
            limit = 0;
            do {
               position = Main.findRandomPositionOnScreen(Camera.main);
            } while(checkCalculationLimit() && checkCollisionWithOthers(position, DEFAULT_RADIUS));
            
            if (limit == CALCULATION_LIMIT) continue;

            if (position != Vector2.zero) {
                var item = Instantiate(prefab);
                item.transform.parent = container.transform;
                item.transform.position = position;
                result.Add(item);
            }
        }

        return result;
    }

    static public Vector2 findRandomPositionOnScreen(Camera cam) {
        Vector2 screenBounds = cam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        Vector2 screenOrigin = cam.ScreenToWorldPoint(Vector2.zero);
        return new Vector2 (Random.Range(-screenOrigin.x, screenOrigin.x), Random.Range(-screenOrigin.y, screenOrigin.y));
    }

    bool checkCollisionWithOthers(Vector2 positionToTest, double radius) {
        bool result = false;
        var maxDistance = radius + radius;
        Debug.Log("> Main -> checkCollisionWithOthers: " + positionToTest);
        foreach (GameObject enemy in _enemyLists)
        {
            Vector2 enemyPosition = enemy.transform.position;
            var distance = Vector2.Distance(enemyPosition, positionToTest);
            if (distance < maxDistance) {
                Debug.Log("> Main -> checkCollisionWithOthers: distance = " + distance);
                result = true;
                break;
            }
        }
        Debug.Log("> Main -> checkCollisionWithOthers: result = " + result);
        return result;
    }

    private Vector3 targetPosition = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if (unitPrefab.scene.IsValid() && Vector2.Distance(unitPrefab.transform.position, targetPosition) > 0.01) {
            unitPrefab.transform.position = Vector2.Lerp(unitPrefab.transform.position, targetPosition, Time.deltaTime);
        }
    }

    void OnCreatePressed() {
        Debug.Log("> Pressed");
        if (unitPrefab.scene.IsValid() == false) {
            unitPrefab = Instantiate(unitPrefab);
            unitPrefab.GetComponent<Unit>().bulletPrefab = bulletPrefab;
            targetPosition = unitPrefab.transform.position;
            
            (unitPrefab.GetComponent<Unit>() as Unit).onTriggerEnter.AddListener(OnUnitCollide);
            
            getButtonCreateMarineGO.SetActive(false);
            getTextEnemyKilledGO.SetActive(true);
            _enemyKilledCounter = 0;
            SetCollectedItems(_enemyKilledCounter);
            _timerStart = Time.time;
        }
    }

    void OnUnitCollide(Collider2D collider) {
        Debug.Log("> OnUnitCollide: " + collider.gameObject);
        var collisionGameObjectParent = collider.gameObject.transform.parent;
        bool isEnemyCollision = collisionGameObjectParent == enemiesContainer.transform;
        Debug.Log("> OnUnitCollide: isEnemyCollision = " + isEnemyCollision);
        float scaleX = healthsBar.rectTransform.localScale.x;
        
        if (isEnemyCollision) {
            _enemyKilledCounter++;
            scaleX -= collider.gameObject.GetComponent<Enemy>().Damage;
            Debug.Log("> OnUnitCollide: localScale = " + scaleX + "|" + _enemyKilledCounter + "|" + enemyCounter);
            SetCollectedItems(_enemyKilledCounter);
            if (scaleX <= 0) {
                scaleX = 0;
                getTextGameOverGO.SetActive(true);
            } else CheckGameComplete();
            
        } 
        else if (collider.tag == "Bullet") {
            _enemyKilledCounter++;
            SetCollectedItems(_enemyKilledCounter);
            CheckGameComplete();
        }
        else if (collisionGameObjectParent == healthsContainer.transform) {
            scaleX += collider.gameObject.GetComponent<Health>().Heal;
        }
        SetHealthBar(scaleX);
        Destroy(collider.gameObject);
    } 

    async void CheckGameComplete() {
        if (_enemyKilledCounter == enemyCounter) {
            getTextGameWinGO.SetActive(true);
            float gameTime = Time.time - _timerStart;
            Debug.Log("Game Time: " + gameTime.ToString());

            Query query = _gql.FindQuery("leaderboard", "AddLeaderboardScore", OperationType.Mutation);
            string results = await _gql.Send(query.ToRequest(new Dictionary<string, object>
            {
                {"time", (int)gameTime},
                {"score", _enemyKilledCounter},
            }));
            Debug.Log("results = " + results); 
            _CallQueryCoroutine(); 
        }
    }

    float CalculateHealth() {
        return 1.0f - 2.0f * ((float)_enemyKilledCounter / (float)enemyCounter);
    }

    void SetHealthBar(float scaleX) {
        healthsBar.rectTransform.localScale = new Vector3(scaleX, 1.0f, 1.0f);
    }

    void SetCollectedItems(int count) {
        var textSplitted = textEnemyKilled.text.Split(':');
        textEnemyKilled.text = textSplitted[0] + ": " + count;
    }
}
