using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {

    [Header("Enviorment")]
    public GameObject playArea;  // usare esta variable para delimitar el area de movimiento del player
    public SpriteRenderer playAreaSpriteRenderer;       // para conocer los limites del escenario
    public float spawnYOffset = -2.6f;      // distancia desde el centro del area de juego
    public float movementDistance;      // espacio total que tendra el player para moverse

    [Header("Player Atributes")]
    public int movementSpeed = 4;
    private Vector2 startingPosition;
    public SpriteRenderer playerSpriteRenderer;
    public int playerScore;    // puntuacion (prefiero guardarla localmente y pasarla al hud solo cuando cambie)
    public int playerLifes;     // vidas restantes
    public Vector3 direction;
    public enum status          // posibles estados del player
    {
        alive,
        dead,
    }
    public status playerStatus; // estado actual del player
    public Sprite initialSprite;            // sprite inicial
    public Sprite bigPowerUpSprite;         // sprite para cuando cojamos el powerUp de multipicador de tamaño
    public Sprite stickyPowerUpSprite;      // 

    [Header("Helpers")]
    public GameObject leftHelper;
    private SpriteRenderer leftHelperSpriteRenderer;
    public GameObject righthHelper;
    private SpriteRenderer rightHelperSpriteRenderer;

    [Header("Referencias al hud")]
    public GameObject hud;          
    public Text hudScore;
    public Text hudLifes;

    [Header("Ball")]
    public BallController ballController;   // free o no free

    [Header("speaker")]
    public AudioSource speaker;

    [Header("Increase Size")]
    public bool sizeIncreased;              // boleana que controla si estamos o no con el tamaño augmentado
    public float sizeMultiplier;            // multiplicador de tamaño
    public float powerUpDuration;           // duracion maxima del power up 
    public float powerUpCurrentTime;        // tiempo transcurrido dese el inicio del pwerUp

    [Header("Sticky")]
    public bool sticky;                     // controlamos si estamos pegajosos o no
    public float stickyCurrentTime;         // tiempo que lleva activo
    public float stickyDuration;            // tiempo maximo que estara operativo

    // Use this for initialization
    void Start () {


        // si este es el primer nivel de juego setamos vidas y puntuacion nuevas
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentSceneIndex == 0)
        {
            playerLifes = 3;
            playerScore = 0;
        } else
        {
            // en el caso que no estemos en la priema escena cargaremos los valores desde DataStorage
            playerLifes = DataStorage.remainingLifes;
            playerScore = DataStorage.storedPoints;
        }


        playerStatus = status.alive;            // marcamos el jugador como vivo
        hudLifes.text = playerLifes.ToString();
        hudScore.text = playerScore.ToString();

        // posicion determinada por el fondo
        startingPosition.x = playArea.transform.position.x;
        startingPosition.y = playArea.transform.position.y + spawnYOffset;
        // aplicamos la posicion calculada al objeto
        gameObject.transform.position = startingPosition;

        // hacemos los helpers no visibles para el juegador y guardamos su spriterenderer
        leftHelperSpriteRenderer = leftHelper.GetComponent<SpriteRenderer>();
        rightHelperSpriteRenderer = righthHelper.GetComponent<SpriteRenderer>();

        leftHelperSpriteRenderer.color = new Color(0, 0, 0, 0);
        rightHelperSpriteRenderer.color = new Color(0, 0, 0, 0);

        // colocamos la bola en su posicion inicial
        //ball.transform.position = new Vector3(startingPosition.x, startingPosition.y + distanceFromPlayer);


    }

    // Update is called once per frame
    void Update () {

        // maquina de estasdos
        switch (playerStatus)
        {
            case (status.alive):
                Move();     // movimiento de la pala
                Shoot();    // liberamiento de la bola

                #region Size Increase Check
                // controlamos el efecto del powerUp si lo cojemos
                if (sizeIncreased)
                {

                    // aplicamos el nuevo sprite
                    playerSpriteRenderer.sprite = bigPowerUpSprite;

                    if (powerUpCurrentTime <= powerUpDuration)
                    {
                        powerUpCurrentTime += Time.deltaTime;
                    }
                    else
                    {
                        sizeIncreased = false;
                        // dejamos el tamaño normal ya que en este momento ya ha pasado el efecto de PU
                        transform.localScale = new Vector3(1, 1, 1);
                        // resetamos el timer
                        powerUpCurrentTime = 0f;
                        playerSpriteRenderer.sprite = initialSprite;
                    }
                }
                #endregion

                #region Sticky Check

                if (sticky)
                {

                    playerSpriteRenderer.sprite = stickyPowerUpSprite;

                    if (stickyCurrentTime <= stickyDuration)
                    {
                        stickyCurrentTime += Time.deltaTime;
                    } else
                    {
                        sticky = false;

                        // VOLVEMOS AL ESTADO NORMAL

                        // resetamos timer y colocamos de nuevo el sprite por defecto
                        stickyCurrentTime = 0;
                        playerSpriteRenderer.sprite = initialSprite;

                    }

                }

                #endregion

                break;
            case (status.dead):
                // no podremos ni disparar ni movernos
            break;

        }



    }

    // control del movimiento del jugador
    private void Move()
    {
        /*  INPUT HELP
            GETKEY es true mietras apretemos
            GETKEYDOWN es true en el frame en que pulsamos
            GETAXIS para coger los botones como horizontal, vertical... en preferencias > input
            GETAXISRAW es como el anterior pero sin gravedad
         */
        bool canGoLeft = true;
        bool canGoRight = true;     // cotnrolamos si podemos movernos 

        // si sobrepasamos por la izquierda
        if (leftHelperSpriteRenderer.bounds.min.x < playAreaSpriteRenderer.bounds.min.x)
        {
            canGoLeft = false;
        }
        else if (rightHelperSpriteRenderer.bounds.max.x > playAreaSpriteRenderer.bounds.max.x)
        {
            canGoRight = false;
        } 

        // si el horizontal esmas grande que 0
        // si usas input.getaxisRaw no hay gravedad con lo cual pasamos de 1 a 0 al instante
        if (Input.GetAxisRaw("Horizontal") > 0 && canGoRight )
        {
            direction = new Vector3(1, 0, 0);
        }
        else if (Input.GetAxisRaw("Horizontal") < 0 && canGoLeft )
        {
            direction = new Vector3(-1, 0, 0);
        }
        else
        {
            direction = new Vector3(0, 0, 0);
        }
           
        // aplicamos el movimiento
        transform.position +=  direction * movementSpeed * Time.deltaTime;


    }

   
    // hacemos la bola que sea libre y pueda moverse
    private void Shoot()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // le decimos a la bola que esta libre
            ballController.SetFree(true);
        }

    }

    // metodos publicos
    // restamos tantas vidas como le pasemos (por defecto sera 1)
    public void DecreaseLifes(int delta = 1)
    {
        playerLifes -= delta;
        UpdateHUDLifes();
    }
    // aumento de la puntuacion del player en delta unidades
    public void IncreaseScore (int delta)
    {
        playerScore += delta;
        UpdateHUDScore();
    }

    // duplicamos su tamaño durante x tiempo
    public void MultiplySize (float multiplier)
    {
        if (sizeIncreased == false && sticky == false)
        {
            sizeIncreased = true;       // marcamos el flag que inciiara la cuenta atras
            transform.localScale = new Vector3(transform.localScale.x * multiplier, transform.localScale.y * multiplier, 1);  // APLICAMOS EL NUEVO TAMAÑO
        }
 
    }

    // funcion que activara el modo sticky (la bola sera la encargada de gestionar el impacto con la pala y pegarse
    public void TurnSticky ()
    {
        if (sticky == false && sizeIncreased == false)
        {
            sticky = true;
        }

    }

    // reproduciremos un sonido cuanco cojamos el powerUp
    public void ReproducePowerUpCachSound (AudioClip sfx)
    {
        speaker.clip = sfx;
        speaker.Play();
        Debug.Log("REPRODUCIMOS SONIDO " + sfx.ToString());
    }


    // una vesz las vidas cambien en el player updatearemos la inofo en el hud 
    private void UpdateHUDLifes()
    {
        hudLifes.text = playerLifes.ToString();
    }
    private void UpdateHUDScore ()
    {
        hudScore.text = playerScore.ToString();
    }
}

public static class DataStorage
{

    public static int storedPoints;
    public static int remainingLifes;

}