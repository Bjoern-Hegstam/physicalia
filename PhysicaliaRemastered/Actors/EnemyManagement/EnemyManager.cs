using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.Actors.Enemies;
using PhysicaliaRemastered.GameManagement;
using PhysicaliaRemastered.Weapons.NewWeapons;
using XNALibrary.TileEngine;
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
        // Get the position of the enemies collision box
        Rectangle enemyBox = enemy.CollisionBox;
        Vector2 enemyPos = enemy.Position - enemy.Origin;
        enemyBox.X += (int)enemyPos.X;
        enemyBox.Y += (int)enemyPos.Y;

        enemyBox.Inflate(ActivationDistance, ActivationDistance);

        return viewport.IsOnScreen(enemyBox);
    }

    public void ActivateVisible(Viewport viewport)
    {
        // Activate Enemies near the screen
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
        // Activate Enemies near the screen
        ActivateVisible(viewport);

        // Update the activated Enemies that are still active
        foreach (Enemy enemy in _activatedEnemies)
        {
            if (enemy.IsActive)
            {
                enemy.Update(gameTime, player);
            }
        }
    }

    /// <summary>
    /// Checks for collisions between the active enemies and the player.
    /// </summary>
    /// <param name="player">Player to check for collisions with.</param>
    public void CheckCollisions(Player player)
    {
        // Only do checks if the player can collide and take damage
        if (!player.CanCollide || !player.CanTakeDamage)
        {
            return;
        }

        // Calculate the position of the collision box
        Rectangle playerBox = player.CollisionBox;
        Vector2 boxPos = player.Position - player.Origin;
        playerBox.X += (int)boxPos.X;
        playerBox.Y += (int)boxPos.Y;

        Weapon weapon = player.CurrentWeapon;
        var weaponBox = new Rectangle();

        if (weapon is { CanCollide: true })
        {
            weaponBox = weapon.CollisionBox;

            // Get the right position of the weapons collision box
            if ((player.SpriteFlip & SpriteEffects.FlipHorizontally) != 0)
            {
                weaponBox.X = playerBox.X - player.CollisionBox.X + player.Width - weaponBox.X - weaponBox.Width;
                weaponBox.X += (int)weapon.PlayerOffset.X;
            }
            else
            {
                weaponBox.X += playerBox.X - player.CollisionBox.X;
                weaponBox.X -= (int)weapon.PlayerOffset.X;
            }

            if ((player.SpriteFlip & SpriteEffects.FlipVertically) != 0)
                // TODO: Fix box position in Y
            {
                weaponBox.Y += (int)weapon.PlayerOffset.Y;
            }
            else
            {
                weaponBox.Y -= (int)weapon.PlayerOffset.Y;
                weaponBox.Y += playerBox.Y - player.CollisionBox.Y;
            }
        }

        foreach (Enemy enemy in _activatedEnemies)
        {
            if (!enemy.CanCollide)
            {
                continue;
            }

            Rectangle enemyBox = enemy.CollisionBox;
            Vector2 enemyPos = enemy.Position - enemy.Origin;
            enemyBox.X += (int)enemyPos.X;
            enemyBox.Y += (int)enemyPos.Y;

            if (playerBox.Intersects(enemyBox))
            {
                enemy.OnCollision(player, BoxSide.Bottom | BoxSide.Left | BoxSide.Right | BoxSide.Top, Vector2.Zero,
                    Vector2.Zero);
            }

            // Check to see whether the player's weapon is damaging the enemy
            if (weapon is { WeaponFired: true } &&
                enemyBox.Intersects(weaponBox))
            {
                enemy.TakeDamage(weapon.CollisionDamage);
            }
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
            enemy.SetDefaults();
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
    public void UpdateAnimations()
    {
        foreach (Enemy enemy in _activatedEnemies)
        {
            if (enemy.IsActive)
            {
                enemy.UpdateAnimation();
            }
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