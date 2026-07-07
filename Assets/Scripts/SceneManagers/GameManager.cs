using Assets.Scripts;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static AntColony colony { get; private set; }
    [SerializeField] public AntColony ColonyPrefab;

    private void Awake()
    {
        Genome.InitializeTraitDefinitions(); // Ensure trait definitions are loaded before creating genomes
        GameSetup.queenGenome ??= new Genome(); // Initialize the queen genome with the provided data
    }

    void Start()
    {
        colony = Instantiate(ColonyPrefab);
        var queen = colony.SpawnQueen(GameSetup.queenGenome);
        int startingAntsCount = (int)queen.genome["OffspringCount"].Value;


        for (int i = 0; i < startingAntsCount; i++)
        {
            queen.SpawnChild();
        }
    }  
}
