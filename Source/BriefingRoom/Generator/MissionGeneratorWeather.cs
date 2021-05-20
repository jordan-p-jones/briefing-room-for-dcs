﻿/*
==========================================================================
This file is part of Briefing Room for DCS World, a mission
generator for DCS World, by @akaAgar (https://github.com/akaAgar/briefing-room-for-dcs)

Briefing Room for DCS World is free software: you can redistribute it
and/or modify it under the terms of the GNU General Public License
as published by the Free Software Foundation, either version 3 of
the License, or (at your option) any later version.

Briefing Room for DCS World is distributed in the hope that it will
be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Briefing Room for DCS World. If not, see https://www.gnu.org/licenses/
==========================================================================
*/

using BriefingRoom4DCS.Data;
using BriefingRoom4DCS.Mission;
using BriefingRoom4DCS.Template;
using System;

namespace BriefingRoom4DCS.Generator
{
    /// <summary>
    /// Generates a <see cref="DCSMission"/>'s weather
    /// </summary>
    internal class MissionGeneratorWeather : IDisposable
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        internal MissionGeneratorWeather() { }

        /// <summary>
        /// IDispose implementation.
        /// </summary>
        public void Dispose() { }

        /// <summary>
        /// Generates weather settings (precipitation, cloud coverage, temperature...) for the mission.
        /// Must be called after mission date has been set because min/max temperature changes every month.
        /// </summary>
        /// <param name="mission">Mission to generate.</param>
        /// <param name="template">Mission template to use.</param>
        /// <param name="theaterDB">Theater database entry.</param>
        /// <param name="month">Month during which the mission takes place.</param>
        /// <param name="turbulenceFromWeather">Amount of turbulence (in m/s) to add to the default wind turbulence.</param>
        internal void GenerateWeather(DCSMission mission, MissionTemplate template, DBEntryTheater theaterDB, Month month, out int turbulenceFromWeather)
        {
            DBEntryWeatherPreset weatherDB;
            if (string.IsNullOrEmpty(template.EnvironmentWeatherPreset)) // Random weather
                weatherDB = Toolbox.RandomFrom(Database.Instance.GetAllEntries<DBEntryWeatherPreset>());
            else
                weatherDB = Database.Instance.GetEntry<DBEntryWeatherPreset>(template.EnvironmentWeatherPreset);

            mission.SetValue("WEATHER_NAME", weatherDB.BriefingDescription);
            mission.SetValue("WEATHER_CLOUDS_BASE", weatherDB.CloudsBase.GetValue());
            mission.SetValue("WEATHER_CLOUDS_PRESET", Toolbox.RandomFrom(weatherDB.CloudsPresets));
            mission.SetValue("WEATHER_CLOUDS_THICKNESS", weatherDB.CloudsThickness.GetValue());
            mission.SetValue("WEATHER_DUST", weatherDB.Dust);
            mission.SetValue("WEATHER_DUST_DENSITY", weatherDB.DustDensity.GetValue());
            mission.SetValue("WEATHER_FOG", weatherDB.Fog);
            mission.SetValue("WEATHER_FOG_THICKNESS", weatherDB.FogThickness.GetValue());
            mission.SetValue("WEATHER_FOG_VISIBILITY", weatherDB.FogVisibility.GetValue());
            mission.SetValue("WEATHER_QNH", weatherDB.QNH.GetValue());
            mission.SetValue("WEATHER_TEMPERATURE", theaterDB.Temperature[(int)month].GetValue());
            mission.SetValue("WEATHER_VISIBILITY", weatherDB.Visibility.GetValue());

            turbulenceFromWeather = weatherDB.Turbulence.GetValue();
        }

        /// <summary>
        /// Generates wind settings for the mission.
        /// Must be called once mission weather level has been set, as weather is used for auto wind.
        /// </summary>
        /// <param name="mission">Mission to generate.</param>
        /// <param name="template">Mission template to use.</param>
        /// <param name="turbulenceFromWeather">Amount of turbulence (in m/s) to add to the default wind turbulence.</param>
        /// <param name="windSpeedAtSeaLevel">Wind speed at sea level, in m/s.</param>
        /// <param name="windDirectionAtSeaLevel">Wind direction at sea level, in radians.</param>
        internal void GenerateWind(DCSMission mission, MissionTemplate template, int turbulenceFromWeather, out double windSpeedAtSeaLevel, out double windDirectionAtSeaLevel)
        {
            windSpeedAtSeaLevel = 0;
            windDirectionAtSeaLevel = 0;
            
            Wind windLevel = template.EnvironmentWind == Wind.Random ? windLevel = PickRandomWindLevel() : template.EnvironmentWind;
            BriefingRoom.PrintToLog($"Wind speed level set to \"{windLevel}\".");

            int windAverage = 0;
            for (int i = 0; i < 3; i++)
            {
                int windSpeed = Database.Instance.Common.Wind[(int)windLevel].Wind.GetValue();
                int windDirection = windSpeed > 0 ? Toolbox.RandomInt(0, 360) : 0;
                if (i == 0)
                {
                    windSpeedAtSeaLevel = windSpeed;
                    windDirectionAtSeaLevel = windDirection * Toolbox.DEGREES_TO_RADIANS;
                }
                windAverage += windSpeed;

                mission.SetValue($"WEATHER_WIND_SPEED{i + 1}", windSpeed);
                mission.SetValue($"WEATHER_WIND_DIRECTION{i + 1}", windDirection);
            }
            windAverage /= 3;

            mission.SetValue($"WEATHER_WIND_NAME", windLevel.ToString()); // TODO: get name from attribute
            mission.SetValue($"WEATHER_WIND_SPEED_AVERAGE", windAverage);

            mission.SetValue("WEATHER_GROUND_TURBULENCE", Database.Instance.Common.Wind[(int)windLevel].Turbulence.GetValue() + turbulenceFromWeather);
        }

        /// <summary>
        /// Picks a random wind level, with a major chance of calm wind and very little chance of powerful wind, so most missions don't take place during a storm.
        /// </summary>
        /// <returns>A wind level</returns>
        private Wind PickRandomWindLevel()
        {
            return Toolbox.RandomFrom(
                Wind.Calm, Wind.Calm, Wind.Calm, Wind.Calm, Wind.Calm,
                Wind.LightBreeze, Wind.LightBreeze, Wind.LightBreeze, Wind.LightBreeze,
                Wind.ModerateBreeze, Wind.ModerateBreeze,
                Wind.StrongBreeze);
        }
    }
}
