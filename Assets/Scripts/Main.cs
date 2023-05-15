using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleGraphQL;

public class Main : MonoBehaviour
{
    public GameObject healthPrefab;
    public GameObject enemyPrefab;
    public GameObject unitPrefab;

    public GameObject enemiesContainer;
    public GameObject healthsContainer;
    public Image healthsBar;
    
    public Text textEnemyKilled;
    public Text textGameOver;
    public Text textGameWin;

    public GraphQLConfig Config;

    [Range(3, 10)]
    public int healthCounter = 3;

    [Range(10, 15)]
    public int enemyCounter = 3;

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
        // StartCoroutine(_CallQueryCoroutine());
        // string results = await _gql.Send(query.ToRequest(new Dictionary<string, object>{}));
        // Debug.Log(results);
    }

    public async void _CallQueryCoroutine() 
    {
        Query query = _gql.FindQuery("leaderboard", "GetLeaderboard", OperationType.Query);
        string results = await _gql.Send(query.ToRequest());
        Debug.Log(results);
        // yield return response.AsCoroutine();

        // // Assert.IsNull(response.Result.Errors);

        // var data = response.Result.Data;
        // Debug.Log("GraphQL Result: " + data);
        // yield return new WaitForSend(
        //     _gql.Send(query.ToRequest(new Dictionary<string, object>{})), 
        //     OnCallQueryCoroutineComplete
        // );
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
            targetPosition = unitPrefab.transform.position;
            
            (unitPrefab.GetComponent<Unit>() as Unit).onTriggerEnter.AddListener(OnUnitCollide);
            
            getButtonCreateMarineGO.SetActive(false);
            getTextEnemyKilledGO.SetActive(true);
            _enemyKilledCounter = 0;
            SetCollectedItems(_enemyKilledCounter);
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

    void CheckGameComplete() {
        if (_enemyKilledCounter == enemyCounter) {
            getTextGameWinGO.SetActive(true);
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
