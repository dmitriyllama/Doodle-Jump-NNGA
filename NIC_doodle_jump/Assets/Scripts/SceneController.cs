using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class SceneController : MonoBehaviour
{
    [SerializeField] private GameObject _regular_platform;
    [SerializeField] private GameObject _extrabounce_platform;
    [SerializeField] private GameObject _individual_prefab;
    [SerializeField] private Transform _start_position;
    [SerializeField] [Range(1, 10)] private int _number_of_platforms;
    [SerializeField] [Range(0, 1)] private float _extrabounce_platform_chance;
    [SerializeField] [Range(0, float.MaxValue)] private float _range_of_platform_spawn;
    [SerializeField] [Range(1, 100)] private int _number_of_nn_individuals;
    
    private float _priviest_highest_platform = 9;
    private float _priviest_lowest_platform = -5f;
    private int _NN_input_layer_size = 1;
    private List<GameObject> NNindividuals = new List<GameObject>();
    private GameObject best_individ;
    [SerializeField] private List<GameObject> platforms = new List<GameObject>();
    
    
    [SerializeField] [Range(0, 1)] private float _mutation_genome_chance;
    [SerializeField] [Range(0, 1)] private float _crossover_genome_chance;
    [SerializeField] [Range(0, 1)] private float _mutation_individ_chance;
    private int generation;
    private Dictionary<String, double> old_generation_fitness;
    private Dictionary<String, double[]> old_generation_weights;
    private Dictionary<String, double[]> new_generation_weights;
    private int _weights_num;
    
    [SerializeField] private TMP_Text _text_best_fittness;
    [SerializeField] private TMP_Text _text_ramaining_individuals;
    [SerializeField] private TMP_Text _text_generation;

    public float max_y_position = 0f;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        generation = 0;
        _NN_input_layer_size = 4 * _number_of_platforms + 2;
        
        for (int i = 0; i < _number_of_nn_individuals; i++)
        {
            NNindividuals.Add(Instantiate(_individual_prefab, _start_position.position + new Vector3(Random.Range(-3f,3f),0,0), _start_position.rotation));
            NNindividuals[i].name = i.ToString();
            NNindividuals[i].GetComponent<NeuralNetworkIndividual>().sceneController = gameObject.GetComponent<SceneController>();
            NNindividuals[i].GetComponent<NeuralNetworkIndividual>().set_input_layer_size(_NN_input_layer_size);
        }

        old_generation_weights = new Dictionary<string, double[]>();
        new_generation_weights = new Dictionary<string, double[]>();
        old_generation_fitness = new Dictionary<string, double>();
        _weights_num = NNindividuals[0].GetComponent<NeuralNetworkIndividual>().getWeightsNum();
        foreach (var iNindividual in NNindividuals)
        {
            old_generation_weights.Add(iNindividual.name, iNindividual.GetComponent<NeuralNetworkIndividual>().getWeights());
            old_generation_fitness.Add(iNindividual.name, 0);
            new_generation_weights.Add(iNindividual.name, new double[_weights_num]);
        }

        best_individ = NNindividuals[0];
        //Reset();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        for (int i = 0; i < NNindividuals.Count; i++)
        {
            setIndividualData(i);
            NNindividuals[i].GetComponent<NeuralNetworkIndividual>().decide();
            old_generation_fitness[NNindividuals[i].name] =
                NNindividuals[i].GetComponent<NeuralNetworkIndividual>().fitnessFunction();
        }

        if (max_y_position > transform.position.y)
        {
           transform.position = new Vector3(0, max_y_position, 0); 
        }

        var fitnesses = new List<double>(old_generation_fitness.Values);

        _text_best_fittness.text = fitnesses.Max().ToString();
        _text_ramaining_individuals.text = NNindividuals.Count.ToString();
        _text_generation.text = generation.ToString();
    }

    void chooseBestIndivid()
    {
        float max_score = float.MinValue;

        foreach (var VARIABLE in NNindividuals)
        {
            if (VARIABLE.GetComponent<NeuralNetworkIndividual>().getScore() > max_score)
            {
                best_individ = VARIABLE;
                max_score = VARIABLE.GetComponent<NeuralNetworkIndividual>().getScore();
            }
        }
    }

    void setIndividualData(int i)
    {
        List<double> _input_data = new List<double>();
        
        //individual velocity
        _input_data.Add(NNindividuals[i].GetComponent<Rigidbody2D>().velocity.y);
        _input_data.Add(NNindividuals[i].GetComponent<Rigidbody2D>().velocity.x);

        for (int j = 0; j < _number_of_platforms; j++)
        {
            if (platforms[j].CompareTag("Bounce_platform"))
            {
                _input_data.Add(1);    
            }
            else
            {
                _input_data.Add(0);
            }
            
        }
        
        for (int j = 0; j < _number_of_platforms; j++)
        {
            _input_data.Add(Sigmoid(NNindividuals[i].transform.position.y - platforms[j].transform.position.y));
        }
        
        for (int j = 0; j < _number_of_platforms; j++)
        {
            _input_data.Add(Sigmoid(NNindividuals[i].transform.position.x - platforms[j].transform.position.x));
        }
        
        for (int j = 0; j < _number_of_platforms; j++)
        {
            _input_data.Add(NNindividuals[i].transform.localScale.x);
        }
        
        NeuralNetworkIndividual nni = NNindividuals[i].GetComponent<NeuralNetworkIndividual>();
        nni.set_input_data(_input_data.ToArray());
    }

    public float getLowestPlatformPositionY()
    {
        return _priviest_lowest_platform;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Platform") || other.CompareTag("Bounce_platform"))
        {
            float _new_height = _priviest_highest_platform + 2f + Random.Range(0f, 5.1f);
            float _new_x = Random.Range(-_range_of_platform_spawn, _range_of_platform_spawn);
            GameObject _new_platform;
            if (Random.Range(0f, 1f) > _extrabounce_platform_chance)
            {
                _new_platform = Instantiate(_regular_platform, new Vector2(_new_x,
                    _new_height), Quaternion.identity);    
            }
            else
            {
                _new_platform = Instantiate(_extrabounce_platform, new Vector2(_new_x,
                    _new_height), Quaternion.identity); 
            }
            
            _new_platform.gameObject.transform.localScale = new Vector3(Random.Range(1f, 10f),1, 1);
            _priviest_highest_platform = _new_height;

            _priviest_lowest_platform = other.gameObject.transform.position.y;
            platforms.Remove(other.gameObject);
            platforms.Add(_new_platform);
        }
        else
        {
            NNindividuals.Remove(other.gameObject);
            
            chooseBestIndivid();
        }
        
        Destroy(other.gameObject);

        if (NNindividuals.Count == 0)
        {
            Reset();
        }
    }

    private void CreateInitialPlatforms()
    {
        float platform_position_x = 0;
        float platform_position_y = -4.5f;

        for (int i = 0; i < _number_of_platforms; i++)
        {
            if (i == 0) _priviest_lowest_platform = platform_position_y;
            if (i == 4) _priviest_highest_platform = platform_position_y;
            
            GameObject _new_platform;
            if (Random.Range(0f, 1f) > _extrabounce_platform_chance)
            {
                _new_platform = Instantiate(_regular_platform, new Vector2(platform_position_x,
                    platform_position_y), Quaternion.identity);    
            }
            else
            {
                _new_platform = Instantiate(_extrabounce_platform, new Vector2(platform_position_x,
                    platform_position_y), Quaternion.identity); 
            }
            
            if (i == 0)
            {
                _new_platform.gameObject.transform.localScale = new Vector3(20,1, 1);
            }
            else
            {
                _new_platform.gameObject.transform.localScale = new Vector3(Random.Range(1f, 10f),1, 1);
            }
            platforms.Add(_new_platform);
            
            platform_position_y = platform_position_y + 2f + Random.Range(0f, 5.1f);
            platform_position_x = Random.Range(-_range_of_platform_spawn, _range_of_platform_spawn);
        }
    }

    private void DestroyAllPlatforms()
    {
        foreach (GameObject platform in platforms)
        {
            Destroy(platform);
        }
        platforms.Clear();
    }

    public void Reset()
    {
        Debug.Log("GENERATION " + generation);
        makeNewGeneration();

        transform.position = Vector3.zero;
        max_y_position = 0;
            
        foreach (var individual in NNindividuals)
        {
            individual.GetComponent<NeuralNetworkIndividual>().resetScore();
        }
        DestroyAllPlatforms();
        CreateInitialPlatforms();

        generation++;
        var new_generation_weights_list = new List<double[]>(new_generation_weights.Values);
        var old_generation_weights_list = new List<double[]>(old_generation_weights.Values);
        
        for (int i = 0; i < _number_of_nn_individuals; i++)
        {
            NNindividuals.Add(Instantiate(_individual_prefab, _start_position.position + new Vector3(Random.Range(-3f,3f),0,0), _start_position.rotation));
            NNindividuals[i].name = i.ToString();
            NNindividuals[i].GetComponent<NeuralNetworkIndividual>().sceneController = gameObject.GetComponent<SceneController>();
            NNindividuals[i].GetComponent<NeuralNetworkIndividual>().set_input_layer_size(_NN_input_layer_size);
            NNindividuals[i].GetComponent<NeuralNetworkIndividual>().setNetworkWeights(new_generation_weights_list[i]);
            for (int j = 0; j < 10; j++)
            {
                Debug.Log(new_generation_weights_list[i][j] + " " + old_generation_weights_list[i][j]);    
            }
            Debug.Log("_____________________________________");
            
        }
        
        old_generation_weights = new Dictionary<string, double[]>();
        new_generation_weights = new Dictionary<string, double[]>();
        old_generation_fitness = new Dictionary<string, double>();
        foreach (var iNindividual in NNindividuals)
        {
            old_generation_weights.Add(iNindividual.name, iNindividual.GetComponent<NeuralNetworkIndividual>().getWeights());
            old_generation_fitness.Add(iNindividual.name, 0);
            new_generation_weights.Add(iNindividual.name, new double[_weights_num]);
        }
    }
    
    public double Sigmoid(double value) {
        return 1.0f / (1.0f + (float) Math.Exp(-value));
    }

    void makeNewGeneration()
    {
        old_generation_fitness.OrderBy(x => x.Value);
        var old_generation_keys = new List<String>(old_generation_fitness.Keys);

        for (int i = 0; i < _number_of_nn_individuals; i++)
        {
            new_generation_weights[old_generation_keys[i]] = old_generation_weights[old_generation_keys[i]];
        }
        
        for (int i = 0; i < _number_of_nn_individuals; i++)
        {
            if (i > _number_of_nn_individuals / 2)
            {
                int first_index = Random.Range(0, _number_of_nn_individuals);
                            int second_index = first_index;
                            while (second_index == first_index )
                            {
                                second_index = Random.Range(0, _number_of_nn_individuals);
                            }
                            new_generation_weights[old_generation_keys[i]] = 
                                crossover(
                                    old_generation_weights[old_generation_keys[first_index]],
                                    old_generation_weights[old_generation_keys[second_index]]);
            }
        }
    
        for (int i = 0; i < _number_of_nn_individuals; i++)
        {
            if (Random.Range(0f, 1f) < _mutation_individ_chance)
            {
                new_generation_weights[old_generation_keys[i]] = mutation(new_generation_weights[old_generation_keys[i]]);
            }
        }
    }

    double[] mutation(double[] old_weights)
    {
        double[] new_weights = new double[_weights_num];
        for (int i = 0; i < _weights_num; i++)
        {
            if (Random.Range(0f, 1f) < _mutation_genome_chance)
            {
                new_weights[i] = Random.Range(-1f, 1f);
            }
            else
            {
                new_weights[i] = old_weights[i];
            }
        }

        return new_weights;
    }
    
    double[] crossover(double[] old_weights1, double[] old_weights2)
    {
        double[] new_weights = new double[_weights_num];
        for (int i = 0; i < _weights_num; i++)
        {
            if (Random.Range(0f, 1f) < _crossover_genome_chance)
            {
                new_weights[i] = old_weights1[i];
            }
            else
            {
                new_weights[i] = old_weights2[i];
            }
        }

        return new_weights;
    }
}
