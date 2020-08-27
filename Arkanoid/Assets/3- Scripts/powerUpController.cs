using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class powerUpController : MonoBehaviour {


    public enum PowerUpTypes
    {
        speed,
        sizeMultiplier,
        glue,
    }

    [Header("PowerUp Atributes")]
    public PowerUpTypes powerUpType;
    public Vector3 direction;           // direccion de caida
    public float speed;                 // velocidad de movimiento

    [Header("Sprite Renderer")]
    public SpriteRenderer powerUpSpriteRenderer;        // lo arrastramos desde el inspector debido a que sino cargaremos mas de lo debido la maquina al ser un instanciado + getComponent

    [Header("Sprites of the powerUps")]
    public Sprite speedSprite;
    public Sprite sizeMultiplierSprite;
    public Sprite glueSprite;

    [Header("Enviorment")]
    public BallController ballControler;        // controlador de la bola para reducir su velocidad una vez el player coja el power up LLAMAREMOS A UNA FUNCION EN LA BOLA QUE LO HARA
    public PlayerController playerController;   // para llamar a la funcion que cambiara su tamaño y activara la habilidad e tenr la bola pegada
    public SpriteRenderer playerSpriteRenderer;
    public SpriteRenderer playAreaRenderer;

    [Header("sfx")]
    public AudioClip cachSfx;


	// Use this for initialization
	void Start () {

        // Dani, puedes hacer que cuando este objeto detecte una colision con el player llame una funcion suya que tenga un timer
        // y que cambie los atributos apropiados. Cuando es impactado el powerUp es destruido

        switch (powerUpType)
        {
            case (PowerUpTypes.speed):
                powerUpSpriteRenderer.sprite = speedSprite;
                break;
            case (PowerUpTypes.sizeMultiplier):
                powerUpSpriteRenderer.sprite = sizeMultiplierSprite;
                break;
            case (PowerUpTypes.glue):
                powerUpSpriteRenderer.sprite = glueSprite;
                break;
            default:
                Debug.LogError("powerUp type not defined");
                break;
        }
		
	}
	
	// Update is called once per frame
	void Update () {


        // si estasmos dentro de la escena de juego nos movemos
        if (powerUpSpriteRenderer.bounds.max.y >= playAreaRenderer.bounds.min.y)
        {
            // nos movemos
            transform.position = transform.position + direction * speed * Time.deltaTime;

            // comrpobamos colision y dependiendo de que tipo de powerUp seamos haremos una cosa u otra
            if (CheckCollision())
            {
                // reproduciremos el sonido de captura de powerUp desde el altavoz del player
                playerController.ReproducePowerUpCachSound(cachSfx);

                switch (powerUpType)
                {
                    case (PowerUpTypes.speed):
                        Debug.LogWarning("Soy un pU de speed");

                        // llamamos al script que reduce la velocidad de la bola (autoa administrado)
                        ballControler.ReduceSpeed(2);
                        // destruimos el PU
                        Destroy(gameObject);

                        break;
                    case (PowerUpTypes.sizeMultiplier):
                        Debug.LogWarning("Soy un pU de size");

                        playerController.MultiplySize(1.5f);

                        // destruimos el PU
                        Destroy(gameObject);
                        break;
                    case (PowerUpTypes.glue):
                        Debug.LogWarning("Soy un pU de glue");

                        playerController.TurnSticky();
                        // destruimos el PU
                        Destroy(gameObject);
                        break;
                }
            }
        }
        else
        {

            Debug.LogError("POWERUPO DESTRUDIO");
            // sino estamos dentro de la pantallla somos destruidos
            Destroy(gameObject);
        }

    }

    // esta funcion se encargara de comprobar las colisiones
    public bool CheckCollision ()
    {
        if (powerUpSpriteRenderer.bounds.Intersects(playerSpriteRenderer.bounds))
        {
            return true;
        } else
        {
            return false;
        }
    }

}
