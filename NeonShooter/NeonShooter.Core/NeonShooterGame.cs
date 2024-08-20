using BloomPostprocess;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

#if !__IOS__
using Microsoft.Xna.Framework.Media;
#endif

namespace NeonShooter
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class NeonShooterGame: Microsoft.Xna.Framework.Game
	{
		// some helpful static properties
		public static NeonShooterGame Instance { get; private set; }
		public static Viewport Viewport { get { return Instance.GraphicsDevice.Viewport; } }
		public static Vector2 ScreenSize { get { return new Vector2(Viewport.Width, Viewport.Height); } }
		public static GameTime GameTime { get; private set; }

		// ---- JASON ---- START ----
		// number of 1/60 frames that passed
		public static double NumFrames { get { return GameTime.ElapsedGameTime.TotalSeconds / (1.0 / 60.0);  } }  
		// if this frame represents that 1/60th of a second tick
		public static bool TickThisFrame { get { return tickThisFrame; } }
		public static int FrameRate { get { return frameRate; } }
		// ---- JASON ---- END ----

		public static ParticleManager<ParticleState> ParticleManager { get; private set; }
		public static Grid Grid { get; private set; }

		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		BloomComponent bloom;

		bool paused = false;

		// ---- JASON ---- START ----
		private bool limitFrameRate60Hz = false;
		private static double frameAccumulation = 0.0;
		private static bool tickThisFrame = false;
		private static int frameRate = 0;
		private static int frameCountThisSecond = 0;
		private static double frameStartSecond = 0.0;
		// ---- JASON ---- END ----

		public NeonShooterGame()
		{
			Instance = this;
			graphics = new GraphicsDeviceManager(this);

			// ---- JASON ---- START ----
			// screen size
			//graphics.PreferredBackBufferWidth = 1920;
			//graphics.PreferredBackBufferHeight = 1080;
			graphics.PreferredBackBufferWidth = 1920 - 128;
			graphics.PreferredBackBufferHeight = 1080 - 128;
			//graphics.IsFullScreen = false;
			// frame rate
			graphics.SynchronizeWithVerticalRetrace = true;  // smoothest animation
			//graphics.SynchronizeWithVerticalRetrace = false;  // full frame-rate
			this.IsFixedTimeStep = false;
			this.TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
			// ---- JASON ---- END ----

			bloom = new BloomComponent(this);
			Components.Add(bloom);
			bloom.Settings = new BloomSettings(null, 0.25f, 4, 2, 1, 1.5f, 1);
			bloom.Visible = false;
		}

		protected override void Initialize()
		{
			this.Content.RootDirectory = "Content";

			ParticleManager = new ParticleManager<ParticleState>(1024 * 20, ParticleState.UpdateParticle);

			const int maxGridPoints = 1600;
			Vector2 gridSpacing = new Vector2((float)Math.Sqrt(Viewport.Width * Viewport.Height / maxGridPoints));
			Grid = new Grid(Viewport.Bounds, gridSpacing);

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			Art.Load(Content);
			Sound.Load(Content);

			EntityManager.Add(PlayerShip.Instance);


#if !__IOS__
			//Known issue that you get exceptions if you use Media PLayer while connected to your PC
			//See http://social.msdn.microsoft.com/Forums/en/windowsphone7series/thread/c8a243d2-d360-46b1-96bd-62b1ef268c66
			//Which means its impossible to test this from VS.
			//So we have to catch the exception and throw it away
			try
			{
				MediaPlayer.IsRepeating = true;
				MediaPlayer.Play(Sound.Music);
			}
			catch { }
#endif
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			GameTime = gameTime;

			// ---- JASON ---- START ----
			// tick = 1/60th of a second
			frameAccumulation += gameTime.ElapsedGameTime.TotalSeconds;
			tickThisFrame = frameAccumulation >= 1.0 / 60.0;
			if (tickThisFrame)
				frameAccumulation -= 1.0 / 60.0;
			// frame rate calculator
			if (gameTime.TotalGameTime.TotalSeconds > frameStartSecond + 1.0)
			{
				frameRate = frameCountThisSecond;
				frameCountThisSecond = 0;  // reset for next second
				frameStartSecond = gameTime.TotalGameTime.TotalSeconds;
			}
			else
				frameCountThisSecond++;  // only count frames full within the second.
			// ---- JASON ---- END ----

			Input.Update();

#if !__IOS__
			// Allows the game to exit
			if (Input.WasButtonPressed(Buttons.Back) || Input.WasKeyPressed(Keys.Escape))
				this.Exit();
#endif

			if (Input.WasKeyPressed(Keys.P))
				paused = !paused;
			if (Input.WasKeyPressed(Keys.B))
				bloom.Visible = !bloom.Visible;

			// ---- JASON ---- START ----
			if (Input.WasKeyPressed(Keys.F))
			{
				limitFrameRate60Hz = !limitFrameRate60Hz;
				NeonShooterGame.Instance.SetFrameRate(limitFrameRate60Hz);
			}
			// ---- JASON ---- END ----

			if (!paused)
			{
				PlayerStatus.Update();
				EntityManager.Update();
				EnemySpawner.Update();
				ParticleManager.Update();
				Grid.Update();
			}
			base.Update(gameTime);
		}

		/// <summary>
		/// Draws the game from background to foreground.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			bloom.BeginDraw();

			GraphicsDevice.Clear(Color.Black);

			spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Additive);
			EntityManager.Draw(spriteBatch);
			spriteBatch.End();

			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
			Grid.Draw(spriteBatch);
			ParticleManager.Draw(spriteBatch);
			spriteBatch.End();

			base.Draw(gameTime);

			// Draw the user interface without bloom
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

			DrawTitleSafeAlignedString("Lives: " + PlayerStatus.Lives, 5);

			// ---- JASON ---- START ----
			DrawTitleSafeAlignedString("Frame Rate: " + NeonShooterGame.FrameRate + " Hz", 35);
			// ---- JASON ---- END ----

			DrawTitleSafeRightAlignedString("Score: " + PlayerStatus.Score, 5);
			DrawTitleSafeRightAlignedString("Multiplier: " + PlayerStatus.Multiplier, 35);
			// draw the custom mouse cursor
			spriteBatch.Draw(Art.Pointer, Input.MousePosition, Color.White);

			if (PlayerStatus.IsGameOver)
			{
				string text = "Game Over\n" +
					"Your Score: " + PlayerStatus.Score + "\n" +
					"High Score: " + PlayerStatus.HighScore;

				Vector2 textSize = Art.Font.MeasureString(text);
				spriteBatch.DrawString(Art.Font, text, ScreenSize / 2 - textSize / 2, Color.White);
			}

			spriteBatch.End();
		}

		private void DrawRightAlignedString(string text, float y)
		{
			var textWidth = Art.Font.MeasureString(text).X;
			spriteBatch.DrawString(Art.Font, text, new Vector2(ScreenSize.X - textWidth - 5, y), Color.White);
		}

		private void DrawTitleSafeAlignedString(string text, int pos)
		{
			// ---- JASON ---- START ----
			spriteBatch.DrawString(Art.Font, text, new Vector2(5, Viewport.TitleSafeArea.X + pos), Color.White);
			//spriteBatch.DrawString(Art.Font, text, new Vector2(Viewport.TitleSafeArea.X + pos), Color.White);
			// ---- JASON ---- END ----
		}

		private void DrawTitleSafeRightAlignedString(string text, float y)
		{
			var textWidth = Art.Font.MeasureString(text).X;
			spriteBatch.DrawString(Art.Font, text, new Vector2(ScreenSize.X - textWidth - 5 - Viewport.TitleSafeArea.X, Viewport.TitleSafeArea.Y + y), Color.White);
		}

		// ---- JASON ---- START ----
		internal void SetFrameRate(bool limitFrameRate60Hz)
		{
			// frame rate
			this.IsFixedTimeStep = limitFrameRate60Hz;
			this.TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);  // only used if IsFixedTimeStep == true
			graphics.SynchronizeWithVerticalRetrace = (limitFrameRate60Hz == false);
			graphics.ApplyChanges();
		}
		// ---- JASON ---- END ----

	}
}
