using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AIDirectorsHolder : MonoBehaviour
{
    private List<AIDirector> aiDirectors = new List<AIDirector>();
    private GameDirector director;

    private bool hasReset = false;
    private bool hasCleared = false;

    void Awake() {
      DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update() {
      string sceneName = SceneManager.GetActiveScene().name;

      if (sceneName == "Rebel HQ_B" && !hasReset) {
        foreach (AIDirector aiDirector in aiDirectors) {
          aiDirector.ResetOnNewScene();
        }

        hasReset = true;
      }

      else if (sceneName == "RoomsV2" && !hasCleared) {
        aiDirectors = new List<AIDirector>();
        hasCleared = true;
      }

      else if (sceneName == "Rebel HQ" && hasReset) {
        hasReset = false;
      }
    }

    public void SetDirector(GameDirector _director) {
      director = _director;

      foreach (AIDirector aiDirector in aiDirectors)
        aiDirector.SetDirector(director);
    }

    public void AddAIDirector(AIDirector aiDirector) {
      aiDirectors.Add(aiDirector);
    }

    public void CreditBotKill(int botPosition) {
      aiDirectors[botPosition].CreditBotKill();
    }

    public List<AIDirector> GetAIDirectorList() {
      return aiDirectors;
    }
}
