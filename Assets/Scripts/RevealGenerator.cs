using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevealGenerator : MonoBehaviour
{
    public float generatorDistance = 13.0f;
    public GameObject fov;

    private void MarkGenerator()
    {
        GameObject[] allGenerators = GameObject.FindGameObjectsWithTag("Generator");

        if (allGenerators != null)
        {
            foreach (GameObject generator in allGenerators)
            {
                float distance = Vector3.Distance(generator.transform.position, transform.position);
                GameObject iconVisible = generator.transform.Find("GeneratorIcon").gameObject;

                if (distance < generatorDistance && iconVisible.activeSelf == false)
                {
                    iconVisible.SetActive(true);
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (fov != null)
        {
            generatorDistance = fov.transform.localScale.x/2 - 2;
        }
    }

    // Update is called once per frame
    void Update()
    {
        MarkGenerator();
    }
}
