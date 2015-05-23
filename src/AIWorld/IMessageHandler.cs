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

namespace AIWorld
{
    /// <summary>
    ///     Contains methods for objects which can handle messages.
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        ///     Is called when a message has been sent to this instance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="contents">The contents.</param>
        void HandleMessage(int message, int contents);
    }
}