using Microsoft.Xna.Framework;
using PhysicaliaRemastered.Actors;

namespace PhysicaliaRemastered.GameManagement;

/// <summary>
/// Level intended for containing a Boss. The BossLevel behaves just like a
/// regular Level with the exception that a Boss is present in the Level and
/// the Level will be considered completed first after the boss has been
/// defeated.
/// 
/// Consider having some kind of event go off after the boss is defeated.
/// </summary>
public class BossLevel : Level
{
    public BossLevel(Game game, Player player)
        : base(game, player)
    {

    }
}