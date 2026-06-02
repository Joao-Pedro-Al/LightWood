using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Dialogo : MonoBehaviour
{
    public static Dialogo Instance;
    private dialogos JSON;

    [Header("Legendas")]
    [SerializeField]
    private TextMeshProUGUI legendas;
    [SerializeField]
    private AudioSource Audio;

    private GameObject Obj_Legends;

    private bool DialogoAtivo = false;

    void Awake()
    {
        string path = Application.streamingAssetsPath + "/dialogos.json";
        string jsonText = File.ReadAllText(path, System.Text.Encoding.UTF8);

        JSON = JsonUtility.FromJson<dialogos>(jsonText);

        Obj_Legends = legendas.gameObject;

        if(Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        } else if(Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void AtivarDialogo(int id)
    {
        foreach(dialogo d in JSON.dialogo)
        {
            if(d.id == id)
            {
                // StartCoroutine(DizerDialogo(d));
                StartCoroutine(LinhaEspera(d));
            }
        }
    }

    IEnumerator LinhaEspera(dialogo d)
    {
        while(DialogoAtivo){yield return null;}

        DialogoAtivo = true;
        StartCoroutine(DizerDialogo(d));
    }

    IEnumerator DizerDialogo(dialogo d)
    {
        if(!Obj_Legends.activeInHierarchy)
            Legendas_OnOff(true);

        // Text
        legendas.text = d.text;

        // Áudio
        if(d.file != null) // Se haver ficheiro de Áudio
        {
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, "dialogos", d.file);

            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.MPEG))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Audio.clip = DownloadHandlerAudioClip.GetContent(request);
                    Audio.Play();
                    while(Audio.isPlaying)
                    {
                        yield return null;
                    }
                }
                else
                {
                    Debug.LogError("Erro ao carregar Áudio: " + request.error);
                }
            }
        }
        else
        {
            yield return new WaitForSeconds(5); // Dar tempo para ler
        }

        // Próximo diálogo
        if(d.cont)
            AtivarDialogo(d.prox);
        else
            Legendas_OnOff(false);

        // Confirmar que o Queu está livre
        DialogoAtivo = false;
    }

    private void Legendas_OnOff(bool atv)
    {
        Obj_Legends.SetActive(atv);
    }
}
