using Assets.Scripts;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    AntColony colony;
    [SerializeField] public World world;
    [SerializeField] public AntColony ColonyPrefab;

    private void Awake()
    {
        Genome.InitializeTraitDefinitions(); // Ensure trait definitions are loaded before creating genomes
    }

    void Start()
    {
        world.RenderChunks(Vector3Int.zero); // Render the initial chunk at the start of the game

        colony = Instantiate(ColonyPrefab);

        var queen = colony.SpawnQueen(GameSetup.queenGenome);
        int startingAntsCount = (int)queen.genome["OffspringCount"].Value;


        for (int i = 0; i < startingAntsCount; i++)
        {
            queen.SpawnChild();
        }
    }  
}
