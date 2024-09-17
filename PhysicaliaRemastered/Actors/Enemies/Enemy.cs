using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Graphics.TileEngine;
using XNALibrary.Interfaces;

namespace PhysicaliaRemastered.Actors.Enemies;

/// <summary>
/// Describes the intelligence level of an Enemy.
/// </summary>
public enum AiLevel
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
    private static int _enemyCount;

    // AI
    private Rectangle _patrolArea;
    private EnemyBehavior _behavior;

    // Post-death
    private float _blinkDelay = 1F;
    private int _blinkCount = 4;
    private float _blinkInterval = 0.15F;
    private float _blinkTime;

    public int UniqueId { get; }

    public int TypeId { get; }

    public bool IsActive
    {
        get => Enabled || Visible;
        set
        {
            Enabled = Visible = value;

            if (value)
                CurrentAnimationType = CurrentAnimationType;
        }
    }

    public bool Enabled { get; set; }

    public bool Visible { get; set; }

    public int Damage { get; set; }

    public float AttackRange { get; set; }

    public Rectangle PatrolArea
    {
        get => _patrolArea;
        set => _patrolArea = value;
    }

    public AiLevel Intelligence { get; set; }

    public Enemy(ActorStartValues startValues)
    {
        // Provide the enemy with a "unique" id
        UniqueId = _enemyCount++;

        TypeId = 0;
        StartValues = startValues;
        SetDefaults();
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
        if (Health > 0)
        {
            base.Update(gameTime);

            // TODO: Remove temp code and make better!
            if (!WithinArea(this, _patrolArea))
                MoveToArea();
        }
        else
        {
            // TODO: Probably redo or make better later (Blink)

            // Start blinking after a set time
            if (_blinkDelay > 0)
                _blinkDelay -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            else if (_blinkCount > 0)
            {
                // Count down to the next blink
                _blinkTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Blink if it's time
                if (_blinkTime <= 0)
                {
                    Visible = !Visible;
                    _blinkTime = _blinkInterval;

                    // Decrease the number of blinks to do
                    if (!Visible)
                        _blinkCount--;
                }

                // Make sure we're not visible if we're done blinking
                if (_blinkCount == 0 && Visible)
                {
                    Visible = false;
                    Enabled = false;
                }
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
        if (Visible)
            base.Draw(spriteBatch, offsetPosition);
    }

    /// <summary>
    /// Checks whether the Actor's collision box is contained within the patrol area.
    /// </summary>
    /// <param name="actor">The Actor who's collision box should be to checked.</param>
    /// <param name="area">Area to use.</param>
    /// <returns>True if the Actor's collision box is completly contained
    /// within the patrol area; false otherwise.</returns>
    public static bool WithinArea(Actor actor, Rectangle area)
    {
        return WithinArea(actor, area, actor.Position);
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
        if ((Position.X < _patrolArea.X + _patrolArea.Width / 2 && Velocity.X < 0) ||
            (Position.X > _patrolArea.X + _patrolArea.Width / 2 && Velocity.X > 0))
        {
            Vector2 velocity = Velocity;
            velocity.X *= -1;
            Velocity = velocity;
        }
    }

    /// <summary>
    /// Sets all fields to their default values. Values stored in Enemy.StartValues
    /// are also set. Override this method to set values specific to the
    /// enemy type.
    /// </summary>
    public virtual void SetDefaults()
    {
        ApplyStartValues();
    }

    /// <summary>
    /// Creates a shallow copy of the Enemy instance.
    /// </summary>
    /// <returns></returns>
    public virtual Enemy Copy(ActorStartValues startValues)
    {
        Enemy enemy = new Enemy(startValues);
        Copy(enemy);

        return enemy;
    }

    /// <summary>
    /// Copies the value if the current instance to the passed in enemy.
    /// </summary>
    /// <param name="enemy"></param>
    public virtual void Copy(Enemy enemy)
    {
        enemy.AttackRange = AttackRange;
        enemy._behavior = _behavior;
        enemy.CanCollide = CanCollide;
        enemy.CanTakeDamage = CanTakeDamage;
        enemy.CollisionBox = CollisionBox;
        enemy.Damage = Damage;
        enemy.Enabled = Enabled;
        enemy.Intelligence = Intelligence;
        enemy.Health = Health;

        enemy._blinkCount = _blinkCount;
        enemy._blinkDelay = _blinkDelay;
        enemy._blinkInterval = _blinkInterval;
        enemy._blinkTime = _blinkTime;

        enemy.CurrentAnimationType = CurrentAnimationType;
    }

    public override ObjectType Type => ObjectType.Enemy;

    public override void TakeDamage(float damageLevel)
    {
        Health -= damageLevel;

        if (Health <= 0)
        {
            Velocity *= Vector2.Zero;
            CanCollide = false;
            CanTakeDamage = false;
        }
    }

    public override void OnCollision(ICollisionObject collidedObject, BoxSide collisionSides, Vector2 position,
        Vector2 velocity)
    {
        //base.OnCollision(collidedObject, collisionSides, position, velocity);

        if (collidedObject.Type == ObjectType.Tile)
        {
            Position = position;

            if ((collisionSides & BoxSide.Left) != 0 || (collisionSides & BoxSide.Right) != 0)
            {
                // Jump
                velocity.Y = 200 * Math.Sign(Acceleration.Y) * -1;

                // Reverse velocity in X
                velocity.X = Velocity.X; // *-1;
            }

            Velocity = velocity;
        }
        else if (collidedObject.Type == ObjectType.Player)
            collidedObject.TakeDamage(Damage);
    }
}