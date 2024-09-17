using PhysicaliaRemastered.Actors.Enemies;

namespace PhysicaliaRemastered.Actors.EnemyManagement;

public interface IEnemyBank
{
    Enemy CreateEnemy(int typeId, ActorStartValues startValues);

    void AddBaseEnemy(int typeId, Enemy enemy);
    void SetupEnemy(Enemy enemy);
}