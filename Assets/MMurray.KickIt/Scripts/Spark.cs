using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*When a block is kicked, a spark is generated and travels a short distance in the opposite direction of the block */
public class Spark : MonoBehaviour
{
    public float sparkSpawnTime;
    public float moveSpeed;
    public float sparkLifetime;         //time in seconds
    const float totalLifetime = 0.12f;
    public Vector2 direction;
    // Start is called before the first frame update
    void Start()
    {
        moveSpeed = 10;
    }

    private void OnDisable()
    {
        sparkLifetime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //move the spark. After it travels a certain distance, deactivate it.
        transform.Translate(new Vector2(direction.x, direction.y) * moveSpeed * Time.deltaTime);
        sparkLifetime += Time.deltaTime;
        if (sparkLifetime > totalLifetime)
        {
            gameObject.SetActive(false);
        }
        
    }

    public void GenerateSpark(Vector2 location, Vector2 direction)
    {
        transform.position = location;

        //randomize the travel a bit
        if (direction.x != 0)
        {
            float yValue = Random.Range(-0.2f, 0.2f);
            direction = new Vector2(direction.x, yValue);
        }
        else if (direction.y != 0)
        {
            float xValue = Random.Range(-0.2f, 0.2f);
            direction = new Vector2(xValue, direction.y);
        }

        this.direction = direction;
    }
}
