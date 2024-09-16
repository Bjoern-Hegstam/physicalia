using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Physicalia;
using PhysicaliaRemastered.Actors.Enemies;
using PhysicaliaRemastered.Weapons.NewWeapons;

namespace PhysicaliaRemastered.Actors.EnemyManagement
{
    /// <summary>
    /// Class for managing over a collection of Enemies.
    /// </summary>
    public class EnemyManager
    {
        #region Constants

        private int DEFAULT_ACTIVATION_DISTANCE = 20;

        #endregion

        #region Fields

        private IEnemyBank enemyBank;
        private List<Enemy> activatedEnemies;
        private List<Enemy> inactiveEnemies;

        private int activationDistance;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the distance from the screen at which an enemy is activated.
        /// </summary>
        public int ActivationDistance
        {
            get { return this.activationDistance; }
            set { this.activationDistance = value; }
        }

        public Enemy[] ActivatedEnemies
        {
            get { return this.activatedEnemies.ToArray(); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new EnemyManager.
        /// </summary>
        /// <param name="enemyBank">Class implementing IEnemyBank, that contains the
        /// definitions for the enemies to use.</param>
        public EnemyManager(IEnemyBank enemyBank)
        {
            this.enemyBank = enemyBank;

            this.activatedEnemies = new List<Enemy>();
            this.inactiveEnemies = new List<Enemy>();

            this.activationDistance = DEFAULT_ACTIVATION_DISTANCE;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Checks whether the passed in Enemy is either on screen or within the
        /// set activation distance from it.
        /// </summary>
        /// <param name="enemy">Enemy to check.</param>
        /// <param name="screenRect">Rectangle with the size and position of the screen.</param>
        /// <returns></returns>
        private bool EnemyOnScreen(Enemy enemy, Rectangle screenRect)
        {
            // Get the position of the enemies collision box
            Rectangle enemyBox = enemy.CollisionBox;
            Vector2 enemyPos = enemy.Position - enemy.Origin;
            enemyBox.X += (int)enemyPos.X; enemyBox.Y += (int)enemyPos.Y;

            // Add the activation distance to the side of the screen rectangle
            screenRect.Inflate(this.activationDistance, this.activationDistance);

            // Return a boolean indicating whether the rectangles are intersecting
            return screenRect.Intersects(enemyBox);
        }

        #endregion

        #region Public methods

        public void ActivateVisible(Rectangle screenRect)
        {
            // Activate Enemies near the screen
            for (int i = this.inactiveEnemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = this.inactiveEnemies[i];

                if (this.EnemyOnScreen(enemy, screenRect))
                {
                    this.inactiveEnemies.RemoveAt(i);
                    enemy.IsActive = true;
                    this.activatedEnemies.Add(enemy);
                }
            }
        }

        /// <summary>
        /// Updates the EnemyManager.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="player"></param>
        /// <param name="screenRect">Rectangle with the current position and
        /// size of the screen.</param>
        public void Update(GameTime gameTime, Player player, Rectangle screenRect)
        {
            // Activate Enemies near the screen
            this.ActivateVisible(screenRect);

            // Update the activated Enemies that are still active
            foreach (Enemy enemy in this.activatedEnemies)
                if (enemy.IsActive)
                    enemy.Update(gameTime, player);
        }

        /// <summary>
        /// Checks for collisions between the active enemies and the player.
        /// </summary>
        /// <param name="player">Player to check for collisions with.</param>
        public void CheckCollisions(Player player)
        {
            // Only do checks if the player can collide and take damage
            if (!player.CanCollide || !player.CanTakeDamage)
                return;

            // Calculate the position of the collision box
            Rectangle playerBox = player.CollisionBox;
            Vector2 boxPos = player.Position - player.Origin;
            playerBox.X += (int)boxPos.X; playerBox.Y += (int)boxPos.Y;

            Weapon weapon = player.CurrentWeapon;
            Rectangle weaponBox = new Rectangle();

            if (weapon != null && weapon.CanCollide)
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
                    weaponBox.Y += (int)weapon.PlayerOffset.Y;
                else
                {
                    weaponBox.Y -= (int)weapon.PlayerOffset.Y;
                    weaponBox.Y += playerBox.Y - player.CollisionBox.Y;
                }
            }

            foreach (Enemy enemy in this.activatedEnemies)
            {
                if (!enemy.CanCollide)
                    continue;

                Rectangle enemyBox = enemy.CollisionBox;
                Vector2 enemyPos = enemy.Position - enemy.Origin;
                enemyBox.X += (int)enemyPos.X; enemyBox.Y += (int)enemyPos.Y;

                if (playerBox.Intersects(enemyBox))
                    enemy.OnCollision(player, BoxSide.Bottom | BoxSide.Left | BoxSide.Right | BoxSide.Top, Vector2.Zero, Vector2.Zero);

                // Check to see whether the player's weapon is damaging the enemy
                if (weapon != null && weapon.WeaponFired &&
                    enemyBox.Intersects(weaponBox))
                    enemy.TakeDamage(weapon.CollisionDamage);
            }
        }

        /// <summary>
        /// Resets the manager to its original state. All enemies managed over are
        /// also set to the start values.
        /// </summary>
        public void Reset()
        {
            // Set the default on all activated enemies
            foreach (Enemy enemy in this.activatedEnemies)
                enemy.SetDefaults();

            // Transfer the reset Enemies to the queue
            while (this.activatedEnemies.Count > 0)
            {
                Enemy enemy = this.activatedEnemies[0];
                this.activatedEnemies.RemoveAt(0);
                this.enemyBank.SetupEnemy(enemy);
                this.inactiveEnemies.Add(enemy);
            }
        }

        /// <summary>
        /// Enqueues an enemy of the given type and assigns it the supplied start values.
        /// </summary>
        /// <param name="typeID">ID of the type of enemy to add.</param>
        /// <param name="startValues">Start values for the new enemy.</param>
        public void EnqueueEnemy(int typeID, ActorStartValues startValues)
        {
            Enemy enemy = this.enemyBank.CreateEnemy(typeID, startValues);

            this.EnqueueEnemy(enemy);
        }

        public void EnqueueEnemy(int typeID, ActorStartValues startValues, Rectangle patrolArea)
        {
            Enemy enemy = this.enemyBank.CreateEnemy(typeID, startValues);
            enemy.PatrolArea = patrolArea;

            this.EnqueueEnemy(enemy);
        }
        
        /// <summary>
        /// Enqueues the passed in Enemy.
        /// </summary>
        /// <param name="enemy">Enemy to enqueue.</param>
        public void EnqueueEnemy(Enemy enemy)
        {
            this.inactiveEnemies.Add(enemy);
        }

        /// <summary>
        /// Updates the animations of all active Enemies
        /// </summary>
        public void UpdateAnimations()
        {
            foreach (Enemy enemy in this.activatedEnemies)
                if (enemy.IsActive)
                    enemy.UpdateAnimation();
        }

        /// <summary>
        /// Draws the active enemies using the supplied SpriteBatch.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw with.</param>
        /// <param name="offsetPosition">Position of the top-left corner of the screen.</param>
        public void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
        {
            foreach (Enemy enemy in this.activatedEnemies)
            {
                enemy.Draw(spriteBatch, offsetPosition);
            }
        }

        #endregion

        #region Session management

        public void SaveSession(GameSession session)
        {
            foreach (Enemy enemy in this.activatedEnemies)
            {
                EnemySave enemySave = new EnemySave(enemy.Position, enemy.Velocity, enemy.Health, enemy.IsActive);
                session.SavedEnemies.Add(enemy.UniqueID, enemySave);
            }
        }

        public void LoadSession(GameSession session)
        {
            for (int i = this.inactiveEnemies.Count - 1; i >= 0; i--)
            {
                if (session.SavedEnemies.ContainsKey(this.inactiveEnemies[i].UniqueID))
                {
                    // Move the enemy
                    Enemy enemy = this.inactiveEnemies[i];
                    this.inactiveEnemies.RemoveAt(i);
                    this.activatedEnemies.Add(enemy);

                    // Setup the enemy
                    EnemySave save = session.SavedEnemies[enemy.UniqueID];
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
                        enemy.IsActive = save.IsActive;
                }
            }
        }

        #endregion
    }
}
