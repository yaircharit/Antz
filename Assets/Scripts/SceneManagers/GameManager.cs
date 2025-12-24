using Assets.Scripts;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    AntColony colony;
    [SerializeField] public AntColony ColonyPrefab;


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
