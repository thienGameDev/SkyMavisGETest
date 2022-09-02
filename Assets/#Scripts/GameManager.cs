using System.Collections;
using AxieMixer.Unity;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace _Scripts
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Button startBtn;
        [SerializeField] private string attackerAxieId = "4191804";
        [SerializeField] private string defenderAxieId = "2724598";
        private bool _isPlaying;
        private float _currentTimeScale;

        private void OnEnable() {
            startBtn.onClick.AddListener(OnStart);
        }

        private void OnDisable() {
            startBtn.onClick.RemoveListener(OnStart);
        }

        private void Awake() {
            PlayerPrefs.SetString("attackerId", attackerAxieId);
            PlayerPrefs.SetString("defenderId", defenderAxieId);
            LoadingAxieGenes();
            _currentTimeScale = 0f;
        }

        // Start is called before the first frame update
        private void Start()
        {
            Time.timeScale = _currentTimeScale;

            Mixer.Init();
            
            // Debug.Log(PlayerPrefs.GetString("defenderGenes"));
        }

        // Update is called once per frame
        private void Update()
        {
            
        }
        

        private void OnStart() {
            _currentTimeScale = 1f;
            Time.timeScale = _currentTimeScale;
            startBtn.gameObject.SetActive(false);
        }
        
        public void OnResume() {
            Time.timeScale = _currentTimeScale;
            _isPlaying = true;
        }

        public void OnPause() {
            Time.timeScale = 0f;
            _isPlaying = false;
        }

        public void OnIncreasingSpeed() {
            Time.timeScale = _currentTimeScale * 3f;
        }
        
        private void LoadingAxieGenes() {
            var attackerGenes = PlayerPrefs.GetString("attackerGenes");
            var defenderGenes = PlayerPrefs.GetString("defenderGenes");
            if (string.IsNullOrEmpty(attackerGenes)) 
                StartCoroutine(GetAxiesGenes(attackerAxieId, "attacker"));
            if (string.IsNullOrEmpty(defenderGenes)) 
                StartCoroutine(GetAxiesGenes(defenderAxieId, "defender"));
        }
        
        public IEnumerator GetAxiesGenes(string axieId, string type)
        {
            string searchString = "{ axie (axieId: \"" + axieId + "\") { id, genes, newGenes}}";
            JObject jPayload = new JObject();
            jPayload.Add(new JProperty("query", searchString));

            var wr = new UnityWebRequest("https://graphql-gateway.axieinfinity.com/graphql", "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jPayload.ToString().ToCharArray());
            wr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            wr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            wr.SetRequestHeader("Content-Type", "application/json");
            wr.timeout = 10;
            yield return wr.SendWebRequest();
            if (wr.error == null)
            {
                var result = wr.downloadHandler != null ? wr.downloadHandler.text : null;
                if (!string.IsNullOrEmpty(result))
                {
                    JObject jResult = JObject.Parse(result);
                    string genesStr = (string)jResult["data"]["axie"]["newGenes"];
                    PlayerPrefs.SetString($"{type}Genes", genesStr);
                }
            }
        }
    }
}
