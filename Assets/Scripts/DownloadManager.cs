using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class DownloadManager : MonoBehaviour
{

    public bool downloaded = false;
    private void Awake()
    {
        if (GameObject.Find("DownloadManager"))
        {
            if(GameObject.Find("DownloadManager") != this.gameObject)
            {
                Destroy(GameObject.Find("DownloadManager"));
            }
        }

        DontDestroyOnLoad(this.gameObject);
#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif
    }

    private void Start()
    {
        StartCoroutine(SaveToPersistentPath());
    }

    private void Update()//move this code
    {
        if (downloaded)
        {
            Debug.Log("Already downloaded remaining levels");
            return;
        }
        if (Application.internetReachability == NetworkReachability.NotReachable) 
        {
            Debug.Log("No internet connection, couldn't download remaining levels");
            return;
        } 
        
        DownloadRemainingLevels();
        downloaded= true;
    }

    public void DownloadRemainingLevels()
    {
        Debug.Log("Downloading remaining 15 levels");
        for (int i = 1; i <= 5; i++)
        {
            StartCoroutine(DownloadLevel("A1" + i.ToString()));
        }
        for (int i = 1; i <= 10; i++)
        {
            StartCoroutine(DownloadLevel("B" + i.ToString()));
        }
    }

    IEnumerator DownloadLevel(string levelID)
    {
        string url = "https://row-match.s3.amazonaws.com/levels/RM_" + levelID;
        
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
            }
            else
            {
                string savePath = string.Format("{0}/{1}", Application.persistentDataPath, "Levels/RM_"+levelID);
                System.IO.File.WriteAllText(savePath, www.downloadHandler.text);
            }
        }
    }

    public IEnumerator SaveToPersistentPath()
    {
        Debug.Log("Streaming assets path is: " + Application.streamingAssetsPath);
        Debug.Log("Saving first 10 levels to the persistent data path");
        if (!Directory.Exists(Application.persistentDataPath + "/Levels"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Levels");
        }
        if (File.Exists(Application.persistentDataPath + "/Levels/RM_A1")) 
        {
            yield break;
        }
        
        for (int i = 1; i <= 10; i++)
        {
            //File.Copy(Resources.Load("/Levels/RM_A" + i.ToString()) as File, Application.persistentDataPath + "/Levels/RM_A" + i.ToString());
            
            string fileInData = Application.persistentDataPath + "/Levels/RM_A" + i.ToString();
            string fileInApk = Application.streamingAssetsPath + "/Levels/RM_A" + i.ToString();
            if (!File.Exists(fileInData))
            {
                var request = UnityWebRequest.Get(fileInApk);
                var download = new DownloadHandlerFile(fileInData);
                request.downloadHandler = download;
                yield return request.SendWebRequest();
            }
        }
        
    }
}
