// AIWorld
// Copyright 2015 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AIWorld.Scripting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace AIWorld.Services
{
    internal class SoundService : GameComponent, ISoundService
    {
        private readonly ICameraService _cameraService;

        private readonly Dictionary<SoundEffectInstance, AudioEmitter> _sounds =
            new Dictionary<SoundEffectInstance, AudioEmitter>();

        public SoundService(Game game, ICameraService cameraService) : base(game)
        {
            _cameraService = cameraService;
        }

        public bool PlaySound(string sound, float volume, Vector3 position)
        {
            try
            {
                var audioEmitter = new AudioEmitter
                {
                    Position = position,
                    Up = Vector3.Up,
                    Forward = Vector3.Right,
                    Velocity = Vector3.Forward
                };

                var soundEffect = Game.Content.Load<SoundEffect>(sound);

                var soundEffectInstance = soundEffect.CreateInstance();
                soundEffectInstance.IsLooped = false;
                soundEffectInstance.Volume = volume;
                soundEffectInstance.Apply3D(_cameraService.AudioListener, audioEmitter);
                soundEffectInstance.Play();

                _sounds[soundEffectInstance] = audioEmitter;
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        [ScriptingFunction]
        public bool PlaySound(string sound, float volume, float x, float y)
        {
            return PlaySound(sound, volume, new Vector3(x, 0, y));
        }

        #region Overrides of GameComponent

        public override void Update(GameTime gameTime)
        {
            foreach (var pair in _sounds.Where(p => p.Key.State == SoundState.Stopped).ToArray())
            {
                _sounds.Remove(pair.Key);
                pair.Key.Dispose();
            }

            base.Update(gameTime);
        }

        #endregion
    }
}