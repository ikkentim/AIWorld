using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace AIWorld.Entities
{
    interface IHitable : IEntity
    {
        bool Hit(Projectile projectile);
    }
}
