using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicController : MonoBehaviour {

    [Header("Musica")]
    public AudioSource speaker;     // audiosource
    [Header("Canciones Ordenadas por indice de nivel")]
    public AudioClip[] songs;       // array de canciones determinadas por el nivel de juegho

	// Use this for initialization
	void Start () {

        int currentScene = SceneManager.GetActiveScene().buildIndex;

        // dependiedo de la escena en la que estemos sonara una musica u otra
        switch (currentScene)
        {
            case (0):
                speaker.clip = songs[0];
                break;
            case (1):
                speaker.clip = songs[1];
                break;
            default:

                Debug.LogError("Unknwon scene");
                break;
        }
        
        speaker.Play();

	}
}
