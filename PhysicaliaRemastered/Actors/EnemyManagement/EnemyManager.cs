using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.Actors.Enemies;
using PhysicaliaRemastered.GameManagement;
using PhysicaliaRemastered.Weapons;
using Viewport = XNALibrary.Graphics.Viewport;

namespace PhysicaliaRemastered.Actors.EnemyManagement;

/// <summary>
/// Class for managing over a collection of Enemies.
/// </summary>
public class EnemyManager
{
    private readonly int _defaultActivationDistance = 20;

    private readonly EnemyLibrary _enemyLibrary;
    private readonly List<Enemy> _activatedEnemies;
    private readonly List<Enemy> _inactiveEnemies;

    /// <summary>
    /// Gets or sets the distance from the screen at which an enemy is activated.
    /// </summary>
    public int ActivationDistance { get; set; }

    public ReadOnlyCollection<Enemy> ActivatedEnemies => _activatedEnemies.AsReadOnly();

    /// <summary>
    /// Creates a new EnemyManager.
    /// </summary>
    /// <param name="enemyLibrary">Class implementing EnemyLibrary, that contains the
    /// definitions for the enemies to use.</param>
    public EnemyManager(EnemyLibrary enemyLibrary)
    {
        _enemyLibrary = enemyLibrary;

        _activatedEnemies = [];
        _inactiveEnemies = [];

        ActivationDistance = _defaultActivationDistance;
    }

    private bool EnemyOnScreen(Enemy enemy, Viewport viewport)
    {
        Rectangle enemyWorldCollisionBox = enemy.WorldCollisionBox;
        enemyWorldCollisionBox.Inflate(ActivationDistance, ActivationDistance);
        return viewport.IsOnScreen(enemyWorldCollisionBox);
    }

    public void ActivateVisibleEnemies(Viewport viewport)
    {
        for (int i = _inactiveEnemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = _inactiveEnemies[i];

            if (!EnemyOnScreen(enemy, viewport))
            {
                continue;
            }

            _inactiveEnemies.RemoveAt(i);
            enemy.IsActive = true;
            _activatedEnemies.Add(enemy);
        }
    }

    public void Update(GameTime gameTime, Player player, Viewport viewport)
    {
        ActivateVisibleEnemies(viewport);

        foreach (Enemy enemy in _activatedEnemies.Where(enemy => enemy.IsActive))
        {
            enemy.Update(gameTime, player);
        }
    }

    public void CheckCollisions(Player player)
    {
        foreach (Enemy enemy in _activatedEnemies)
        {
            CheckCollisions(player, enemy);
        }
    }

    private void CheckCollisions(Player player, Enemy enemy)
    {
        if (!enemy.CanCollide)
        {
            return;
        }

        Rectangle enemyWorldCollisionBox = enemy.WorldCollisionBox;
        if (player is { CanCollide: true, CanTakeDamage: true } &&
            player.WorldCollisionBox.Intersects(enemyWorldCollisionBox))
        {
            player.TakeDamage(enemy.Damage);
        }

        // Check to see whether the player's weapon is damaging the enemy
        Weapon? playerWeapon = player.CurrentWeapon;
        if (playerWeapon is null)
        {
            return;
        }

        if (playerWeapon.CanCollide &&
            playerWeapon is { WeaponFiredDuringLastUpdate: true } &&
            enemyWorldCollisionBox.Intersects(playerWeapon.WorldCollisionBox))
        {
            enemy.TakeDamage(playerWeapon.CollisionDamage);
        }
    }

    /// <summary>
    /// Resets the manager to its original state. All enemies managed over are
    /// also set to the start values.
    /// </summary>
    public void Reset()
    {
        // Set the default on all activated enemies
        foreach (Enemy enemy in _activatedEnemies)
        {
            enemy.ApplyStartValues();
        }

        // Transfer the reset Enemies to the queue
        while (_activatedEnemies.Count > 0)
        {
            Enemy enemy = _activatedEnemies[0];
            _activatedEnemies.RemoveAt(0);
            _enemyLibrary.SetupEnemy(enemy);
            _inactiveEnemies.Add(enemy);
        }
    }

    /// <summary>
    /// Enqueues an enemy of the given type and assigns it the supplied start values.
    /// </summary>
    /// <param name="typeId">ID of the type of enemy to add.</param>
    /// <param name="startValues">Start values for the new enemy.</param>
    public void EnqueueEnemy(int typeId, ActorStartValues startValues)
    {
        Enemy enemy = _enemyLibrary.CreateEnemy(typeId, startValues);

        EnqueueEnemy(enemy);
    }

    public void EnqueueEnemy(int typeId, ActorStartValues startValues, Rectangle patrolArea)
    {
        Enemy enemy = _enemyLibrary.CreateEnemy(typeId, startValues);
        enemy.PatrolArea = patrolArea;

        EnqueueEnemy(enemy);
    }

    /// <summary>
    /// Enqueues the passed in Enemy.
    /// </summary>
    /// <param name="enemy">Enemy to enqueue.</param>
    public void EnqueueEnemy(Enemy enemy)
    {
        _inactiveEnemies.Add(enemy);
    }

    /// <summary>
    /// Updates the animations of all active Enemies
    /// </summary>
    public void UpdateActorStates()
    {
        foreach (Enemy enemy in _activatedEnemies.Where(enemy => enemy.IsActive))
        {
            enemy.UpdateActorState();
        }
    }

    /// <summary>
    /// Draws the active enemies using the supplied SpriteBatch.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to draw with.</param>
    /// <param name="offsetPosition">Position of the top-left corner of the screen.</param>
    public void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
        foreach (Enemy enemy in _activatedEnemies)
        {
            enemy.Draw(spriteBatch, offsetPosition);
        }
    }

    public void SaveGame(SaveGame saveGame)
    {
        foreach (Enemy enemy in _activatedEnemies)
        {
            var enemySave = new EnemySave(enemy.Position, enemy.Velocity, enemy.Health, enemy.IsActive);
            saveGame.SavedEnemies.Add(enemy.UniqueId, enemySave);
        }
    }

    public void LoadGame(SaveGame saveGame)
    {
        for (int i = _inactiveEnemies.Count - 1; i >= 0; i--)
        {
            if (saveGame.SavedEnemies.ContainsKey(_inactiveEnemies[i].UniqueId))
            {
                // Move the enemy
                Enemy enemy = _inactiveEnemies[i];
                _inactiveEnemies.RemoveAt(i);
                _activatedEnemies.Add(enemy);

                // Setup the enemy
                EnemySave save = saveGame.SavedEnemies[enemy.UniqueId];
                enemy.Position = save.Position;
                enemy.Velocity = save.Velocity;
                enemy.Health = save.Health;

                if (enemy.Health <= 0)
                {
                    enemy.IsActive = false;
                    enemy.CanCollide = false;
                    enemy.CanTakeDamage = false;
                }
                else
                {
                    enemy.IsActive = save.IsActive;
                }
            }
        }
    }
}