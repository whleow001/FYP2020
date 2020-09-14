using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevealGenerator : MonoBehaviour
{
    public float generatorDistance = 13.0f;

    private void MarkGenerator()
    {
        GameObject[] allGenerators = GameObject.FindGameObjectsWithTag("Generator");
        GameObject closestGenerator;

        if (allGenerators != null)
        {
            foreach (GameObject generator in allGenerators)
            {
                float distance = Vector3.Distance(generator.transform.position, transform.position);
                GameObject iconVisible = generator.transform.Find("GeneratorIcon").gameObject;

                if (distance < generatorDistance && iconVisible.activeSelf == false)
                {
                    closestGenerator = generator;
                    iconVisible.SetActive(true);
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject fov = transform.Find("FieldOfView").gameObject;
        if (fov != null)
        {
            generatorDistance = fov.transform.localScale.x;
        }
    }

    // Update is called once per frame
    void Update()
    {
        MarkGenerator();
    }
}
