using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Collision;

namespace XNALibrary.ParticleEngine;

public interface IParticleEngine
{
    Particle[] Particles { get; }

    bool HasDefinition(int definitionId);

    void AddDefinition(ParticleDefinition definition);

    // If rinseBuffer is true, all inactive particles of the removed type
    // are cleared from the buffer
    void RemoveDefinition(int definitionId, bool rinseBuffer);

    void Add(int typeId, int count);
    void Add(int typeId, int count, Vector2 position);

    // Velocity is calculated based on the angle and the default velocity
    void Add(int typeId, int count, Vector2 position, float angle);
    //void Add(int typeId, int count, Vector2 position, Vector2 velocity);
    //void Add(int typeId, int count, Vector2 position, Vector2 velocity, Vector2 acceleration);

    void CheckCollisions(IEnumerable<ICollisionObject> collObjects);
    void CheckCollisions(ICollisionObject collObject);

    // Makes the IParticleEngine empty its buffer of inactive particles
    void ClearBuffer();

    void Prepare();

    void Update(GameTime gameTime);
    void Draw(SpriteBatch spriteBatch);
    void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition);
}