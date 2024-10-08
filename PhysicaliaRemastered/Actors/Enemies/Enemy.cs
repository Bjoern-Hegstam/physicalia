using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Collision;
using XNALibrary.TileEngine;

namespace PhysicaliaRemastered.Actors.Enemies;

public class Enemy : Actor
{
    private static int _enemyCount;

    private Rectangle _patrolArea;

    // Post-death
    private float _blinkDelay = 1F;
    private int _blinkCount = 4;
    private float _blinkInterval = 0.15F;
    private float _blinkTime;

    public int UniqueId { get; }

    public readonly int TypeId = 0;

    public bool IsActive
    {
        get => Enabled || Visible;
        set => Enabled = Visible = value;
    }

    public bool Enabled { get; set; }

    public bool Visible { get; set; }

    public int Damage { get; set; }

    public Rectangle PatrolArea
    {
        get => _patrolArea;
        set => _patrolArea = value;
    }

    public Enemy(ActorStartValues startValues)
    {
        UniqueId = _enemyCount++;

        StartValues = startValues;
        ApplyStartValues();
    }

    public virtual void Update(GameTime gameTime, Player player)
    {
        if (CurrentState is not ActorState.Dead)
        {
            base.Update(gameTime);

            if (!IsWithin(_patrolArea))
            {
                MoveToArea();
            }
        }
        else
        {
            // Start blinking after a set time
            if (_blinkDelay > 0)
            {
                _blinkDelay -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
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
                    {
                        _blinkCount--;
                    }
                }

                // Make sure we're not visible if we're done blinking
                if (_blinkCount != 0 || !Visible)
                {
                    return;
                }

                Visible = false;
                Enabled = false;
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch, Vector2 viewportPosition)
    {
        if (Visible)
        {
            base.Draw(spriteBatch, viewportPosition);
        }
    }

    /// <summary>
    /// Makes the current Enemy move towards the assigned patrol area. If the
    /// Enemy is already within the patrol area nothing will happen.
    /// </summary>
    private void MoveToArea()
    {
        // Make the enemy move in the correct direction in X
        if ((Position.X < _patrolArea.X + _patrolArea.Width / 2f && Velocity.X < 0) ||
            (Position.X > _patrolArea.X + _patrolArea.Width / 2f && Velocity.X > 0))
        {
            Vector2 velocity = Velocity;
            velocity.X *= -1;
            Velocity = velocity;
        }
    }

    /// <summary>
    /// Creates a shallow copy of the Enemy instance.
    /// </summary>
    /// <returns></returns>
    public virtual Enemy Copy(ActorStartValues startValues)
    {
        var enemy = new Enemy(startValues);
        Copy(enemy);

        return enemy;
    }

    /// <summary>
    /// Copies the value if the current instance to the passed in enemy.
    /// </summary>
    /// <param name="enemy"></param>
    public virtual void Copy(Enemy enemy)
    {
        enemy.CanCollide = CanCollide;
        enemy.CanTakeDamage = CanTakeDamage;
        enemy.CollisionBoxDefinition = CollisionBoxDefinition;
        enemy.Damage = Damage;
        enemy.Enabled = Enabled;
        enemy.Health = Health;

        enemy._blinkCount = _blinkCount;
        enemy._blinkDelay = _blinkDelay;
        enemy._blinkInterval = _blinkInterval;
        enemy._blinkTime = _blinkTime;

        enemy.CurrentState = CurrentState;
    }

    public override void TakeDamage(float damageLevel)
    {
        Health -= damageLevel;

        if (Health > 0)
        {
            return;
        }

        Velocity = Vector2.Zero;
        CanCollide = false;
        CanTakeDamage = false;
    }

    public override void OnCollision(ICollidable collidedObject, List<BoxSide> collidedSides,
        Vector2 suggestedNewPosition, Vector2 suggestedNewVelocity)
    {
        switch (collidedObject)
        {
            case Tile:
            {
                Position = suggestedNewPosition;

                if (collidedSides.Contains(BoxSide.Left) || collidedSides.Contains(BoxSide.Right))
                {
                    // Jump
                    suggestedNewVelocity.Y = 200 * Math.Sign(Acceleration.Y) * -1;

                    // Reverse velocity in X
                    suggestedNewVelocity.X = Velocity.X; // *-1;
                }

                Velocity = suggestedNewVelocity;
                break;
            }
            case Player:
                collidedObject.TakeDamage(Damage);
                break;
        }
    }
}