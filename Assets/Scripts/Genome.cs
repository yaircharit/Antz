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


    // Constructor to initialize the genome traits with default values or specified values
    public Genome(float size = 1, float strength = 1.3f, float speed = 8, float viewDistance = 5, float viewAngle = 360, float hungerThreshold = 0.5f)
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

