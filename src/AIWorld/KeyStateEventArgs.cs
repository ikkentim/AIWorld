using System;
using Microsoft.Xna.Framework.Input;

namespace AIWorld
{
    public class KeyStateEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyStateEventArgs"/> class.
        /// </summary>
        /// <param name="newKeys">The new keys.</param>
        /// <param name="oldKeys">The old keys.</param>
        public KeyStateEventArgs(Keys[] newKeys, Keys[] oldKeys)
        {
            if (newKeys == null) throw new ArgumentNullException("newKeys");
            if (oldKeys == null) throw new ArgumentNullException("oldKeys");
            NewKeys = newKeys;
            OldKeys = oldKeys;
        }

        /// <summary>
        /// Gets the new keys.
        /// </summary>
        public Keys[] NewKeys { get; private set; }
        /// <summary>
        /// Gets the old keys.
        /// </summary>
        public Keys[] OldKeys { get; private set; }
    }
}