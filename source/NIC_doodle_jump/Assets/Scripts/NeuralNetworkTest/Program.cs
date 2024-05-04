using System;
using NeuronDotNet.Core.Backpropagation;
using NeuronDotNet.Core;
using btl.generic;

//
// www.primaryobjects.com
//
namespace NeuralNetworkTest
{
    class Program
    {
        public static BackpropagationNetwork network;

        public static void setNetworkWeights(BackpropagationNetwork aNetwork, double[] weights)
        {
            // Setup the network's weights.
            int index = 0;

            foreach (BackpropagationConnector connector in aNetwork.Connectors)
            {
                foreach (BackpropagationSynapse synapse in connector.Synapses)
                {
                    synapse.Weight = weights[index++];
                    synapse.SourceNeuron.SetBias(weights[index++]);
                }
            }
        }

        public static double fitnessFunction(double[] weights)
        {
            double fitness = 0;

            setNetworkWeights(network, weights);

            // AND
            double output = network.Run(new double[2] { 0, 0 })[0];
            // The closest the output is to zero, the more fit it is.
            fitness += 1 - output;

            output = network.Run(new double[2] { 0, 1 })[0];
            // The closest the output is to zero, the more fit it is.
            fitness += 1 - output;

            output = network.Run(new double[2] { 1, 0 })[0];
            // The closest the output is to zero, the more fit it is.
            fitness += 1 - output;

            output = network.Run(new double[2] { 1, 1 })[0];
            // The closest the output is to one, the more fit it is.
            fitness += output;

            /*// OR
            double output = network.Run(new double[2] { 0, 0 })[0];
            // The closest the output is to zero, the more fit it is.
            fitness += 1 - output;

            output = network.Run(new double[2] { 0, 1 })[0];
            // The closest the output is to one, the more fit it is.
            fitness += output;

            output = network.Run(new double[2] { 1, 0 })[0];
            // The closest the output is to one, the more fit it is.
            fitness += output;

            output = network.Run(new double[2] { 1, 1 })[0];
            // The closest the output is to one, the more fit it is.
            fitness += output;*/

            /*// XOR
            double output = network.Run(new double[2] { 0, 0 })[0];
            // The closest the output is to zero, the more fit it is.
            fitness += 1 - output;

            output = network.Run(new double[2] { 0, 1 })[0];
            // The closest the output is to one, the more fit it is.
            fitness += output;

            output = network.Run(new double[2] { 1, 0 })[0];
            // The closest the output is to one, the more fit it is.
            fitness += output;

            output = network.Run(new double[2] { 1, 1 })[0];
            // The closest the output is to zero, the more fit it is.
            fitness += 1 - output;*/

            return fitness;
        }

        static void Main(string[] args)
        {
            LinearLayer inputLayer = new LinearLayer(2);
            SigmoidLayer hiddenLayer = new SigmoidLayer(2);
            SigmoidLayer outputLayer = new SigmoidLayer(1);

            BackpropagationConnector connector = new BackpropagationConnector(inputLayer, hiddenLayer);
            BackpropagationConnector connector2 = new BackpropagationConnector(hiddenLayer, outputLayer);
            network = new BackpropagationNetwork(inputLayer, outputLayer);
            network.Initialize();

            GA ga = new GA(0.50, 0.01, 100, 2000, 12);
            ga.FitnessFunction = new GAFunction(fitnessFunction);
            ga.Elitism = true;
            ga.Go();

            double[] weights;
            double fitness;
            ga.GetBest(out weights, out fitness);
            Console.WriteLine("Best brain had a fitness of " + fitness);

            setNetworkWeights(network, weights);

            double input1;
            string strInput1 = "";
            while (strInput1 != "q")
            {
                Console.Write("Input 1: ");
                strInput1 = Console.ReadLine().ToString();

                if (strInput1 != "q")
                {
                    input1 = Convert.ToDouble(strInput1);
                    if (input1 != 'q')
                    {
                        Console.Write("Input 2: ");
                        double input2 = Convert.ToDouble(Console.ReadLine().ToString());
                        double[] output = network.Run(new double[2] { input1, input2 });
                        Console.WriteLine("Output: " + output[0]);
                    }
                }
            }
        }
    }
}
