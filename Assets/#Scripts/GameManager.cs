using System;
using System.Collections;
using AxieMixer.Unity;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace _Scripts
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject preGameUI;
        [SerializeField] private Button startBtn;
        [SerializeField] private TMP_InputField attackerCountInput;
        [SerializeField] private TMP_InputField defenderCountInput;
        [SerializeField] private GameObject inGameUI;
        [SerializeField] private Button pauseBtn;
        [SerializeField] private Button increaseSpeedBtn;
        [SerializeField] private Button decreaseSpeedBtn;
        
        [SerializeField] private string attackerAxieId = "4191804";
        [SerializeField] private string defenderAxieId = "2724598";
        
        private bool _isPlaying;
        private float _currentTimeScale;
        private const float MAX_INCREASE = 16f;
        private const float MIN_DECREASE = 0.5f;
        private void OnEnable() {
            startBtn.onClick.AddListener(OnStart);
            pauseBtn.onClick.AddListener(OnPauseOrResume);
            increaseSpeedBtn.onClick.AddListener(OnIncreasingSpeed);
            decreaseSpeedBtn.onClick.AddListener(OnDecreasingSpeed);
        }

        private void OnDisable() {
            startBtn.onClick.RemoveListener(OnStart);
            pauseBtn.onClick.RemoveListener(OnPauseOrResume);
            increaseSpeedBtn.onClick.RemoveListener(OnIncreasingSpeed);
            decreaseSpeedBtn.onClick.RemoveListener(OnDecreasingSpeed);
        }
        
        private void Awake() {
            Mixer.Init();
            PlayerPrefs.SetString("attackerId", attackerAxieId);
            PlayerPrefs.SetString("defenderId", defenderAxieId);
            LoadingAxieGenes();
            _currentTimeScale = 0f;
        }

        // Start is called before the first frame update
        private void Start()
        {
            Time.timeScale = _currentTimeScale;
            var attackerCount = PlayerPrefs.GetString("attackerCount", "20");
            var defenderCount = PlayerPrefs.GetString("defenderCount", "10");
            attackerCountInput.text = attackerCount;
            defenderCountInput.text = defenderCount;
        }
        
        private void OnStart() {
            EventManager.StartListening("EndGame", EndGame);
            var attackerCount = attackerCountInput.text;
            var defenderCount = defenderCountInput.text;
            PlayerPrefs.SetString("attackerCount", attackerCount);
            PlayerPrefs.SetString("defenderCount", defenderCount);
            Spawner.Instance.SpawnAxies(Convert.ToInt32(attackerCount), Convert.ToInt32(defenderCount));
            _currentTimeScale = 1f;
            Time.timeScale = _currentTimeScale;
            preGameUI.gameObject.SetActive(false);
            inGameUI.SetActive(true);
            _isPlaying = true;
        }

        private void OnPauseOrResume() {
            Time.timeScale = _isPlaying ? 0f : _currentTimeScale;
            pauseBtn.GetComponentInChildren<Text>().text = _isPlaying ? "Resume" : "Pause";
            increaseSpeedBtn.enabled = decreaseSpeedBtn.enabled = !_isPlaying;
            _isPlaying = !_isPlaying;
        }

        private void OnIncreasingSpeed() {
            _currentTimeScale *= 2f;
            if (_currentTimeScale > MAX_INCREASE) _currentTimeScale = MAX_INCREASE;
            Time.timeScale = _currentTimeScale;
        }

        private void OnDecreasingSpeed() {
            _currentTimeScale /= 2f;
            if (_currentTimeScale < MIN_DECREASE) _currentTimeScale = MIN_DECREASE;
            Time.timeScale = _currentTimeScale;
        }
        
        private void EndGame(int param) {
            _currentTimeScale = 1f;
            Time.timeScale = _currentTimeScale;
            EventManager.StopListening("EndGame", EndGame);
            inGameUI.SetActive(false);
            startBtn.GetComponentInChildren<Text>().text = "Restart";
            preGameUI.gameObject.SetActive(true);
        }
        
        private void LoadingAxieGenes() {
            var attackerGenes = PlayerPrefs.GetString("attackerGenes");
            var defenderGenes = PlayerPrefs.GetString("defenderGenes");
            if (string.IsNullOrEmpty(attackerGenes)) 
                StartCoroutine(GetAxiesGenes(attackerAxieId, "attacker"));
            if (string.IsNullOrEmpty(defenderGenes)) 
                StartCoroutine(GetAxiesGenes(defenderAxieId, "defender"));
        }

        private IEnumerator GetAxiesGenes(string axieId, string type)
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
