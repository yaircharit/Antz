using UnityEngine;

public class GameManager : MonoBehaviour
{
    AntColony colony;
    [SerializeField] public AntColony ColonyPrefab;


    void Start()
    {
        colony = Instantiate(ColonyPrefab);

        var queen = colony.SpawnAnt(GameSetup.queenGenome, true);
        int startingAntsCount = (int)queen.genome["OffspringCount"].Value;


        for (int i = 0; i < startingAntsCount; i++)
        {
            Genome currAntGenome = queen.genome.Clone();
            currAntGenome.Mutate(true);
            colony.SpawnAnt(currAntGenome);
        }
    }  
}
