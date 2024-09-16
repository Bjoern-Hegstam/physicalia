using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNALibrary.Graphics.Animation;
using XNALibrary.Graphics.ParticleEngine;
using XNALibrary.Graphics.Sprites;
using XNALibrary.Graphics.TileEngine;

namespace XNALibrary.Interfaces;

public interface ITextureLibrary
{
    Texture2D this[int key] { get; }

    bool AddTexture(int key, Texture2D texture);
    bool RemoveTexture(int key);
    Texture2D GetTexture(int key);

    bool ContainsKey(int key);
    void Clear();

    void LoadXml(string path, GraphicsDevice graphics);
    void LoadXml(XmlReader reader, GraphicsDevice graphics);
}

public interface ITileLibrary
{
    bool AddTile(int key, Tile tile);
    bool RemoveTile(int key);
    Tile GetTile(int key);

    bool ContainsKey(int key);
    void Clear();

    void LoadXml(string path, ISpriteLibrary spriteLibrary, IAnimationManager animationManager);
    void LoadXml(XmlReader reader, ISpriteLibrary spriteLibrary, IAnimationManager animationManager);
}

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

    void CheckCollisions(ICollisionObject[] collObjects);
    void CheckCollisions(ICollisionObject collObject);

    // Makes the IParticleEngine empty its buffer of inactive particles
    void ClearBuffer();

    void Prepare();

    void Update(GameTime gameTime);
    void Draw(SpriteBatch spriteBatch);
    void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition);
}

public interface ISpriteLibrary
{
    void AddSprite(int key, Sprite sprite);
    void RemoveSprite(int key);
    Sprite GetSprite(int key);

    void LoadXml(string path);
}

public interface IScreenSampler
{
    Vector2 Position { get; set;}

    int MaxWidth { get;  set;}
    int MaxHeight { get; set;}

    int Width { get; set;}
    int Height { get; set;}

    bool IsOnScreen(Rectangle boundingBox);
    bool IsOnScreen(int x, int y, int width, int height);
}

public interface IInputHandler
{
    KeyboardState OldKeyBoardState { get; }
    KeyboardState CurrentKeyBoardState { get; }

    MouseState OldMouseState { get; }
    MouseState CurrentMouseState { get; }

    GamePadState[] OldGamePadState { get; }
    GamePadState[] CurrentGamePadState { get; }

    bool IsPressed(Keys key);
    bool IsPressed(PlayerIndex playerIndex, Buttons button);

    bool IsReleased(Keys key);
    bool IsReleased(PlayerIndex playerIndex, Buttons button);

    bool IsHolding(Keys key);
    bool IsHolding(PlayerIndex playerIndex, Buttons button);

    Vector2 GetMouseMove();
    Vector2 GetNormalizedMouseMove();

    void GetMouseMove(out Vector2 distance);
    void GetNormalizedMouseMove(out Vector2 distance);
}


public interface IAnimationManager
{
    bool AddBankAnimation(int key, Animation animation);
    bool AddBankAnimation(int key, Rectangle startFrame, int columns, int rows, float framerate, int textureKey);
    void RemoveBankAnimation(int key);
    Animation GetBankAnimation(int key);

    void ClearPlaybackAnimations();
    bool AddPlaybackAnimation(Animation animation);
    Animation AddPlaybackAnimation(int bankKey);

    bool HasBankAnimation(int bankKey);

    void LoadXml(string path);
    void LoadXml(XmlReader reader);
}