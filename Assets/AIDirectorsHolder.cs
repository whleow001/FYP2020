using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AIDirectorsHolder : MonoBehaviour
{
    private List<AIDirector> aiDirectors = new List<AIDirector>();
    private GameDirector director;

    private bool hasReset = false;

    void Awake() {
      DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update() {
      //print(SceneManager.GetActiveScene().name);
      if (SceneManager.GetActiveScene().name == "Rebel HQ_B" && !hasReset) {
        foreach (AIDirector aiDirector in aiDirectors) {
          aiDirector.ResetOnNewScene();
        }

        hasReset = true;
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
