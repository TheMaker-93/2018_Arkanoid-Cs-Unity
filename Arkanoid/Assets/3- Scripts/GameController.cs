using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    public int level = 0;           // nivel de juego
    public GameObject brickskContainer;       // padre de todos los bricks del juego
    public enum gameStatus
    {
        play,
        endLevel,
        gameLost,
    }
    public gameStatus gStatus;

    public PlayerController playerController;

    [Header("Interface Elements")]
    public GameObject levelClearCanvas;
    public GameObject levelLostCanvas;



	// Use this for initialization
	void Start () {

        // el estado inicial sera el de waiting para esperar que el jugador dispare
		gStatus = gameStatus.play;

    }
	
	// Update is called once per frame
	void Update () {

        // maquina de estados del juego
        switch (gStatus)
        {

            case (gameStatus.play):

                // comprobamos si ya no quedan bricks
                if (CheckEndLevel() || Input.GetKeyDown(KeyCode.Q)) 
                {
                    gStatus = gameStatus.endLevel;
                }
                // comprovamos que al player le quede salud
                else if (playerController.playerLifes == 0 || Input.GetKeyDown(KeyCode.Insert))
                {
                    gStatus = gameStatus.gameLost;
                }

                break;
            case (gameStatus.gameLost):

                levelLostCanvas.SetActive(true);

                Time.timeScale = 0f;            // paramos el tiempo
                if (Input.anyKeyDown)
                {
                    // ocultamos el mensaje por si a caso
                    levelLostCanvas.SetActive(false);

                    Time.timeScale = 1f;            // reanudamos el tiempo ( se que depende para que cosas no funciona pero para simplificar opto por esta solucion)

                    // empezamos el juego desde el primer nivel
                    SceneManager.LoadScene(0, LoadSceneMode.Single);

                }


                break;


            case (gameStatus.endLevel):
                // el juego ya habra acabado y estaremos esperando al input del player
                // pasra pasar al siguiente nivel

                // activamos el panel que muestra que hemos acabado el nivel
                levelClearCanvas.SetActive(true);
                DataStorage.remainingLifes = playerController.playerLifes;
                DataStorage.storedPoints = playerController.playerScore;
                Time.timeScale = 0f;            // paramos el tiempo

                if (Input.anyKeyDown)
                {
                    levelClearCanvas.SetActive(false);
                    Time.timeScale = 1f;            // reanudamos el tiempo ( se que depende para que cosas no funciona pero para simplificar opto por esta solucion)
                    SceneManager.LoadScene(1);      // no hay mas escenas por eso cargamos la misma escena de nuevo
                }

                break;
        }



	}

    // funcion que comprobara si quedan o no bricks en juego
    private bool CheckEndLevel()
    {
        // sino quedan hijos en el contenedor de los bricks
        if (brickskContainer.transform.childCount == 0)
        {
            Debug.LogWarning("El juego ha acabado");
            // deberiamos mostrar un hud avisando que el juego ha acabado y esperar
            // a qu eel player le de a x tecla para seguir
            return true;
        } else
        {
            return false;
        }
    }
}
