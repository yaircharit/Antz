using System;
using System.Collections.Generic;
using UnityEngine;


public class NeuralNetwork : IComparable<NeuralNetwork>
{
    private Func<double, double> activation;
    private int[] layers;//layers    
    private float[][] neurons;//neurons    
    private float[][] biases;//biasses    
    private float[][][] weights;//weights    
    private int[] activations;//layers
    public float fitness = 0;//fitness

    public NeuralNetwork(int max_hidden_layers, int max_hidden_neurons, int inputs_count, int outputs_count, float weights_range = 0.5f, float biases_range = 0.5f)
    {
        int hidden_layers_count = UnityEngine.Random.Range(0, max_hidden_layers + 1);
        int hidden_neurons_count = UnityEngine.Random.Range(0, max_hidden_neurons + 1);

        int[] layers = new int[hidden_layers_count + 2];
        layers[0] = inputs_count;
        layers[hidden_layers_count + 1] = outputs_count;

        switch ( max_hidden_layers )
        {
            case 0:
                break;
            case 1:
                layers[1] = hidden_layers_count;
                break;
            default:
                int count = 0;
                while ( count < hidden_neurons_count )
                {
                    layers[UnityEngine.Random.Range(0, hidden_layers_count + 1)] += UnityEngine.Random.Range(1, hidden_neurons_count - count + 1);
                }

                List<int> layers_list = new List<int>();
                foreach ( int neurons in layers )
                {
                    if ( neurons > 0 )
                    {
                        layers_list.Add(neurons);
                    }
                }
                layers = layers_list.ToArray();
                break;
        }
        Debug.Log(layers.ToString());
        //init(layers, activation_method, weights_range, biases_range);
    }

    public NeuralNetwork(int[] layers, float weights_range = 0.5f, float biases_range = 0.5f)
    {
        init(layers, weights_range, biases_range);
    }

    private void init(int[] layers, float weights_range = 0.5f, float biases_range = 0.5f)
    {
        

        this.layers = new int[layers.Length];
        neurons = new float[layers.Length][];
        biases = new float[layers.Length][];
        weights = new float[layers.Length][][];

        for ( int i = 0; i < layers.Length; i++ ) //iterate layers
        {
            this.layers[i] = layers[i];
            neurons[i] = new float[layers[i]];
            biases[i] = new float[layers[i]];

            if ( i > 0 )
                weights[i] = new float[layers[i]][];

            for ( int j = 0; j < layers[i]; i++ ) // iterate neurons in layer
            {
                biases[i][j] = UnityEngine.Random.Range(-biases_range, biases_range);

                if ( i > 0 )//iterate neurons in previous layer
                {
                    weights[i][j] = new float[layers[i - 1]];
                    for ( int k = 0; k < layers[i - 1]; k++ )
                    {
                        Debug.Log(String.Format("{0},{1},{2}",i,j,k));
                        weights[i][j][k] = UnityEngine.Random.Range(-weights_range, weights_range);
                    }
                }
            }
        }
    }

    public float[] Forward(float[] inputs)
    {
        for ( int i = 0; i < inputs.Length; i++ )
        {
            neurons[0][i] = inputs[i];
        }
        for ( int i = 1; i < layers.Length; i++ )
        {
            int layer = i - 1;
            for ( int j = 0; j < neurons[i].Length; j++ )
            {
                float value = 0f;
                for ( int k = 0; k < neurons[i - 1].Length; k++ )
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k];
                }
                neurons[i][j] = (float)Math.Tanh(value + biases[i][j]);
            }
        }
        return neurons[neurons.Length - 1];
    }

    //used as a simple mutation function for any genetic implementations.
    public void Mutate(int chance, float val)
    {
        for ( int i = 0; i < biases.Length; i++ )
        {
            for ( int j = 0; j < biases[i].Length; j++ )
            {
                biases[i][j] = (UnityEngine.Random.Range(0f, chance) <= 5) ? biases[i][j] += UnityEngine.Random.Range(-val, val) : biases[i][j];
            }
        }

        for ( int i = 0; i < weights.Length; i++ )
        {
            for ( int j = 0; j < weights[i].Length; j++ )
            {
                for ( int k = 0; k < weights[i][j].Length; k++ )
                {
                    weights[i][j][k] = (UnityEngine.Random.Range(0f, chance) <= 5) ? weights[i][j][k] += UnityEngine.Random.Range(-val, val) : weights[i][j][k];

                }
            }
        }
    }

    //Comparing For NeuralNetworks performance.
    public int CompareTo(NeuralNetwork other)
    {
        if ( other == null )
            return 1;
        if ( fitness > other.fitness )
            return 1;
        else if ( fitness < other.fitness )
            return -1;
        else
            return 0;
    }
}
