using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class SceneController : MonoBehaviour
{
    [Header("references")]
    [SerializeField] private GameObject _regular_platform;
    [SerializeField] private GameObject _extrabounce_platform;
    [SerializeField] private GameObject _individual_prefab;
    [SerializeField] private Transform _start_position;
    [SerializeField] private List<GameObject> platforms = new List<GameObject>();
    [Header("Hyper parameters")]
    [SerializeField] [Range(1, 10)] private int _number_of_platforms;
    [SerializeField] [Range(0, 1)] private float _extrabounce_platform_chance;
    [SerializeField] [Range(0, float.MaxValue)] private float _range_of_platform_spawn;
    [SerializeField] [Range(1, 100)] private int _number_of_nn_individuals;
    private double previous_max_fittness = 0;
    
    private float _priviest_highest_platform = 9;
    private float _priviest_lowest_platform = -5f;
    private int _NN_input_layer_size = 1;
    private List<GameObject> NNindividuals = new List<GameObject>();
    private GameObject best_individ;
    
    [SerializeField] [Range(0, 1)] private float _mutation_genome_chance;
    [SerializeField] [Range(0, 1)] private float _crossover_genome_chance;
    [SerializeField] [Range(0, 1)] private float _mutation_individ_chance;
    private int generation;
    private Dictionary<String, double> old_generation_fitness;
    private Dictionary<String, double[]> old_generation_weights;
    private Dictionary<String, double[]> new_generation_weights;
    private int _weights_num;
    
    [Header("Presentation of info")]
    [SerializeField] private TMP_Text _text_best_fittness;
    [SerializeField] private TMP_Text _text_ramaining_individuals;
    [SerializeField] private TMP_Text _text_generation;

    public float max_y_position = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        generation = 0;
        _NN_input_layer_size = 4 * _number_of_platforms + 2;
        
        //instantiating all individuals
        for (int i = 0; i < _number_of_nn_individuals; i++)
        {
            NNindividuals.Add(Instantiate(_individual_prefab, _start_position.position + new Vector3(Random.Range(-3f,3f),0,0), _start_position.rotation));
            NNindividuals[i].name = i.ToString();
            NNindividuals[i].GetComponent<NeuralNetworkIndividual>().sceneController = gameObject.GetComponent<SceneController>(); //link controller to the NN individual
            NNindividuals[i].GetComponent<NeuralNetworkIndividual>().set_input_layer_size(_NN_input_layer_size); //initialize NN 
            NNindividuals[i].GetComponent<SpriteRenderer>().color =
                new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));//color individual
        }
        
        //get info about model size
        _weights_num = NNindividuals[0].GetComponent<NeuralNetworkIndividual>().getWeightsNum();

        //instantiating dictionaries
        old_generation_weights = new Dictionary<string, double[]>();
        new_generation_weights = new Dictionary<string, double[]>();
        old_generation_fitness = new Dictionary<string, double>();
        foreach (var iNindividual in NNindividuals)
        {
            old_generation_weights.Add(iNindividual.name, iNindividual.GetComponent<NeuralNetworkIndividual>().getWeights());
            old_generation_fitness.Add(iNindividual.name, 0);
            new_generation_weights.Add(iNindividual.name, new double[_weights_num]);
        }

        best_individ = NNindividuals[0];
    }

    // LateUpdate is called once per frame after every other function
    void LateUpdate()
    {
        //iterate over every individual
        int index = 0;
        foreach(var individual in NNindividuals)
        {
            setIndividualData(index);//passing down information about environment to the NN
            individual.GetComponent<NeuralNetworkIndividual>().decide();//Force NN to send move command
            old_generation_fitness[individual.name] =
                individual.GetComponent<NeuralNetworkIndividual>().fitnessFunction();//update fitness of individual
            index++;
        }

        //moving camera according to max y position
        if (max_y_position > transform.position.y)
        {
           transform.position = new Vector3(0, max_y_position, 0); 
        }

        //generating list of fitnesses 
        var fitnesses = new List<double>(old_generation_fitness.Values);

        //presenting info about generation to the screen
        _text_best_fittness.text = "Fitness " + fitnesses.Max();
        _text_ramaining_individuals.text = "Individuals left " + NNindividuals.Count;
        _text_generation.text = "Number of generation " + generation;

        //Reset generation if termination button was pressed
        if (Input.GetKeyDown(KeyCode.T))
        {
            foreach (var individual in NNindividuals)
            {
                Destroy(individual);
            }
            NNindividuals.Clear();
            Reset(); //Restarts generation
        }
    }

    //method for further development, chooses best individual according to it fitness
    void chooseBestIndivid()
    {
        float max_score = float.MinValue;

        foreach (var individual in NNindividuals)
        {
            if (individual.GetComponent<NeuralNetworkIndividual>().getScore() > max_score)
            {
                best_individ = individual;
                max_score = individual.GetComponent<NeuralNetworkIndividual>().getScore();
            }
        }
    }

    //read information about environment and send it to NN
    void setIndividualData(int i)
    {
        List<double> _input_data = new List<double>();
        
        //Normalized individual velocity
        _input_data.Add(Sigmoid(NNindividuals[i].GetComponent<Rigidbody2D>().velocity.y));
        _input_data.Add(Sigmoid(NNindividuals[i].GetComponent<Rigidbody2D>().velocity.x));

        //platform type
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
        
        //Normalized relative position of platform (distance between it and individual)
        for (int j = 0; j < _number_of_platforms; j++)
        {
            _input_data.Add(Sigmoid(NNindividuals[i].transform.position.y - platforms[j].transform.position.y));
        }
        for (int j = 0; j < _number_of_platforms; j++)
        {
            _input_data.Add(Sigmoid(NNindividuals[i].transform.position.x - platforms[j].transform.position.x));
        }
        //Normalized platform size
        for (int j = 0; j < _number_of_platforms; j++)
        {
            _input_data.Add(Sigmoid(NNindividuals[i].transform.localScale.x));
        }
        
        //sending info to NN
        NNindividuals[i].GetComponent<NeuralNetworkIndividual>().set_input_data(_input_data.ToArray());
    }
    
    //normalization function
    public double Sigmoid(double value) {
        return 1.0f / (1.0f + (float) Math.Exp(-value));
    }
    
    //Method that called when object triggers eraser collider
    private void OnTriggerEnter2D(Collider2D other)
    {
        //if object is platform
        if (other.CompareTag("Platform") || other.CompareTag("Bounce_platform"))
        {
            //generate new platform
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
            
            _new_platform.gameObject.transform.localScale = new Vector3(Random.Range(4f, 10f),1, 1);
            _priviest_highest_platform = _new_height;
            
            //remove old platform and update platforms list
            _priviest_lowest_platform = other.gameObject.transform.position.y;
            platforms.Remove(other.gameObject);
            platforms.Add(_new_platform);
        }
        else //if it individual
        {
            //remove it from NNindividuals list
            NNindividuals.Remove(other.gameObject);
            
            chooseBestIndivid();
        }
        
        //Destruction of object that triggered collider
        Destroy(other.gameObject);

        //if there are no more individuals
        if (NNindividuals.Count == 0)
        {
            Reset(); //restarts generation
        }
    }

    //method that called in generation restart (aka Reset)
    private void CreateInitialPlatforms()
    {
        float platform_position_x = 0;
        float platform_position_y = -4.5f;

        for (int i = 0; i < _number_of_platforms; i++)
        {
            if (i == 0) _priviest_lowest_platform = platform_position_y; //if it's lowest platform
            if (i == _number_of_platforms - 1) _priviest_highest_platform = platform_position_y; //if it's highest platform
            
            //generation of new platform
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
            
            //extra size if it's lowest
            if (i == 0)
            {
                _new_platform.gameObject.transform.localScale = new Vector3(15,1, 1);
            }
            else
            {
                _new_platform.gameObject.transform.localScale = new Vector3(Random.Range(3f, 10f),1, 1);
            }
            platforms.Add(_new_platform);
            
            //setting platform position
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

    //method that called to reset generation
    public void Reset()
    {
        Debug.Log("GENERATION " + generation);
        
        //create weights for new generation
        makeNewGeneration();

        //reseting camera position
        transform.position = Vector3.zero;
        max_y_position = 0;
            
        //reseting individuals scores 
        foreach (var individual in NNindividuals)
        {
            individual.GetComponent<NeuralNetworkIndividual>().resetScore();
        }
        //reseting and generating platforms
        DestroyAllPlatforms();
        CreateInitialPlatforms();

        //create new individuals with new weights
        var new_generation_weights_list = new List<double[]>(new_generation_weights.Values);
        var old_generation_weights_list = new List<double[]>(old_generation_weights.Values);
        
        //instantiating individuals
        for (int i = 0; i < _number_of_nn_individuals; i++)
        {
            Debug.Log("Individual number: " + i);
            NNindividuals.Add(Instantiate(_individual_prefab, _start_position.position + new Vector3(Random.Range(-3f,3f),0,0), _start_position.rotation));
            NNindividuals[i].name = i.ToString();
            NNindividuals[i].GetComponent<NeuralNetworkIndividual>().sceneController = gameObject.GetComponent<SceneController>(); //link controller to the NN individual
            NNindividuals[i].GetComponent<NeuralNetworkIndividual>().set_input_layer_size(_NN_input_layer_size); //initialize NN
            NNindividuals[i].GetComponent<NeuralNetworkIndividual>().setNetworkWeights(new_generation_weights_list[i]); 
            NNindividuals[i].GetComponent<SpriteRenderer>().color =
                new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));//color individual
            for (int j = 0; j < 10; j++)
            {
                Debug.Log(new_generation_weights_list[i][j] + " " + old_generation_weights_list[i][j]); //log first ten new weights    
            }
            Debug.Log("_____________________________________");
            
        }
        
        //reseting generations lists
        old_generation_weights = new Dictionary<string, double[]>();
        new_generation_weights = new Dictionary<string, double[]>();
        old_generation_fitness = new Dictionary<string, double>();
        foreach (var iNindividual in NNindividuals)
        {
            old_generation_weights.Add(iNindividual.name, iNindividual.GetComponent<NeuralNetworkIndividual>().getWeights());
            old_generation_fitness.Add(iNindividual.name, 0);
            new_generation_weights.Add(iNindividual.name, new double[_weights_num]);
        }
        
        generation++;
    }

    //function that creates new generation weights
    void makeNewGeneration()
    {
        //order individuals by fitness
        old_generation_fitness.OrderBy(x => x.Value);
        var old_generation_keys = new List<String>(old_generation_fitness.Keys);
        
        //save best model to file
        using(StreamWriter sr = new StreamWriter(@"Assets\NNmodels\"+ generation + ".txt"))
        {
            foreach(var item in old_generation_weights[old_generation_keys[0]])
            {
                sr.WriteLine(item);
            }
        }

        //create new weights
        for (int i = 0; i < _number_of_nn_individuals; i++)
        {
            //clone best from previous
            if (i < _number_of_nn_individuals / 2)
            {
                new_generation_weights[old_generation_keys[i]] = old_generation_weights[old_generation_keys[i]];
            }
            //crossover
            else if (i < _number_of_nn_individuals / 2 + _number_of_nn_individuals / 4)
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
            //random part
            else 
            {
                for (int j = 0; j < _weights_num; j++)
                {
                    new_generation_weights[old_generation_keys[i]][j] = Random.Range(-10f, 10f);
                }
            }
            
            //mutate everyone with certain chance
            if (Random.Range(0f, 1f) < _mutation_individ_chance)
            {
                new_generation_weights[old_generation_keys[i]] = mutation(new_generation_weights[old_generation_keys[i]]);
            }
        }
    }

    //method called to mutate weights
    double[] mutation(double[] old_weights)
    {
        double[] new_weights = new double[_weights_num];
        for (int i = 0; i < _weights_num; i++)
        {
            if (Random.Range(0f, 1f) < _mutation_genome_chance)
            {
                new_weights[i] = Random.Range(-10f, 10f);
            }
            else
            {
                new_weights[i] = old_weights[i];
            }
        }
        
        return new_weights;
    }
    
    //method called to crossover weights between themself
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
