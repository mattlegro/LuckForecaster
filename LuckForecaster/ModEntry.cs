using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Objects;
using StardewValley;
using StardewModdingAPI.Utilities;
using System.Threading.Tasks;

namespace LuckForecaster
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private ModConfig Config;
        private string LuckForecast;
        private string WeatherForecast;
        private string[] WeeklyRecipe;
        private TV Television;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            if (Config.EnableShortcutKeys)
                helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the game begins a new day (including when the player loads a save).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private async void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            Television = new TV();
            LuckForecast = null;
            WeatherForecast = null;
            WeeklyRecipe = null;

            if (Config.ShowLuckForecastOnWakeUp)
            {
                await Task.Delay(Config.InitialDelay * 1000);
                ShowLuckForecast();
            }

            if (Config.ShowWeatherOnWakeUp)
            {
                await Task.Delay(Config.InitialDelay * 1000);
                ShowWeather();
            }

            if (Config.ShowRecipeAlert)
            {
                await Task.Delay(Config.OffsetDelay * 1000);
                ShowRecipeAlert();
            }

        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            if (Config.TipKey == e.Button)
            {
                ShowLuckForecast();
            }
            else if (Config.WeatherKey == e.Button)
            {
                ShowWeather();
            }
        }

        private void ShowLuckForecast()
        {
            if (LuckForecast == null)
            {
                LuckForecast = Helper.Reflection
                                .GetMethod(Television, "getFortuneForecast")
                                .Invoke<string>(Game1.player);
            }

            Game1.addHUDMessage(new HUDMessage(LuckForecast, 2));
        }

        private void ShowWeather()
        {
            if (WeatherForecast == null)
            {
                WeatherForecast = Helper.Reflection
                                .GetMethod(Television, "getWeatherForecast")
                                .Invoke<string>();
            }

            Game1.addHUDMessage(new HUDMessage(WeatherForecast, 2));
        }

        private void ShowRecipeAlert()
        {
            SDate date = SDate.Now();
            int day = date.Day;
            Monitor.Log($"{Game1.player.Name} got {day} for the day of the month.", LogLevel.Debug);
            if (WeeklyRecipe == null && (day % 7 == 3 || day % 7 == 0))
            {
                WeeklyRecipe = Helper.Reflection
                                .GetMethod(Television, "getWeeklyRecipe")
                                .Invoke<string[]>();
                this.Monitor.Log($"{Game1.player.Name} got {WeeklyRecipe[0]} and {WeeklyRecipe[1]}.", LogLevel.Debug);
                string resultStr = WeeklyRecipe[1].Substring(0, 11);
                if (resultStr != "You already")
                {
                    Game1.addHUDMessage(new HUDMessage("There is a new recipe to learn on TV today!", 2));
                }
            }

            
        }

    }
}
