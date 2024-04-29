using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Text;
using NeuronDotNet.Core.Backpropagation;
using NeuronDotNet.Core;
using btl.generic;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public class NeuralNetworkIndividual : MonoBehaviour
{
    [SerializeField] public SceneController sceneController;
    
    public static BackpropagationNetwork network;
    private int _input_layer_size;
    private int _hidden_layer_size = 32;
    private double[] _input_data;
    private double[] weights;
    private int weights_num;
    
    private float _score = 0f;
    private float _time = 0f;

    public float getScore()
    {
        return _score;
    }

    public void resetScore()
    {
        _score = 0;
        _time = 0;
    }
    
    // Start is called before the first frame update
    void Start()
    {
    }
    
    // Update is called once per frame
    void Update()
    {
        _time += Time.deltaTime;
        if (transform.position.y > _score)
        {
            _score = transform.position.y;
        }

        if (_score > sceneController.max_y_position)
        {
            sceneController.max_y_position = _score;
        }
    }

    public void set_input_layer_size(int size)
    {
        _input_layer_size = size;

        _input_data = new double[_input_layer_size];
        
        LinearLayer inputLayer = new LinearLayer(_input_layer_size);
        SigmoidLayer hiddenLayer = new SigmoidLayer(_hidden_layer_size);
        SigmoidLayer outputLayer = new SigmoidLayer(1);

        BackpropagationConnector connector = new BackpropagationConnector(inputLayer, hiddenLayer);
        BackpropagationConnector connector2 = new BackpropagationConnector(hiddenLayer, outputLayer);
        network = new BackpropagationNetwork(inputLayer, outputLayer);
        network.Initialize();

        weights_num = _input_layer_size * _hidden_layer_size + _hidden_layer_size * 1;
        weights = new double[weights_num];
        for (int i = 0; i < weights_num; i++)
        {
            weights[i] = Random.Range(-1f, 1f);
        };
        
        setNetworkWeights(weights);
    }

    public void set_input_data(double [] data)
    {
        if (data.Length == _input_layer_size)
        {
            for (int i = 0; i < _input_layer_size; i++)
            {
                _input_data[i] = data[i];    
            }
        }
        else
        {
            for (int i = 0; i < _input_layer_size; i++)
            {
                _input_data[i] = 0;    
            }
        }
    }

    public void decide()
    {
        double[] output = network.Run(_input_data);
        //Debug.Log(output[0]);
        GetComponent<IndividualMovement>().Move((float)output[0]*2 - 1);
    }
    
    public void setNetworkWeights(double[] weights)
    {
        // Setup the network's weights.
        int index = 0;

        foreach (BackpropagationConnector connector in network.Connectors)
        {
            foreach (BackpropagationSynapse synapse in connector.Synapses)
            {
                synapse.Weight = weights[index];
                synapse.SourceNeuron.SetBias(weights[index]);
                index++;
            }
        }
    }

    public double fitnessFunction()
    {
        double a = 1;
        double b = 1;
        double fitness = a*_score + b*_score/_time;

        return fitness;
    }

    public double[] getWeights()
    {
        return weights;
    }

    public int getWeightsNum()
    {
        return weights_num;
    }
}

