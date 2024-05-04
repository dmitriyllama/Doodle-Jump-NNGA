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

// https://www.primaryobjects.com/2009/05/06/using-neural-networks-and-genetic-algorithms-in-c-net/


// Base class for NNindividuals
public class NeuralNetworkIndividual : MonoBehaviour
{
    [SerializeField] public SceneController sceneController;
    
    public BackpropagationNetwork network;
    private int _input_layer_size;
    private int _hidden_layer_size_1 = 16;
    private double[] _input_data; //stores information about environment
    private double[] weights; //stores model of NN
    private int weights_num; //number of weights
    
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
    
    // fitness function
    public double fitnessFunction()
    {
        double a = 2;
        double b = 1;
        double c = 0.5; //time added after report to penalize inability of staying on platform
        double fitness = a*_score + b*_score/_time + c*_time;

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
    
    // Update is called once per frame
    void Update()
    {
        _time += Time.deltaTime;
        if (transform.position.y > _score)
        {
            _score = transform.position.y;
        }

        //Automatically updates best score among all individuals
        if (_score > sceneController.max_y_position)
        {
            sceneController.max_y_position = _score;
        }
    }

    //This method initializes Neural Network
    public void set_input_layer_size(int size)
    {
        _input_layer_size = size;

        //environment info provided by scene controller
        _input_data = new double[_input_layer_size];
        
        //NN architecture
        LinearLayer inputLayer = new LinearLayer(_input_layer_size); 
        SigmoidLayer hiddenLayer_1 = new SigmoidLayer(_hidden_layer_size_1);
        SigmoidLayer outputLayer = new SigmoidLayer(1);
        
        BackpropagationConnector connector1 = new BackpropagationConnector(inputLayer, hiddenLayer_1);
        BackpropagationConnector connector2 = new BackpropagationConnector(hiddenLayer_1, outputLayer);
        network = new BackpropagationNetwork(inputLayer, outputLayer);
        network.Initialize();

        //number of all weights in dense NN
        weights_num = _input_layer_size * _hidden_layer_size_1 + _hidden_layer_size_1 * 1;
        weights = new double[weights_num];
        //initially random weights
        for (int i = 0; i < weights_num; i++)
        {
            weights[i] = Random.Range(-1f, 1f);
        }
        
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
        this.GetComponent<IndividualMovement>().Move(((float)output[0]-0.5f)*20);
    }
    
    public void setNetworkWeights(double[] new_weights)
    {
        // Setup the network's weights.
        int index = 0;
        foreach (BackpropagationConnector connector in network.Connectors)
        {
            foreach (BackpropagationSynapse synapse in connector.Synapses)
            {
                synapse.Weight = new_weights[index];
                weights[index] = new_weights[index];
                synapse.SourceNeuron.SetBias(0);
                index++;
            }
        }
    }
}

