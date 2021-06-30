﻿using Terraria.ModLoader;

namespace WireShark {
    public class WireShark : Mod {
        public override void Load() {
            WiringWarpper.Initialize();
            On.Terraria.Wiring.Actuate += Wiring_Actuate;
            On.Terraria.Wiring.ActuateForced += Wiring_ActuateForced;
            On.Terraria.Wiring.CheckMech += Wiring_CheckMech;
            On.Terraria.Wiring.DeActive += Wiring_DeActive;
            On.Terraria.Wiring.HitSwitch += Wiring_HitSwitch;
            On.Terraria.Wiring.Initialize += Wiring_Initialize;
            On.Terraria.Wiring.PokeLogicGate += Wiring_PokeLogicGate;
            On.Terraria.Wiring.ReActive += Wiring_ReActive;
            On.Terraria.Wiring.SetCurrentUser += Wiring_SetCurrentUser;
            On.Terraria.Wiring.SkipWire_int_int += Wiring_SkipWire_int_int;
            On.Terraria.Wiring.SkipWire_Point16 += Wiring_SkipWire_Point16;
            On.Terraria.Wiring.TripWire += Wiring_TripWire;
            On.Terraria.Wiring.UpdateMech += Wiring_UpdateMech;

            On.Terraria.WorldGen.StartRoomCheck += WorldGen_StartRoomCheck;
        }

        private bool WorldGen_StartRoomCheck(On.Terraria.WorldGen.orig_StartRoomCheck orig, int x, int y) {
            return false;
        }

        private void Wiring_UpdateMech(On.Terraria.Wiring.orig_UpdateMech orig) {
            WiringWarpper.UpdateMech();
        }

        private void Wiring_SkipWire_Point16(On.Terraria.Wiring.orig_SkipWire_Point16 orig, Terraria.DataStructures.Point16 point) {
            
        }

        private void Wiring_SkipWire_int_int(On.Terraria.Wiring.orig_SkipWire_int_int orig, int x, int y) {
            
        }

        private void Wiring_SetCurrentUser(On.Terraria.Wiring.orig_SetCurrentUser orig, int plr) {
            WiringWarpper.SetCurrentUser(plr);
        }

        private void Wiring_ReActive(On.Terraria.Wiring.orig_ReActive orig, int i, int j) {
            WiringWarpper.ReActive(i, j);
        }

        private void Wiring_PokeLogicGate(On.Terraria.Wiring.orig_PokeLogicGate orig, int lampX, int lampY) {
            WiringWarpper.PokeLogicGate(lampX, lampY);
        }
        
        private void Wiring_Initialize(On.Terraria.Wiring.orig_Initialize orig) {
            WiringWarpper.Initialize();
        }


        private void Wiring_HitSwitch(On.Terraria.Wiring.orig_HitSwitch orig, int i, int j) {
            WiringWarpper.HitSwitch(i, j);
        }

        private void Wiring_DeActive(On.Terraria.Wiring.orig_DeActive orig, int i, int j) {
            WiringWarpper.DeActive(i, j);
        }

        private bool Wiring_CheckMech(On.Terraria.Wiring.orig_CheckMech orig, int i, int j, int time) {
            return WiringWarpper.CheckMech(i, j, time);
        }

        private void Wiring_ActuateForced(On.Terraria.Wiring.orig_ActuateForced orig, int i, int j) {
            WiringWarpper.ActuateForced(i, j);
        }

        private bool Wiring_Actuate(On.Terraria.Wiring.orig_Actuate orig, int i, int j) {
            return WiringWarpper.Actuate(i, j);
        }


        private void Wiring_TripWire(On.Terraria.Wiring.orig_TripWire orig, int left, int top, int width, int height) {
            WiringWarpper.BigTripWire(left, top, width, height);
        }


        public override void Unload() {
            On.Terraria.Wiring.Actuate -= Wiring_Actuate;
            On.Terraria.Wiring.ActuateForced -= Wiring_ActuateForced;
            On.Terraria.Wiring.CheckMech -= Wiring_CheckMech;
            On.Terraria.Wiring.DeActive -= Wiring_DeActive;
            On.Terraria.Wiring.HitSwitch -= Wiring_HitSwitch;
            On.Terraria.Wiring.Initialize -= Wiring_Initialize;
            On.Terraria.Wiring.PokeLogicGate -= Wiring_PokeLogicGate;
            On.Terraria.Wiring.ReActive -= Wiring_ReActive;
            On.Terraria.Wiring.SetCurrentUser -= Wiring_SetCurrentUser;
            On.Terraria.Wiring.SkipWire_int_int -= Wiring_SkipWire_int_int;
            On.Terraria.Wiring.SkipWire_Point16 -= Wiring_SkipWire_Point16;
            On.Terraria.Wiring.TripWire -= Wiring_TripWire;
            On.Terraria.Wiring.UpdateMech -= Wiring_UpdateMech;


            On.Terraria.WorldGen.StartRoomCheck -= WorldGen_StartRoomCheck;
        }
    }
}
