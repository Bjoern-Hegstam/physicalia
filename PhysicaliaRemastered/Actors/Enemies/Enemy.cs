using System;
using Microsoft.Xna.Framework;

namespace PhysicaliaRemastered.Actors.Enemies;

/// <summary>
/// Describes the intelligence level of an Enemy.
/// </summary>
public enum AILevel
{
    Dumb,
    Smart,
    Savant
}

/// <summary>
/// Descrines the behavior of an Enemy.
/// </summary>
public enum EnemyBehavior
{
    Idle,
    Seek,
    Attack,
    Follow
}

public class Enemy : Actor
{
    #region Fields

    private static int enemyCount = 0;
    private int uniqueID;

    private int typeID;

    private int damageValue;
    private bool enabled;
    private bool visible;

    // AI
    private float attackRange;
    private Rectangle patrolArea;
    private AILevel intelligence;
    private EnemyBehavior behavior;

    // Post-death
    private float blinkDelay = 1F;
    private int blinkCount = 4;
    private float blinkInterval = 0.15F;
    private float blinkTime = 0;

    #endregion

    #region Properties

    public int UniqueID
    {
        get { return this.uniqueID; }
    }

    public int TypeID
    {
        get { return this.typeID; }
    }

    public bool IsActive
    {
        get { return this.enabled || this.visible; }
        set
        {
                this.enabled = this.visible = value;

                if (value)
                    this.CurrentAnimationType = this.CurrentAnimationType;
            }
    }

    public bool Enabled
    {
        get { return this.enabled; }
        set { this.enabled = value; }
    }

    public bool Visible
    {
        get { return this.visible; }
        set { this.visible = value; }
    }

    public int Damage
    {
        get { return this.damageValue; }
        set { this.damageValue = value; }
    }

    public float AttackRange
    {
        get { return this.attackRange; }
        set { this.attackRange = value; }
    }

    public Rectangle PatrolArea
    {
        get { return this.patrolArea; }
        set { this.patrolArea = value; }
    }

    public AILevel Intelligence
    {
        get { return this.intelligence; }
        set { this.intelligence = value; }
    }

    #endregion

    public Enemy(ActorStartValues startValues)
    {
            // Provide the enemy with a "unique" id
            this.uniqueID = Enemy.enemyCount++;

            this.typeID = 0;
            this.StartValues = startValues;
            this.SetDefaults();
        }

    public override void UpdateAnimation()
    {
            base.UpdateAnimation();
        }

    /// <summary>
    /// Updates the Enemy. The Player reference is used for determining the
    /// behavior of the Enemy.
    /// </summary>
    /// <param name="gameTime"></param>
    /// <param name="player"></param>
    public virtual void Update(GameTime gameTime, Player player)
    {
            if (this.Health > 0)
            {
                base.Update(gameTime);

                // TODO: Remove temp code and make better!
                if (!Enemy.WithinArea(this, this.patrolArea))
                    this.MoveToArea();
            }
            else
            {
                // TODO: Probably redo or make better later (Blink)

                // Start blinking after a set time
                if (this.blinkDelay > 0)
                    this.blinkDelay -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                else if (this.blinkCount > 0)
                {
                    // Count down to the next blink
                    this.blinkTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // Blink if it's time
                    if (this.blinkTime <= 0)
                    {
                        this.visible = !this.visible;
                        this.blinkTime = this.blinkInterval;

                        // Decrease the number of blinks to do
                        if (!this.visible)
                            this.blinkCount--;
                    }

                    // Make sure we're not visible if we're done blinking
                    if (this.blinkCount == 0 && this.visible)
                    {
                        this.visible = false;
                        this.enabled = false;
                    }
                }
            }
        }

