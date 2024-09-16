using PhysicaliaRemastered.Actors.Enemies;

namespace PhysicaliaRemastered.Actors.EnemyManagement;

public interface IEnemyBank
{
    Enemy CreateEnemy(int typeID, ActorStartValues startValues);

    void AddBaseEnemy(int typeID, Enemy enemy);
    void SetupEnemy(Enemy enemy);
}