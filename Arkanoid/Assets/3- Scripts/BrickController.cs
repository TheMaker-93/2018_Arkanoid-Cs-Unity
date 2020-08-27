using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickController : MonoBehaviour {

    [Header("Player")]
    public GameObject player;
    public PlayerController playerController;

    private SpriteRenderer brickSpriteRendereer;     // cambiare el alfa del bloque para indicar que le queda menos salud
    private Color color;                            // color de nuestro sprite (para poder overridear su alfa)                       

    public enum brickTypes
    {
        red,green,yellow,purple,blue,
    };
    //public int id;

    [Header("BirckType")]
    public brickTypes type;             // tipo de bloque segun el color
    public float totalHealth;             // salud total
    public float health;                  // salud actual
    public int inputScore;              // puntuacion que nos dara al destruirlo

    [Header("PowerUps")]
    public GameObject powerUpPrefab;       // prefab del powerUp, escogeremos el tipo cuando sea instanciado
    public enum PowerUpTypes
    {
        speed,
        sizeMultiplier,
        glue,
        none,
    }
    public PowerUpTypes powerUpType;              // tipo de poweUp que aparecera
    // aquello que el powerUp necesitara al aparecer
    public SpriteRenderer playAreaRenderer;
    public SpriteRenderer playerSpriteRenderer;
    public BallController ballControler;
    public int spawninProvability;              // variable del 1 al 10 que indiara con que frequencia aparecera


	// Use this for initialization
	void Start () {

        // el uso intensivo de getComopones tiene un sentido.
        //          Por problemas de vision que tengo me es mucho mas rapido trabajar con poca maraña de informacion en el inspector
        //          Planteo esto como una herramienta para el desarrollador, aquello que no necesite ver o tocar no deberia poder verlo (de ahi que sean provados y lso coja con get components)
        brickSpriteRendereer = GetComponent<SpriteRenderer>();
        color = brickSpriteRendereer.color;  
        health = totalHealth;

		
	}
	

    // funcion que se encargara de quitar vida y darnos puntuacion si es necesario tras destruir el objeto
    public void DiminishHealth (int damage = 1)
    {
        health -= damage;

        // si necesita mas de un golpe para movirr usaremos esto para representar mediante el alfa la salud restante
        if (totalHealth > 1)
        {
            color.a = ((health / totalHealth) * 255) / 255;     // dependiendod e la vida total tendra mas o menos alfa al recibr daño
            brickSpriteRendereer.color = color;         // aplicamos el color ya modificado
        }

        if (health <= 0)
        {
            // si el tipo de powerUp no es none aparecera el powerUp
            if (powerUpType != PowerUpTypes.none)
            {
                // aplicams cierta aleatoriedad a la hora de hacer aparecer los powerUp
                if (Random.Range(1,11) <= spawninProvability)
                {
                    GameObject pu = Instantiate(powerUpPrefab);
                    pu.transform.position = transform.position;
                    powerUpController puController = pu.GetComponent<powerUpController>();

                    // cargamos valores en la instancia 
                    puController.playAreaRenderer = this.playAreaRenderer;
                    puController.playerController = this.playerController;
                    puController.playerSpriteRenderer = this.playerSpriteRenderer;
                    puController.ballControler = this.ballControler;
                    puController.powerUpType = (powerUpController.PowerUpTypes)this.powerUpType;
                }

                

            }

            playerController.IncreaseScore(inputScore);
            Destroy(this.gameObject);
        } 


    }
}
