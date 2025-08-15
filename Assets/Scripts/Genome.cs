using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Genome
{
    // Genome properties that define the characteristics of the ant
    public float Size { get; private set; } // Size of the ant, affects speed and pheromone detection
    public float Strength { get; private set; }  // Determines how strong the ant is, affects speed when carrying food (1 is normal strength)
    public float Speed { get; private set; }
    public float ViewDistance { get; private set; }
    public float ViewAngle { get; private set; } // Angle in degrees
    public float HungerThreshold { get; private set; } // Threshold for hunger, when the ant needs to find food

    // Pheromone properties (ACO configuration)

    // Maybe devide to different classes for pheromone types? 1 class to inherit/contain from all types?
    // Maybe will be useful for new mutations

    // Constructor to initialize the genome traits with default values or specified values
    public Genome(float size = 1, float strength = 1.4f, float speed = 8, float viewDistance = 10, float viewAngle = 120, float hungerThreshold = 0.4f)
    {
        Size = size;
        Strength = strength;
        Speed = speed;
        ViewDistance = viewDistance;
        ViewAngle = viewAngle;
        HungerThreshold = hungerThreshold;
    }


    public Genome Clone()
    {
        return new Genome(Size, Strength, Speed, ViewDistance, ViewAngle, HungerThreshold);
    }

    public void Mutate()
    {
        Random rand = new();
        Size += (float)(rand.NextDouble() - 0.5);
        Strength += (float)(rand.NextDouble() - 0.5);
        Speed += (float)(rand.NextDouble() - 0.5);
    }
}