    public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
            if (this.visible)
                base.Draw(spriteBatch, offsetPosition);
        }

    #region Patrol control

    /// <summary>
    /// Checks whether the Actor's collision box is contained within the patrol area.
    /// </summary>
    /// <param name="actor">The Actor who's collision box should be to checked.</param>
    /// <param name="area">Area to use.</param>
    /// <returns>True if the Actor's collision box is completly contained
    /// within the patrol area; false otherwise.</returns>
    public static bool WithinArea(Actor actor, Rectangle area)
    {
            return Enemy.WithinArea(actor, area, actor.Position);
        }

    /// <summary>
    /// Checks whether the Actor's collision box is contained within the patrol area.
    /// </summary>
    /// <param name="actor">The Actor who's collision box should be checked.</param>
    /// <param name="area">Area to use.</param>
    /// <param name="position">Position to use insted of the Actor's own position.</param>
    /// <returns>True if the Actor's collision box is completly contained
    /// within the patrol area; false otherwise.</returns>
    public static bool WithinArea(Actor actor, Rectangle area, Vector2 position)
    {
            Rectangle collBox = actor.CollisionBox;
            collBox.X += (int)(actor.Position.X - actor.Origin.X);
            collBox.Y += (int)(actor.Position.Y - actor.Origin.Y);

            return area.Contains(collBox);
        }

    /// <summary>
    /// Makes the current Enemy move towards the assigned patrol area. If the
    /// Enemy is already within the patrol area nothing will happen.
    /// </summary>
    private void MoveToArea()
    {
            // Make the enemy move in the correct direction in X
            if ((this.Position.X < this.patrolArea.X + this.patrolArea.Width / 2 && this.Velocity.X < 0) ||
                (this.Position.X > this.patrolArea.X + this.patrolArea.Width / 2 && this.Velocity.X > 0))
            {
                Vector2 velocity = this.Velocity;
                velocity.X *= -1;
                this.Velocity = velocity;
            }
        }

    #endregion

    #region Other

    /// <summary>
    /// Sets all fields to their default values. Values stored in Enemy.StartValues
    /// are also set. Override this method to set values specific to the
    /// enemy type.
    /// </summary>
    public virtual void SetDefaults()
    {
            this.ApplyStartValues();
        }

    /// <summary>
    /// Creates a shallow copy of the Enemy instance.
    /// </summary>
    /// <returns></returns>
    public virtual Enemy Copy(ActorStartValues startValues)
    {
            Enemy enemy = new Enemy(startValues);
            this.Copy(enemy);

            return enemy;
        }

    /// <summary>
    /// Copies the value if the current instance to the passed in enemy.
    /// </summary>
    /// <param name="enemy"></param>
    public virtual void Copy(Enemy enemy)
    {
            enemy.attackRange = this.attackRange;
            enemy.behavior = this.behavior;
            enemy.CanCollide = this.CanCollide;
            enemy.CanTakeDamage = this.CanTakeDamage;
            enemy.CollisionBox = this.CollisionBox;
            enemy.damageValue = this.damageValue;
            enemy.enabled = this.enabled;
            enemy.intelligence = this.intelligence;
            enemy.Health = this.Health;

            enemy.blinkCount = this.blinkCount;
            enemy.blinkDelay = this.blinkDelay;
            enemy.blinkInterval = this.blinkInterval;
            enemy.blinkTime = this.blinkTime;

            enemy.CurrentAnimationType = this.CurrentAnimationType;
        }

    #endregion

    #region ICollisionObject

    public override ObjectType Type
    {
        get { return ObjectType.Enemy; }
    }

    public override void TakeDamage(float damageLevel)
    {
            this.Health -= damageLevel;

            if (this.Health <= 0)
            {
                this.Velocity*= Vector2.Zero;
                this.CanCollide = false;
                this.CanTakeDamage = false;
            }
        }

    public override void OnCollision(ICollisionObject collidedObject, BoxSide collisionSides, Vector2 position, Vector2 velocity)
    {
            //base.OnCollision(collidedObject, collisionSides, position, velocity);

            if (collidedObject.Type == ObjectType.Tile)
            {
                this.Position = position;

                if ((collisionSides & BoxSide.Left) != 0 || (collisionSides & BoxSide.Right) != 0)
                {
                    // Jump
                    velocity.Y = 200 * Math.Sign(this.Acceleration.Y) * -1;

                    // Reverse velocity in X
                    velocity.X = this.Velocity.X;// *-1;
                }

                this.Velocity = velocity;
            }
            else if (collidedObject.Type == ObjectType.Player)
                collidedObject.TakeDamage(this.damageValue);
        }

    #endregion
}