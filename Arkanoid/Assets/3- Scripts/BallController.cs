using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour {

    [Header("Player")]
    public GameObject player;              // para poder colisionar con el
    private PlayerController playerController;
    public SpriteRenderer playerSpriteRenderer;     // para calcular su tamaño a la hora de impactar
    public float playerWidth, playerHeight;
    public float distanceFromPlayer;       // distancia del player al estar pegada

    public SpriteRenderer leftImpactZoneSprRenderer;           // helpers para el diseñador poder delimitar las zonas de rebote de la pala a su gusto sin tocar codigo o numeros
    public SpriteRenderer rightImpactZoneSprRenderer;
    public SpriteRenderer playAreaSpriteRenderer;               // comprobaremos que no hayamos salido de este para restar vidas

    [Header("Ball Atributes")]
    public Vector3 direction;
    public float speed;
    public bool isFree;           // controla si esta en la pala o no
    public float radius;
    private SpriteRenderer ballSpriteRenderer;

    [Header("Slow down")]
    private float slowDownMultiplier;    
    public bool slowedDown;         // controla si vamos lentos o no
    public float slowDonwTimer;     // controla el tiempo que llevamos con el powerup de lento activo
    public float slowDownTime;      // tiempo que estaremos afectados por el slow down

    [Header("Enviorment")]
    public float hBorderWidth;
    public GameObject leftBorder;
    public GameObject rightBorder;
    public float vBoderHeight;
    public GameObject topBorder;

    public GameObject[] bricksCollection;       // todos los bricks del juego
    public SpriteRenderer[] bricksSpriteRenderesCollection;     // los spriterenderes de todos los bricks en el juego
    public BrickController[] brickControllersCollection;        // coleccion de bricks para acceder a su tipo y dar una puntuacion correspondiente a este
    private float calculationMargin = 1f;                            // distancia a un lado del bloque que nos permitira afirmar con claredad que impactaremos en el

    [Header("SFX")]
    public AudioSource speaker;         // altavoz que emitira el sonido de impacto
    public AudioClip impactSound;       // sonido de impacto
    public AudioClip brickImpactSound;  // sonido de impacto con el ladrillo
    public AudioClip lifeLostSound;     // sonido que sonara al perder una vida

	// Use this for initialization
	void Start () {

        // empezara cautiva
        isFree = false;
        // cargamos el sprite renderer
        ballSpriteRenderer = GetComponent<SpriteRenderer>();
        // calculo de los tamaños de los marcos
        hBorderWidth = leftBorder.GetComponent<SpriteRenderer>().bounds.size.x;      // usamos uno de referencia debido a que son ambos identicos
        vBoderHeight = topBorder.GetComponent<SpriteRenderer>().bounds.size.y;
        
        // guardamos el tamaño del player para calcular la colision con el
        playerWidth = playerSpriteRenderer.bounds.size.x;
        playerHeight = playerSpriteRenderer.bounds.size.y;
        // guardamos una referencia a su codigo
        playerController = player.GetComponent<PlayerController>();
        radius = ballSpriteRenderer.bounds.extents.x;                     // TEMPORAL HASTA ACTUALIZAR LAS COLISIONES CON LO LADRILLOS

        // rellenamos la bricksSpriteRenderesCollection durante el start del juego (algo mas lento al empear el nivel pero mas sencillo para el diseñador)
        // tambien cargamos sus scripts de control para poder a la hora de destruirlos determinar de que tipo son
        bricksSpriteRenderesCollection = new SpriteRenderer[bricksCollection.Length];
        brickControllersCollection = new BrickController[bricksCollection.Length];
        for (int i = 0; i <= bricksCollection.Length - 1; i += 1)
        {
            bricksSpriteRenderesCollection[i] = bricksCollection[i].GetComponent<SpriteRenderer>();
            brickControllersCollection[i] = bricksCollection[i].GetComponent<BrickController>();
        }

        // direccion vertical hacia arriba
        //direction = new Vector3(-1, 1, 0);       // EL TIRO HA DE SER VERTICAL, CUIDADO CON EL 0.5
        // posicionamiento inicial de la bola en el eje del player
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y + distanceFromPlayer, player.transform.position.z);
        // cargamos el sonido en el audiosource
        speaker.clip = impactSound;

    }

    // Update is called once per frame
    void Update () {
        Move();


        // DEBUG FUNCIONA EL POWER UP FALTA HACER QUE LO LLAME AL COLISIONAR CON 


        #region Slow Donw
        // si se ha activado el power up de slow donw
        if (slowedDown)
        {

            // IMPORTANTE
            // NO LE CAMBIO EL SPRITE A LA NAVE PORQUE NO HAY MAS SPRITES DIFERENTES

            // si aun no hemos llegado al limite de tiempo
            if (slowDonwTimer < slowDownTime)
            {
                // aumentamos su valor
                slowDonwTimer += Time.deltaTime;
            }
            // si ya hemos llegado al limite de tiempo
            else
            {
                slowedDown = false;
                speed = speed * slowDownMultiplier;
                slowDonwTimer = 0f;
            }
        }
        #endregion

    }

    void Move ()
    {
        // linea del forward de la bola
        Debug.DrawLine(transform.position, transform.position + direction * 10,Color.cyan);

        // cuando la bola es libre
        if (isFree)
        {

            EnviormentCollisions();
            BrickCollisions();
            PlayerCollisions();

            // aplicamos el movsimiento
            transform.position = transform.position + direction * speed * Time.deltaTime;

        // cuando esta confinada al player
        } else if (isFree == false)
        {
                // aqui haremos que siga al player en su eje x estando centrada en este
                transform.position = new Vector3(player.transform.position.x, player.transform.position.y + distanceFromPlayer, player.transform.position.z);

        }
    }

    // para liberar o capturar la bola
    public void SetFree ( bool onOf)
    {
        isFree = onOf;

    }

    // para hacer que la bola vaya a la mitad de velocidad durante 30 segundos
    public void ReduceSpeed ( int slowDownMutli )
    {
        if (slowedDown == false)
        {
            slowDownMultiplier = slowDownMutli;
            speed = speed / slowDownMultiplier;
            slowedDown = true;          // activamos el flag para que el timer sepa que debe actuar
        }

    }

    // calculo de las colisiones con el entorno
    private void EnviormentCollisions()
    {
        // Colision con la pared izquierda
        if (ballSpriteRenderer.bounds.min.x <= playAreaSpriteRenderer.bounds.min.x)
        {
            speaker.clip = impactSound;
            speaker.Play();
            direction.x = Mathf.Abs(direction.x);
            // colision con la pared derecha
        }
        else if (ballSpriteRenderer.bounds.max.x >= playAreaSpriteRenderer.bounds.max.x)
        {
            speaker.clip = impactSound;
            speaker.Play();
            direction.x = -Mathf.Abs(direction.x);
        }
        // colision con el techo
        if (ballSpriteRenderer.bounds.max.y >= playAreaSpriteRenderer.bounds.max.y)
        {
            speaker.clip = impactSound;
            speaker.Play();
            direction.y = -Mathf.Abs(direction.y);
        }

        // comprobamos que siga dentro del area de juego
        else if (transform.position.y + radius < playAreaSpriteRenderer.bounds.min.y)
        {
            Debug.LogWarning("PELOTA FUERA DE JUEGO");

            speaker.clip = lifeLostSound;
            speaker.Play();

            // disminuimos la salud del player
            playerController.DecreaseLifes();
            // pegamos de nuevo la bola al player
            SetFree(false);
            direction = new Vector2(0, 1);
        }
    }

    // calculo de las colisiones con los bricks
    private void BrickCollisions()
    {
        bool impacted = false;

        // iteramos por el array de bricks
        for (int i = 0; i <= bricksCollection.Length - 1; i++)
        {
            // si la posicion a calcular no esta vacia y no hemos impactado ya
            if (bricksCollection[i] != null && impacted == false)
            {
                    // calculamos colisiones
                    // si estamos entre sus x
                    if (transform.position.x + radius >= bricksCollection[i].transform.position.x - bricksSpriteRenderesCollection[i].bounds.extents.x &&
                    transform.position.x - radius <= bricksCollection[i].transform.position.x + bricksSpriteRenderesCollection[i].bounds.extents.x)
                    {
                        Debug.DrawLine(transform.position, bricksCollection[i].transform.position, Color.yellow);

                        // impacto inferior ( si estamos mas arriba del limite inferior del bloque pero no por encima de la mitad de este
                        if (transform.position.y + radius >= bricksCollection[i].transform.position.y - bricksSpriteRenderesCollection[i].bounds.extents.y &&
                            transform.position.y + radius < bricksCollection[i].transform.position.y &&
                            direction.y > 0)
                        {

                            direction.y = -Mathf.Abs(direction.y);
                            Debug.Log("Impacto con borde inferior de brick");
                            impacted = true;

                            // IMPACTO SUPERIOR
                        }
                        else if (transform.position.y - radius <= bricksCollection[i].transform.position.y + bricksSpriteRenderesCollection[i].bounds.extents.y &&
                          transform.position.y - radius > bricksCollection[i].transform.position.y &&
                          direction.y < 0)
                        {
                            direction.y = Mathf.Abs(direction.y);
                            Debug.Log("impacto con borde superior de brick");
                            impacted = true;

                        } 

                        // CUANDO ESTE ENTRE LAS Y DEL BRICK
                        else if (transform.position.y + radius >= bricksCollection[i].transform.position.y - bricksSpriteRenderesCollection[i].bounds.extents.y &&
                            transform.position.y - radius <= bricksCollection[i].transform.position.y + bricksSpriteRenderesCollection[i].bounds.extents.y)
                        {
                            // IMPACTO LATERAL DERECHO
                            if (transform.position.x - radius <= bricksCollection[i].transform.position.x + bricksSpriteRenderesCollection[i].bounds.extents.x &&
                                transform.position.x - radius > bricksCollection[i].transform.position.x &&
                                direction.x < 0)
                            {

                                direction.x = Mathf.Abs(direction.x);
                                Debug.Log("impacto con borde DERECHO de brick");
                                impacted = true;

                                // IMPACTO LATERAL IZQUIERDO
                            }
                            else if (transform.position.x + radius >= bricksCollection[i].transform.position.x - bricksSpriteRenderesCollection[i].bounds.extents.x &&
                                transform.position.x + radius < bricksCollection[i].transform.position.x &&
                                direction.x > 0)
                            {
                                direction.x = -Mathf.Abs(direction.x);
                                Debug.Log("impacto con borde IZQUIERDO de brick");
                                impacted = true;

                            }
                        }
                    }

                }

                // si se ha detectado una colision
                if (impacted == true)
                {
                    // reducimos su salud
                    brickControllersCollection[i].DiminishHealth();
                    // sonido
                    speaker.clip = brickImpactSound;   // si hemos colisionado con algun ladrillo cambiamos nuestro sonido
                    speaker.Play();

                    // si se ha detectado un impacto salimos de bucle pero antes resproducimos el sonido
                    break;
                }

        }

    }
    
    // NOT WORKING ------ //
    private void ImprovedBrickCollisions()
    {
        bool impacted = false;      // CONTROL DE IMPACTO

        // iteramos por todos los ladrillos
        for (int i = 0; i < bricksCollection.Length; i++)
        {
            // comprobamos que hay objeto con el cual calcular impacto
            if (bricksCollection[i] != null)
            {
                // si no hemos impactado en el update actual calculamos colision
                if (impacted == false)
                {       
                    // IMPACTO INFERIOR
                    // si estamos por debajo del brick y justo tocandolo en la y, ademas nuestro minimo derecho y maximo izquierdo estan en sus x
                    if (transform.position.y < bricksCollection[i].transform.position.y &&
                        ballSpriteRenderer.bounds.max.y >= bricksSpriteRenderesCollection[i].bounds.min.y &&
                        // limites horizonatles
                        ballSpriteRenderer.bounds.max.x >= bricksSpriteRenderesCollection[i].bounds.min.x &&
                        ballSpriteRenderer.bounds.min.x <= bricksSpriteRenderesCollection[i].bounds.max.x )
                    {

                        direction.y = -Mathf.Abs(direction.y);
                        impacted = true;
                        Debug.LogWarning("IMPACTO INFERIOR");

                        // IMPACTO SUPERIOR
                    } else if (transform.position.y > bricksCollection[i].transform.position.y &&
                        ballSpriteRenderer.bounds.min.y <= bricksSpriteRenderesCollection[i].bounds.max.y &&
                        // limites horizonatles
                        ballSpriteRenderer.bounds.max.x >= bricksSpriteRenderesCollection[i].bounds.min.x &&
                        ballSpriteRenderer.bounds.min.x <= bricksSpriteRenderesCollection[i].bounds.max.x)
                    {

                        direction.y = Mathf.Abs(direction.y);
                        impacted = true;
                        Debug.LogWarning("IMPACTO INFERIOR");


                        // IMPACTO LATERAL DERECHO
                        // LA BOLA debe tener una posicion en la x mayor que la del bloque
                        // 
                    } else if (transform.position.x > bricksCollection[i].transform.position.x &&
                        ballSpriteRenderer.bounds.min.x <= bricksSpriteRenderesCollection[i].bounds.max.x &&
                        // limites verticales
                        ballSpriteRenderer.bounds.max.y >= bricksSpriteRenderesCollection[i].bounds.min.y &&
                        ballSpriteRenderer.bounds.min.y <= bricksSpriteRenderesCollection[i].bounds.max.y)
                    {

                        direction.x = Mathf.Abs(direction.x);
                        impacted = true;
                        Debug.LogWarning("IMPACTO DERECHO");

                        // IMPACTO LATERAL IZQUIERDO
                    } else if (transform.position.x < bricksCollection[i].transform.position.x &&
                        ballSpriteRenderer.bounds.max.x >= bricksSpriteRenderesCollection[i].bounds.min.x &&
                        // limites verticales
                        ballSpriteRenderer.bounds.max.y >= bricksSpriteRenderesCollection[i].bounds.min.y &&
                        ballSpriteRenderer.bounds.min.y <= bricksSpriteRenderesCollection[i].bounds.max.y)
                    {

                        direction.x =  - Mathf.Abs(direction.x);
                        impacted = true;
                        Debug.LogWarning("IMPACTO IZQUIERDO");

                    }

                }


                // si se da el caso que despues de calcular colisiones con ese ladrillo ha dado positivo en impacto :
                if (impacted)
                {
                    // reducimos su salud
                    brickControllersCollection[i].DiminishHealth();
                    // sonido
                    speaker.clip = brickImpactSound;   // si hemos colisionado con algun ladrillo cambiamos nuestro sonido
                    speaker.Play();

                    // si se ha detectado un impacto salimos de bucle pero antes resproducimos el sonido
                    break;
                }

            }
        }


    }
    private void ImprovedBrickCollisions2()
    {
        bool impacted = false;      // CONTROL DE IMPACTO

        // iteramos por todos los ladrillos
        for (int i = 0; i < bricksCollection.Length; i++)
        {
            // comprobamos que hay objeto con el cual calcular impacto
            if (bricksCollection[i] != null)
            {
                // si no hemos impactado en el update actual calculamos colision
                if (impacted == false)
                {

                    // IMPACTO INFERIOR
                    // si estamos por debajo del brick y justo tocandolo en la y, ademas nuestro minimo derecho y maximo izquierdo estan en sus x
                    if (transform.position.y < bricksCollection[i].transform.position.y &&
                        ballSpriteRenderer.bounds.max.y >= bricksSpriteRenderesCollection[i].bounds.min.y &&
                        // limites horizonatles
                        ballSpriteRenderer.bounds.max.x >= bricksSpriteRenderesCollection[i].bounds.min.x &&
                        ballSpriteRenderer.bounds.min.x <= bricksSpriteRenderesCollection[i].bounds.max.x)
                    {

                        direction.y = -Mathf.Abs(direction.y);
                        impacted = true;
                        Debug.LogWarning("IMPACTO INFERIOR");

                        // IMPACTO SUPERIOR
                    }
                    else if (transform.position.y > bricksCollection[i].transform.position.y &&
                      ballSpriteRenderer.bounds.min.y <= bricksSpriteRenderesCollection[i].bounds.max.y &&
                      // limites horizonatles
                      ballSpriteRenderer.bounds.max.x >= bricksSpriteRenderesCollection[i].bounds.min.x &&
                      ballSpriteRenderer.bounds.min.x <= bricksSpriteRenderesCollection[i].bounds.max.x)
                    {

                        direction.y = Mathf.Abs(direction.y);
                        impacted = true;
                        Debug.LogWarning("IMPACTO INFERIOR");


                        // IMPACTO LATERAL DERECHO
                        // LA BOLA debe tener una posicion en la x mayor que la del bloque
                        // 
                    }
                    else if (transform.position.x > bricksCollection[i].transform.position.x &&
                      ballSpriteRenderer.bounds.min.x <= bricksSpriteRenderesCollection[i].bounds.max.x &&
                      // limites verticales
                      ballSpriteRenderer.bounds.max.y >= bricksSpriteRenderesCollection[i].bounds.min.y &&
                      ballSpriteRenderer.bounds.min.y <= bricksSpriteRenderesCollection[i].bounds.max.y)
                    {

                        direction.x = Mathf.Abs(direction.x);
                        impacted = true;
                        Debug.LogWarning("IMPACTO DERECHO");

                        // IMPACTO LATERAL IZQUIERDO
                    }
                    else if (transform.position.x < bricksCollection[i].transform.position.x &&
                      ballSpriteRenderer.bounds.max.x >= bricksSpriteRenderesCollection[i].bounds.min.x &&
                      // limites verticales
                      ballSpriteRenderer.bounds.max.y >= bricksSpriteRenderesCollection[i].bounds.min.y &&
                      ballSpriteRenderer.bounds.min.y <= bricksSpriteRenderesCollection[i].bounds.max.y)
                    {

                        direction.x = -Mathf.Abs(direction.x);
                        impacted = true;
                        Debug.LogWarning("IMPACTO IZQUIERDO");

                    }

                }


                // si se da el caso que despues de calcular colisiones con ese ladrillo ha dado positivo en impacto :
                if (impacted)
                {
                    // reducimos su salud
                    brickControllersCollection[i].DiminishHealth();
                    // sonido
                    speaker.clip = brickImpactSound;   // si hemos colisionado con algun ladrillo cambiamos nuestro sonido
                    speaker.Play();

                    // si se ha detectado un impacto salimos de bucle pero antes resproducimos el sonido
                    break;
                }

            }
        }


    }
    // ------------------- //

    // colisiones con el player
    private void PlayerCollisions()
    {
        // si estasmos entre las dos zonas de impacto 
        if (ballSpriteRenderer.bounds.max.x >= leftImpactZoneSprRenderer.bounds.min.x &&
            ballSpriteRenderer.bounds.min.x <= rightImpactZoneSprRenderer.bounds.max.x)
        {
            // si estamos justo tocando la zona superior
            if (ballSpriteRenderer.bounds.min.y <= playerSpriteRenderer.bounds.max.y &&
                ballSpriteRenderer.bounds.min.y >= player.transform.position.y)
            {
                // ANTES DE APLICAR LA NUEVA DIRECCION COMPROVAREMOS EN QUE SECCION HEMOS IMPACTADO DE LA PALA
                // area de impacto izquierda
                if (ballSpriteRenderer.bounds.max.x >= leftImpactZoneSprRenderer.bounds.min.x &&
                    ballSpriteRenderer.bounds.min.x <= leftImpactZoneSprRenderer.bounds.max.x)
                {
                    direction.x = -0.75f;
                    direction.y = 0.75f; // direccion positiva en la y para subir
                    speaker.clip = impactSound;
                    speaker.Play();
                    //Debug.LogWarning("Impacto en area IZQUIERDA de la pala");

                    // area de impacto derecha
                }
                else if (ballSpriteRenderer.bounds.max.x >= rightImpactZoneSprRenderer.bounds.min.x &&
                        ballSpriteRenderer.bounds.min.x <= rightImpactZoneSprRenderer.bounds.max.x)
                {
                    direction.x = 0.75f;
                    direction.y = 0.75f; // direccion positiva en la y para subir
                    speaker.clip = impactSound;
                    speaker.Play();
                    //Debug.LogWarning("Impacto en area DERECHA de la pala");

                }
                // Area de impacto central (restante)
                else if (ballSpriteRenderer.bounds.max.x > leftImpactZoneSprRenderer.bounds.max.x &&
                        ballSpriteRenderer.bounds.min.x < rightImpactZoneSprRenderer.bounds.min.x)
                {
                    direction.x = 0;
                    direction.y = 1f; // direccion positiva en la y para subir
                    speaker.clip = impactSound;
                    speaker.Play();
                    //Debug.LogWarning("Impacto en el area CENTRAL de la pala");
                }
                else
                {
                    Debug.Log("Impacto en area NO IMPACTABLE");
                }
            }

        }
    }




}

