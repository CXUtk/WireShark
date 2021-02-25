using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WireShark {
    public class MyModPlayer : ModPlayer {

        public override void OnEnterWorld(Player player) {
            WiringWarpper.GetWireAccelerator().Preprocess();
        }

        public override void PostUpdate() {
            base.PostUpdate();
            //if (Main.mouseLeft && Main.mouseLeftRelease) {
            //    Main.NewText(Main.MouseWorld.ToTileCoordinates16(), Microsoft.Xna.Framework.Color.Red);
            //}
        }
    }
}
